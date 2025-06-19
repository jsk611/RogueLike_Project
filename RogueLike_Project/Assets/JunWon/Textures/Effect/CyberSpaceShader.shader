Shader "Custom/CyberSpaceShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CyberColor ("Cyber Color", Color) = (0.2, 1, 0.8, 0.8)
        _GlitchColor ("Glitch Color", Color) = (1, 0.3, 0.8, 0.6)
        _GlitchSpeed ("Glitch Speed", Range(0, 5)) = 2.0
        _WaveSpeed ("Wave Speed", Range(0, 3)) = 1.5
        _Alpha ("Alpha", Range(0, 1)) = 0.8
        _EmissionPower ("Emission Power", Range(0, 5)) = 2.0
        _NoiseScale ("Noise Scale", Range(1, 20)) = 10.0
        _DistortionAmount ("Distortion Amount", Range(0, 0.5)) = 0.1
        _FresnelPower ("Fresnel Power", Range(0, 10)) = 2.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        
        LOD 100
        
        Pass
        {
            Name "CyberSpacePass"
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _CyberColor;
                float4 _GlitchColor;
                float _GlitchSpeed;
                float _WaveSpeed;
                float _Alpha;
                float _EmissionPower;
                float _NoiseScale;
                float _DistortionAmount;
                float _FresnelPower;
            CBUFFER_END
            
            // 간단한 노이즈 함수
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            // 2D 노이즈 함수
            float noise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);
                
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(a, b, u.x) + 
                       (c - a) * u.y * (1.0 - u.x) + 
                       (d - b) * u.x * u.y;
            }
            
            // FBM (Fractal Brownian Motion) 노이즈
            float fbm(float2 st)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 0.0;
                
                for (int i = 0; i < 4; i++)
                {
                    value += amplitude * noise(st);
                    st *= 2.0;
                    amplitude *= 0.5;
                }
                return value;
            }
            
            // 글리치 효과 함수
            float2 glitchEffect(float2 uv, float time)
            {
                float glitchNoise = noise(uv * _NoiseScale + time * _GlitchSpeed);
                float glitchWave = sin(glitchNoise * 10.0) * _DistortionAmount;
                
                return uv + float2(glitchWave, glitchWave * 0.5);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                
                output.positionHCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y;
                
                // UV 애니메이션
                float2 animatedUV = input.uv;
                animatedUV.y += time * _WaveSpeed * 0.1;
                
                // 글리치 효과 적용
                float2 glitchUV = glitchEffect(animatedUV, time);
                
                // 노이즈 생성
                float noiseValue = fbm(glitchUV * _NoiseScale);
                float animatedNoise = fbm(glitchUV * _NoiseScale + time * _WaveSpeed);
                
                // 사이버 웨이브 생성 (Y축 기반)
                float cyberWave = sin(input.positionWS.y * 2.0 + time * _WaveSpeed * 2.0) * 0.5 + 0.5;
                
                // Fresnel 효과
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);
                
                // 색상 블렌딩
                float4 baseColor = lerp(_CyberColor, _GlitchColor, fresnel);
                baseColor = lerp(baseColor, _GlitchColor, cyberWave * 0.5);
                
                // 노이즈로 색상 변조
                baseColor.rgb *= (noiseValue * 0.5 + 0.5);
                baseColor.rgb += animatedNoise * 0.3;
                
                // 발광 효과
                float3 emission = baseColor.rgb * _EmissionPower;
                
                // 글리치 스파크 효과
                float sparkNoise = noise(input.uv * 50.0 + time * _GlitchSpeed * 5.0);
                if (sparkNoise > 0.95)
                {
                    emission += _GlitchColor.rgb * 3.0;
                }
                
                // 에너지 리플 효과
                float dist = distance(input.uv, float2(0.5, 0.5));
                float ripple = sin(dist * 20.0 - time * _WaveSpeed * 5.0) * 0.5 + 0.5;
                emission += ripple * _CyberColor.rgb * 0.5;
                
                // 알파 계산
                float alpha = pow(noiseValue, 2.0) * _Alpha;
                alpha *= fresnel * 1.5; // Fresnel로 가장자리 강조
                alpha += cyberWave * 0.3; // 웨이브 효과 추가
                
                // 최종 색상
                float4 finalColor = float4(baseColor.rgb + emission, alpha);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    
    FallBack "Sprites/Default"
} 