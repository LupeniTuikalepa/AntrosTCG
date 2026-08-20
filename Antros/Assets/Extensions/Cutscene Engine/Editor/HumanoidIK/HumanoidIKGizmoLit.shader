Shader "Hidden/CutsceneEngine/HumanoidIKGizmoLit"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _SrcBlend("Source Blend", Float) = 5
        [HideInInspector] _DstBlend("Destination Blend", Float) = 10
        [HideInInspector] _Cull("Cull", Float) = 2
        [HideInInspector] _ZWrite("Z Write", Float) = 1
        [HideInInspector] _ZTest("Z Test", Float) = 4
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            Cull [_Cull]
            ZWrite [_ZWrite]
            ZTest [_ZTest]

            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 2.0
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            fixed4 _Color;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalVS : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                Varyings output;
                output.positionCS = UnityObjectToClipPos(input.positionOS);
                output.normalVS = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, input.normalOS));
                return output;
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                half3 normalVS = normalize(input.normalVS);
                half diffuse = 0.35h + 0.65h * saturate(dot(normalVS, normalize(half3(0.35h, 0.45h, 1.0h))));
                return fixed4(_Color.rgb * diffuse, _Color.a);
            }
            ENDCG
        }
    }
}
