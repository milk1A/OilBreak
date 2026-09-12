Shader "Custom/LaserAlwaysVisible"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,0,0,1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Overlay"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            ZWrite Off
            ZTest Always

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs pos =
                    GetVertexPositionInputs(
                        input.positionOS.xyz
                    );

                output.positionHCS =
                    pos.positionCS;

                output.color =
                    input.color * _BaseColor;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return input.color;
            }

            ENDHLSL
        }
    }
}