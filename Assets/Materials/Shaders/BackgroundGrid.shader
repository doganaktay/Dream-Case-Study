Shader "Archi/BackgroundGrid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _SecondaryColor ("Secondary Color", Color) = (1,1,1,1)
        _GridWidth ("Grid Width", float) = 0
        _GridHeight ("Grid Height", float) = 0
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

        ZWrite Off
		Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _BaseColor;
            fixed4 _SecondaryColor;
            int _GridWidth;
            int _GridHeight;

            float AccumulateDistance(float2 gvFromCenter, float radius){
                float accumulatedDistance = 0;

                for(float y = -1.; y <= 1; y++){
                    for(float x = -1.; x <= 1.; x++){
                        float2 offset = float2(x,y);
                        float d = length(gvFromCenter + offset);
                        accumulatedDistance += smoothstep(radius, radius * .9, d);
                    }
                }

                return accumulatedDistance;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 gv = float2(i.uv.x * _GridHeight, i.uv.y * _GridWidth);
                float2 id = floor(gv);
                gv = frac(gv);

                float2 gvFromCenter = gv - .5;

                fixed4 col = tex2D(_MainTex, gv);
                col = col.r <= 0.001 ? _BaseColor : _SecondaryColor;
                //col += AccumulateDistance(gvFromCenter, .4);
                return col;
            }
            ENDCG
        }
    }

    Fallback "Unlit/Color"
}
