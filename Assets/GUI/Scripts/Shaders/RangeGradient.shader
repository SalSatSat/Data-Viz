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
		[HideInInspector] _Color1 ("Left Color", Color) = (0, 0, 0, 0)
		[HideInInspector] _Color2 ("Right Color", Color) = (1, 1, 1, 1)
		[HideInInspector] _Color3 ("Out of Range Color", Color) = (0.5, 0.5, 0.5, 0.5)
		[HideInInspector] _Min ("Min Value", Float) = 0
		[HideInInspector] _Max ("Max Value", Float) = 1
	}
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

			fixed4 _Color1;
			fixed4 _Color2;
			fixed4 _Color3;
			float _Min;
			float _Max;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = lerp(_Color1, _Color2, v.uv.x);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				if ((_Min > 0 && i.uv.x >= 0 && i.uv.x <= _Min) ||
					(_Max < 1 && i.uv.x >= _Max && i.uv.x <= 1))
					i.color = _Color3;
				return i.color;
			}
			ENDCG
		}
	}
}
