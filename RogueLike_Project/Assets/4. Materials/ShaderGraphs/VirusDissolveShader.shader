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
            
            // 노이즈 함수 (디지털 글리치 효과용)
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            // 디지털 노이즈
            float digitalNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // 글리치 효과 (버텍스 노이즈)
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float glitchNoise = digitalNoise(worldPos.xz * 10 + _Time.y * 5) * _GlitchIntensity;
                
                // 디졸브가 진행될수록 글리치 강화
                glitchNoise *= (_DissolveAmount * 2);
                
                v.vertex.xyz += v.normal * glitchNoise * 0.01;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = worldPos;
                o.normal = UnityObjectToWorldNormal(v.normal);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 메인 텍스처
                fixed4 mainCol = tex2D(_MainTex, i.uv) * _MainColor;
                
                // 디졸브 노이즈 (다중 스케일)
                float2 dissolveUV1 = i.uv * _NoiseScale;
                float2 dissolveUV2 = i.uv * _NoiseScale * 2.3;
                float2 dissolveUV3 = i.worldPos.xz * 0.1 + _Time.y * 0.1;
                
                float dissolveNoise1 = tex2D(_DissolveTexture, dissolveUV1).r;
                float dissolveNoise2 = tex2D(_DissolveTexture, dissolveUV2).g;
                float dissolveNoise3 = digitalNoise(dissolveUV3);
                
                // 복합 노이즈 (더 복잡한 패턴)
                float combinedNoise = (dissolveNoise1 * 0.5 + dissolveNoise2 * 0.3 + dissolveNoise3 * 0.2);
                
                // 시간 기반 변화
                combinedNoise += sin(_Time.y * 3 + i.worldPos.x * 5) * 0.1;
                
                // 디졸브 계산
                float dissolveStep = _DissolveAmount;
                float dissolveEdge = dissolveStep + _DissolveEdgeWidth;
                
                // 클리핑 (완전히 사라지는 부분)
                clip(combinedNoise - dissolveStep);
                
                // 가장자리 글로우 효과
                float edgeFactor = 1 - smoothstep(dissolveStep, dissolveEdge, combinedNoise);
                
                // 엣지 컬러 계산 (사이버틱한 글로우)
                fixed4 edgeCol = _EdgeColor * _EdgeIntensity;
                edgeCol *= pow(edgeFactor, _GlowPower);
                
                // 디지털 스캔라인 효과
                float scanline = sin(i.worldPos.y * 50 + _Time.y * 10) * 0.1 + 0.9;
                edgeCol.rgb *= scanline;
                
                // 최종 컬러 조합
                fixed4 finalCol = mainCol + edgeCol;
                
                // 디졸브 진행에 따른 투명도 변화
                float alpha = mainCol.a * (1 - smoothstep(0.8, 1.0, _DissolveAmount));
                alpha += edgeFactor * _EdgeColor.a;
                
                // 글리치 색상 왜곡
                if (_DissolveAmount > 0.3)
                {
                    float glitchFactor = (_DissolveAmount - 0.3) / 0.7;
                    float colorGlitch = digitalNoise(i.uv * 20 + _Time.y * 50);
                    
                    if (colorGlitch > 0.8)
                    {
                        finalCol.rgb = lerp(finalCol.rgb, _EdgeColor.rgb, glitchFactor * 0.5);
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