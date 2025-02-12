Shader "Custom/DataPacketGlitchEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PrimaryColor ("Primary Color", Color) = (0,1,0,1)
        _SecondaryColor ("Secondary Color", Color) = (0,0.7,1,1)
        _GlowSpeed ("Glow Speed", Range(0.1, 5.0)) = 1.0
        _GlowIntensity ("Glow Intensity", Range(0.1, 2.0)) = 1.0
        _GridSize ("Grid Size", Range(10, 100)) = 30
        _DataFlowSpeed ("Data Flow Speed", Range(0.1, 10.0)) = 1.0
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.5
        _GlitchSpeed ("Glitch Speed", Range(0.1, 10.0)) = 3.0
        _PacketSize ("Packet Size", Range(1, 10)) = 3
        _PacketDensity ("Packet Density", Range(0.1, 1)) = 0.7
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
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // Glitch vertex position
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
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Base grid
                float2 grid = frac(i.uv * _GridSize);
                float gridLine = step(0.95, grid.x) + step(0.95, grid.y);
                
                // Data packets
                float timeOffset = _Time.y * _DataFlowSpeed;
                float2 packetUV = i.uv * _GridSize * _PacketSize + timeOffset;
                float2 packetId = floor(packetUV);
                float packet = random(packetId);
                float packetActive = step(1.0 - _PacketDensity, packet);
                
                // Glitch effect
                float glitchTime = _Time.y * _GlitchSpeed;
                float2 glitchUV = i.uv;
                float glitchStrength = random(float2(glitchTime, 0)) * _GlitchIntensity;
                
                if (random(floor(glitchTime * 3.0)) < _GlitchIntensity * 0.3) {
                    glitchUV.x += glitchStrength * 0.1;
                    if (random(floor(glitchTime * 7.0)) < 0.2) {
                        glitchUV.y += glitchStrength * 0.05;
                    }
                }
                
                // Scan lines
                float scanLine = step(0.98, frac(i.uv.y * 100.0 + _Time.y));
                
                // Data flow lines
                float2 flowDir = random2(packetId);
                float flowLine = smoothstep(0.9, 1.0, 
                    sin(dot(grid - 0.5, normalize(flowDir)) * 10.0 + timeOffset));
                
                // Edge highlight
                float rim = 1.0 - saturate(dot(i.normal, i.viewDir));
                float rimPower = 3.0;
                rim = pow(rim, rimPower);
                
                // Glow effect
                float glow = sin(_Time.y * _GlowSpeed) * 0.5 + 0.5;
                glow = glow * _GlowIntensity;
                
                // Color mixing
                float4 col = lerp(_PrimaryColor, _SecondaryColor, 
                    packetActive * sin(_Time.y + random(packetId)) * 0.5 + 0.5);
                
                // Final composition
                float4 finalColor = col;
                finalColor.rgb += gridLine * 0.3;
                finalColor.rgb += flowLine * 0.2;
                finalColor.rgb += rim * _SecondaryColor.rgb * 0.5;
                finalColor.rgb += scanLine * 0.1;
                finalColor.rgb *= (0.8 + glow * 0.4);
                
                // Apply glitch color distortion
                if (glitchStrength > 0.8) {
                    finalColor.rgb = float3(
                        finalColor.r * 1.5,
                        finalColor.g * 0.8,
                        finalColor.b
                    );
                }
                
                finalColor.a = col.a * (gridLine * 0.3 + packetActive * 0.7 + flowLine * 0.2);
                
                // Apply texture
                fixed4 texCol = tex2D(_MainTex, glitchUV);
                finalColor *= texCol;
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}