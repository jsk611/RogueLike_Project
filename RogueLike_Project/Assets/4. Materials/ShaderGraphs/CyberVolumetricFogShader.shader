Shader "Custom/CyberVolumetricFogShader"
{
    Properties
    {
        _MainTex ("Fog Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _FogColor ("Fog Color", Color) = (0.5, 0.8, 1.0, 0.8)
        _EmissionColor ("Emission Color", Color) = (0.2, 0.4, 0.8, 1.0)
        
        [Header(Fog Properties)]
        _Density ("Fog Density", Range(0.0, 3.0)) = 1.0
        _FogDepth ("Fog Depth", Range(0.1, 10.0)) = 2.0
        _Softness ("Edge Softness", Range(0.1, 5.0)) = 1.0
        _BaseFogIntensity ("Base Fog Intensity", Range(0.0, 2.0)) = 0.6
        
        [Header(Movement)]
        _ScrollSpeed ("Scroll Speed", Vector) = (0.1, 0.05, 0.02, 0)
        _NoiseScale ("Noise Scale", Range(0.1, 10.0)) = 2.0
        _NoiseStrength ("Noise Strength", Range(0.0, 2.0)) = 0.5
        
        [Header(Cyber Effects)]
        _PulseSpeed ("Pulse Speed", Range(0.1, 10.0)) = 2.0
        _PulseIntensity ("Pulse Intensity", Range(0.0, 1.0)) = 0.2
        _FlickerSpeed ("Flicker Speed", Range(1.0, 20.0)) = 8.0
        _FlickerIntensity ("Flicker Intensity", Range(0.0, 0.5)) = 0.05
        
        [Header(Volumetric Settings)]
        _FresnelPower ("Fresnel Power", Range(0.1, 5.0)) = 1.5
        _RimIntensity ("Rim Intensity", Range(0.0, 2.0)) = 0.8
        _InternalGlow ("Internal Glow", Range(0.0, 2.0)) = 1.2
        _OuterVisibility ("Outer Visibility", Range(0.0, 2.0)) = 1.0
        
        [Header(Transparency)]
        _Alpha ("Base Alpha", Range(0.0, 1.0)) = 0.9
        _FadeDistance ("Fade Distance", Range(0.1, 100.0)) = 25.0
        _MinAlpha ("Minimum Alpha", Range(0.0, 1.0)) = 0.3
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }
        
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest LEqual
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _NoiseTex_ST;
            
            fixed4 _FogColor;
            fixed4 _EmissionColor;
            
            float _Density;
            float _FogDepth;
            float _Softness;
            float _BaseFogIntensity;
            
            float4 _ScrollSpeed;
            float _NoiseScale;
            float _NoiseStrength;
            
            float _PulseSpeed;
            float _PulseIntensity;
            float _FlickerSpeed;
            float _FlickerIntensity;
            
            float _FresnelPower;
            float _RimIntensity;
            float _InternalGlow;
            float _OuterVisibility;
            
            float _Alpha;
            float _FadeDistance;
            float _MinAlpha;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                float3 viewDir : TEXCOORD4;
                float4 color : COLOR;
                float depth : TEXCOORD5;
                UNITY_FOG_COORDS(6)
            };
            
            // 노이즈 함수
            float noise(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            // 부드러운 노이즈
            float smoothNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = noise(i);
                float b = noise(i + float2(1.0, 0.0));
                float c = noise(i + float2(0.0, 1.0));
                float d = noise(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            
            // 프랙탈 노이즈
            float fractalNoise(float2 uv, int octaves)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                for (int i = 0; i < octaves; i++)
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
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.noiseUV = TRANSFORM_TEX(v.uv, _NoiseTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                o.color = v.color;
                o.depth = length(_WorldSpaceCameraPos - o.worldPos);
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y;
                
                // UV 애니메이션 (안개 움직임)
                float2 animatedUV = i.uv + _ScrollSpeed.xy * time;
                float2 animatedNoiseUV = i.noiseUV * _NoiseScale + _ScrollSpeed.zw * time;
                
                // 기본 텍스처 샘플링
                fixed4 fogTex = tex2D(_MainTex, animatedUV);
                
                // 볼류메트릭 노이즈 생성
                float noiseValue = fractalNoise(animatedNoiseUV, 3);
                float secondaryNoise = fractalNoise(animatedNoiseUV * 2.5 - time * 0.1, 2);
                float volumetricNoise = lerp(noiseValue, secondaryNoise, 0.5) * _NoiseStrength;
                
                // 펄스 효과 (사이버 테마) - 완화됨
                float pulse = sin(time * _PulseSpeed) * _PulseIntensity + 1.0;
                
                // 플리커 효과 (홀로그램 느낌) - 완화됨
                float flicker = 1.0 + sin(time * _FlickerSpeed) * _FlickerIntensity;
                flicker *= 1.0 + smoothNoise(float2(time * 13.7, time * 7.3)) * _FlickerIntensity * 0.5;
                
                // 프레넬 효과 (완화된 볼류메트릭 느낌)
                float NdotV = saturate(dot(normalize(i.worldNormal), i.viewDir));
                float fresnel = 1.0 - NdotV;
                fresnel = pow(fresnel, _FresnelPower);
                
                // 기본 안개 강도 (바깥쪽에서도 보이도록)
                float baseFog = _BaseFogIntensity + (1.0 - NdotV) * _OuterVisibility;
                
                // 림 라이팅 (완화됨)
                float rim = fresnel * _RimIntensity;
                
                // 내부 글로우
                float internalGlow = NdotV * _InternalGlow;
                
                // 거리 기반 페이드 (완화됨)
                float distanceFade = saturate(1.0 - (i.depth / _FadeDistance));
                distanceFade = max(distanceFade, 0.2); // 최소 가시성 보장
                
                // 안개 밀도 계산 (기본 가시성 향상)
                float density = _Density * fogTex.a * i.color.a;
                density *= (baseFog + volumetricNoise * 0.5); // 노이즈 영향 완화
                density *= pulse * flicker;
                density *= distanceFade;
                
                // 최종 색상 조합
                fixed3 finalColor = _FogColor.rgb * fogTex.rgb * i.color.rgb;
                
                // 기본 안개 색상 강화 (바깥쪽 가시성 향상)
                finalColor = lerp(finalColor, _FogColor.rgb, 0.3);
                
                // 림과 내부 글로우 추가
                finalColor += _EmissionColor.rgb * rim * 0.5; // 림 효과 완화
                finalColor += _FogColor.rgb * internalGlow * 0.7; // 내부 글로우 완화
                
                // 외부 가시성 향상을 위한 추가 색상
                finalColor += _FogColor.rgb * _OuterVisibility * 0.2;
                
                // 사이버 글리치 효과 (완화됨)
                float glitchNoise = smoothNoise(float2(time * 5.7, i.uv.y * 50.0));
                if (glitchNoise > 0.99) // 더 드물게
                {
                    finalColor += _EmissionColor.rgb * 0.3; // 강도 완화
                }
                
                // 최종 알파 계산 (기본 가시성 향상)
                float finalAlpha = saturate(density * _Alpha);
                finalAlpha = max(finalAlpha, _MinAlpha * distanceFade); // 최소 알파 보장
                finalAlpha = smoothstep(0.0, _Softness, finalAlpha);
                
                fixed4 col = fixed4(finalColor, finalAlpha);
                
                // Unity 안개 적용
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
} 