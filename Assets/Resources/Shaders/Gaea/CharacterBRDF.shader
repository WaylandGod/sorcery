///////////////////////////////////////////
// author     : chen yong
// create time: 2014/01/01
// modify time: 
// description: 
///////////////////////////////////////////

Shader "Gaea/CharacterBRDF" {
	Properties {
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "grey" {}
	//	_BumpMap ("Normalmap", 2D) = "bump" {}
		_BRDFTex ("NdotL NdotH (RGBA)", 2D) = "white" {}
		_EmisColor ("Emissive Color", Color) = (1, 1, 1, 1)
		_EmisRange ("Emissive Range", Range (0, 2)) = 0
		_GlobalScale ("Global Scale", Float) = 1
	}	
	SubShader { 
		Tags { "RenderType"="Opaque"}
		Fog { Mode Off }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf PseudoBRDF novertexlights approxview noforwardadd         
		#include "UnityCG.cginc"
				
		struct MySurfaceOutput {
			fixed3 Albedo;
			fixed3 Normal;
			fixed3 Emission;
			half Specular;
			fixed Gloss;
			fixed Alpha;
		};
	
		sampler2D _BRDFTex;
		float _GlobalScale;
	
		inline fixed4 LightingPseudoBRDF (MySurfaceOutput s, fixed3 lightDir, fixed3 viewDir, fixed atten)
		{			
			// Half vector
			fixed3 halfDir = normalize (lightDir + viewDir);
			
			// N.L
			fixed NdotL = dot (s.Normal, lightDir);
			// N.H
			fixed NdotH = dot (s.Normal, halfDir);
			
			// remap N.L from [-1..1] to [0..1]
			// this way we can shade pixels facing away from the light - helps to simulate bounce lights
			fixed biasNdotL = NdotL * 0.5 + 0.5;
			
			// lookup light texture
			//  rgb = diffuse term
			//    a = specular term
			fixed4 l = tex2D (_BRDFTex, fixed2(biasNdotL, NdotH));
		
			fixed4 c;
			// mask specular term by Gloss factor
			// modulate specular with Albedo to allow metalic-ish look
			c.rgb = s.Albedo * (l.rgb + s.Gloss * l.a) * _GlobalScale;
			c.a = s.Alpha;			
						
			return c;
		}
	
		sampler2D _MainTex;
		//sampler2D _BumpMap;
		float3 _EmisColor;
		float _EmisRange;
		
		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
		};
		
		void surf (Input IN, inout MySurfaceOutput o) {
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = tex.rgb;
			o.Gloss = tex.a;
			o.Alpha = tex.a;
			o.Emission = _EmisColor*_EmisRange*tex.a;
		//	o.Normal = tex2D(_BumpMap, IN.uv_BumpMap).rgb * 2.0 - 1.0;
		}
		ENDCG
	}

	FallBack "Mobile/Diffuse"
}