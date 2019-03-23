Shader "Custom/Water"
{
    Properties
    {
        _Color ("Water Color", Color) = (1,1,1,1)
        _Bumpmap ("NormalMap", 2D) = "bump" {}
        _Cube ("Cube", Cube) = "" {}
        _SPColor ("Specular Color", color) = (1,1,1,1)
        _SPPower ("Specular Power", Range(50,300)) = 150
        _SPMulti ("Specular Multiply", Range(1,10)) = 3
        _Refract("Refract Strength", Range(0,0.2)) = 0.1
    }
    SubShader
    {
        // Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
        Tags {"RenderType"="Opaque"}
        GrabPass{}

        CGPROGRAM
        #pragma surface surf WaterSpecular alpha:fade

        fixed4 _Color;
        sampler2D _Bumpmap;
        sampler2D _GrabTexture;
        samplerCUBE _Cube;
        float4 _SPColor;
        float _SPPower;
        float _SPMulti;
        float _Refract;

        struct Input
        {
            float2 uv_Bumpmap;
            float3 worldRefl;
            float3 viewDir;
            float4 screenPos;
            INTERNAL_DATA
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color;
            float3 normal1 = UnpackNormal(tex2D(_Bumpmap, IN.uv_Bumpmap + _Time.x * 0.1));
            float3 normal2 = UnpackNormal(tex2D(_Bumpmap, IN.uv_Bumpmap - _Time.x * 0.1));
            o.Normal = (normal1 + normal2) / 2;
            
            float3 refcolor = texCUBE(_Cube, WorldReflectionVector(IN, o.Normal));

            //refraction term
            float3 screenUV = IN.screenPos.rgb / IN.screenPos.a;
            float3 refraction = tex2D(_GrabTexture, (screenUV.xy + o.Normal.xy * _Refract));

            //rim term
            float rim = saturate(dot(o.Normal, IN.viewDir));
            // rim = pow(1-rim, 2);
            // o.Emission = refcolor * rim * 2;
            // o.Alpha = saturate(rim+0.5);
            rim = pow(1-rim,2);
            o.Emission = (refcolor * rim + refraction) * 0.5;
            o.Alpha = 1;
        }
        float4 LightingWaterSpecular(SurfaceOutput s, float3 lightDir, float3 viewDir, float atten){
            //specular term
            float3 H = normalize(lightDir + viewDir);
            float spec = saturate(dot(H,s.Normal));
            spec = pow(spec, _SPPower);

            //final term
            float4 finalColor;
            finalColor.rgb = spec * _SPColor.rgb * + _SPMulti;
            finalColor.a = s.Alpha + spec;
            return finalColor;
        }
        ENDCG
    }
    FallBack "Legacy Shaders/Transparent/Vertexlit"
}
