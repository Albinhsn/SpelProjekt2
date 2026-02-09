Shader "Shader/GlitchVolume"
{
    Properties
    {
        // This property is necessary to make the CommandBuffer.Blit bind the source texture to _MainTex
        _MainTex("Main Texture", 2DArray) = "grey" {}
    }

    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

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

    // List of properties to control your post process effect
    TEXTURE2D_X(_MainTex);

    float _GlitchSpeed;
    float _GlitchStrength;
    float _ScanlineSpeed;
    float _ScanlineStrength;

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 uvy = float2(input.texcoord.y, input.texcoord.y);

        // Noise
        float2 t = float2(_Time.y, _Time.y);
        float noise = GradientNoiseFloat(uvy * _GlitchStrength + t * _GlitchSpeed, 1);
        noise       = Remap2(noise, 0, 1, -1, 1);

        // Flickering
        float flick = GradientNoiseFloat(t * _GlitchSpeed, 1);
        flick = flick * flick * flick * 0.1f;

        // He then multiplies Noise * Flickering 
        // This is used as an offset in the x,y direction for the sampling the result

        // Scanline
        float s = clamp(sin(60000 * uvy.y + _Time.y * _ScanlineSpeed), _ScanlineStrength, 1);

        // NOTE(ah): This is extremely dumb but he does it in the tutorial
        // Why do you remap a range that you clamped in [ScanlineStrength, 1] and assume
        // that it's [-1,1] only to map it to something else
        // Just shift the range?
        // It's even worse because you already want the range to be in [0,1] in the end?
        // So just do the remap properly the first time instead of the clamp?
        s = Remap2(s, -1, 1, 0.4, 1);

        s = _ScanlineStrength == 0 ? 1 : s;

        float offset = float2(flick * noise, 0);
        float3 sourceColor = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, ClampAndScaleUVForBilinearPostProcessTexture(input.texcoord.xy + offset)).xyz;

        float3 color = s * sourceColor;
        return float4(color, 1);
    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "Glitch"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}
