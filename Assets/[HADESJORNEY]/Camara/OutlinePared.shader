Shader "Custom/OutlinePared"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0,0,1,1)
        _OutlineWidth ("Outline Width", Float) = 0.02
    }

    SubShader
    {
        Tags { "Queue" = "Geometry+2" }

        // Paso 1: Marcar donde SÍ se ve (expandido para cubrir el borde)
        Pass
        {
            ZTest LEqual
            ZWrite Off
            ColorMask 0
            Cull Back

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float _OutlineWidth;

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 pos : SV_POSITION; };

            v2f vert(appdata v)
            {
                v2f o;
                float3 expandido = v.vertex.xyz + v.normal * _OutlineWidth * 1.5;
                o.pos = UnityObjectToClipPos(float4(expandido, 1.0));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target { return 0; }
            ENDCG
        }

        // Paso 2: Marcar interior detrás de pared
        Pass
        {
            ZTest Greater
            ZWrite Off
            ColorMask 0
            Cull Back

            Stencil
            {
                Ref 2
                Comp NotEqual
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; };
            v2f vert(appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); return o; }
            fixed4 frag(v2f i) : SV_Target { return 0; }
            ENDCG
        }

        // Paso 3: Dibujar solo el borde detrás de pared
        Pass
        {
            ZTest Greater
            ZWrite Off
            Cull Front

            Stencil
            {
                Ref 0
                Comp Equal
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 _OutlineColor;
            float _OutlineWidth;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f { float4 pos : SV_POSITION; };

            v2f vert(appdata v)
            {
                v2f o;
                float3 expandido = v.vertex.xyz + v.normal * _OutlineWidth;
                o.pos = UnityObjectToClipPos(float4(expandido, 1.0));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target { return _OutlineColor; }
            ENDCG
        }
    }
}