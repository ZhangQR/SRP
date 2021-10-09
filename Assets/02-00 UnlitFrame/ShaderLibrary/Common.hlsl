#ifndef CUSTOM_COMMON_INCLUDED
#define CUSTOM_COMMON_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "UnityInput.hlsl"

// float3 TransformObjectToWorld (float3 positionOS) {
// 	return mul(unity_ObjectToWorld,float4(positionOS,1.0)).xyz;
// }
//
// Hclip 是还没有进行透视除法的，除了才是 NDC
// float4 TransformWorldToHClip(float3 positionWS)
// {
// 	return mul(unity_MatrixVP,float4(positionWS,1.0));
// }

// 因为在 SpaceTransforms 中用的都是大写的矩阵
#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_MATRIX_P glstate_matrix_projection

// 上面那些就做个示范，这些很基础的变化在 core 中都有
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
	
#endif