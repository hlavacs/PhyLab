Shader "Custom/SpectralBark"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

		_Color1("Gamma", Range(0,1)) = 0.0
		_Color2("Röntgen", Range(0,1)) = 0.0
		_Color3("UV", Range(0,1)) = 0.0
		_Color8("Infrared", Range(0,1)) = 0.0
		_Color9("Microwave", Range(0,1)) = 0.0
		_Color10("Radio", Range(0,1)) = 0.0
		[HideInInspector] _TreeInstanceColor("TreeInstanceColor", Vector) = (1,1,1,1)
		[HideInInspector] _TreeInstanceScale("TreeInstanceScale", Vector) = (1,1,1,1)
		[HideInInspector] _SquashAmount("Squash", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:TreeVertBark 

		#include "UnityBuiltin3xTreeLibrary.cginc"

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        //fixed4 _Color;
		half _Color1;
		half _Color2;
		half _Color3;
		half _Color8;
		half _Color9;
		half _Color10;

		int _GlobalSpectralValue;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Metallic = 0;
			o.Smoothness = 0;
			o.Occlusion = 0;
            
			switch (_GlobalSpectralValue) {
				case 1: {
					o.Albedo = _Color1.rrr;
					o.Emission = _Color1.rrr;
					o.Normal = 0;
				} break;
				case 2: {
					o.Albedo = _Color2.rrr;
					o.Emission = _Color2.rrr;
					o.Normal = 0;
				} break;
				case 3: {
					o.Albedo = _Color3.rrr;
					o.Emission = _Color3.rrr;
					o.Normal = 0;
				} break;
				case 4: {
					o.Albedo = c.bbb;
					o.Emission = c.bbb;
					o.Normal = 0;
				} break;
				case 5: {
					o.Albedo = c.ggg;
					o.Emission = c.ggg;
					o.Normal = 0;
				} break;
				case 6: {
					o.Albedo = c.rrr;
					o.Emission = c.rrr;
					o.Normal = 0;
				} break;
				case 7: {
					o.Albedo = c.rgb;
					o.Metallic = _Metallic;
					o.Smoothness = _Glossiness;
					o.Occlusion = 1.0;
				} break;
				case 8: {
					o.Albedo = _Color8.rrr;
					o.Emission = _Color8.rrr;
					o.Normal = 0;
				} break;
				case 9: {
					o.Albedo = _Color9.rrr;
					o.Emission = _Color9.rrr;
					o.Normal = 0;
				} break;
				case 10: {
					o.Albedo = _Color10.rrr;
					o.Emission = _Color10.rrr;
					o.Normal = 0;
				} break;
			}


            o.Alpha = c.a;
        }
        ENDCG


			// Pass to render object as a shadow caster
			Pass{
				Name "ShadowCaster"
				Tags { "LightMode" = "ShadowCaster" }

				CGPROGRAM
				#pragma vertex vert_surf
				#pragma fragment frag_surf
				#pragma multi_compile_shadowcaster
				#include "HLSLSupport.cginc"
				#include "UnityCG.cginc"
				#include "Lighting.cginc"

				#define INTERNAL_DATA
				#define WorldReflectionVector(data,normal) data.worldRefl

				#include "UnityBuiltin3xTreeLibrary.cginc"

				sampler2D _MainTex;

				struct Input {
					float2 uv_MainTex;
				};

				struct v2f_surf {
					V2F_SHADOW_CASTER;
					float2 hip_pack0 : TEXCOORD1;
					UNITY_VERTEX_OUTPUT_STEREO
				};
				float4 _MainTex_ST;
				v2f_surf vert_surf(appdata_full v) {
					v2f_surf o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					TreeVertLeaf(v);
					o.hip_pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					return o;
				}
				fixed _Cutoff;
				float4 frag_surf(v2f_surf IN) : SV_Target {
					half alpha = tex2D(_MainTex, IN.hip_pack0.xy).a;
					clip(alpha - _Cutoff);
					SHADOW_CASTER_FRAGMENT(IN)
				}
				ENDCG
		}

    }
    FallBack "Diffuse"
}
