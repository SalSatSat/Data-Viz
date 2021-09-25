Shader "Custom/BorderPost"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always

		CGINCLUDE
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			#pragma multi_compile BLUR_3_STEPS BLUR_5_STEPS BLUR_7_STEPS BLUR_9_STEPS

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
		ENDCG

		// #0: Compare object ids to draw edges
		Pass
		{
			CGPROGRAM
				sampler2D _IdsDepth;
				sampler2D _CameraDepthTexture;
				float _DepthOffset;
				float4 _BorderColor;

				// Offset for the 8 neighbouring texels
				static const half2 kOffsets[8] = {
					half2(-1,-1),
					half2(0,-1),
					half2(1,-1),
					half2(-1,0),
					half2(1,0),
					half2(-1,1),
					half2(0,1),
					half2(1,1)
				};

				float4 frag(v2f i) : SV_Target
				{
					int currentId = tex2D(_MainTex, i.uv);
				
					// Get the current depth (from the second render)
					// Depth: near=1, far=0
					float currentDepth = SAMPLE_DEPTH_TEXTURE(_IdsDepth, i.uv) + _DepthOffset;
					float sceneDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
					float maxDepth = currentDepth;

					float4 color = _BorderColor;

					// Check if the current texel is next to a texel with different object id
					int isEdge = 0;
					[unroll]
					for (int tap = 0; tap < 8; ++tap)
					{
						float2 uv = i.uv + (kOffsets[tap] * _MainTex_TexelSize.xy);
						int otherId = tex2D(_MainTex, uv);
						int value = (currentId != otherId);
						float neighbourDepth = SAMPLE_DEPTH_TEXTURE(_IdsDepth, uv) + _DepthOffset;
						
						maxDepth = max(maxDepth, neighbourDepth);
						isEdge |= value;
					}

					//isEdge *= sceneDepth <= maxDepth; // Erase edge where scene is closer to camera than closest neighbour
					
					return color * isEdge;
				}
			ENDCG
		}

		// #1: Blur pass, either horizontal or vertical
		Pass
		{
			CGPROGRAM
				float2 _BlurDirection;
				sampler2D _IdsTex;

				// Gaussian kernel, that blurs green channel
				#if BLUR_9_STEPS
				static const float kSteps = 9;
				static const float kOffset = 4;
				static const float kCurveWeights[9] = {
					0.0204001988,
					0.0577929595,
					0.1215916882,
					0.1899858519,
					0.2204586031,
					0.1899858519,
					0.1215916882,
					0.0577929595,
					0.0204001988,
				};
				#elif BLUR_7_STEPS
				static const float kSteps = 7;
				static const float kOffset = 3;
				static const float kCurveWeights[7] = {
					0.0238563927,
					0.0977401219,
					0.2275945913,
					0.3016177876,
					0.2275945913,
					0.0977401219,
					0.0238563927,
				};
				#elif BLUR_5_STEPS
				static const float kSteps = 5;
				static const float kOffset = 2;
				static const float kCurveWeights[5] = {
					0.0613595978,
					0.2447701955,
					0.3877404133,
					0.2447701955,
					0.0613595978,
				};
				#else
				static const float kSteps = 3;
				static const float kOffset = 1;
				static const float kCurveWeights[3] = {
					0.2,
					0.6,
					0.2,
				};
				#endif

				float4 frag(v2f i) : SV_Target
				{
					int currentId = tex2D(_IdsTex, i.uv);
					float2 offset = _MainTex_TexelSize.xy * _BlurDirection;
					float2 uv = i.uv - offset * kOffset;
					float3 col = 0;
					float alpha = 0;
					int count = 0;
					
					[unroll]
					for (int tap = 0; tap < kSteps; ++tap)
					{
						int otherId = tex2D(_IdsTex, uv);
						float4 value = tex2D(_MainTex, uv);
						value *= (otherId == currentId);
						col += value;
						count += (value.a > 0);
						alpha += value.a * kCurveWeights[tap];
						uv += offset;
					}
					if (count > 0)
						col /= count;
					return float4(col, alpha);
				}
			ENDCG
		}

		// #2: Final post-processing pass
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
 
			CGPROGRAM
				sampler2D _IdsTex;
				float _Intensity;

#if BLUR_9_STEPS
				static const float multiplier = 5;
#elif BLUR_7_STEPS
				static const float multiplier = 4;
#elif BLUR_5_STEPS
				static const float multiplier = 3;
#else
				static const float multiplier = 2;
#endif

				half4 frag(v2f i) : SV_Target
				{
					int id = tex2D(_IdsTex, i.uv);
					float4 border = tex2D(_MainTex, i.uv);
					border.a = saturate(border.a * multiplier * _Intensity);
					border.a *= (id == 0);
					
					return border;
				}
			ENDCG
		}

		// #3: Debug pass
        Pass
        {
            CGPROGRAM
                float4 _ColorMask;
                float _Multiplier;
                float _Divider;

                half4 frag(v2f i) : SV_Target
                {
                    float4 col = tex2D(_MainTex, i.uv);
                    if (all(_ColorMask))
                        return _Multiplier * col * col.a / _Divider;

                    col *= _ColorMask;
                    float x = (col.r + col.g + col.b + col.a) / (_ColorMask.r + _ColorMask.g + _ColorMask.b + _ColorMask.a);
                    x = _Multiplier * x / _Divider;
                    return half4(x, x, x, 1);
                }
            ENDCG
        }
	}
}
