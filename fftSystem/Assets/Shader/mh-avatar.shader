Shader "TI/MH-Avatar"
{
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    _AlphaMapShirt ("Shirt Alpha Map (Greyscale)", 2D) = "black" {}
	_AlphaMapPants ("Pants Alpha Map (Greyscale)", 2D) = "black" {}
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
}

SubShader {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    LOD 200

CGPROGRAM
#pragma surface surf Lambert alphatest:_Cutoff

sampler2D _MainTex;
sampler2D _AlphaMapShirt;
sampler2D _AlphaMapPants;


float4 _Color;

struct Input {
    float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
    half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
    o.Albedo = c.rgb;
	float rPants = 0;
	rPants = tex2D(_AlphaMapPants, IN.uv_MainTex).r;
	float rShirt = 0; 
	rShirt = tex2D(_AlphaMapShirt, IN.uv_MainTex).r;
	
	if(rPants == 1 || rShirt == 1)
	{
		o.Alpha = 0;
	}
	else
	{
		o.Alpha = c.a;
	}
	
	
}
ENDCG
}

Fallback "Transparent/Cutout/Diffuse"
}