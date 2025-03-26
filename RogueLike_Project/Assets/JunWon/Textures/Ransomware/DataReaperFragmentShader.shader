Shader "Custom/DataReaperFragmentShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _GridTex ("Digital Grid", 2D) = "black" {}
        
        _Color ("Main Color", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color", Color) = (0,1,1,1)
        
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _EdgeWidth ("Edge Width", Range(0, 0.1)) = 0.02
        _GridStrength ("Grid Strength", Range(0, 1)) = 0.5
        
        _FragmentSize ("Fragment Size", Range(0.001, 0.1)) = 0.02
        _FragmentSpread ("Fragment Spread", Range(0, 1)) = 0.5
        _FragmentShift ("Fragment Shift", Range(0, 1)) = 0.2
        
        _GlitchFrequency ("Glitch Frequency", Range(0, 20)) = 5
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.1
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        Cull Back
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
                float2 gridUV : TEXCOORD2;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD3;
                float3 worldNormal : TEXCOORD4;
                float4 color : COLOR;
                UNITY_FOG_COORDS(5)
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            sampler2D _GridTex;
            float4 _GridTex_ST;
            
            fixed4 _Color;
            fixed4 _EmissionColor;
            
            float _DissolveAmount;
            float _EdgeWidth;
            float _GridStrength;
            
            float _FragmentSize;
            float _FragmentSpread;
            float _FragmentShift;
            
            float _GlitchFrequency;
            float _GlitchIntensity;
            
            // 랜덤 함수
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            // 픽셀화 함수
            float2 pixelate(float2 uv, float size)
            {
                return floor(uv / size) * size;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                
                // 디졸브 진행에 따른 버텍스 오프셋 계산
                float3 vertexOffset = float3(0, 0, 0);
                
                if (_DissolveAmount > 0)
                {
                    // 노이즈 값 기반 랜덤 오프셋
                    float2 noiseUV = v.uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
                    float noise = tex2Dlod(_NoiseTex, float4(noiseUV, 0, 0)).r;
                    
                    // 디졸브 임계값과 노이즈 값 비교하여 오프셋 계산
                    float threshold = _DissolveAmount - 0.3; // 먼저 디졸브되는 부분
                    
                    if (noise < threshold)
                    {
                        // 조각이 퍼지는 방향 계산
                        float3 direction = normalize(v.normal + float3(
                            random(v.uv + _Time.x) * 2.0 - 1.0, 
                            random(v.uv + _Time.y) * 2.0 - 1.0, 
                            random(v.uv + _Time.z) * 2.0 - 1.0
                        ));
                        
                        // 디졸브 진행도에 따라 오프셋 강도 계산
                        float offsetStrength = saturate((threshold - noise) / 0.3) * _FragmentSpread;
                        
                        // 최종 버텍스 오프셋
                        vertexOffset = direction * offsetStrength * _DissolveAmount * 0.5;
                        
                        // 랜덤 회전 추가
                        float rotAngle = _DissolveAmount * random(v.uv) * 6.28318;
                        float sinR = sin(rotAngle);
                        float cosR = cos(rotAngle);
                        float3 rotOffset = float3(
                            vertexOffset.x * cosR - vertexOffset.z * sinR,
                            vertexOffset.y,
                            vertexOffset.x * sinR + vertexOffset.z * cosR
                        );
                        
                        vertexOffset = rotOffset;
                    }
                }
                
                // 글리치 효과 - 랜덤 오프셋
                float glitchTime = _Time.y * _GlitchFrequency;
                float2 blockPos = floor(v.uv * 10) / 10;
                float blockNoise = random(blockPos + floor(glitchTime));
                
                if (blockNoise > 0.95 && _GlitchIntensity > 0)
                {
                    float glitchOffset = (random(blockPos + glitchTime) * 2 - 1) * 0.1 * _GlitchIntensity;
                    vertexOffset.x += glitchOffset;
                }
                
                // 최종 버텍스 위치 계산
                o.vertex = UnityObjectToClipPos(v.vertex + float4(vertexOffset, 0));
                
                // UV 좌표 계산
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.noiseUV = TRANSFORM_TEX(v.uv, _NoiseTex);
                o.gridUV = TRANSFORM_TEX(v.uv, _GridTex) + float2(_Time.y * 0.1, _Time.y * 0.05); // 그리드 애니메이션
                
                // 월드 위치 및 노말 계산
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                
                // 정점 컬러 전달
                o.color = v.color * _Color;
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // 기본 텍스처 샘플링
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
    
                // 노이즈 샘플링
                float noise = tex2D(_NoiseTex, i.noiseUV).r;
    
                // 프레넬 효과 계산 (시점 각도에 따른 가장자리 효과)
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                float fresnel = 1.0 - saturate(dot(normalize(i.worldNormal), viewDir));
                fresnel = pow(fresnel, 3) * 0.8;
    
                // 가장자리 효과 계산 (프레넬 + 노이즈 기반)
                float edgeFactor = smoothstep(0.4, 0.6, fresnel + noise * 0.2);
    
                // 기본 모델은 정상적으로 렌더링하되, 가장자리에만 효과 집중
                if (edgeFactor > 0)
                {
                    // 글리치 효과 (가장자리에만)
                    float2 glitchUV = i.uv;
                    if (_GlitchIntensity > 0 && edgeFactor > 0.2)
                    {
                        float glitchTime = _Time.y * _GlitchFrequency;
                        float rowNoise = random(float2(floor(i.uv.y * 20) / 20, floor(glitchTime)));
            
                        if (rowNoise > 0.8)
                        {
                            float glitchAmount = (random(float2(i.uv.y, glitchTime)) * 2 - 1) * 0.03 * _GlitchIntensity * edgeFactor;
                            glitchUV.x += glitchAmount;
                        }
                    }
        
                    // 가장자리에 격자 무늬 추가
                    float grid = tex2D(_GridTex, i.gridUV).r;
        
                    // 가장자리 발광 효과
                    col.rgb = lerp(col.rgb, _EmissionColor.rgb * (1.5 + grid), edgeFactor * 0.7);
        
                    // 가장자리에만 디지털 노이즈 패턴 추가
                    col.rgb += grid * _EmissionColor.rgb * edgeFactor * _GridStrength;
                }
    
                return col;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}