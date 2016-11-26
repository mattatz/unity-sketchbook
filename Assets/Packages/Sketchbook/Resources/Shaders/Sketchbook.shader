Shader "mattatz/Sketchbook" {

	Properties {
		_SrcBlend ("SrcBlend", Int) = 5 // SrcAlpha
		_DstBlend ("DstBlend", Int) = 10 // OneMinusSrcAlpha
		_ZWrite ("ZWrite", Int) = 1 // On
		_ZTest ("ZTest", Int) = 4 // LEqual
		_Cull ("Cull", Int) = 0 // Off
		_ZBias ("ZBias", Float) = 0.0
	}

	SubShader {
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		Blend [_SrcBlend] [_DstBlend]
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		// Cull [_Cull]
		Cull Back
		Offset [_ZBias], [_ZBias]

		CGINCLUDE

		#include "UnityCG.cginc"

		struct appdata {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			float4 color : COLOR;
		};

		ENDCG

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct v2f {
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			v2f vert (appdata IN) {
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.color = IN.color;
				return OUT;
			}

			fixed4 frag (v2f IN) : SV_Target {
				return IN.color;
			}

			ENDCG
		}

		Pass {

			Tags { "LightMode"="ForwardBase" }

			CGPROGRAM

			#include "UnityLightingCommon.cginc"

			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			struct v2g {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct g2f {
				float4 pos : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			v2g vert (appdata IN) {
				v2g OUT;
				OUT.vertex = IN.vertex;
				OUT.uv = IN.uv;
				OUT.color = IN.color;
				return OUT;
			}

			[maxvertexcount(3)]
			void geom(triangle v2g IN[3], inout TriangleStream<g2f> OUT) {
				float3 a = IN[0].vertex.xyz, b = IN[1].vertex.xyz, c = IN[2].vertex.xyz;
				float3 normal = cross(normalize(b - a), normalize(c - a));

				for (int i = 0; i < 3; i++) {
					g2f pIn;
					pIn.pos = mul(UNITY_MATRIX_MVP, IN[i].vertex);
					pIn.uv = IN[i].uv;
					pIn.color = IN[i].color;
					pIn.normal = normal;
					OUT.Append(pIn);
				}
			}

			fixed4 frag (g2f IN) : SV_Target {
				float3 normal = normalize(IN.normal);
				normal = UnityObjectToWorldNormal(normal);

				// standard diffuse
				half nl = max(0, dot(normal, _WorldSpaceLightPos0.xyz));
				half3 diff = nl * _LightColor0;
				// return fixed4(diff * (normal + 1.0) * 0.5, 1.0);

				IN.color.rgb *= diff;
				return IN.color;
			}

			ENDCG
		}



	}
}
