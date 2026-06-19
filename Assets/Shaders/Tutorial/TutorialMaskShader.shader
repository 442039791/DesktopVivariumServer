Shader "Tutorial/MaskShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Mask Color", Color) = (0,0,0,0.8)
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Float) = 0.1
        _Width ("Width", Float) = 0.2
        _Height ("Height", Float) = 0.2
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color;
            float2 _Center;
            float _Radius;
            float _Width;
            float _Height;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 计算到中心的距离
                float dist = distance(i.uv, _Center);

                // 圆形镂空
                float circleMask = step(dist, _Radius);

                // 矩形镂空
                float2 delta = abs(i.uv - _Center);
                float rectMask = step(delta.x, _Width / 2) * step(delta.y, _Height / 2);

                // 使用圆形或矩形（可通过代码控制）
                // 这里默认使用圆形，如果Radius很小则使用矩形
                float mask = _Radius > 0.01 ? circleMask : rectMask;

                // 反转遮罩（高亮区域透明，其他区域有颜色）
                float alpha = (1.0 - mask) * _Color.a;

                return float4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }

    Fallback "UI/Default"
}
