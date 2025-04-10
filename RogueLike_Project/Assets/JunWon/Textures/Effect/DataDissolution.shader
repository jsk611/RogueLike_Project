Shader "Custom/DataDissolution" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (0,0.5,1,1)
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _DissolveScale ("Dissolve Scale", Range(0.1, 10)) = 1
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0
        _VoxelSize ("Voxel Size", Range(0.001, 0.1)) = 0.01
        _VoxelHollow ("Voxel Hollow", Range(0, 0.5)) = 0.2
        _EmissionStrength ("Emission Strength", Range(0, 5)) = 1
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
            #pragma require samplelod
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float _DissolveAmount;
            float _DissolveScale;
            float _GlitchIntensity;
            float _VoxelSize;
            float _VoxelHollow;
            float _EmissionStrength;
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 normal : NORMAL;
                float dissolveNoise : TEXCOORD2;
            };
            
            // 랜덤 함수 - 간소화
            float rand(float2 co) {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            // 노이즈 함수 (버텍스 셰이더용 수정)
            float noise(float2 co) {
                float2 scaledUV = co * _DissolveScale;
                float4 noiseColor = tex2Dlod(_NoiseTex, float4(scaledUV, 0, 0));
                return noiseColor.x; // .r 대신 .x 사용
            }
            
            // 간소화된 글리치 효과
            float3 applyGlitch(float3 pos, float2 uv, float intensity) {
                float time = _Time.y;
                float glitchX = sin(time * 20) * 0.02 * intensity;
                float glitchY = cos(time * 15) * 0.01 * intensity;
                
                pos.x += glitchX * sin(uv.y * 10);
                pos.y += glitchY * cos(uv.x * 8);
                
                return pos;
            }
            
            // 간소화된 복셀화 함수
            float3 voxelize(float3 pos) {
                return floor(pos / _VoxelSize + 0.5) * _VoxelSize;
            }
            
            v2f vert(appdata v) {
                v2f o;
                
                float3 localPos = v.vertex.xyz;
                float noiseVal = noise(v.uv);
                o.dissolveNoise = noiseVal;
                
                // 복셀화 적용 (분해 정도에 따라)
                if (_DissolveAmount > 0) {
                    localPos = voxelize(localPos);
                    
                    // 분해 효과 (상승 및 흩어짐)
                    float vertDissolve = saturate((_DissolveAmount * 2 - noiseVal) * 2);
                    
                    if (vertDissolve > 0) {
                        // 위로 상승
                        localPos.y += vertDissolve * vertDissolve * 2;
                        
                        // x, z 방향으로 흩어짐
                        float dispX = (rand(v.uv + float2(0, _Time.y)) - 0.5) * 2;
                        float dispZ = (rand(v.uv + float2(_Time.y, 0)) - 0.5) * 2;
                        localPos.xz += float2(dispX, dispZ) * vertDissolve;
                    }
                    
                    // 글리치 적용 - 단순화된 버전 사용
                    if (_GlitchIntensity > 0) {
                        localPos = applyGlitch(localPos, v.uv, _GlitchIntensity * _DissolveAmount);
                    }
                }
                
                // 버텍스 위치 업데이트
                float4 modifiedVertex = float4(localPos, v.vertex.w);
                
                o.vertex = UnityObjectToClipPos(modifiedVertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, modifiedVertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                // 텍스처 샘플링
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // 디졸브 마스크 생성
                float dissolveNoise = i.dissolveNoise;
                float dissolveMask = step(dissolveNoise, _DissolveAmount);
                
                // 엣지 발광 효과
                float edgeWidth = 0.05;
                float edgeMask = step(dissolveNoise - edgeWidth, _DissolveAmount) - dissolveMask;
                
                // 발광 색상 적용
                col.rgb = lerp(col.rgb, _GlowColor.rgb * _EmissionStrength, edgeMask);
                
                // 분해 부분 투명도 조절
                col.a *= 1 - dissolveMask;
                
                // 디졸브가 50% 이상일 때 와이어프레임 효과 추가
                if (_DissolveAmount > 0.5) {
                    // 복셀 패턴 생성
                    float3 voxPos = frac(i.worldPos / _VoxelSize);
                    float voxelEdge = 
                        step(voxPos.x, 0.05) + step(0.95, voxPos.x) + 
                        step(voxPos.y, 0.05) + step(0.95, voxPos.y) + 
                        step(voxPos.z, 0.05) + step(0.95, voxPos.z);
                    
                    // 와이어프레임 발광 색상
                    if (voxelEdge > 0) {
                        col.rgb = lerp(col.rgb, _GlowColor.rgb * _EmissionStrength * 1.5, saturate(voxelEdge));
                    }
                }
                
                // 글리치가 강할 때 노이즈 패턴 추가
                if (_GlitchIntensity > 0.3) {
                    float timeOffset = _Time.y * 10;
                    float noiseLine = step(0.97, frac(i.uv.y * 30 + timeOffset * rand(floor(i.uv.y * 30))));
                    col.rgb = lerp(col.rgb, _GlowColor.rgb, noiseLine * _GlitchIntensity);
                }
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}