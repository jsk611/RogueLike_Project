Shader "Custom/DataStreamParticleShader" {
    Properties {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _PrimaryColor ("Primary Color", Color) = (0,1,0.5,1)
        _SecondaryColor ("Secondary Color", Color) = (0,0.6,1,1)
        _GlowColor ("Glow Color", Color) = (0,1,0,1)
        
        // 효과 관련 프로퍼티
        _StreamWidth ("Stream Width", Range(0.01, 0.5)) = 0.05
        _StreamSpeed ("Stream Speed", Range(0.5, 10)) = 4
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 1.0
        _FlickerSpeed ("Flicker Speed", Range(0, 10)) = 5.0
        _NoiseAmount ("Noise Amount", Range(0, 1)) = 0.1
        _FadeDistance ("Fade Distance", Range(0, 1)) = 0.2
    }
    
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
        LOD 100
        
        Blend SrcAlpha One // 가산 블렌딩으로 발광 효과
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
                float4 velocity : TEXCOORD1; // 속도 정보는 velocity로 전달
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 params : TEXCOORD1; // 파라미터 저장용
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _PrimaryColor;
            fixed4 _SecondaryColor;
            fixed4 _GlowColor;
            float _StreamWidth;
            float _StreamSpeed;
            float _GlowIntensity;
            float _FlickerSpeed;
            float _NoiseAmount;
            float _FadeDistance;
            
            // 랜덤 함수
            float random(float2 st) {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            v2f vert (appdata v) {
                v2f o;
                
                // 파티클 색상에서 정보 추출
                float direction = v.color.r;      // r 채널: 방향 (0-1 각도)
                float streamID = v.color.g * 20;  // g 채널: 스트림 ID (0-20)
                float speedFactor = v.color.b;    // b 채널: 속도 계수
                
                // 방향 벡터 계산 (direction 값을 각도로 변환)
                float angle = direction * 6.28318; // 0-1 값을 0-2π로 변환
                float2 streamDir = float2(cos(angle), sin(angle));
                
                // 파티클 위치 사용 (이미 스크립트에서 배치됨)
                float4 worldPos = v.vertex;
                
                // 시간 및 노이즈 계산
                float time = _Time.y * _StreamSpeed * speedFactor;
                float noiseTime = time + streamID * 0.5;
                
                // 노이즈 적용 (약간의 흔들림)
                float2 noise = float2(
                    sin(noiseTime + v.vertex.x * 10.0),
                    cos(noiseTime * 1.2 + v.vertex.y * 10.0)
                ) * _NoiseAmount;
                
                // 최종 위치에 노이즈 추가
                worldPos.xy += noise;
                
                o.vertex = UnityObjectToClipPos(worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // 방향에 따른 색상 변화
                float colorLerp = sin(time * 0.2 + streamID) * 0.5 + 0.5;
                o.color = lerp(_PrimaryColor, _SecondaryColor, colorLerp);
                
                // 파라미터 저장
                o.params = float4(direction, streamID, speedFactor, time);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                // 파라미터 추출
                float direction = i.params.x;
                float streamID = i.params.y;
                float speedFactor = i.params.z;
                float time = i.params.w;
                
                // 데이터 스트림 효과 (UV 좌표를 기반으로 한 스트림 모양)
                float streamCenter = abs(i.uv.y - 0.5) / 0.5; // 0: 중앙, 1: 가장자리
                float streamShape = 1.0 - streamCenter;
                streamShape = pow(streamShape, 2.0); // 테두리를 더 부드럽게
                
                // 데이터 패턴 효과 (불규칙한 밝기 변화)
                float dataNoise = random(floor(i.uv * 10.0 + time));
                float dataChunk = step(0.6, dataNoise) * 0.3 + 0.7; // 30% 밝기 변화
                
                // 더 복잡한 데이터 패턴 (0과 1이 흐르는 느낌)
                float binaryPattern = step(0.5, random(floor(i.uv.x * 15.0 + time) / 15.0));
                
                // 깜빡임 효과
                float flicker = sin(time * _FlickerSpeed + streamID) * 0.5 + 0.5;
                flicker = pow(flicker, 0.5) * 0.3 + 0.7; // 부드러운 깜빡임 (30% 변화)
                
                // 발광 효과
                float glow = _GlowIntensity * flicker;
                
                // 텍스처 샘플링
                fixed4 texColor = tex2D(_MainTex, i.uv);
                
                // 스트림 강도 계산
                float streamIntensity = streamShape * dataChunk * flicker;
                
                // 최종 색상
                fixed4 finalColor = i.color * streamIntensity;
                finalColor.rgb += _GlowColor.rgb * glow * streamShape * binaryPattern * 0.5;
                finalColor.a = streamShape * texColor.a;
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}