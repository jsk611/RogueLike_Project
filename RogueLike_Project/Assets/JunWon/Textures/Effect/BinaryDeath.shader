Shader "Custom/BinaryNumbersParticleShader" {
    Properties {
        _ZeroTex ("Zero Texture", 2D) = "white" {}
        _OneTex ("One Texture", 2D) = "white" {}
        _PrimaryColor ("Primary Color", Color) = (0,1,0.5,1)
        _SecondaryColor ("Secondary Color", Color) = (0,0.6,1,1)
        _GlowColor ("Glow Color", Color) = (0,1,0,1)
        
        // 효과 관련 프로퍼티
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 1.0
        _FlickerSpeed ("Flicker Speed", Range(0, 10)) = 5.0
        _RiseSpeed ("Rise Speed", Range(0, 10)) = 1.0
        _SpinSpeed ("Spin Speed", Range(-5, 5)) = 1.0
        _FadeDistance ("Fade Distance", Range(0, 5)) = 2.0
        _DigitalEffect ("Digital Effect", Range(0, 1)) = 0.5
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
                float4 customData : TEXCOORD1; // x: 파티클 나이, y: 랜덤 시드, z: 0 또는 1 선택
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 customData : TEXCOORD1;
            };
            
            sampler2D _ZeroTex;
            sampler2D _OneTex;
            float4 _ZeroTex_ST;
            float4 _OneTex_ST;
            fixed4 _PrimaryColor;
            fixed4 _SecondaryColor;
            fixed4 _GlowColor;
            float _GlowIntensity;
            float _FlickerSpeed;
            float _RiseSpeed;
            float _SpinSpeed;
            float _FadeDistance;
            float _DigitalEffect;
            
            // 랜덤 함수
            float random(float2 st) {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            v2f vert (appdata v) {
                v2f o;
                
                // 파티클에 따라 0과 1 중 선택
                float binaryType = v.customData.z;
                if (binaryType <= 0.0) {
                    // customData에 값이 없으면 UV 좌표로 결정
                    binaryType = step(0.5, random(v.uv));
                }
                
                // 파티클 시드 (각 파티클별 고유값)
                float seed = v.customData.y;
                if (seed <= 0.0) {
                    // customData에 값이 없으면 버텍스 위치로 결정
                    seed = random(v.vertex.xy);
                }
                
                // 상승 효과 (y축으로 이동)
                float time = _Time.y;
                float rise = time * _RiseSpeed + seed * 10.0; // 각 파티클마다 다른 높이
                
                // 회전 효과
                float spin = time * _SpinSpeed + seed * 6.28318; // 각 파티클마다 다른 회전 각도
                float2x2 rotationMatrix = float2x2(
                    cos(spin), -sin(spin),
                    sin(spin), cos(spin)
                );
                float2 rotatedPos = mul(rotationMatrix, v.vertex.xy);
                
                // 좌우 흔들림 효과
                float wiggle = sin(time * 3.0 + seed * 10.0) * 0.1 * seed;
                
                // 최종 위치 계산
                float3 newPos = float3(
                    rotatedPos.x + wiggle,
                    v.vertex.y + rise,
                    v.vertex.z
                );
                
                // 월드 공간에서의 버텍스 위치 계산
                float4 worldPos = mul(unity_ObjectToWorld, float4(newPos, 1.0));
                
                // 카메라와의 거리에 따른 페이드 계산
                float viewDist = distance(_WorldSpaceCameraPos.xyz, worldPos.xyz);
                float fadeAlpha = saturate((_FadeDistance - viewDist) / _FadeDistance);
                
                o.vertex = UnityObjectToClipPos(float4(newPos, 1.0));
                o.uv = v.uv;
                
                // 색상에 페이드와 파티클 색상 반영
                o.color = v.color;
                o.color.a *= fadeAlpha;
                
                // 커스텀 데이터 전달
                o.customData = float4(binaryType, seed, v.customData.x, 0);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                // 0 또는 1 결정
                float binaryType = i.customData.x;
                float seed = i.customData.y;
                float age = i.customData.z; // 파티클 나이 (있는 경우)
                
                // 시간 변수
                float time = _Time.y;
                
                // 깜빡임 효과
                float flicker = sin(time * _FlickerSpeed + seed * 10.0) * 0.5 + 0.5;
                flicker = pow(flicker, 0.5); // 더 자연스러운 깜빡임
                
                // 디지털 노이즈 효과
                float2 noiseUV = i.uv + time * 0.1;
                float digitalNoise = random(floor(noiseUV * 20.0) / 20.0);
                float glitchEffect = step(0.7, digitalNoise) * _DigitalEffect;
                
                // 텍스처 선택 및 샘플링
                float2 offsetUV = i.uv;
                
                // 글리치 효과 (가끔 UV 왜곡)
                if (glitchEffect > 0.0) {
                    offsetUV.x += (digitalNoise - 0.5) * 0.1 * _DigitalEffect;
                }
                
                // 0 또는 1 텍스처 샘플링
                fixed4 texColor;
                if (binaryType < 0.5) {
                    texColor = tex2D(_ZeroTex, offsetUV);
                } else {
                    texColor = tex2D(_OneTex, offsetUV);
                }
                
                // 색상 계산
                float colorLerp = sin(time * 0.5 + seed * 6.28318) * 0.5 + 0.5;
                fixed4 baseColor = lerp(_PrimaryColor, _SecondaryColor, colorLerp);
                
                // 디지털 효과 (가끔 색상 변화)
                if (glitchEffect > 0.0) {
                    baseColor.rgb = lerp(baseColor.rgb, _GlowColor.rgb, glitchEffect * 0.5);
                }
                
                // 최종 색상
                fixed4 finalColor = texColor * i.color;
                finalColor.rgb *= baseColor.rgb;
                
                // 발광 효과
                finalColor.rgb += _GlowColor.rgb * flicker * _GlowIntensity * 0.3;
                
                // 알파값 계산
                finalColor.a *= texColor.a * i.color.a;
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}