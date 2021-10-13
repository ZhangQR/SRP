using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// 每一章都用命名空间包括起来，就是一个可独立运行的项目
namespace NiuBiRenderPipeline
{
    /// <summary>
    /// 因为 Render 会传过来很多相机，我们与其在 RenderPipelineInstance 中去渲染所有的相机
    /// 不如单独建一个类去处理单个相机
    /// </summary>
    public class CameraRenderer
    {

        // 把 Unity 传过来的上下文保存起来
        ScriptableRenderContext context;
        
        // 当前相机渲染器的相机
        Camera camera;
        
        CullingResults cullingResults;

        const string bufferName = "Render Camera";

        // 因为 unlitShaderTagId 不管在哪个类实例中都是 SRPDefaultUnlit，所以用 static
        // 也就是 “绝对事实” 可以用 static
        // 在可编程渲染管线中，默认的 LightMode 就是 SRPDefaultUnlit，所以没有指定 lightmode 的就会是这个，比如 Unlit/Color
        // 但是 standard 是指定了的哦 ~ Tags { "LightMode" = "ForwardBase" }
        // 在 URP 中，默认的 LightMode 是 UniversalForward，并且也是 Unlit 没有指定 LightMode，Lit 指定了
        static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

        CommandBuffer buffer = new CommandBuffer
        {
            name = bufferName
        };

        /// <summary>
        /// 这里的参数跟 Instance 里面的一样，但是 camera 只有一个，不是数组
        /// </summary>
        /// <param name="context"></param>
        /// <param name="camera"></param>
        public void Render(ScriptableRenderContext context, Camera camera)
        {
            this.context = context;
            this.camera = camera;

            // 如果剔除失败了，就不要渲染了（剔除失败难道不是只会导致效率变慢嘛？为什么直接舍弃这一次渲染）
            if (!Cull())
            {
                return;
            }

            Setup();
            DrawVisibleGeometry();
            Submit();
        }

        /// <summary>
        /// 设置相机信息
        /// </summary>
        void Setup()
        {
            // 设置相机的 VP 矩阵，V 矩阵里面包含了相机的位置和方向，P 包含了正交/透视投影
            // 如果这一句写在 Clear 下面，那么 Clear 在 Frame Debugger 中就是 Draw GL，
            // 原理是用 Hidden/InternalClear shader 画一个全屏的矩形，效率不高。
            // 但如果我们先设置好相机 VP 等属性，然后再 Clear，就会变成高效的 Clear。
            context.SetupCameraProperties(camera);
            
            // 每一帧一开始都要清除上一帧的内容，清除颜色缓冲还是深度还是模板可自由配置
            // ClearRenderTarget 被缓冲名字的 Sample 给包裹,如果写在 Sample 下面，就会嵌套
            // 写在上面就会合并，详情做下面的实验
            buffer.ClearRenderTarget(true, true, Color.clear);
            
            // 第一条命令是在 Comman Buffer 中
            // ExecuteBuffer 不是真的执行，是将命令放到 context 中
            // 这里的参数，并不是显示在 Frame debugger 里的
            // Frame debugger 显示的是缓冲区名字
            buffer.BeginSample(bufferName);
            
            //{
            //    // 验证，同一个 Buffer，BeginSample-EndSample-BeginSample-EndSample 在 Frame Debugger 中会合并
            //    // 并且显示的是缓冲区的名字，与 BeginSample 的参数无关
            //    CommandBuffer test1 = new CommandBuffer { name = "test1" };
            //    test1.BeginSample("1111");
            //    context.ExecuteCommandBuffer(test1);
            //    test1.Clear();

            //    context.DrawSkybox(camera);

            //    test1.EndSample("1111");
            //    context.ExecuteCommandBuffer(test1);
            //    test1.Clear();

            //    test1.BeginSample("1111");
            //    context.ExecuteCommandBuffer(test1);
            //    test1.Clear();

            //    context.DrawSkybox(camera);

            //    test1.EndSample("1111");
            //    context.ExecuteCommandBuffer(test1);
            //    test1.Clear();
            //}

            //{
            //    // 验证，同一个 Buffer，BeginSample-BeginSample-EndSample-EndSample 在 Frame Debugger 中会嵌套
            //    CommandBuffer test1 = new CommandBuffer { name = "test1" };
            //    test1.BeginSample("1111");
            //    context.ExecuteCommandBuffer(test1);
            //    test1.Clear();

            //    context.DrawSkybox(camera);

            //    test1.BeginSample("1111");
            //    context.ExecuteCommandBuffer(test1);
            //    test1.Clear();

            //    context.DrawSkybox(camera);

            //    test1.EndSample("1111");
            //    context.ExecuteCommandBuffer(test1);
            //    test1.Clear();

            //    test1.EndSample("1111");
            //    context.ExecuteCommandBuffer(test1);
            //    test1.Clear();

            //}
            
            ExecuteBuffer();

        }

        /// <summary>
        /// 我们并不需要渲染场景中的每一个物体，在当前相机视椎体之外的物体提前剔除是一种优化
        /// </summary>
        /// <returns>获取剔除结果的参数是否成功，我们调用相机的 TryGetCullingParameters 来自动获得结果
        /// 但是 degenerate camera settings 有可能导致获取结果失败 </returns>
        bool Cull()
        {
            if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
            {
                // 如果获取剔除结果成功了就给字段赋值，方面后面使用
                cullingResults = context.Cull(ref p);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 渲染可见几何体，顺序是：不透明物体（从前往后），天空盒，透明物体（从后往前）
        /// </summary>
        void DrawVisibleGeometry()
        {
            // ------------------------不透明物体------------------------
            var sortingSettings = new SortingSettings(camera)
            {
                // CommonOpaque 绘制顺序是从前往后渲染，这样后面的物体被遮挡的地方就不必进行渲染，是一种优化
                // CommonOpaque 不仅仅是从前往后，还包含 Render Queue 和材质球等的影响
                criteria = SortingCriteria.CommonOpaque
            };

            // DrawingSettings 控制怎么渲染，也就是 lightmode，sorting，覆盖着色器等
            var drawingSettings = new DrawingSettings(
                unlitShaderTagId, sortingSettings);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

            // DrawRenderers 至少需要三个参数，分别用来控制，渲染什么，如何渲染（LightMode，Sorting，覆盖着色器），进一步筛选（Render Queue）
            context.DrawRenderers(
                cullingResults, ref drawingSettings, ref filteringSettings
            );

            // ----------------------天空盒-----------------------------
            // 如果没有设置好相机，那么相机移动或者旋转不会有任何反应
            context.DrawSkybox(camera);

            // ----------------------不透明物体--------------------------
            // 不透明物体顺序基本是从后往前，因为他们不会写入深度，所有从前往后没有意义
            // 从后往前可以保障正确的混合，但是也会出现问题，比如说物体相交，或者有很大的一块透明物体
            // 一般可以通过将物体切小块来解决
            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;

            context.DrawRenderers(
                cullingResults, ref drawingSettings, ref filteringSettings
            );
        }

        
        /// <summary>
        /// 在没有提交之前，GPU 不会做操作，之前的执行 CommanBuffer，或者 context 的命令都只是“加入队列”
        /// </summary>
        void Submit()
        {
            buffer.EndSample(bufferName);
            ExecuteBuffer();
            context.Submit();
        }

        /// <summary>
        /// 执行 Command Buffer，注意，执行并不是真的执行，只是将命令加入队列
        /// 加入之后就把命令清除，这样 buffer 可以重复使用
        /// </summary>
        void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }
    }
}

