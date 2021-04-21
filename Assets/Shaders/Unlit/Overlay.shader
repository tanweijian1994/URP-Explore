Shader "Unlit/Overlay"
{
    Properties
    {
        [HideInInspector][MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)
    }
    
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    struct Attributes
    {
        float4 positionOS      : POSITION;
        float4 color           : COLOR;
        float2 uv              : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float2 uv         : TEXCOORD0;
        float4 color      : COLOR;
        float4 positionCS : SV_POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
    };

    CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    CBUFFER_END

    TEXTURE2D(_BaseMap);
    SAMPLER(sampler_BaseMap);

    Varyings VertDefault(Attributes input)
    {
        Varyings output = (Varyings)0;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        
        output.uv = input.uv;
        output.color = input.color * _BaseColor;
        output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
        return output;
    }

    half4 FragDefault(Varyings input) : SV_Target
    {
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
        half3 color = texColor.rgb * input.color.rgb;
        half alpha = texColor.a * input.color.a;
        
        return half4(color, alpha);
    }
    
    ENDHLSL
    
    
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }

        Lighting Off
        Cull Off
        ZTest Always
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            Name "Overlay"
            
            HLSLPROGRAM
            
            #pragma vertex VertDefault
            #pragma fragment FragDefault

            #pragma multi_compile_instancing
            
            ENDHLSL
        }
    }
}
