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
            Cull Off

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

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                float noise = GradientNoise(v.vertex.xz + _Time.x * 4) * 2 * 3.14159265358;
                o.vertex = UnityObjectToClipPos(v.vertex + v.normal * Remap2(sin(noise), -1, 1, -0.05, 0.05));
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
}
