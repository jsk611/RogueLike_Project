Shader "Custom/DataPacketEffect"
{
     Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PrimaryColor ("Primary Color", Color) = (0,1,0.8,1)
        _SecondaryColor ("Secondary Color", Color) = (0,0.4,1,1)
        _GlowSpeed ("Glow Speed", Range(0.1, 5.0)) = 1.0
        _GlowIntensity ("Glow Intensity", Range(0.1, 2.0)) = 1.0
        _GridSize ("Grid Size", Range(10, 100)) = 30
        _DataFlowSpeed ("Data Flow Speed", Range(0.1, 10.0)) = 1.0
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.5
        _GlitchSpeed ("Glitch Speed", Range(0.1, 10.0)) = 3.0
        _PacketSize ("Packet Size", Range(1, 10)) = 3
        _PacketDensity ("Packet Density", Range(0.1, 1)) = 0.7
        _PacketTrailLength ("Packet Trail Length", Range(0.1, 2.0)) = 0.5
        _PatternIntensity ("Pattern Intensity", Range(0.1, 2.0)) = 1.0
        _FlowDirection ("Flow Direction", Vector) = (0,0,1,0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
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
                float3 viewDir : TEXCOORD2;
                float3 normal : NORMAL;
                float4 screenPos : TEXCOORD3;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _PrimaryColor;
            float4 _SecondaryColor;
            float _GlowSpeed;
            float _GlowIntensity;
            float _GridSize;
            float _DataFlowSpeed;
            float _GlitchIntensity;
            float _GlitchSpeed;
            float _PacketSize;
            float _PacketDensity;
            float _PacketTrailLength;
            float _PatternIntensity;
            float4 _FlowDirection;
            
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898,78.233))) * 43758.5453123);
            }
            
            float2 random2(float2 st)
            {
                st = float2(dot(st,float2(127.1,311.7)),
                           dot(st,float2(269.5,183.3)));
                return -1.0 + 2.0 * frac(sin(st) * 43758.5453123);
            }

            // 새로운 노이즈 패턴 함수
            float noise(float2 st) {
                float2 i = floor(st);
                float2 f = frac(st);
                
                float2 u = f*f*(3.0-2.0*f);

                return lerp(
                    lerp(random(i + float2(0.0,0.0)), 
                         random(i + float2(1.0,0.0)), u.x),
                    lerp(random(i + float2(0.0,1.0)), 
                         random(i + float2(1.0,1.0)), u.x),
                    u.y
                );
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // 글리치 효과
                float glitchTime = _Time.y * _GlitchSpeed;
                float glitchNoise = random(floor(v.vertex.xy + glitchTime));
                float3 glitchOffset = float3(
                    sin(glitchTime * 13.0) * glitchNoise * _GlitchIntensity * 0.01,
                    cos(glitchTime * 17.0) * glitchNoise * _GlitchIntensity * 0.01,
                    0
                );
                
                v.vertex.xyz += glitchOffset * v.normal;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 시간 기반 변수들
                float timeOffset = _Time.y * _DataFlowSpeed;
                float glitchTime = _Time.y * _GlitchSpeed;
                
                // 뷰 방향 기반 UV 왜곡
                float2 viewBasedUV = i.uv + i.viewDir.xy * 0.1;
                
                // 향상된 그리드 패턴
                float2 grid = frac(viewBasedUV * _GridSize);
                float gridLine = smoothstep(0.95, 1.0, grid.x) + smoothstep(0.95, 1.0, grid.y);
                
                // 패킷 흐름 방향 계산
                float2 flowDirection = normalize(_FlowDirection.xy + i.viewDir.xy);
                float2 packetUV = viewBasedUV * _GridSize * _PacketSize + 
                                 flowDirection * timeOffset;
                
                // 패킷 생성 및 움직임
                float2 packetId = floor(packetUV);
                float packet = noise(packetId * 0.5);
                float packetShape = smoothstep(1.0 - _PacketDensity, 
                                            1.0 - _PacketDensity + 0.1, 
                                            packet);
                
                // 패킷 트레일 효과
                float trail = 0;
                for(float t = 0; t < 1.0; t += 0.1) {
                    float2 trailUV = packetUV - flowDirection * t * _PacketTrailLength;
                    float trailPacket = noise(floor(trailUV) * 0.5);
                    trail += smoothstep(1.0 - _PacketDensity, 
                                     1.0 - _PacketDensity + 0.1, 
                                     trailPacket) * (1.0 - t);
                }
                
                // 패턴 강화
                float pattern = noise(viewBasedUV * _GridSize + timeOffset) * _PatternIntensity;
                float enhancedPattern = smoothstep(0.4, 0.6, pattern);
                
                // 스캔라인 효과
                float scanLine = step(0.98, frac(i.screenPos.y * 100.0 + timeOffset));
                
                // 엣지 하이라이트
                float rim = 1.0 - saturate(dot(i.normal, i.viewDir));
                rim = pow(rim, 3.0);
                
                // 글로우 효과
                float glow = sin(timeOffset * _GlowSpeed) * 0.5 + 0.5;
                glow = glow * _GlowIntensity;
                
                // 컬러 믹싱
                float4 col = lerp(_PrimaryColor, _SecondaryColor, 
                    (packetShape + trail * 0.3) * (sin(timeOffset + packet) * 0.5 + 0.5));
                
                // 최종 컬러 조합
                float4 finalColor = col;
                finalColor.rgb += gridLine * 0.3;
                finalColor.rgb += enhancedPattern * 0.15;
                finalColor.rgb += rim * _SecondaryColor.rgb * 0.5;
                finalColor.rgb += trail * _PrimaryColor.rgb * 0.3;
                finalColor.rgb += scanLine * 0.15;
                finalColor.rgb *= (0.8 + glow * 0.4);
                
                // 글리치 컬러 왜곡
                float glitchStrength = random(float2(glitchTime, 0)) * _GlitchIntensity;
                if (glitchStrength > 0.8) {
                    finalColor.rgb = float3(
                        finalColor.r * 1.5,
                        finalColor.g * 0.8,
                        finalColor.b
                    );
                }
                
                // 알파 채널 조정
                finalColor.a = col.a * (
                    gridLine * 0.3 + 
                    packetShape * 0.7 + 
                    trail * 0.4 + 
                    enhancedPattern * 0.2
                );
                
                // 텍스처 적용
                float2 glitchUV = i.uv + glitchStrength * 0.1;
                fixed4 texCol = tex2D(_MainTex, glitchUV);
                finalColor *= texCol;
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}