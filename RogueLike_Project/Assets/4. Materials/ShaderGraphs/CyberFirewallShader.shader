Shader "Custom/CyberFirewallShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _GridColor ("Grid Color", Color) = (0.0, 1.0, 0.5, 1.0)
        _NodeColor ("Node Color", Color) = (1.0, 0.8, 0.0, 1.0)
        _ScanColor ("Scan Color", Color) = (0.0, 0.8, 1.0, 1.0)
        _BlockColor ("Block Color", Color) = (1.0, 0.2, 0.2, 1.0)
        
        [Header(Grid Settings)]
        _GridScale ("Grid Scale", Range(1.0, 100.0)) = 20.0
        _GridThickness ("Grid Thickness", Range(0.01, 0.2)) = 0.05
        _GridIntensity ("Grid Intensity", Range(0.0, 2.0)) = 1.0
        
        [Header(Hologram Settings)]
        _HologramFlicker ("Hologram Flicker", Range(0.0, 1.0)) = 0.3
        _HologramSpeed ("Hologram Speed", Range(0.1, 10.0)) = 2.0
        _HologramNoise ("Hologram Noise", Range(0.0, 1.0)) = 0.2
        
        [Header(Security Scan)]
        _ScanSpeed ("Scan Speed", Range(0.1, 20.0)) = 5.0
        _ScanWidth ("Scan Width", Range(0.01, 0.5)) = 0.1
        _ScanIntensity ("Scan Intensity", Range(0.0, 3.0)) = 2.0
        
        [Header(Node Effects)]
        _NodeSize ("Node Size", Range(0.01, 0.1)) = 0.03
        _NodePulseSpeed ("Node Pulse Speed", Range(0.1, 10.0)) = 3.0
        _NodeIntensity ("Node Intensity", Range(0.0, 2.0)) = 1.5
        
        [Header(Block Detection)]
        _BlockThreshold ("Block Threshold", Range(0.0, 1.0)) = 0.7
        _BlockFlashSpeed ("Block Flash Speed", Range(1.0, 20.0)) = 8.0
        
        [Header(Transparency)]
        _Alpha ("Alpha", Range(0.0, 1.0)) = 0.7
        _Fresnel ("Fresnel Power", Range(0.1, 5.0)) = 2.0
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
            fixed4 _GridColor;
            fixed4 _NodeColor;
            fixed4 _ScanColor;
            fixed4 _BlockColor;
            
            float _GridScale;
            float _GridThickness;
            float _GridIntensity;
            
            float _HologramFlicker;
            float _HologramSpeed;
            float _HologramNoise;
            
            float _ScanSpeed;
            float _ScanWidth;
            float _ScanIntensity;
            
            float _NodeSize;
            float _NodePulseSpeed;
            float _NodeIntensity;
            
            float _BlockThreshold;
            float _BlockFlashSpeed;
            
            float _Alpha;
            float _Fresnel;
            
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
            
            // 랜덤 함수
            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            // 2D 노이즈
            float noise2D(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);
                
                float a = rand(i);
                float b = rand(i + float2(1.0, 0.0));
                float c = rand(i + float2(0.0, 1.0));
                float d = rand(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }
            
            // 격자 패턴 생성
            float gridPattern(float2 uv, float time)
            {
                float2 scaledUV = uv * _GridScale;
                float2 gridUV = frac(scaledUV);
                
                // 수직 격자선
                float verticalGrid = step(1.0 - _GridThickness, gridUV.x) + step(gridUV.x, _GridThickness);
                
                // 수평 격자선
                float horizontalGrid = step(1.0 - _GridThickness, gridUV.y) + step(gridUV.y, _GridThickness);
                
                // 격자 조합
                float grid = saturate(verticalGrid + horizontalGrid);
                
                // 홀로그램 깜빡임 효과
                float flicker = 1.0 + _HologramFlicker * sin(time * _HologramSpeed * 10.0) * 
                               noise2D(scaledUV + time * 0.1);
                
                return grid * _GridIntensity * flicker;
            }
            
            // 격자 교차점 (노드) 생성
            float gridNodes(float2 uv, float time)
            {
                float2 scaledUV = uv * _GridScale;
                float2 gridUV = frac(scaledUV);
                float2 gridID = floor(scaledUV);
                
                // 격자 교차점에서의 거리
                float2 nodeCenter = float2(0.5, 0.5);
                float nodeDistance = distance(gridUV, nodeCenter);
                
                // 노드 생성 (일부 교차점만)
                float nodeExists = step(0.7, rand(gridID + float2(1.337, 2.718)));
                
                // 노드 크기 (펄스 효과)
                float pulse = 1.0 + 0.3 * sin(time * _NodePulseSpeed + rand(gridID) * 6.28);
                float nodeSize = _NodeSize * pulse;
                
                // 노드 마스크
                float nodeMask = 1.0 - smoothstep(0.0, nodeSize, nodeDistance);
                
                return nodeMask * nodeExists * _NodeIntensity;
            }
            
            // 보안 스캔 효과
            float securityScan(float2 uv, float time)
            {
                // 수직 스캔라인
                float scanLine1 = sin((uv.x + time * _ScanSpeed) * 6.28);
                scanLine1 = smoothstep(1.0 - _ScanWidth, 1.0, scanLine1);
                
                // 수평 스캔라인
                float scanLine2 = sin((uv.y + time * _ScanSpeed * 0.7) * 6.28);
                scanLine2 = smoothstep(1.0 - _ScanWidth, 1.0, scanLine2);
                
                // 대각선 스캔
                float diagScan = sin((uv.x + uv.y + time * _ScanSpeed * 0.5) * 6.28);
                diagScan = smoothstep(1.0 - _ScanWidth, 1.0, diagScan);
                
                return saturate(scanLine1 + scanLine2 + diagScan * 0.5) * _ScanIntensity;
            }
            
            // 위협 탐지 및 차단 효과
            float threatDetection(float2 uv, float time)
            {
                float2 scaledUV = uv * _GridScale * 0.5;
                float threat = noise2D(scaledUV + time * 0.3);
                
                // 위험 임계값 초과 시 차단 효과
                if (threat > _BlockThreshold)
                {
                    // 빠른 깜빡임 효과
                    float flash = step(0.5, frac(time * _BlockFlashSpeed));
                    return flash * 0.8;
                }
                
                return 0.0;
            }
            
            // 홀로그램 스태틱 노이즈
            float hologramStatic(float2 uv, float time)
            {
                float staticNoise = noise2D(uv * 50.0 + time * 5.0);
                return staticNoise * _HologramNoise * 0.3;
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
                
                // 각종 효과 계산
                float grid = gridPattern(uv, time);
                float nodes = gridNodes(uv, time);
                float scan = securityScan(uv, time);
                float threat = threatDetection(uv, time);
                float staticNoise = hologramStatic(uv, time);
                
                // 프레넬 효과 (홀로그램 테두리 글로우)
                float fresnel = 1.0 - saturate(dot(i.normal, i.viewDir));
                fresnel = pow(fresnel, _Fresnel);
                
                // 기본 색상은 격자 색상
                fixed4 finalColor = _GridColor;
                
                // 격자 패턴 적용
                finalColor.rgb = lerp(float3(0, 0, 0), finalColor.rgb, grid);
                
                // 노드 색상 추가
                finalColor.rgb = lerp(finalColor.rgb, _NodeColor.rgb, nodes);
                
                // 스캔 효과 추가
                finalColor.rgb = lerp(finalColor.rgb, _ScanColor.rgb, scan);
                
                // 위협 차단 효과 (빨간색 경고)
                finalColor.rgb = lerp(finalColor.rgb, _BlockColor.rgb, threat);
                
                // 홀로그램 스태틱 노이즈 추가
                finalColor.rgb += staticNoise;
                
                // 프레넬 글로우 (홀로그램 가장자리 효과)
                finalColor.rgb += fresnel * _GridColor.rgb * 0.5;
                
                // 메인 텍스처와 블렌딩
                finalColor.rgb *= mainTex.rgb;
                
                // 알파 계산
                float finalAlpha = _Alpha;
                
                // 격자가 있는 곳에서 더 진하게
                finalAlpha *= (0.3 + grid * 0.7);
                
                // 노드에서 더 진하게
                finalAlpha += nodes * 0.3;
                
                // 스캔 효과에서 더 진하게
                finalAlpha += scan * 0.2;
                
                // 위협 탐지 시 더 진하게
                finalAlpha += threat * 0.4;
                
                // 프레넬 효과로 가장자리 강화
                finalAlpha += fresnel * 0.3;
                
                // 홀로그램 깜빡임 적용
                float hologramFlicker = 1.0 + _HologramFlicker * 0.2 * sin(time * _HologramSpeed * 8.0);
                finalAlpha *= hologramFlicker;
                
                // 메인 텍스처 알파 적용
                finalAlpha *= mainTex.a;
                
                finalColor.a = saturate(finalAlpha);
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/VertexLit"
} 