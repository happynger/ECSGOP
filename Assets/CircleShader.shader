// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/CircleShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Thickness("Thickness", Range(0.0, 0.5)) = 0.05
		_Radius("Radius", Range(0.0, 0.5)) = 0.4
		_Dropoff("Dropoff", Range(0.01, 4)) = 0.1
    }
    SubShader
    {
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"

			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
			UNITY_INSTANCING_BUFFER_END(Props)
			float _Thickness;
			float _Radius;
			float _Dropoff;

			struct appdata
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct fragmentInput
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXTCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			fragmentInput vert(appdata v)
			{
				fragmentInput o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy - fixed2(0.5, 0.5);

				return o;
			}

			float antialias(float r, float d, float t, float p)
			{
				if (d < (r - 0.5 * t))
					return -pow(d - r + 0.5 * t, 2) / pow(p * t, 2) + 1.0;
				else if (d > (r + 0.5 * t))
					return -pow(d - r - 0.5 * t, 2) / pow(p * t, 2) + 1.0;
				else
					return 1.0;
			}

			fixed4 frag(fragmentInput i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float distance = sqrt(pow(i.uv.x, 2) + pow(i.uv.y, 2));

				fixed4 c = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

				return fixed4(c.r, c.g, c.b, c.a * antialias(_Radius, distance, _Thickness, _Dropoff));
			}

			ENDCG
		}
    }
    FallBack "Diffuse"
}
