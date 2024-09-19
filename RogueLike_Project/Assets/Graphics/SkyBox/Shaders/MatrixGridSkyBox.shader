Shader "Custom/MatrixGridSkyBox"
{
    Properties
    {
        _GridColor ("Grid Color", Color) = (0,1,0,1)
        _BackgroundTopColor ("Background Top Color", Color) = (0,0,0,1)
        _BackgroundBottomColor ("Background Bottom Color", Color) = (0,0,0,1)
        _GridSpacing ("Grid Spacing", Float) = 10.0
        _LineThickness ("Line Thickness", Float) = 0.1
        _GlowIntensity ("Glow Intensity", Float) = 1.0
        _AnimationSpeed ("Animation Speed", Float) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float3 worldPos : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            uniform float _GridSpacing;
            uniform float _LineThickness;
            uniform float4 _GridColor;
            uniform float4 _BackgroundTopColor;
            uniform float4 _BackgroundBottomColor;
            uniform float _GlowIntensity;
            uniform float _AnimationSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 시간에 따른 애니메이션
                float time = _Time.y * _AnimationSpeed;
                float3 pos = i.worldPos + float3(time, time, time);

                // 그리드 계산
                float grid = 0.0;
                for(int axis = 0; axis < 3; axis++)
                {
                    float coord = pos[axis] / _GridSpacing;
                    float gridLine = abs(frac(coord) - 0.5) / fwidth(coord);
                    grid = max(grid, 1.0 - smoothstep(_LineThickness, _LineThickness + 0.02, gridLine));
                }

                // 글로우 효과
                float glow = grid * _GlowIntensity;

                // 배경 그라데이션 계산 (위에서 아래로)
                float3 dir = normalize(i.worldPos);
                float gradient = clamp(dot(dir, float3(0, 1, 0)), 0.0, 1.0);
                float3 backgroundColor = lerp(_BackgroundBottomColor.rgb, _BackgroundTopColor.rgb, gradient);

                // 최종 색상 합성
                float3 finalColor = backgroundColor + _GridColor.rgb * grid + float3(glow, glow, glow);
                float alpha = lerp(_BackgroundBottomColor.a, _GridColor.a, grid);

                return float4(finalColor, alpha);
            }
            ENDCG
        }
    }
}
