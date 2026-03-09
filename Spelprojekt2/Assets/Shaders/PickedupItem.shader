Shader "Custom/PickedupItem"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="HDRenderPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {

            Name "Forward"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            float2 GradientNoiseDir(float2 p)
            {
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 280 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            float GradientNoise(float2 p)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(GradientNoiseDir(ip), fp);
                float d01 = dot(GradientNoiseDir(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(GradientNoiseDir(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(GradientNoiseDir(ip + float2(1, 1)), fp - float2(1, 1));

                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
            }

            float GradientNoiseFloat(float2 uv, float scale)
            {
                return GradientNoise(uv * scale) + 0.5;
            }

            float Remap2(float value, float in_min, float in_max, float out_min, float out_max)
            {
                return out_min + (value - in_min) * (out_max - out_min) / (in_max - in_min);
            }

            float Fresnel(float3 normal, float3 view_dir, float power)
            {
                return pow((1.0 - saturate(dot(normalize(normal), normalize(view_dir)))), power);
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 view_dir : TEXCOORD1;
                float3 normal : NORMAL;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                float _MaxVertexDisplacement = 0.1f;
                v2f o;
                float noise = GradientNoise(v.vertex.xz + _Time.x * 4) * 2 * 3.14159265358;

                float3 p    = v.vertex.xyz + v.normal * Remap2(sin(noise), -1, 1, 0, _MaxVertexDisplacement);
                p = v.vertex.xyz;
                o.vertex    = UnityObjectToClipPos(float4(p, v.vertex.w));
                o.normal    = normalize(v.normal);
                o.uv        = v.uv;

                float3 world_p = mul(UNITY_MATRIX_M, float4(p, 1.0)).xyz;
                o.view_dir     = normalize(_WorldSpaceCameraPos - world_p);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float _FresnelPower = 5.37;
                float _NoiseTimeScale = 0.3;

                float _Noise1UVScale = 14.0;
                float _Noise1TimeScale = _NoiseTimeScale;

                float _Noise2UVScale = 13.4;
                float _Noise2TimeScale = -_NoiseTimeScale;

                // Fresnel 
                float fresnel_sample = Fresnel(i.normal, i.view_dir, _FresnelPower);
                
                // Double perlin samples
                float noise1 = GradientNoise(i.uv * _Noise1UVScale + _Time.y * _Noise1TimeScale) + 0.5;
                float noise2 = GradientNoise(i.uv * _Noise2UVScale + _Time.y * _Noise2TimeScale) + 0.5;

                float sample = fresnel_sample * noise1 * noise2;
                // return float4(noise1, noise1, noise1, 1.0);
                return float4(_Color.rgb, _Color.a * sample);
            }
            ENDHLSL
        }
    }
}
