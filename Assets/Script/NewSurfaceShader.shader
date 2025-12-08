Shader "ChromaKeyKit/UI/ChromaKey_UI" {
    Properties{
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _KeyColor("KeyColor", Color) = (0,0,0,1) // ºÚÉ«±³¾°
        _DChroma("D Chroma", Range(0.0, 1.0)) = 0.5
        _DChromaT("D ChromaT", Range(0.0, 1.0)) = 0.05
        _DLuma("D Luma", Range(0.0, 1.0)) = 0.5
        _DLumaT("D LumaT", Range(0.0, 1.0)) = 0.05
    }

    SubShader {
        Tags { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _KeyColor;
            float _DChroma;
            float _DLuma;
            float _DLumaT;
            float _DChromaT;

            v2f vert(appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            half3 RGB_To_YCbCr(half3 rgb) {
                half Y = 0.299 * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b;
                half Cb = 0.564 * (rgb.b - Y);
                half Cr = 0.713 * (rgb.r - Y);
                return half3(Cb, Cr, Y);
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 c = tex2D(_MainTex, i.texcoord) * i.color;
                
                // ¿ÙºÚµ×Âß¼­
                half3 src_YCbCr = RGB_To_YCbCr(c.rgb);
                half3 key_YCbCr = RGB_To_YCbCr(_KeyColor);
                half dChroma = distance(src_YCbCr.xy, key_YCbCr.xy);
                half dLuma = distance(src_YCbCr.z, key_YCbCr.z);

                if (dLuma < _DLuma && dChroma < _DChroma) {
                    half a = 0;
                    if (dChroma > _DChroma - _DChromaT) {
                        a = (dChroma - _DChroma + _DChromaT) / _DLumaT;
                    }
                    if (dLuma > _DLuma - _DLumaT) {
                        a = max(a, (dLuma - _DLuma + _DLumaT) / _DLumaT);
                    }
                    c.a = a;
                }
                
                return c;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}