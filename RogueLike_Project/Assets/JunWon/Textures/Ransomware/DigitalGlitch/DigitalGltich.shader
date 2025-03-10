Shader "Custom/DigitalGlitch"
{
    Properties
    {
        _BaseMap("Base Map (RGB)", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        
        [Space(10)]
        [Header(Glitch Settings)]
        _GlitchIntensity("Glitch Intensity", Range(0, 1)) = 0.5
        _BlockSize("Block Size", Range(0.001, 0.1)) = 0.01
        _RGBSplitIntensity("RGB Split", Range(0, 0.1)) = 0.02
        _NoiseScale("Noise Scale", Range(0.1, 100)) = 10
        _GlitchSpeed("Glitch Speed", Range(0, 10)) = 1
        
        [Space(10)]
        [Header(Material Properties)]
        _Metallic("Metallic", Range(0, 1)) = 0
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
        [Toggle(_NORMALMAP)] _NormalMapToggle("Use Normal Map", Float) = 0
        [NoScaleOffset]_BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            
            #pragma shader_feature_local _NORMALMAP
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
                #ifdef _NORMALMAP
                float4 tangentOS    : TANGENT;
                #endif
            };
            
            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                #ifdef _NORMALMAP
                float4 tangentWS    : TEXCOORD2;
                float3 bitangentWS  : TEXCOORD3;
                #endif
                float3 positionWS   : TEXCOORD4;
            };
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            #ifdef _NORMALMAP
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            #endif
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _GlitchIntensity;
                float _BlockSize;
                float _RGBSplitIntensity;
                float _NoiseScale;
                float _GlitchSpeed;
                float _Metallic;
                float _Smoothness;
                float _BumpScale;
            CBUFFER_END
            
            // 랜덤 함수
            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            // 노이즈 함수
            float noise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                
                float a = rand(i);
                float b = rand(i + float2(1.0, 0.0));
                float c = rand(i + float2(0.0, 1.0));
                float d = rand(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // 글리치 버텍스 변형 (약간의 지터 효과)
                float3 positionOS = input.positionOS.xyz;
                
                if (_GlitchIntensity > 0)
                {
                    // 시간에 따라 변화하는 씨드
                    float timeSeed = _Time.y * _GlitchSpeed;
                    
                    // 글리치 효과 강도에 따라 버텍스 위치 변형
                    // 특정 버텍스만 랜덤하게 변형
                    if (rand(input.positionOS.xy + timeSeed) < _GlitchIntensity * 0.1)
                    {
                        float glitchAmount = (rand(input.positionOS.zx + timeSeed) * 2 - 1) * 0.02 * _GlitchIntensity;
                        positionOS.xyz += input.normalOS * glitchAmount;
                    }
                }
                
                // 변형된 위치로 계산
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionHCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                
                #ifdef _NORMALMAP
                output.tangentWS = float4(normalInput.tangentWS, input.tangentOS.w);
                output.bitangentWS = normalInput.bitangentWS;
                #endif
                
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // 시간 기반 변수
                float time = _Time.y * _GlitchSpeed;
                float2 uv = input.uv;
                
                // 글리치 효과 적용
                if (_GlitchIntensity > 0)
                {
                    // 블록 기반 글리치 계산
                    float2 blockPos = floor(uv / _BlockSize) * _BlockSize;
                    float blockRand = rand(blockPos + floor(time));
                    
                    // 글리치 효과 계산
                    float2 distortion = 0;
                    
                    // 수평 글리치 (랜덤한 블록)
                    if (blockRand < _GlitchIntensity * 0.3)
                    {
                        distortion.x = (rand(blockPos + time) - 0.5) * 0.05 * _GlitchIntensity;
                    }
                    
                    // 픽셀화 효과 (랜덤한 블록)
                    if (blockRand < _GlitchIntensity * 0.2)
                    {
                        float2 pixelSize = _BlockSize * 5;
                        uv = floor(uv / pixelSize) * pixelSize;
                    }
                    
                    // 노이즈 기반 왜곡
                    float noiseValue = noise(uv * _NoiseScale + time);
                    if (noiseValue < _GlitchIntensity * 0.4)
                    {
                        distortion.y += (noiseValue - 0.5) * 0.01 * _GlitchIntensity;
                    }
                    
                    // 최종 UV 왜곡 적용
                    uv += distortion;
                }
                
                // RGB 분리 효과
                float2 rOffset = float2(_RGBSplitIntensity * _GlitchIntensity, 0);
                float2 gOffset = float2(0, 0);
                float2 bOffset = float2(-_RGBSplitIntensity * _GlitchIntensity, 0);
                
                half4 baseMap = half4(
                    SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv + rOffset).r,
                    SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv + gOffset).g,
                    SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv + bOffset).b,
                    SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).a
                );
                
                // 기본 색상 정보
                half4 color = baseMap * _BaseColor;
                
                // 가끔 색상 반전
                if (rand(floor(uv * 10) + time) < _GlitchIntensity * 0.05)
                {
                    color.rgb = 1 - color.rgb;
                }
                
                // 노말맵 샘플링
                #ifdef _NORMALMAP
                half4 normalMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv);
                half3 normalTS = UnpackNormalScale(normalMap, _BumpScale);
                half3 normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, input.bitangentWS, input.normalWS));
                #else
                half3 normalWS = normalize(input.normalWS);
                #endif
                
                // 라이팅 계산
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = input.positionWS;
                lightingInput.normalWS = NormalizeNormalPerPixel(normalWS);
                lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                
                // 메테리얼 속성 설정
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = color.rgb;
                surfaceData.alpha = color.a;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                
                // PBR 라이팅 계산
                half4 finalColor = UniversalFragmentPBR(lightingInput, surfaceData);
                
                return finalColor;
            }
            ENDHLSL
        }
        
        // 그림자 캐스팅 패스
        Pass
        {
            Name "ShadowCaster"
            Tags {"LightMode" = "ShadowCaster"}
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    
    // 폴백 셰이더 (비URP 환경용)
    FallBack "Universal Render Pipeline/Lit"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.LitShader"
}