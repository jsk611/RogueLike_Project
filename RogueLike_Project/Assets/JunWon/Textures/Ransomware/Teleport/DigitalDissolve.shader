Shader "Custom/DigitalDataDissolve" {
    Properties {
        [MainTexture] _BaseMap("Base Map (RGB)", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        
        _DissolveMap ("Dissolve Map", 2D) = "white" {}
        _GridTex ("Grid Pattern", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _EdgeColor1 ("Edge Color 1", Color) = (0, 1, 0.8, 1)  // 사이버틱한 청록색
        _EdgeColor2 ("Edge Color 2", Color) = (0, 0.7, 1, 1)  // 전자적인 파란색
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.1
        _EmissionIntensity ("Emission Intensity", Range(0, 5)) = 2
        _GridSize ("Grid Size", Range(10, 100)) = 30
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.1
        _BinaryGridSize ("Binary Pattern Size", Range(20, 200)) = 100
        _ScrollSpeed ("Data Scroll Speed", Range(0, 10)) = 2
    }

    SubShader {
        Tags { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
            };

            sampler2D _BaseMap;
            float4 _BaseColor;
            sampler2D _DissolveMap;
            sampler2D _GridTex;
            float4 _BaseMap_ST;
            float _DissolveAmount;
            float4 _EdgeColor1;
            float4 _EdgeColor2;
            float _EdgeWidth;
            float _EmissionIntensity;
            float _GridSize;
            float _GlitchIntensity;
            float _BinaryGridSize;
            float _ScrollSpeed;

            // 랜덤 함수
            float random(float2 st) {
                return frac(sin(dot(st.xy, float2(12.9898,78.233))) * 43758.5453123);
            }

            // 디지털 글리치 효과
            float2 glitch(float2 uv, float time) {
                float2 offset = float2(0,0);
                
                // 수평 글리치
                float y = floor(uv.y * 20) / 20;
                float noise = random(float2(y, time));
                if (noise > 0.96) {
                    offset.x = (random(float2(y, time * 2)) - 0.5) * 0.1;
                }
                
                // 수직 글리치
                float x = floor(uv.x * 15) / 15;
                noise = random(float2(x, time));
                if (noise > 0.96) {
                    offset.y = (random(float2(x, time * 2)) - 0.5) * 0.1;
                }
                
                return offset;
            }

            // 이진수 패턴 생성
            float binaryPattern(float2 uv) {
                float2 id = floor(uv * _BinaryGridSize);
                return step(0.5, random(id));
            }

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float time = _Time.y;
                
                // 글리치 효과 적용
                float2 glitchOffset = glitch(i.uv, time) * _GlitchIntensity;
                float2 uv = i.uv + glitchOffset;
                
                // 스크린 공간 UV 계산
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                
                // 기본 텍스처
                fixed4 col = tex2D(_BaseMap, uv) * _BaseColor;
                
                // 그리드 패턴
                float2 gridUV = uv * _GridSize;
                float grid = tex2D(_GridTex, gridUV).r;
                
                // 스크롤하는 이진수 패턴
                float2 scrollUV = float2(uv.x, uv.y - time * _ScrollSpeed);
                float binary = binaryPattern(scrollUV);
                
                // 디졸브 맵에 그리드와 이진수 패턴 결합
                float dissolve = tex2D(_DissolveMap, uv).r;
                dissolve *= grid;
                dissolve = lerp(dissolve, binary, 0.3);
                
                // 프레넬 효과
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = 1.0 - saturate(dot(viewDir, i.worldNormal));
                
                // 디졸브 계산
                float dissolveThreshold = _DissolveAmount * (1 + fresnel * 0.5);
                float cutout = dissolve - dissolveThreshold;
                
                // 엣지 효과
                float edge = smoothstep(0, _EdgeWidth, cutout);
                
                // 엣지 색상 계산
                float4 edgeColor = lerp(_EdgeColor1, _EdgeColor2, fresnel) * _EmissionIntensity;
                
                // 데이터 흐름 효과
                float dataFlow = frac(uv.y - time * 0.5);
                edgeColor.rgb *= (1 + dataFlow * 0.5);
                
                // 최종 색상 조합
                col.rgb = lerp(edgeColor.rgb, col.rgb, edge);
                
                // 디지털 노이즈 추가
                float noise = random(uv + time) * 0.1;
                col.rgb += noise * (1 - edge);
                
                // 알파값 계산
                col.a *= step(0, cutout);
                
                // 엣지 부분 발광 효과
                if (cutout < _EdgeWidth && cutout > 0) {
                    col.rgb += edgeColor.rgb * (1 - cutout/_EdgeWidth);
                    col.a = 1;
                }
                
                return col;
            }
            ENDCG
        }
    }
}