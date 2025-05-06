Shader "Custom/DigitalStripeDissolve" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (0,0.5,1,1)
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _StripeWidth ("Stripe Width", Range(1, 50)) = 10
        _StripeSpeed ("Stripe Speed", Range(0, 10)) = 1
        _StripeIntensity ("Stripe Intensity", Range(0, 1)) = 0.5
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.3
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05
    }
    
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite On
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float _DissolveAmount;
            float _StripeWidth;
            float _StripeSpeed;
            float _StripeIntensity;
            float _GlitchIntensity;
            float _EdgeWidth;
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 normal : NORMAL;
                float noise : TEXCOORD2;
            };
            
            // 랜덤 함수
            float rand(float2 co) {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            // 노이즈 함수
            float sampleNoise(float2 uv) {
                return tex2Dlod(_NoiseTex, float4(uv, 0, 0)).r;
            }
            
            // 글리치 효과
            float3 applyGlitch(float3 pos, float2 uv, float intensity) {
                float time = _Time.y;
                
                // 수평 글리치 - 특정 Y값에서만 발생
                float glitchLineY = floor(uv.y * 20) / 20;
                float glitchNoise = rand(float2(glitchLineY, floor(time * 10)));
                float glitchThreshold = 0.75; // 글리치가 발생할 임계값
                
                if (glitchNoise > glitchThreshold) {
                    // 강한 글리치 - X축으로 오프셋
                    float glitchAmount = (glitchNoise - glitchThreshold) * 4.0 * intensity;
                    pos.x += (rand(float2(glitchLineY, time)) * 2 - 1) * glitchAmount;
                }
                
                return pos;
            }
            
            v2f vert(appdata v) {
                v2f o;
                
                float3 localPos = v.vertex.xyz;
                float noise = sampleNoise(v.uv);
                o.noise = noise;
                
                // 디졸브 진행도에 따라 글리치 효과 적용
                if (_DissolveAmount > 0 && _GlitchIntensity > 0) {
                    localPos = applyGlitch(localPos, v.uv, _GlitchIntensity * _DissolveAmount);
                }
                
                // 버텍스 위치 업데이트
                float4 modifiedVertex = float4(localPos, v.vertex.w);
                
                o.vertex = UnityObjectToClipPos(modifiedVertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, modifiedVertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                // 시간 계산
                float time = _Time.y * _StripeSpeed;
                
                // 수평 스트라이프 패턴 생성
                float stripePattern = sin(i.worldPos.y * _StripeWidth + time * 5) * 0.5 + 0.5;
                
                // 노이즈와 스트라이프 패턴을 조합하여 디졸브 마스크 생성
                float dissolveNoise = i.noise * 0.5 + stripePattern * 0.5;
                
                // 추가 글리치 효과 (수평선)
                float glitchLine = 0;
                if (_GlitchIntensity > 0.2) {
                    float linePos = floor(i.uv.y * 30) / 30;
                    float lineNoise = rand(float2(linePos, floor(time * 8)));
                    
                    // 간헐적으로 글리치 라인 생성
                    if (lineNoise > 0.93) {
                        glitchLine = smoothstep(0.5, 0.51, rand(float2(i.uv.y, time)));
                    }
                }
                
                // 텍스처 샘플링
                float2 uvOffset = float2(0, 0);
                
                // 글리치 효과가 있으면 UV 왜곡 추가
                if (_GlitchIntensity > 0 && _DissolveAmount > 0.3) {
                    float glitchAmount = _GlitchIntensity * _DissolveAmount;
                    
                    // 스캔라인 효과
                    float scanLine = frac(i.uv.y * 10 - time);
                    float scanGlitch = step(0.9, scanLine) * glitchAmount * stripePattern;
                    
                    // UV 왜곡
                    uvOffset.x += (rand(float2(floor(i.uv.y * 20) / 20, time)) * 2 - 1) * scanGlitch * 0.1;
                }
                
                fixed4 col = tex2D(_MainTex, i.uv + uvOffset) * _Color;
                
                // 디졸브 마스크와 디졸브 진행도 비교
                float dissolveMask = step(dissolveNoise, _DissolveAmount);
                
                // 엣지 발광 효과 (지지직거리는 느낌)
                float edgeMask = step(dissolveNoise - _EdgeWidth, _DissolveAmount) - dissolveMask;
                
                // 발광 색상 계산 (스트라이프 패턴에 따라 강도 변화)
                float3 glowColorMod = _GlowColor.rgb * (0.8 + stripePattern * 0.4);
                
                // 글리치 라인이 있으면 색상 변경
                glowColorMod = lerp(glowColorMod, float3(1, 1, 1), glitchLine * 0.7);
                
                // 발광 효과 적용
                col.rgb = lerp(col.rgb, glowColorMod, edgeMask);
                
                // 글리치 라인 추가
                col.rgb = lerp(col.rgb, glowColorMod, glitchLine * _GlitchIntensity * 0.5);
                
                // 스트라이프 패턴에 따라 투명도 변화 (지지직거리는 느낌)
                float alpha = 1.0;
                
                if (_DissolveAmount > 0) {
                    // 디졸브 영역 투명화
                    alpha *= 1.0 - dissolveMask;
                    
                    // 디졸브 진행 중인 부분에 스트라이프 효과 추가
                    if (_DissolveAmount > 0.1 && _DissolveAmount < 0.9) {
                        float dissolveProgress = smoothstep(0.0, 0.2, _DissolveAmount) * 
                                              smoothstep(1.0, 0.8, _DissolveAmount);
                        
                        // 스트라이프 패턴에 따라 깜빡임
                        float flicker = sin(time * 20) * 0.5 + 0.5;
                        float stripeFade = stripePattern * _StripeIntensity * dissolveProgress * flicker;
                        
                        // 디졸브되는 영역 근처에서만 스트라이프 효과 적용
                        float proximity = smoothstep(_DissolveAmount + 0.1, _DissolveAmount - 0.1, dissolveNoise);
                        alpha *= lerp(1.0, stripeFade, proximity * 0.5);
                    }
                }
                
                col.a *= alpha * _Color.a;
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}