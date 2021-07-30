Shader "Custom/SpectralTerrain"
{
	Properties{
		// used in fallback on old cards & base map
		[HideInInspector] _MainTex("BaseMap (RGB)", 2D) = "white" {}
		[HideInInspector] _Color("Main Color", Color) = (1,1,1,1)
		_Color1("Gamma", Range(0, 1)) = 0.0
		_Color2("Röntgen", Range(0, 1)) = 0.0
		_Color3("UV", Range(0, 1)) = 0.0
		_Color8("Infrared", Range(0, 1)) = 0.0
		_Color9("Microwave", Range(0, 1)) = 0.0
		_Color10("Radio", Range(0, 1)) = 0.0

	}
		SubShader{
			Tags {
				"Queue" = "Geometry-100"
				"RenderType" = "Opaque"
			}

			CGPROGRAM
			#pragma surface surf Standard vertex:SplatmapVert finalcolor:SplatmapFinalColor finalgbuffer:SplatmapFinalGBuffer addshadow fullforwardshadows
			#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
			#pragma multi_compile_fog // needed because finalcolor oppresses fog code generation.
			#pragma target 3.0
			// needs more than 8 texcoords
			#pragma exclude_renderers gles
			#include "UnityPBSLighting.cginc"

			#pragma multi_compile __ _NORMALMAP

			#define TERRAIN_STANDARD_SHADER
			#define TERRAIN_INSTANCED_PERPIXEL_NORMAL
			#define TERRAIN_SURFACE_OUTPUT SurfaceOutputStandard
			#include "TerrainSplatmapCommon.cginc"

			half _Metallic0;
			half _Metallic1;
			half _Metallic2;
			half _Metallic3;

			half _Smoothness0;
			half _Smoothness1;
			half _Smoothness2;
			half _Smoothness3;

			half _Color1;
			half _Color2;
			half _Color3;
			half _Color8;
			half _Color9;
			half _Color10;

			int _GlobalSpectralValue;


			void surf(Input IN, inout SurfaceOutputStandard o) {
				half4 splat_control;
				half weight;
				fixed4 mixedDiffuse;
				half4 defaultSmoothness = half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);
				SplatmapMix(IN, defaultSmoothness, splat_control, weight, mixedDiffuse, o.Normal);
				o.Albedo = half3(1, 1, 1);
				o.Alpha = weight;
				o.Smoothness = 0;
				o.Metallic = 0;
				o.Occlusion = 1;

				switch (_GlobalSpectralValue) {
				case 1: {
					o.Occlusion = _Color1;
				} break;
				case 2: {
					o.Occlusion = _Color2.r;
				} break;
				case 3: {
					o.Occlusion = _Color3.r;
				} break;
				case 4: {
					o.Albedo = mixedDiffuse.bbb;
					o.Metallic = dot(splat_control, half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));
					o.Smoothness = mixedDiffuse.a;
				} break;
				case 5: {
					o.Albedo = mixedDiffuse.ggg;
					o.Metallic = dot(splat_control, half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));
					o.Smoothness = mixedDiffuse.a;
				} break;
				case 6: {
					o.Albedo = mixedDiffuse.rrr;
					o.Metallic = dot(splat_control, half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));
					o.Smoothness = mixedDiffuse.a;
				} break;
				case 7: {
					o.Albedo = mixedDiffuse.rgb;
					o.Metallic = dot(splat_control, half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));
					o.Smoothness = mixedDiffuse.a;
				} break;
				case 8: {
					o.Occlusion = _Color8.r;
				} break;
				case 9: {
					o.Occlusion = _Color9.r;
				} break;
				case 10: {
					o.Occlusion = _Color10.r;
				} break;
				}


			}
			ENDCG

			UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
			UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
		}

			Dependency "AddPassShader" = "Hidden/TerrainEngine/Splatmap/Standard-AddPass"
			Dependency "BaseMapShader" = "Hidden/TerrainEngine/Splatmap/Standard-Base"
			Dependency "BaseMapGenShader" = "Hidden/TerrainEngine/Splatmap/Standard-BaseGen"

			Fallback "Nature/Terrain/Diffuse"
}
