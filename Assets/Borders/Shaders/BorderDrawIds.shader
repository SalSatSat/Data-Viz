Shader "Custom/BorderDrawIds"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			// Push towards camera a bit, so that coord mismatch due to dynamic batching is not affecting us
			// Offset -0.02, 0
			
			CGPROGRAM
				 #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                float _ObjectId;

                struct appdata
                {
                    float4 vertex   : POSITION;
                };

                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    return o;
                }

                float frag(v2f v) : SV_Target
                {
                    return _ObjectId;
                }
			ENDCG
		}
	}
}
