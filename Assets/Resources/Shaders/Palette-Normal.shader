Shader "Palette/Normal"
{
    Properties
    {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _ColorMap ("Color Map (RGB)", 2D) = "white" {}
        [Toggle(INVERT_COLORS)] _InvertColors("Invert Colors", Float) = .0
        _DarkFilterLevel("Dark Filter Level %", Range(.0, 50.0)) = .0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #pragma multi_compile _ INVERT_COLORS

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float _DarkFilterLevel;
            float4 _MainTex_ST, _ColorMap_ST, _ColorMap_TexelSize;
            // Enable high precision on mobile
            sampler2D_float _MainTex, _ColorMap;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the main texture
                fixed4 col = tex2D(_MainTex, i.uv);

            #ifdef INVERT_COLORS
                col.rgb = 1.0 - col.rgb;
            #endif

                // Apply Dark Filter Level %
                col.rgb *= 1.0 - _DarkFilterLevel / 100.0;

                // Apply fog and return the final color
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
