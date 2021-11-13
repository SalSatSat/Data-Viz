// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)

Shader "Custom/RangeGradient"
{
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Opaque"
			"IgnoreProjector" = "True"
			"PreviewType" = "Plane"
		}

		Fog{ Mode Off }
		Lighting Off
		ZWrite Off
		Cull Off

		Blend SrcAlpha OneMinusSrcAlpha	// Traditional transparency
		//Blend One OneMinusSrcAlpha		// Premultiplied transparency
		//Blend One One					// Additive

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			fixed4 MinColor;
			fixed4 MaxColor;
			fixed4 OutOfRangeColor;
			float MinFilter;
			float MaxFilter;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = lerp(MinColor, MaxColor, v.uv.x);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				if ((MinFilter > 0 && i.uv.x >= 0 && i.uv.x <= MinFilter) ||
					(MaxFilter < 1 && i.uv.x >= MaxFilter && i.uv.x <= 1))
					i.color = OutOfRangeColor;
				return i.color;
			}
			ENDCG
		}
	}
}
