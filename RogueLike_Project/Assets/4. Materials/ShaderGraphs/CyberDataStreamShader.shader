Shader "Custom/CyberDataStreamShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _DataColor ("Data Color", Color) = (0.2, 1.0, 0.8, 1.0)
        _StreamColor ("Stream Color", Color) = (0.0, 0.8, 1.0, 1.0)
        _GlitchColor ("Glitch Color", Color) = (1.0, 0.3, 0.8, 1.0)
        
        [Header(Data Stream Settings)]
        _StreamSpeed ("Stream Speed", Range(0.1, 10.0)) = 2.0
        _StreamIntensity ("Stream Intensity", Range(0.0, 2.0)) = 1.0
        _StreamWidth ("Stream Width", Range(0.01, 0.5)) = 0.1
        
        [Header(Glitch Settings)]
        _GlitchSpeed ("Glitch Speed", Range(0.1, 20.0)) = 5.0
        _GlitchIntensity ("Glitch Intensity", Range(0.0, 1.0)) = 0.3
        
        [Header(Binary Pattern)]
        _BinaryScale ("Binary Scale", Range(1.0, 50.0)) = 20.0
        _BinarySpeed ("Binary Speed", Range(0.1, 5.0)) = 1.0
        _BinaryIntensity ("Binary Intensity", Range(0.0, 1.0)) = 0.5
        
        [Header(Transparency)]
        _Alpha ("Alpha", Range(0.0, 1.0)) = 0.8
        _Fresnel ("Fresnel Power", Range(0.1, 5.0)) = 2.0
        
        [Header(Scanning Effect)]
        _ScanlineSpeed ("Scanline Speed", Range(0.1, 10.0)) = 3.0
        _ScanlineIntensity ("Scanline Intensity", Range(0.0, 2.0)) = 1.0
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
            fixed4 _DataColor;
            fixed4 _StreamColor;
            fixed4 _GlitchColor;
            
            float _StreamSpeed;
            float _StreamIntensity;
            float _StreamWidth;
            
            float _GlitchSpeed;
            float _GlitchIntensity;
            
            float _BinaryScale;
            float _BinarySpeed;
            float _BinaryIntensity;
            
            float _Alpha;
            float _Fresnel;
            
            float _ScanlineSpeed;
            float _ScanlineIntensity;
            
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
            
            // 간단한 랜덤 함수
            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            // 글리치 효과
            float glitchEffect(float2 uv, float time)
            {
                float glitch = 0.0;
                
                // 수평 글리치 라인
                float lineY = floor(uv.y * 20) / 20;
                float lineNoise = rand(float2(lineY, floor(time * _GlitchSpeed)));
                
                if (lineNoise > 0.85)
                {
                    glitch += 0.5 * _GlitchIntensity;
                }
                
                // 픽셀 노이즈
                float pixelNoise = rand(uv * 50.0 + time * _GlitchSpeed);
                if (pixelNoise > 0.95)
                {
                    glitch += 0.3 * _GlitchIntensity;
                }
                
                return saturate(glitch);
            }
            
            // 바이너리 패턴
            float binaryPattern(float2 uv, float time)
            {
                float2 scaledUV = uv * _BinaryScale;
                float2 gridUV = frac(scaledUV);
                float2 gridID = floor(scaledUV);
                
                // 0과 1 생성
                float binary = step(0.5, rand(gridID + floor(time * _BinarySpeed)));
                
                // 디지트 모양 마스크
                float digitMask = step(0.2, gridUV.x) * step(gridUV.x, 0.8) * 
                                 step(0.2, gridUV.y) * step(gridUV.y, 0.8);
                
                return binary * digitMask * _BinaryIntensity;
            }
            
            // 데이터 스트림 (간단한 버전)
            float dataStream(float2 uv, float time)
            {
                float streams = 0.0;
                
                // 3개의 고정된 스트림
                float3 streamPos = float3(0.2, 0.5, 0.8);
                
                // 스트림 1
                float stream1 = 1.0 - smoothstep(0.0, _StreamWidth, abs(uv.x - streamPos.x));
                float packet1 = 1.0 - smoothstep(0.0, 0.1, abs(frac(uv.y * 2.0 - time * _StreamSpeed) - 0.5));
                streams += stream1 * packet1;
                
                // 스트림 2
                float stream2 = 1.0 - smoothstep(0.0, _StreamWidth, abs(uv.x - streamPos.y));
                float packet2 = 1.0 - smoothstep(0.0, 0.1, abs(frac(uv.y * 2.0 - time * _StreamSpeed + 0.3) - 0.5));
                streams += stream2 * packet2;
                
                // 스트림 3
                float stream3 = 1.0 - smoothstep(0.0, _StreamWidth, abs(uv.x - streamPos.z));
                float packet3 = 1.0 - smoothstep(0.0, 0.1, abs(frac(uv.y * 2.0 - time * _StreamSpeed + 0.6) - 0.5));
                streams += stream3 * packet3;
                
                return saturate(streams * _StreamIntensity);
            }
            
            // 스캔라인 효과
            float scanlineEffect(float2 uv, float time)
            {
                float scanline = sin((uv.y + time * _ScanlineSpeed) * 30.0);
                scanline = smoothstep(0.8, 1.0, scanline);
                return scanline * _ScanlineIntensity;
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
                
                // 효과 계산
                float glitch = glitchEffect(uv, time);
                float binary = binaryPattern(uv, time);
                float streams = dataStream(uv, time);
                float scanlines = scanlineEffect(uv, time);
                
                // 프레넬 효과
                float fresnel = 1.0 - saturate(dot(i.normal, i.viewDir));
                fresnel = pow(fresnel, _Fresnel);
                
                // 색상 조합
                fixed4 finalColor = _DataColor;
                
                // 데이터 스트림 추가
                finalColor = lerp(finalColor, _StreamColor, streams);
                
                // 바이너리 패턴 추가
                finalColor.rgb += binary * _StreamColor.rgb;
                
                // 글리치 효과 추가
                finalColor = lerp(finalColor, _GlitchColor, glitch);
                
                // 스캔라인 추가
                finalColor.rgb += scanlines * _StreamColor.rgb * 0.5;
                
                // 프레넬 글로우 추가
                finalColor.rgb += fresnel * _StreamColor.rgb * 0.3;
                
                // 메인 텍스처 적용
                finalColor.rgb *= mainTex.rgb;
                
                // 알파 계산
                float finalAlpha = _Alpha;
                finalAlpha *= (1.0 + streams * 0.5);
                finalAlpha *= (1.0 + glitch * 0.3);
                finalAlpha += fresnel * 0.2;
                finalAlpha *= mainTex.a;
                
                finalColor.a = saturate(finalAlpha);
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/VertexLit"
} 