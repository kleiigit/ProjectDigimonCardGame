Shader "UI/OutlineShadowSoft"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineSize ("Outline Size", Float) = 1.0
        _Feather ("Feather", Float) = 0.1

        _ShadowColor ("Shadow Color", Color) = (0,0,0,0.5)
        _ShadowOffset ("Shadow Offset", Vector) = (2, -2, 0, 0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineSize;
            float _Feather;

            float4 _ShadowColor;
            float4 _ShadowOffset;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord;
                fixed4 col = tex2D(_MainTex, uv);
                float alpha = col.a;

                float2 offset = _OutlineSize / _ScreenParams.xy;

                // Amostragem em 8 direções para Outline
                float outline = 0.0;
                outline += tex2D(_MainTex, uv + float2(offset.x, 0)).a;
                outline += tex2D(_MainTex, uv + float2(-offset.x, 0)).a;
                outline += tex2D(_MainTex, uv + float2(0, offset.y)).a;
                outline += tex2D(_MainTex, uv + float2(0, -offset.y)).a;
                outline += tex2D(_MainTex, uv + offset).a;
                outline += tex2D(_MainTex, uv - offset).a;
                outline += tex2D(_MainTex, uv + float2(offset.x, -offset.y)).a;
                outline += tex2D(_MainTex, uv + float2(-offset.x, offset.y)).a;

                outline /= 8.0;

                float smoothOutline = saturate((outline - alpha) / _Feather);

                // Sombra
                float2 shadowOffset = _ShadowOffset.xy / _ScreenParams.xy;
                fixed4 shadowSample = tex2D(_MainTex, uv + shadowOffset);
                float shadowAlpha = shadowSample.a;

                // Composição
                fixed4 result = col;

                // Se for totalmente transparente mas tiver sombra -> desenha sombra
                if (alpha == 0 && shadowAlpha > 0)
                {
                    result = fixed4(_ShadowColor.rgb, shadowAlpha * _ShadowColor.a);
                }

                // Se for totalmente transparente mas tiver outline -> desenha outline
                if (alpha == 0 && smoothOutline > 0)
                {
                    result = fixed4(_OutlineColor.rgb, smoothOutline * _OutlineColor.a);
                }

                // Se for parte do sprite -> mantém
                return result;
            }
            ENDCG
        }
    }
}
