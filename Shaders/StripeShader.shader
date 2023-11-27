Shader "Unlit/StripeShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

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
			float4 _MainTex_ST;

			float _RelativeDels[100];
			int _RelDelsLength;
			fixed4 _StripColors[50];

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//For debugging
				fixed4 col = fixed4(1,0,0,1);

				for (uint j = 0; j < _RelDelsLength; j += 2) 
				{
					if (_RelativeDels[j] <= i.uv.x && _RelativeDels[j + 1] > i.uv.x) 
					{
						col = _StripColors[j/2];
						break;
					}
				}

				return col;
			}
			ENDCG
		}
	}
}
