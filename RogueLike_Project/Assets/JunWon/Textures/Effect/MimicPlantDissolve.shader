Shader "Custom/MimicPlantDissolve" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        
        [Header(Dissolve)]
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _DissolveTex ("Dissolve Texture", 2D) = "white" {}
        _DissolveEdgeWidth ("Edge Width", Range(0, 0.3)) = 0.1
        _DissolveEdgeColor ("Edge Color", Color) = (1, 0.5, 0, 1)
        _DissolveEdgeEmission ("Edge Emission", Range(0, 10)) = 2
        
        [Header(Emergence Animation)]
        _EmergenceHeight ("Emergence Height", Float) = 2.0
        _EmergenceNoise ("Emergence Noise", 2D) = "white" {}
        _EmergenceSpeed ("Emergence Speed", Float) = 1.0
        _GroundLevel ("Ground Level", Float) = 0.0
        _WindIntensity ("Wind Intensity", Range(0, 1)) = 0.3
        _WindSpeed ("Wind Speed", Float) = 2.0
        
        [Header(Glitch Effects)]
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0
        _GlitchSpeed ("Glitch Speed", Float) = 10
        _GlitchTex ("Glitch Texture", 2D) = "black" {}
        _DigitalNoiseScale ("Digital Noise Scale", Range(1, 50)) = 10
        
        [Header(Hologram)]
        _HologramAlpha ("Hologram Alpha", Range(0, 1)) = 1
        _ScanlineFrequency ("Scanline Frequency", Float) = 50
        _ScanlineSpeed ("Scanline Speed", Float) = 2
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.5
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
            sampler2D _DissolveTex;
            sampler2D _EmergenceNoise;
            sampler2D _GlitchTex;
            float4 _MainTex_ST;
            float4 _DissolveTex_ST;
            
            fixed4 _Color;
            float _DissolveAmount;
            float _DissolveEdgeWidth;
            fixed4 _DissolveEdgeColor;
            float _DissolveEdgeEmission;
            
            float _EmergenceHeight;
            float _EmergenceSpeed;
            float _GroundLevel;
            float _WindIntensity;
            float _WindSpeed;
            
            float _GlitchIntensity;
            float _GlitchSpeed;
            float _DigitalNoiseScale;
            
            float _HologramAlpha;
            float _ScanlineFrequency;
            float _ScanlineSpeed;
            float _ScanlineIntensity;
            
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
                float3 normal : NORMAL;
                float4 screenPos : TEXCOORD2;
                float4 vertexColor : COLOR;
                float emergenceNoise : TEXCOORD3;
            };
            
            // 랜덤 함수
            float rand(float2 co) {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            // 3D 노이즈 함수
            float noise3D(float3 pos) {
                float3 p = floor(pos);
                float3 f = frac(pos);
                f = f * f * (3.0 - 2.0 * f);
                
                float n = p.x + p.y * 57.0 + 113.0 * p.z;
                return lerp(lerp(lerp(rand(n + float2(0, 0)), rand(n + float2(1, 0)), f.x),
                                lerp(rand(n + float2(0, 57)), rand(n + float2(1, 57)), f.x), f.y),
                           lerp(lerp(rand(n + float2(0, 113)), rand(n + float2(1, 113)), f.x),
                                lerp(rand(n + float2(0, 170)), rand(n + float2(1, 170)), f.x), f.y), f.z);
            }
            
            // 디지털 글리치 함수
            float3 digitalGlitch(float3 pos, float2 uv, float intensity) {
                float time = _Time.y * _GlitchSpeed;
                
                // 수직 글리치 스트라이프
                float stripeY = floor(uv.y * _DigitalNoiseScale);
                float glitchNoise = rand(float2(stripeY, floor(time * 5)));
                
                if (glitchNoise > 0.8) {
                    // 강한 글리치 - X축으로 오프셋
                    float glitchOffset = (glitchNoise - 0.8) * 5.0 * intensity;
                    pos.x += sin(time * 20 + stripeY) * glitchOffset * 0.1;
                    
                    // Y축 비틀림
                    pos.y += cos(time * 15 + pos.x) * intensity * 0.05;
                }
                
                // 작은 지터링 효과
                pos += (noise3D(pos * 50 + time) - 0.5) * intensity * 0.02;
                
                return pos;
            }
            
            v2f vert(appdata v) {
                v2f o;
                
                float3 localPos = v.vertex.xyz;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // 이머전스 노이즈 계산
                float2 noiseUV = v.uv + _Time.y * _EmergenceSpeed * 0.1;
                o.emergenceNoise = tex2Dlod(_EmergenceNoise, float4(noiseUV, 0, 0)).r;
                
                // 바람 효과 (식물의 자연스러운 흔들림)
                if (_WindIntensity > 0) {
                    float windTime = _Time.y * _WindSpeed;
                    float windNoise = sin(windTime + worldPos.x * 0.1) * cos(windTime * 1.5 + worldPos.z * 0.1);
                    
                    // 높이에 따라 바람 효과 증가
                    float heightFactor = saturate((worldPos.y - _GroundLevel) / _EmergenceHeight);
                    windNoise *= heightFactor * _WindIntensity;
                    
                    localPos.x += windNoise * 0.1;
                    localPos.z += sin(windTime * 0.8 + worldPos.x * 0.05) * heightFactor * _WindIntensity * 0.05;
                }
                
                // 글리치 효과로 버텍스 왜곡
                if (_GlitchIntensity > 0) {
                    localPos = digitalGlitch(localPos, v.uv, _GlitchIntensity);
                }
                
                // 버텍스 위치 업데이트
                float4 modifiedVertex = float4(localPos, v.vertex.w);
                
                o.vertex = UnityObjectToClipPos(modifiedVertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, modifiedVertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.vertexColor = v.color;
                
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                // 기본 텍스처
                fixed4 col = tex2D(_MainTex, i.uv) * _Color * i.vertexColor;
                
                // 디졸브 효과
                float dissolveNoise = tex2D(_DissolveTex, TRANSFORM_TEX(i.uv, _DissolveTex)).r;
                
                // 등장 효과 (월드 Y 좌표 기반)
                float emergenceHeight = i.worldPos.y - _GroundLevel;
                float emergenceFactor = saturate((emergenceHeight + i.emergenceNoise * _EmergenceHeight) / _EmergenceHeight);
                
                // 디졸브와 등장 효과 결합
                float combinedDissolve = lerp(dissolveNoise, emergenceFactor, _DissolveAmount);
                
                // 엣지 효과 계산
                float dissolveThreshold = _DissolveAmount;
                float edgeStart = dissolveThreshold - _DissolveEdgeWidth;
                float edgeEnd = dissolveThreshold;
                
                float edge = smoothstep(edgeStart, edgeEnd, combinedDissolve);
                float dissolveEdge = edge * (1 - step(dissolveThreshold, combinedDissolve));
                
                // 홀로그램 스캔라인 효과
                float2 screenUV = i.screenPos.xy / max(i.screenPos.w, 0.0001);
                float scanline = sin(screenUV.y * _ScanlineFrequency + _Time.y * _ScanlineSpeed);
                float hologramEffect = saturate(scanline * 0.5 + 0.5);
                
                // 디지털 노이즈 패턴
                float digitalPattern = 0;
                if (_GlitchIntensity > 0) {
                    float2 digitalUV = i.uv * _DigitalNoiseScale;
                    digitalPattern = step(0.5, frac(digitalUV.x)) * step(0.5, frac(digitalUV.y));
                    digitalPattern *= sin(_Time.y * _GlitchSpeed * 2) * 0.5 + 0.5;
                }
                
                // 글리치 효과
                float3 glitchColor = col.rgb;
                if (_GlitchIntensity > 0) {
                    float glitchEffect = tex2D(_GlitchTex, i.uv + _Time.y * _GlitchSpeed * 0.1).r;
                    
                    // RGB 채널 분리 효과
                    float2 redOffset = float2(_GlitchIntensity * 0.01, 0);
                    float2 blueOffset = float2(-_GlitchIntensity * 0.01, 0);
                    
                    float redChannel = tex2D(_MainTex, i.uv + redOffset).r;
                    float blueChannel = tex2D(_MainTex, i.uv + blueOffset).b;
                    
                    glitchColor.r = lerp(glitchColor.r, redChannel, glitchEffect * _GlitchIntensity);
                    glitchColor.b = lerp(glitchColor.b, blueChannel, glitchEffect * _GlitchIntensity);
                    
                    // 디지털 패턴 오버레이
                    glitchColor = lerp(glitchColor, float3(0, 1, 1), digitalPattern * _GlitchIntensity * 0.3);
                }
                
                // 최종 색상 계산
                col.rgb = glitchColor;
                
                // 발광 효과 추가
                float3 emission = _DissolveEdgeColor.rgb * dissolveEdge * _DissolveEdgeEmission;
                
                // 스캔라인 발광 추가
                emission += float3(0, 0.5, 1) * hologramEffect * _ScanlineIntensity * _HologramAlpha;
                
                col.rgb += emission;
                
                // 알파 계산
                float finalAlpha = 1 - step(dissolveThreshold, combinedDissolve);
                
                // 홀로그램 효과로 알파 조절
                finalAlpha *= _HologramAlpha;
                finalAlpha *= lerp(1.0, hologramEffect, _ScanlineIntensity);
                
                // 글리치로 인한 투명도 변화
                if (_GlitchIntensity > 0) {
                    float glitchAlpha = tex2D(_GlitchTex, i.uv + _Time.y * _GlitchSpeed * 0.05).r;
                    finalAlpha *= lerp(1.0, glitchAlpha, _GlitchIntensity * 0.3);
                }
                
                col.a = finalAlpha * _Color.a;
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}