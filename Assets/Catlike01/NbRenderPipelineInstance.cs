using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catlike01
{
    /// <summary>
    /// 中文名：牛逼渲染管线
    /// 学习：https://catlikecoding.com/unity/tutorials/custom-srp/custom-render-pipeline/
    /// </summary>
    public class NbRenderPipelineInstance : RenderPipeline
    {
        NbRenderPipelineAsset renderPipelineAsset;

        // 把 Unity 传过来的上下文保存起来
        ScriptableRenderContext context;

        // Render 中要逐相机计算，所以这里保存一个
        Camera camera;

        const string bufferName = "Render Camera";
        CommandBuffer buffer = new CommandBuffer { name = bufferName };

        public NbRenderPipelineInstance(NbRenderPipelineAsset asset)
        {
            renderPipelineAsset = asset;
        }

        /// <summary>
        /// 因为逐相机操作很多，所以直接定义只有一个相机的函数
        /// </summary>
        /// <param name="context"></param>
        /// <param name="camera"></param>
        public void Render(ScriptableRenderContext context, Camera camera)
        {
            this.context = context;
            this.camera = camera;
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
            // Frame debugger 显示的是缓冲名字
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
        /// 渲染可见几何体
        /// </summary>
        void DrawVisibleGeometry()
        {
            // 如果没有设置好相机，那么相机移动或者旋转不会有任何反应
            context.DrawSkybox(camera);
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

        // 渲染主循环/入口
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach(var c in cameras)
            {
                Render(context, c);
            }

            // 依然保留一个正常的逻辑，上面一堆函数只是把它打散了
            //var cmd = new CommandBuffer();
            //cmd.name = "ExampleCommandBuffer";
            //cmd.ClearRenderTarget(true, true, Color.white);
            //context.ExecuteCommandBuffer(cmd);
            //cmd.Release();
            //context.Submit();
        }
    }
}
