Shader "CustomFX/HDRAdditiveParticle" {
Properties {
	_TintColor ("Smoke Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_HDRAmount ("HDR Mult", Range(1,20)) = 1
}
 
Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend One One
	Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
 
	SubShader {
		Pass {
 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_particles
 
			#include "UnityCG.cginc"

			float4 _TintColor;
			sampler2D _MainTex;

			float _HDRAmount;

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
 
			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				#ifdef SOFTPARTICLES_ON
				float4 projPos : TEXCOORD1;
				#endif
			};
 
			float4 _MainTex_ST;
 
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				#ifdef SOFTPARTICLES_ON
				o.projPos = ComputeScreenPos (o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
				#endif
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}
 
			sampler2D _CameraDepthTexture;
			float _InvFade;
 
			fixed4 frag (v2f i) : COLOR
			{
				#ifdef SOFTPARTICLES_ON
				float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
				float partZ = i.projPos.z;
				float fade = saturate ((sceneZ-partZ));
				i.color.a *= fade;
				#endif

				float4 tex = tex2D(_MainTex, i.texcoord);
				
				float3 color = tex * i.color * _TintColor * ( _HDRAmount * i.color.a ) * i.color.a;
				return float4( color, tex.a * i.color.a * _TintColor.a );
			}
			ENDCG 
		}
	}
}
}