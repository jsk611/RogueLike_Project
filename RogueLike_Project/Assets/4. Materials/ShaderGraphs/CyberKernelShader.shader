Shader "Custom/CyberKernelShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _CoreColor ("Core Color", Color) = (1.0, 0.3, 0.0, 1.0)
        _EnergyColor ("Energy Color", Color) = (0.0, 0.8, 1.0, 1.0)
        _PulseColor ("Pulse Color", Color) = (1.0, 1.0, 0.2, 1.0)
        _ScanColor ("Scan Color", Color) = (0.2, 1.0, 0.8, 1.0)
        
        [Header(Core Settings)]
        _CoreSize ("Core Size", Range(0.1, 1.0)) = 0.3
        _CoreIntensity ("Core Intensity", Range(0.0, 5.0)) = 2.0
        _CorePulseSpeed ("Core Pulse Speed", Range(0.1, 10.0)) = 3.0
        _CoreGlow ("Core Glow", Range(0.0, 2.0)) = 1.0
        
        [Header(Energy Rings)]
        _RingCount ("Ring Count", Range(1, 8)) = 4
        _RingSpeed ("Ring Speed", Range(0.1, 20.0)) = 5.0
        _RingThickness ("Ring Thickness", Range(0.01, 0.2)) = 0.05
        _RingIntensity ("Ring Intensity", Range(0.0, 3.0)) = 1.5
        
        [Header(Data Pulse)]
        _PulseSpeed ("Pulse Speed", Range(0.1, 15.0)) = 8.0
        _PulseWidth ("Pulse Width", Range(0.01, 0.3)) = 0.1
        _PulseIntensity ("Pulse Intensity", Range(0.0, 3.0)) = 2.0
        _PulseFrequency ("Pulse Frequency", Range(0.1, 5.0)) = 2.0
        
        [Header(System Scan)]
        _ScanSpeed ("Scan Speed", Range(0.1, 25.0)) = 10.0
        _ScanWidth ("Scan Width", Range(0.01, 0.3)) = 0.08
        _ScanIntensity ("Scan Intensity", Range(0.0, 4.0)) = 2.5
        _ScanPattern ("Scan Pattern", Range(1, 6)) = 3
        
        [Header(Energy Field)]
        _FieldScale ("Field Scale", Range(1.0, 50.0)) = 15.0
        _FieldSpeed ("Field Speed", Range(0.1, 10.0)) = 4.0
        _FieldIntensity ("Field Intensity", Range(0.0, 2.0)) = 0.8
        
        [Header(Activation Level)]
        _ActivationLevel ("Activation Level", Range(0.0, 1.0)) = 1.0
        _PowerSurge ("Power Surge", Range(0.0, 2.0)) = 1.2
        
        [Header(Transparency)]
        _Alpha ("Alpha", Range(0.0, 1.0)) = 0.8
        _Fresnel ("Fresnel Power", Range(0.1, 5.0)) = 2.5
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
        }
        
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _CoreColor;
            fixed4 _EnergyColor;
            fixed4 _PulseColor;
            fixed4 _ScanColor;
            
            float _CoreSize;
            float _CoreIntensity;
            float _CorePulseSpeed;
            float _CoreGlow;
            
            float _RingCount;
            float _RingSpeed;
            float _RingThickness;
            float _RingIntensity;
            
            float _PulseSpeed;
            float _PulseWidth;
            float _PulseIntensity;
            float _PulseFrequency;
            
            float _ScanSpeed;
            float _ScanWidth;
            float _ScanIntensity;
            float _ScanPattern;
            
            float _FieldScale;
            float _FieldSpeed;
            float _FieldIntensity;
            
            float _ActivationLevel;
            float _PowerSurge;
            
            float _Alpha;
            float _Fresnel;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 normal : NORMAL;
                float3 viewDir : TEXCOORD2;
            };
            
            // 랜덤 함수
            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            // 2D 노이즈
            float noise2D(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);
                
                float a = rand(i);
                float b = rand(i + float2(1.0, 0.0));
                float c = rand(i + float2(0.0, 1.0));
                float d = rand(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }
            
            // 중심 코어 효과
            float coreEffect(float2 uv, float time)
            {
                float2 center = float2(0.5, 0.5);
                float distanceFromCenter = distance(uv, center);
                
                // 코어 펄스
                float pulse = 1.0 + _PowerSurge * sin(time * _CorePulseSpeed) * 0.3;
                float coreSize = _CoreSize * pulse * _ActivationLevel;
                
                // 코어 마스크
                float coreMask = 1.0 - smoothstep(0.0, coreSize, distanceFromCenter);
                
                // 코어 글로우
                float glowMask = 1.0 - smoothstep(0.0, coreSize + _CoreGlow, distanceFromCenter);
                glowMask -= coreMask;
                
                return (coreMask + glowMask * 0.5) * _CoreIntensity * _ActivationLevel;
            }
            
            // 에너지 링 효과
            float energyRings(float2 uv, float time)
            {
                float2 center = float2(0.5, 0.5);
                float distanceFromCenter = distance(uv, center);
                
                float rings = 0.0;
                
                // 여러 개의 에너지 링 생성
                for(int i = 1; i <= 4; i++)
                {
                    float ringRadius = float(i) * 0.15 + time * _RingSpeed * 0.01;
                    ringRadius = frac(ringRadius);
                    
                    // 링 마스크
                    float ringMask = 1.0 - smoothstep(0.0, _RingThickness, abs(distanceFromCenter - ringRadius));
                    
                    // 링 강도 (시간에 따라 변화)
                    float ringIntensity = sin(time * _RingSpeed + float(i) * 2.0) * 0.5 + 0.5;
                    
                    rings += ringMask * ringIntensity;
                }
                
                return rings * _RingIntensity * _ActivationLevel;
            }
            
            // 데이터 펄스 효과
            float dataPulse(float2 uv, float time)
            {
                float2 center = float2(0.5, 0.5);
                float distanceFromCenter = distance(uv, center);
                
                // 방사형 펄스
                float pulse = sin((distanceFromCenter - time * _PulseSpeed * 0.1) * 20.0);
                pulse = smoothstep(1.0 - _PulseWidth, 1.0, pulse);
                
                // 펄스 주파수
                float frequency = sin(time * _PulseFrequency) * 0.5 + 0.5;
                
                return pulse * frequency * _PulseIntensity * _ActivationLevel;
            }
            
            // 시스템 스캔 효과
            float systemScan(float2 uv, float time)
            {
                float scan = 0.0;
                
                // 패턴에 따른 스캔 방식
                if (_ScanPattern <= 2)
                {
                    // 수평 스캔
                    scan = sin((uv.y + time * _ScanSpeed * 0.1) * 10.0);
                }
                else if (_ScanPattern <= 4)
                {
                    // 수직 스캔
                    scan = sin((uv.x + time * _ScanSpeed * 0.1) * 10.0);
                }
                else
                {
                    // 원형 스캔
                    float2 center = float2(0.5, 0.5);
                    float angle = atan2(uv.y - center.y, uv.x - center.x);
                    scan = sin(angle * 3.0 + time * _ScanSpeed * 0.2);
                }
                
                scan = smoothstep(1.0 - _ScanWidth, 1.0, scan);
                
                return scan * _ScanIntensity * _ActivationLevel;
            }
            
            // 에너지 필드 효과
            float energyField(float2 uv, float time)
            {
                float2 scaledUV = uv * _FieldScale;
                
                // 다층 노이즈 필드
                float field1 = noise2D(scaledUV + time * _FieldSpeed * 0.1);
                float field2 = noise2D(scaledUV * 2.0 - time * _FieldSpeed * 0.05);
                float field3 = noise2D(scaledUV * 0.5 + time * _FieldSpeed * 0.15);
                
                float field = (field1 + field2 * 0.5 + field3 * 0.3) / 1.8;
                
                return field * _FieldIntensity * _ActivationLevel;
            }
            
            // 전력 급상승 효과
            float powerSurge(float2 uv, float time)
            {
                // 불규칙한 전력 변동
                float surge = noise2D(float2(time * 3.0, 0.0)) * _PowerSurge;
                
                // 활성화 레벨에 따른 강도 조절
                surge *= _ActivationLevel;
                
                return surge * 0.3;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float time = _Time.y;
                float2 uv = i.uv;
                
                // 메인 텍스처
                fixed4 mainTex = tex2D(_MainTex, uv);
                
                // 각종 효과 계산
                float core = coreEffect(uv, time);
                float rings = energyRings(uv, time);
                float pulse = dataPulse(uv, time);
                float scan = systemScan(uv, time);
                float field = energyField(uv, time);
                float surge = powerSurge(uv, time);
                
                // 프레넬 효과 (커널 가장자리 글로우)
                float fresnel = 1.0 - saturate(dot(i.normal, i.viewDir));
                fresnel = pow(fresnel, _Fresnel);
                
                // 기본 색상은 코어 색상
                fixed4 finalColor = _CoreColor;
                
                // 중심 코어 적용
                finalColor.rgb = lerp(float3(0, 0, 0), finalColor.rgb, core);
                
                // 에너지 링 추가
                finalColor.rgb = lerp(finalColor.rgb, _EnergyColor.rgb, rings);
                
                // 데이터 펄스 추가
                finalColor.rgb = lerp(finalColor.rgb, _PulseColor.rgb, pulse);
                
                // 시스템 스캔 추가
                finalColor.rgb = lerp(finalColor.rgb, _ScanColor.rgb, scan);
                
                // 에너지 필드 추가
                finalColor.rgb += field * _EnergyColor.rgb * 0.3;
                
                // 전력 급상승 효과
                finalColor.rgb += surge * _PulseColor.rgb;
                
                // 프레넬 글로우 (커널 테두리 효과)
                finalColor.rgb += fresnel * _EnergyColor.rgb * 0.4;
                
                // 활성화 레벨에 따른 전체 강도 조절
                finalColor.rgb *= (0.5 + _ActivationLevel * 0.5);
                
                // 메인 텍스처와 블렌딩
                finalColor.rgb *= mainTex.rgb;
                
                // 알파 계산
                float finalAlpha = _Alpha;
                
                // 코어에서 더 진하게
                finalAlpha *= (0.2 + core * 0.8);
                
                // 에너지 링에서 더 진하게
                finalAlpha += rings * 0.3;
                
                // 펄스 효과에서 더 진하게
                finalAlpha += pulse * 0.2;
                
                // 스캔 효과에서 더 진하게
                finalAlpha += scan * 0.2;
                
                // 프레넬 효과로 가장자리 강화
                finalAlpha += fresnel * 0.4;
                
                // 활성화 레벨 적용
                finalAlpha *= _ActivationLevel;
                
                // 메인 텍스처 알파 적용
                finalAlpha *= mainTex.a;
                
                finalColor.a = saturate(finalAlpha);
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/VertexLit"
} 