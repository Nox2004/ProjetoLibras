Shader "PostProcessing/OutlineEffect"
{
    HLSLINCLUDE
        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
        TEXTURE2D_SAMPLER2D(_CameraGBufferTexture2, sampler_CameraGBufferTexture2);

        float4x4 UNITY_MATRIX_MVP;
        float4x4 _ViewProjectInverse;

        float _OutlineThickness;
        float _OutlineDensity;
        float3 _OutlineColor;
        
        struct FragInput
        {
            float4 vertex    : SV_Position;
            float2 texcoord  : TEXCOORD0;
            float3 cameraDir : TEXCOORD1;
        };

        FragInput VertMain(AttributesDefault v)
        {
            FragInput o;
            
            o.vertex   = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz, 1.0));
            o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
            o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif

            float4 cameraLocalDir = mul(_ViewProjectInverse, float4(o.texcoord.x * 2.0 - 1.0, o.texcoord.y * 2.0 - 1.0, 0.5, 1.0));
            cameraLocalDir.xyz /= cameraLocalDir.w;
            cameraLocalDir.xyz -= _WorldSpaceCameraPos;

            float4 cameraForwardDir = mul(_ViewProjectInverse, float4(0.0, 0.0, 0.5, 1.0));
            cameraForwardDir.xyz /= cameraForwardDir.w;
            cameraForwardDir.xyz -= _WorldSpaceCameraPos;

            o.cameraDir = cameraLocalDir.xyz / length(cameraForwardDir.xyz);
            
            return o;
        }

        float4 FragMain(FragInput i) : SV_Target
        {
            float3 sceneColor  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord).rgb;
            float  sceneDepth  = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord).r;
            float3 sceneNormal = SAMPLE_TEXTURE2D(_CameraGBufferTexture2, sampler_CameraGBufferTexture2, i.texcoord).xyz * 2.0 - 1.0;

            if (sceneDepth > 0.0)
            {
                float3 toCameraDir = normalize(-i.cameraDir);
                float silhouette = dot(toCameraDir, normalize(sceneNormal));

                silhouette = saturate(silhouette + _OutlineThickness);
                silhouette = smoothstep(_OutlineDensity, 1.0, silhouette);
                
                sceneColor = lerp(_OutlineColor, sceneColor, silhouette);
            }

            return float4(sceneColor, 1.0);
        }
    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
                #pragma vertex VertMain
                #pragma fragment FragMain
            ENDHLSL
        }
    }
}
