Shader "Custom/CoreEnergyEffect" {
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (0.2, 0.8, 1.0, 1.0)
        
        [Header(Core Energy)]
        _CoreSize ("Core Size", Range(0.1, 2.0)) = 0.5
        _CoreIntensity ("Core Intensity", Range(0, 10)) = 3.0
        _CoreColor ("Core Color", Color) = (1, 1, 1, 1)
        _CorePulseSpeed ("Core Pulse Speed", Range(0, 10)) = 2.0
        _CoreNoiseScale ("Core Noise Scale", Range(0.1, 5)) = 1.0
        
        [Header(Energy Rings)]
        _RingCount ("Ring Count", Range(1, 10)) = 3
        _RingSpeed ("Ring Speed", Range(-5, 5)) = 1.0
        _RingThickness ("Ring Thickness", Range(0.01, 0.3)) = 0.05
        _RingIntensity ("Ring Intensity", Range(0, 5)) = 2.0
        _RingGlow ("Ring Glow", Range(0, 1)) = 0.3
        
        [Header(Electric Arcs)]
        _ArcIntensity ("Arc Intensity", Range(0, 2)) = 1.0
        _ArcSpeed ("Arc Speed", Range(0, 20)) = 5.0
        _ArcScale ("Arc Scale", Range(1, 50)) = 20.0
        _ArcColor ("Arc Color", Color) = (0.5, 0.8, 1.0, 1.0)
        _ArcFrequency ("Arc Frequency", Range(0, 1)) = 0.3
        
        [Header(Energy Flux)]
        _FluxAmount ("Flux Amount", Range(0, 1)) = 0.5
        _FluxSpeed ("Flux Speed", Range(0, 10)) = 3.0
        _FluxScale ("Flux Scale", Range(0.1, 5)) = 2.0
        _FluxColor ("Flux Color", Color) = (0.3, 0.6, 1.0, 1.0)
        
        [Header(Fresnel Effect)]
        _FresnelPower ("Fresnel Power", Range(0, 10)) = 3.0
        _FresnelIntensity ("Fresnel Intensity", Range(0, 5)) = 2.0
        _FresnelColor ("Fresnel Color", Color) = (0, 0.5, 1.0, 1.0)
        
        [Header(Distortion)]
        _DistortionAmount ("Distortion Amount", Range(0, 0.2)) = 0.05
        _DistortionSpeed ("Distortion Speed", Range(0, 5)) = 1.0
        _DistortionScale ("Distortion Scale", Range(0.1, 10)) = 1.0
        
        [Header(Overall Effects)]
        _EmissionBoost ("Emission Boost", Range(0, 10)) = 2.0
        _Transparency ("Transparency", Range(0, 1)) = 0.8
        _TimeScale ("Time Scale", Range(0, 5)) = 1.0
    }
    
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            
            fixed4 _Color;
            float _CoreSize;
            float _CoreIntensity;
            fixed4 _CoreColor;
            float _CorePulseSpeed;
            float _CoreNoiseScale;
            
            float _RingCount;
            float _RingSpeed;
            float _RingThickness;
            float _RingIntensity;
            float _RingGlow;
            
            float _ArcIntensity;
            float _ArcSpeed;
            float _ArcScale;
            fixed4 _ArcColor;
            float _ArcFrequency;
            
            float _FluxAmount;
            float _FluxSpeed;
            float _FluxScale;
            fixed4 _FluxColor;
            
            float _FresnelPower;
            float _FresnelIntensity;
            fixed4 _FresnelColor;
            
            float _DistortionAmount;
            float _DistortionSpeed;
            float _DistortionScale;
            
            float _EmissionBoost;
            float _Transparency;
            float _TimeScale;
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : NORMAL;
                float3 viewDir : TEXCOORD2;
                float4 vertexColor : COLOR;
                float4 screenPos : TEXCOORD3;
            };
            
            // 고급 노이즈 함수들
            float hash(float2 p) {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }
            
            float noise(float2 p) {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                return lerp(lerp(hash(i + float2(0, 0)), hash(i + float2(1, 0)), f.x),
                           lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), f.x), f.y);
            }
            
            // 프랙탈 노이즈
            float fbm(float2 p) {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                for (int i = 0; i < 4; i++) {
                    value += amplitude * noise(p * frequency);
                    amplitude *= 0.5;
                    frequency *= 2.0;
                }
                return value;
            }
            
            // 전기 아크 생성
            float electricArc(float2 uv, float time) {
                float2 center = float2(0.5, 0.5);
                float2 dir = uv - center;
                float dist = length(dir);
                float angle = atan2(dir.y, dir.x);
                
                // 여러 아크 생성
                float arc = 0.0;
                for (int i = 0; i < 3; i++) {
                    float arcAngle = angle + sin(time * _ArcSpeed + i * 2.0) * 0.5;
                    float arcNoise = fbm(float2(arcAngle * _ArcScale, time * _ArcSpeed + i));
                    
                    // 아크의 두께와 강도
                    float arcWidth = 0.02 + arcNoise * 0.01;
                    float arcMask = smoothstep(arcWidth, 0.0, abs(sin(arcAngle * 8.0 + time * _ArcSpeed)));
                    
                    // 거리에 따른 아크 페이드
                    float distMask = smoothstep(0.8, 0.1, dist);
                    
                    arc += arcMask * distMask * arcNoise;
                }
                
                return saturate(arc * _ArcIntensity);
            }
            
            // 에너지 링 생성
            float energyRings(float2 uv, float time) {
                float2 center = float2(0.5, 0.5);
                float dist = length(uv - center);
                
                float rings = 0.0;
                for (int i = 1; i <= _RingCount; i++) {
                    float ringRadius = (float(i) / _RingCount) * 0.5;
                    float ringTime = time * _RingSpeed + i * 0.5;
                    
                    // 회전하는 링
                    float ringOffset = sin(ringTime) * 0.1;
                    float ringDist = abs(dist - (ringRadius + ringOffset));
                    
                    // 링의 두께와 글로우
                    float ring = smoothstep(_RingThickness + _RingGlow, _RingThickness, ringDist);
                    ring += smoothstep(_RingThickness, 0.0, ringDist) * 2.0;
                    
                    // 펄스 효과
                    float pulse = sin(ringTime * 2.0) * 0.5 + 0.5;
                    ring *= pulse;
                    
                    rings += ring;
                }
                
                return saturate(rings * _RingIntensity);
            }
            
            // 에너지 플럭스 생성
            float3 energyFlux(float2 uv, float time) {
                float2 fluxUV = uv * _FluxScale;
                fluxUV += float2(sin(time * _FluxSpeed), cos(time * _FluxSpeed * 0.7)) * 0.1;
                
                float flux1 = fbm(fluxUV + time * _FluxSpeed * 0.1);
                float flux2 = fbm(fluxUV * 2.0 + time * _FluxSpeed * 0.15);
                float flux3 = fbm(fluxUV * 4.0 - time * _FluxSpeed * 0.08);
                
                float3 fluxColor = _FluxColor.rgb;
                fluxColor *= (flux1 + flux2 * 0.5 + flux3 * 0.25) * _FluxAmount;
                
                return fluxColor;
            }
            
            // 코어 에너지 생성
            float coreEnergy(float2 uv, float time) {
                float2 center = float2(0.5, 0.5);
                float dist = length(uv - center);
                
                // 코어 크기와 펄스
                float corePulse = sin(time * _CorePulseSpeed) * 0.3 + 0.7;
                float coreRadius = _CoreSize * corePulse;
                
                // 코어 노이즈
                float2 coreNoiseUV = uv * _CoreNoiseScale + time * 0.1;
                float coreNoise = fbm(coreNoiseUV) * 0.2 + 0.8;
                
                // 코어 마스크
                float core = smoothstep(coreRadius + 0.1, coreRadius - 0.1, dist);
                core *= coreNoise;
                
                return core * _CoreIntensity;
            }
            
            v2f vert(appdata v) {
                v2f o;
                
                float time = _Time.y * _TimeScale;
                
                // 왜곡 효과
                float3 localPos = v.vertex.xyz;
                if (_DistortionAmount > 0) {
                    float2 distortUV = v.uv * _DistortionScale + time * _DistortionSpeed;
                    float3 distortion = float3(
                        fbm(distortUV) - 0.5,
                        fbm(distortUV + 1.7) - 0.5,
                        fbm(distortUV + 3.4) - 0.5
                    ) * _DistortionAmount;
                    
                    localPos += distortion;
                }
                
                o.vertex = UnityObjectToClipPos(float4(localPos, v.vertex.w));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, float4(localPos, 1.0)).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                o.vertexColor = v.color;
                o.screenPos = ComputeScreenPos(o.vertex);
                
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                float time = _Time.y * _TimeScale;
                
                // 기본 텍스처
                fixed4 mainCol = tex2D(_MainTex, i.uv) * _Color * i.vertexColor;
                
                // 프레넬 효과
                float fresnel = 1.0 - saturate(dot(i.worldNormal, i.viewDir));
                fresnel = pow(fresnel, _FresnelPower) * _FresnelIntensity;
                
                // 코어 에너지
                float core = coreEnergy(i.uv, time);
                
                // 에너지 링
                float rings = energyRings(i.uv, time);
                
                // 전기 아크
                float arcs = 0.0;
                if (_ArcIntensity > 0) {
                    // 랜덤한 시간에 아크 발생
                    float arcTrigger = step(_ArcFrequency, hash(floor(time * 10.0)));
                    arcs = electricArc(i.uv, time) * arcTrigger;
                }
                
                // 에너지 플럭스
                float3 flux = energyFlux(i.uv, time);
                
                // 색상 조합
                float3 finalColor = mainCol.rgb;
                
                // 코어 색상 추가
                finalColor += _CoreColor.rgb * core;
                
                // 링 색상 추가 (코어 색상과 블렌딩)
                finalColor += lerp(_Color.rgb, _CoreColor.rgb, 0.5) * rings;
                
                // 아크 색상 추가
                finalColor += _ArcColor.rgb * arcs * 2.0;
                
                // 플럭스 색상 추가
                finalColor += flux;
                
                // 프레넬 글로우 추가
                finalColor += _FresnelColor.rgb * fresnel;
                
                // 전체 발광 강화
                finalColor *= _EmissionBoost;
                
                // 알파 계산
                float alpha = mainCol.a;
                alpha += (core + rings + arcs) * 0.5;
                alpha += fresnel * 0.3;
                alpha = saturate(alpha) * _Transparency;
                
                return fixed4(finalColor, alpha);
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
} 