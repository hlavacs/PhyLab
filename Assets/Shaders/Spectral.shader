Shader "Custom/Spectral"
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
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
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
			o.Occlusion = 0.0;
            
			switch (_GlobalSpectralValue) {
				case 1: {
					o.Albedo = _Color1.rrr;
					o.Emission = o.Albedo;
				} break;
				case 2: {
					o.Albedo = _Color2.rrr;
					o.Emission = o.Albedo;
				} break;
				case 3: {
					o.Albedo = _Color3.rrr;
					o.Emission = o.Albedo;
				} break;
				case 4: {
					o.Albedo = c.bbb;
					o.Occlusion = 1.0;
				} break;
				case 5: {
					o.Albedo = c.ggg;
					o.Occlusion = 1.0;
				} break;
				case 6: {
					o.Albedo = c.rrr;
					o.Occlusion = 1.0;
				} break;
				case 7: {
					o.Albedo = c.rgb;
					o.Metallic = _Metallic;
					o.Smoothness = _Glossiness;
					o.Occlusion = 1.0;
				} break;
				case 8: {
					o.Albedo = _Color8.rrr;
					o.Emission = o.Albedo;
				} break;
				case 9: {
					o.Albedo = _Color9.rrr;
					o.Emission = o.Albedo;
				} break;
				case 10: {
					o.Albedo = _Color10.rrr;
					o.Emission = o.Albedo;
				} break;
			}


            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
