// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx
// Modified by Peisen Xu and Nanyang Yang
// Further modified by Shivi Vats

Shader "Point Cloud/Square_BothEyes_heatmap"
{
	Properties
	{
		_Tint("Tint", Color) = (0.5, 0.5, 0.5, 1)
		_PointSize("PointSize", Float) = 0.05
		_ModelScalingFactor("ModelScalingFactor", Float) = 0.05
		_PointScalingFactor("PointScalingFactor", Float) = 1
		[Toggle] _ShaderInterpolation("ShaderInterpolation", Int) = 1
		[Toggle] _AdaptivePoint("AdaptivePoint", Int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex Vertex
            #pragma geometry Geometry
            #pragma fragment Fragment
            #pragma multi_compile_fog
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #pragma multi_compile _ _COMPUTE_BUFFER
            #include "Square_BothEyes_heatmap.cginc"
            ENDCG
        }
        Pass
        {
            Tags { "LightMode"="ShadowCaster" }
            CGPROGRAM
            #pragma vertex Vertex
            #pragma geometry Geometry
            #pragma fragment Fragment
            #pragma multi_compile _ _COMPUTE_BUFFER
            #define PCX_SHADOW_CASTER 1
            #include "Square_BothEyes_heatmap.cginc"
            ENDCG
        }
    }
    CustomEditor "Pcx.SquareMaterialInspector"
}
