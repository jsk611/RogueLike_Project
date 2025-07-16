Shader "Custom/VirusDissolveShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _DissolveTexture ("Dissolve Noise", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _DissolveEdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05
        _EdgeColor ("Edge Color", Color) = (0, 1, 1, 1)
        _EdgeIntensity ("Edge Intensity", Range(0, 10)) = 3
        _GlowPower ("Glow Power", Range(1, 5)) = 2
        _MainColor ("Main Color", Color) = (1, 1, 1, 1)
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.1
        _NoiseScale ("Noise Scale", Range(0.1, 5)) = 1

        _EffectTime ("Effect Time", Float) = 0
        _EffectSpeed ("Effect Speed", Float) = 1
        
        // 추가 프로퍼티들
        _DissolveDirection ("Dissolve Direction", Vector) = (0, 1, 0, 0)
        _EdgePulseSpeed ("Edge Pulse Speed", Range(0, 10)) = 2
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
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
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float time : TEXCOORD3;
                float3 localPos : TEXCOORD4;
            };
            
            sampler2D _MainTex;
            sampler2D _DissolveTexture;
            float4 _MainTex_ST;
            float4 _DissolveTexture_ST;
            
            float _DissolveAmount;
            float _DissolveEdgeWidth;
            fixed4 _EdgeColor;
            float _EdgeIntensity;
            float _GlowPower;
            fixed4 _MainColor;
            float _GlitchIntensity;
            float _NoiseScale;

            float _EffectTime;
            float _EffectSpeed;
            float4 _DissolveDirection;
            float _EdgePulseSpeed;
           
            
            // 개선된 노이즈 함수들
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            // 더 부드러운 노이즈
            float smoothNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }
            
            // 프랙탈 노이즈 (더 복잡한 패턴)
            float fractalNoise(float2 uv)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                for(int i = 0; i < 4; i++)
                {
                    value += amplitude * smoothNoise(uv * frequency);
                    amplitude *= 0.5;
                    frequency *= 2.0;
                }
                
                return value;
            }
            
            v2f vert (appdata v)
            {
                v2f o;

                float t = _EffectSpeed * _EffectTime;
                
                // 로컬 좌표 저장
                o.localPos = v.vertex.xyz;
                
                // 글리치 효과 (버텍스 노이즈) - 더 부드럽게
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // 다중 주파수 글리치
                float glitchNoise1 = smoothNoise(worldPos.xz * 8 + t * 2) * 0.6;
                float glitchNoise2 = smoothNoise(worldPos.xz * 20 + t * 5) * 0.4;
                float combinedGlitch = (glitchNoise1 + glitchNoise2) * _GlitchIntensity;
                
                // 디졸브 진행에 따른 글리치 강화 (더 점진적)
                float dissolveMultiplier = smoothstep(0.0, 1.0, _DissolveAmount);
                combinedGlitch *= (1.0 + dissolveMultiplier * 3.0);
                
                // 버텍스 변형
                v.vertex.xyz += v.normal * combinedGlitch * 0.008;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = worldPos;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.time = t;

                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float t = i.time;
                
                // 메인 텍스처
                fixed4 mainCol = tex2D(_MainTex, i.uv) * _MainColor;
                
                // === 개선된 디졸브 노이즈 ===
                
                // 방향성 있는 디졸브 (아래에서 위로, 또는 설정된 방향)
                float directionalMask = dot(i.localPos, _DissolveDirection.xyz) * 0.5 + 0.5;
                
                // 다중 스케일 노이즈
                float2 dissolveUV1 = i.uv * _NoiseScale;
                float2 dissolveUV2 = i.uv * _NoiseScale * 3.7;
                float2 dissolveUV3 = i.worldPos.xz * 0.05 + t * 0.02;
                
                float dissolveNoise1 = tex2D(_DissolveTexture, dissolveUV1 + t * 0.01).r;
                float dissolveNoise2 = tex2D(_DissolveTexture, dissolveUV2 - t * 0.015).g;
                float dissolveNoise3 = fractalNoise(dissolveUV3);
                
                // 복합 노이즈 (가중치 조정)
                float combinedNoise = dissolveNoise1 * 0.4 + 
                                    dissolveNoise2 * 0.3 + 
                                    dissolveNoise3 * 0.3;
                
                // 방향성 추가
                combinedNoise = lerp(combinedNoise, directionalMask, 0.3);
                
                // 시간 기반 변화 (더 부드럽게)
                combinedNoise += sin(t * 1.5 + i.worldPos.x * 2) * 0.05;
                combinedNoise += cos(t * 2.3 + i.worldPos.z * 1.5) * 0.03;
                
                // === 디졸브 계산 ===
                float dissolveStep = _DissolveAmount;
                float dissolveEdge = dissolveStep + _DissolveEdgeWidth;
                
                // 클리핑
                clip(combinedNoise - dissolveStep);
                
                // === 개선된 엣지 효과 ===
                float edgeFactor = 1 - smoothstep(dissolveStep, dissolveEdge, combinedNoise);
                
                // 펄스 효과
                float pulseEffect = sin(t * _EdgePulseSpeed + i.worldPos.y * 3) * 0.3 + 0.7;
                
                // 엣지 컬러 계산
                fixed4 edgeCol = _EdgeColor * _EdgeIntensity * pulseEffect;
                edgeCol *= pow(edgeFactor, _GlowPower);
                
                // 더 부드러운 스캔라인 효과
                float scanline1 = sin(i.worldPos.y * 30 + t * 3) * 0.1 + 0.9;
                float scanline2 = sin(i.worldPos.y * 80 + t * 7) * 0.05 + 0.95;
                edgeCol.rgb *= scanline1 * scanline2;
                
                // === 최종 컬러 조합 ===
                fixed4 finalCol = mainCol + edgeCol;
                
                // 부드러운 투명도 변화
                float alpha = mainCol.a * (1 - smoothstep(0.7, 1.0, _DissolveAmount));
                alpha += edgeFactor * _EdgeColor.a;
                
                // === 개선된 글리치 색상 효과 ===
                if (_DissolveAmount > 0.2)
                {
                    float glitchFactor = smoothstep(0.2, 1.0, _DissolveAmount);
                    
                    // 더 자연스러운 색상 글리치
                    float colorGlitch1 = fractalNoise(i.uv * 15 + t * 20);
                    float colorGlitch2 = fractalNoise(i.uv * 25 + t * 30);
                    
                    // 임계값 조정으로 더 자연스럽게
                    if (colorGlitch1 > 0.85)
                    {
                        float3 glitchColor = lerp(_EdgeColor.rgb, float3(1, 0.2, 1), colorGlitch2);
                        finalCol.rgb = lerp(finalCol.rgb, glitchColor, glitchFactor * 0.3);
                    }
                    
                    // RGB 채널 분리 효과 (크로마틱 수차)
                    if (colorGlitch2 > 0.9)
                    {
                        float2 offset = float2(0.002, 0) * glitchFactor;
                        finalCol.r = tex2D(_MainTex, i.uv + offset).r * _MainColor.r;
                        finalCol.b = tex2D(_MainTex, i.uv - offset).b * _MainColor.b;
                    }
                }
                
                finalCol.a = alpha;
                return finalCol;
            }
            ENDCG
        }
    }
    
    Fallback "Transparent/Diffuse"
}