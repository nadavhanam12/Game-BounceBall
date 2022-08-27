Shader "Custom/Shader"
{
	Properties
    {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader
    {
        Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard keepalpha nolightmap vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

        struct appdata_particles
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            fixed4 color : COLOR;
            float4 targetPositionAndRandom : TEXCOORD0;
            float4 texcoords : TEXCOORD1;
            float4 centerAndAgePercent : TEXCOORD2;
        };

		struct Input
        {
            float2 texcoord;
            fixed4 color;
		};

		half _Glossiness;
		half _Metallic;

        float remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        void vert(inout appdata_particles v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.color = v.color;              // copy vertex color to pixel shader
            o.texcoord = v.texcoords.xy;    // copy uv to pixel shader

            // we want to animate the center of the particle towards its target position, at a random speed
            float blendPercent = saturate(remap(v.centerAndAgePercent.w, 0.0f, v.targetPositionAndRandom.w, 0.0f, 1.0f));
            //float blendPercent=1;
            blendPercent = smoothstep(0.0f, 1.0f, blendPercent); // ease in/out so it looks nicer compared to linear

            // remember the local offset from the center of the particle to the corner (we need to ad this on to the new center, at the end)
            float3 localOffset = v.centerAndAgePercent.xyz - v.vertex.xyz;

            blendPercent=blendPercent*4;
            if (blendPercent >1.0f) {
                blendPercent=1.0f;
            }
            // move the center towards its target
            float3 newCenter = lerp(v.centerAndAgePercent.xyz, v.targetPositionAndRandom.xyz, blendPercent);

            // store the final position of the vertex billboard (each corner of the quad)
            v.vertex.xyz = newCenter + localOffset;
        }

		void surf(Input IN, inout SurfaceOutputStandard o)
        {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.texcoord) * IN.color;
            o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
