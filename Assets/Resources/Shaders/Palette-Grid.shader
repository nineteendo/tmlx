Shader "Palette/Grid"
{
    Properties
    {
        [MainTexture] _ColorIdxs ("Base (RGB) Trans (A)", 2D) = "white" {} // NOTE - Fixes _MainTex_TexelSize not updating
        _ColorMap ("Color Map (RGB)", 2D) = "white" {}
        [Toggle(FLIP_HORIZONTAL)] _FlipHorizontal("Flip Horizontal", Float) = .0
        [Toggle(FLIP_VERTICAL)] _FlipVertical("Flip Vertical", Float) = .0
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

            #pragma multi_compile _ FLIP_HORIZONTAL
            #pragma multi_compile _ FLIP_VERTICAL

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

            static const float CELL_SCALE = 5.0, STRENGTH = .1;

            float _DarkFilterLevel;
            float4 _ColorIdxs_ST, _ColorIdxs_TexelSize, _ColorMap_ST, _ColorMap_TexelSize;
            // Enable high precision on mobile
            sampler2D_float _ColorIdxs, _ColorMap;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _ColorIdxs);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the main texture
                fixed4 col = tex2D(_ColorIdxs, i.uv);

                // Sample the color map texture
                float width = _ColorMap_TexelSize.z;
                float height = _ColorMap_TexelSize.w;
                float xScale, yScale;
                if (width <= 4.0)
                    xScale = width <= 2.0 ? 1.0 : 3.0;
                else
                    xScale = width <= 16.0 ? 15.0 : 255.0;

                if (height <= 4.0)
                    yScale = height <= 2.0 ? 1.0 : 3.0;
                else
                    yScale = height <= 16.0 ? 15.0 : 255.0;

            #ifdef FLIP_HORIZONTAL
                float x = 1.0 - floor(1.0 + col.r * xScale) / width;
            #else
                float x = floor(col.r * xScale) / width;
            #endif

            #ifdef FLIP_VERTICAL
                float y = 1.0 - floor(1.0 + col.g * yScale) / height;
            #else
                float y = floor(col.g * yScale) / height;
            #endif

                col.rgb = tex2D(_ColorMap, TRANSFORM_TEX(float2(x, y), _ColorMap)).rgb;

                // Add grid
                // FIXME - top left & bottom right triangle don't behave the same on low resolutions
                if (uint(i.uv.x * _ColorIdxs_TexelSize.z * CELL_SCALE + .5) % CELL_SCALE == .0 || uint(i.uv.y * _ColorIdxs_TexelSize.w * CELL_SCALE + .5) % CELL_SCALE == .0)
                    col.rgb += STRENGTH - 2.0 * STRENGTH * col.rgb;

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
