Shader "Custom/SpritePixelDisintegrate"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Progress ("Progress", Range(0,1)) = 0
        _PixelSize ("Pixel Size", Float) = 32
        _ScatterStrength ("Scatter Strength", Float) = 0.35
        _FadeSoftness ("Fade Softness", Range(0.001, 0.2)) = 0.04
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Progress;
            float _PixelSize;
            float _ScatterStrength;
            float _FadeSoftness;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float2 hash22(float2 p)
            {
                float n = hash21(p);
                return frac(float2(n, n * 1.357) * float2(13.37, 31.79));
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 grid = max(_PixelSize, 1.0);
                float2 cell = floor(i.uv * grid);
                float2 rand = hash22(cell) * 2.0 - 1.0;
                float reveal = hash21(cell);

                float localProgress = saturate((_Progress - reveal * _FadeSoftness) / max(1.0 - reveal * _FadeSoftness, 0.0001));
                float2 sampleUV = i.uv - rand * (_ScatterStrength * localProgress / grid);

                fixed4 col = tex2D(_MainTex, sampleUV) * i.color;
                float alphaMask = 1.0 - smoothstep(reveal, reveal + _FadeSoftness, _Progress);
                col.a *= alphaMask;
                col.rgb *= col.a;
                return col;
            }
            ENDCG
        }
    }
}
