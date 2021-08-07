// Copyright (C) Muhammad Salihin Bin Zaol-kefli
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Muhammad Salihin Bin Zaol-kefli  (salsatsat@gmail.com)

Shader "Custom/RangeGradient"
{
	Properties
	{
		[PerRenderedData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color1 ("Left Color", Color) = (0, 0, 0, 0)
		_Color2 ("Right Color", Color) = (0, 0, 0, 0)

	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
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
				float4 color : COLOR;
			};

			fixed4 _Color1;
			fixed4 _Color2;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = lerp(_Color1, _Color2, v.uv.x);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				return i.color;
			}
			ENDCG
		}
	}
}
