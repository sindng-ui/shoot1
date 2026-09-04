Shader "HappyShoot/DamageTextOutline"
{
    Properties
    {
        _MainTex ("Font Texture", 2D) = "white" {}
        _Color ("Text Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0.001, 0.05)) = 0.015
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Lighting Off
        Cull Off
        ZTest Always
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord;
                float centerA = tex2D(_MainTex, uv).a;

                // 8-directional texel sampling for crisp 2~3px heavy outline
                float2 d = _MainTex_TexelSize.xy * 2.2f;
                float maxA = centerA;
                maxA = max(maxA, tex2D(_MainTex, uv + float2(d.x, 0)).a);
                maxA = max(maxA, tex2D(_MainTex, uv - float2(d.x, 0)).a);
                maxA = max(maxA, tex2D(_MainTex, uv + float2(0, d.y)).a);
                maxA = max(maxA, tex2D(_MainTex, uv - float2(0, d.y)).a);
                maxA = max(maxA, tex2D(_MainTex, uv + float2(d.x * 0.707, d.y * 0.707)).a);
                maxA = max(maxA, tex2D(_MainTex, uv - float2(d.x * 0.707, d.y * 0.707)).a);
                maxA = max(maxA, tex2D(_MainTex, uv + float2(d.x * 0.707, -d.y * 0.707)).a);
                maxA = max(maxA, tex2D(_MainTex, uv + float2(-d.x * 0.707, d.y * 0.707)).a);

                if (maxA < 0.05)
                {
                    discard;
                }

                // If inside glyph core, show text color; otherwise outline
                float isCore = smoothstep(0.40, 0.55, centerA);
                fixed4 textColor = i.color;
                fixed4 outColor = _OutlineColor;
                outColor.a *= i.color.a;

                fixed4 finalCol = lerp(outColor, textColor, isCore);
                finalCol.a *= maxA;
                return finalCol;
            }
            ENDCG
        }
    }
}
