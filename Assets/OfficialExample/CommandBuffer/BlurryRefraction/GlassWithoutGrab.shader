// Similar to regular FX/Glass/Stained BumpDistort shader
// from standard Effects package, just without grab pass,
// and samples a texture with a different name.
// 计算假折射 _GrabBlurTexture 是通过后处理模糊后传过来的屏幕图
// https://docs.unity3d.com/Manual/GraphicsCommandBuffers.html

Shader "CommandBuffer/BlurryRefraction/Stained BumpDistort (no grab)" {
Properties {
	_BumpAmt  ("Distortion", range (0,64)) = 10
	_TintAmt ("Tint Amount", Range(0,1)) = 0.1
	_MainTex ("Tint Color (RGB)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
}

Category {

	// We must be transparent, so other objects are drawn before this one.
	Tags { "Queue"="Transparent" "RenderType"="Opaque" }

	SubShader {

		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }
			
			
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord: TEXCOORD0;
				};

				struct v2f {
					float4 vertex : POSITION;
					float4 uvgrab : TEXCOORD0;
					float2 uvbump : TEXCOORD1;
					float2 uvmain : TEXCOORD2;
					UNITY_FOG_COORDS(3)
				};

				float _BumpAmt;
				half _TintAmt;
				float4 _BumpMap_ST;
				float4 _MainTex_ST;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					// 对 OpenGL 的特殊做处理
					#if UNITY_UV_STARTS_AT_TOP
					float scale = -1.0;
					#else
					float scale = 1.0;
					#endif
					// 将 xy 从 [-w,w] 变成 [0,w]
					o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
					o.uvgrab.zw = o.vertex.zw;
					o.uvbump = TRANSFORM_TEX( v.texcoord, _BumpMap );
					o.uvmain = TRANSFORM_TEX( v.texcoord, _MainTex );
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				sampler2D _GrabBlurTexture;
				float4 _GrabBlurTexture_TexelSize;
				sampler2D _BumpMap;
				sampler2D _MainTex;

				half4 frag (v2f i) : SV_Target
				{
					// return fixed4(i.uvgrab.xy,0,1.0);	// 屏幕空间

					// 一种优化，不需要重构 z，只要 xy 就好咯
					half2 bump = UnpackNormal(tex2D( _BumpMap, i.uvbump )).rg;
					// 根据法线的 xy 进行一点偏移，因为是采样屏幕，所以乘上屏幕纹素
					float2 offset = bump * _BumpAmt * _GrabBlurTexture_TexelSize.xy;

					/// **** 验证 i.uvgrab.z 属于 [0,w] ****
					float z01 = i.uvgrab.z / i.uvgrab.w;
					#if UNITY_REVERSED_Z
					// return fixed4(1-z01.xxx,1);
					// return fixed4(1-i.vertex.zzz,1); // 两个是一样的
					#else
					// return fixed4(z01.xxx,1);
					// return fixed4(i.uvgrab.zzz,1);	// 两个是一样的
					#endif

					// return fixed4(i.uvgrab.zzz,1);
					// 因为有反向 Z，所以离相机越近，offset 越大
					i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;

					// 采样 _GrabBlurTexture，模拟折射（假）
					half4 col = tex2Dproj (_GrabBlurTexture, UNITY_PROJ_COORD(i.uvgrab));
					// half4 col = tex2D (_GrabBlurTexture, i.uvgrab.xy/i.uvgrab.w);	// 跟上面一样哒
					half4 tint = tex2D(_MainTex, i.uvmain);
					col = lerp (col, tint, _TintAmt);
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG
				}
			}

		}

}
