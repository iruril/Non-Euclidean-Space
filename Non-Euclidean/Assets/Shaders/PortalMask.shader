Shader "Portals/PortalMask"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
		Tags 
		{ 
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
			"RenderPipeline" = "UniversalPipeline"
		}
		LOD 100
		Cull Off

		HLSLINCLUDE
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		ENDHLSL

        Pass
        {
			HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				struct Attributes
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct Varyings
				{
					float4 vertex : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
				CBUFFER_END

				TEXTURE2D(_MainTex);
				SAMPLER(sampler_MainTex);

				Varyings vert(Attributes v)
				{
					Varyings o;
					o.vertex = TransformObjectToHClip(v.vertex.xyz);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					float4 clipVertex = o.vertex / o.vertex.w;
					o.uv = ComputeScreenPos(clipVertex).xy;
					return o;
				}

				half4 frag(Varyings i) : SV_Target
				{
					half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
					return color;
				}
			ENDHLSL
        }
    }
}
