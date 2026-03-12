Shader "Custom/AdditiveOutline"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _WSOutlineWidth("World space outline width", Float) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline"="HDRenderPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Stencil
        {
            Ref 2 //Replace if other shader uses stencil buffers
            Comp NotEqual //I have no idea why this works, it should be Comp Equal to ensure it only writes when fragment in buffer equals 0
            Pass Replace //Write 1 to the stencil buffer if it (ensure it will not write again)
        }

        Pass
        {
            Cull Front
            Zwrite Off
            ZTest Less
            Blend SrcAlpha OneMinusSrcAlpha
            
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _WSOutlineWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityWorldToClipPos(mul(unity_ObjectToWorld, v.position) + UnityObjectToWorldDir(v.normal) * _WSOutlineWidth);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
}
