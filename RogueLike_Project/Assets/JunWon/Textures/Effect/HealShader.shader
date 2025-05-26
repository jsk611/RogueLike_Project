Shader "Custom/HealFieldTileEffect" {
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
        _HealColor ("Heal Color", Color) = (0,1,0.5,1)
        _GlowColor ("Glow Color", Color) = (0,1,0,1)
        _SecondaryColor ("Secondary Color", Color) = (0.2,0.8,0.3,1)
        
        // 타일 모양 효과 관련 프로퍼티
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 1.5
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2.0
        _WaveSpeed ("Wave Speed", Range(0, 10)) = 3.0
        _EdgeSharpness ("Edge Sharpness", Range(0.1, 1.0)) = 0.8
        _CenterIntensity ("Center Intensity", Range(0, 2)) = 1.0
        _Transparency ("Transparency", Range(0, 1)) = 0.7
        _SpreadProgress ("Spread Progress", Range(0, 1)) = 1.0
    }
    
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 localUV : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _HealColor;
            fixed4 _GlowColor;
            fixed4 _SecondaryColor;
            float _GlowIntensity;
            float _PulseSpeed;
            float _WaveSpeed;
            float _EdgeSharpness;
            float _CenterIntensity;
            float _Transparency;
            float _SpreadProgress;
            
            v2f vert (appdata v) {
                v2f o;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                o.localUV = v.uv; // 로컬 UV (0~1)
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                // 시간 변수
                float time = _Time.y;
                
                // UV를 중앙 기준으로 변환 (-0.5 ~ 0.5)
                float2 centeredUV = i.localUV - 0.5;
                
                // 사각형의 각 모서리까지의 거리 (맨하탄 거리 사용)
                float distToEdge = max(abs(centeredUV.x), abs(centeredUV.y));
                
                // 중앙에서부터 퍼져나가는 효과
                float spreadEffect = smoothstep(0.0, _SpreadProgress * 0.5, 0.5 - distToEdge);
                
                // 펄스 효과 (중앙에서 모서리로)
                float pulseWave = sin(time * _PulseSpeed - distToEdge * 15.0) * 0.5 + 0.5;
                
                // 웨이브 효과 (사각형을 따라 흐르는)
                float waveX = sin(time * _WaveSpeed + centeredUV.x * 10.0) * 0.5 + 0.5;
                float waveY = sin(time * _WaveSpeed + centeredUV.y * 10.0) * 0.5 + 0.5;
                float combinedWave = (waveX + waveY) * 0.5;
                
                // 모서리 강조 효과
                float edgeGlow = 1.0 - smoothstep(0.4, 0.5, distToEdge);
                edgeGlow = pow(edgeGlow, _EdgeSharpness);
                
                // 중앙 강조 효과
                float centerGlow = 1.0 - smoothstep(0.0, 0.3, distToEdge);
                centerGlow *= _CenterIntensity;
                
                // 격자 패턴 (타일 느낌)
                float gridX = abs(frac(i.localUV.x * 8.0) - 0.5) * 2.0;
                float gridY = abs(frac(i.localUV.y * 8.0) - 0.5) * 2.0;
                float grid = min(gridX, gridY);
                grid = smoothstep(0.0, 0.1, grid);
                
                // 베이스 텍스처
                fixed4 texColor = tex2D(_MainTex, i.uv);
                
                // 색상 믹스
                float colorMix = pulseWave * combinedWave;
                fixed4 baseColor = lerp(_HealColor, _SecondaryColor, colorMix);
                
                // 최종 색상 계산
                fixed4 finalColor = texColor * baseColor * i.color;
                
                // 각종 효과들을 조합
                float totalEffect = (pulseWave * 0.4 + combinedWave * 0.3 + edgeGlow * 0.2 + centerGlow * 0.1);
                totalEffect *= spreadEffect; // 퍼지는 효과 적용
                totalEffect *= grid; // 격자 패턴 적용
                
                // 글로우 효과 추가
                finalColor.rgb += _GlowColor.rgb * totalEffect * _GlowIntensity;
                
                // 알파값 (모서리까지 퍼지는 효과)
                float alpha = totalEffect * _Transparency * spreadEffect;
                finalColor.a = alpha * texColor.a * i.color.a;
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}