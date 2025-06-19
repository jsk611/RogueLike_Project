Shader "Custom/CyberDustParticle"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        
        [Header(Cyber Effects)]
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.3
        _GlitchSpeed ("Glitch Speed", Float) = 10
        _GlitchTex ("Glitch Noise", 2D) = "black" {}
        
        [Header(Digital Dissolve)]
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _DissolveTex ("Dissolve Texture", 2D) = "white" {}
        _DissolveEdge ("Dissolve Edge Width", Range(0, 0.5)) = 0.1
        _DissolveEdgeColor ("Dissolve Edge Color", Color) = (0, 1, 1, 1)
        
        [Header(Hologram Effect)]
        _HoloStrength ("Hologram Strength", Range(0, 2)) = 1
        _ScanlineFreq ("Scanline Frequency", Float) = 20
        _ScanlineSpeed ("Scanline Speed", Float) = 2
        _Fresnel ("Fresnel Power", Range(0, 5)) = 2
        
        [Header(Energy Pulse)]
        _PulseSpeed ("Pulse Speed", Float) = 3
        _PulseIntensity ("Pulse Intensity", Range(0, 2)) = 1
        _EmissionBoost ("Emission Boost", Range(0, 5)) = 2
    }
    
    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane" 
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
        Cull Off 
        Lighting Off 
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_particles
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 screenPos : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                UNITY_FOG_COORDS(3)
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex;
            sampler2D _GlitchTex;
            sampler2D _DissolveTex;
            float4 _MainTex_ST;
            
            fixed4 _Color;
            float _GlitchIntensity;
            float _GlitchSpeed;
            float _DissolveAmount;
            float _DissolveEdge;
            fixed4 _DissolveEdgeColor;
            float _HoloStrength;
            float _ScanlineFreq;
            float _ScanlineSpeed;
            float _Fresnel;
            float _PulseSpeed;
            float _PulseIntensity;
            float _EmissionBoost;
            
            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                // 글리치 버텍스 왜곡
                float glitchNoise = tex2Dlod(_GlitchTex, float4(v.texcoord + _Time.y * _GlitchSpeed, 0, 0)).r;
                float3 glitchOffset = float3(
                    sin(_Time.y * _GlitchSpeed + v.vertex.x) * _GlitchIntensity * 0.1,
                    cos(_Time.y * _GlitchSpeed + v.vertex.y) * _GlitchIntensity * 0.1,
                    sin(_Time.y * _GlitchSpeed + v.vertex.z) * _GlitchIntensity * 0.05
                ) * glitchNoise;
                
                v.vertex.xyz += glitchOffset;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.color = v.color * _Color;
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                
                // 스크린 좌표 계산
                o.screenPos = ComputeScreenPos(o.vertex).xy / ComputeScreenPos(o.vertex).w;
                
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 기본 텍스처
                fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
                
                // 디지털 디졸브 효과
                float dissolveNoise = tex2D(_DissolveTex, i.texcoord).r;
                float dissolveThreshold = _DissolveAmount;
                
                // 디졸브 엣지 계산
                float edge = smoothstep(dissolveThreshold - _DissolveEdge, dissolveThreshold + _DissolveEdge, dissolveNoise);
                float dissolveEdge = (1 - edge) * step(dissolveThreshold, dissolveNoise);
                
                // 디졸브로 잘라내기
                clip(dissolveNoise - dissolveThreshold);
                
                // 홀로그램 스캔라인 효과
                float scanline = sin(i.screenPos.y * _ScanlineFreq + _Time.y * _ScanlineSpeed);
                float holoEffect = lerp(1, scanline * 0.5 + 0.5, _HoloStrength);
                
                // 에너지 펄스 효과
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                float energyPulse = lerp(1, pulse, _PulseIntensity);
                
                // 글리치 색상 효과
                float glitchEffect = tex2D(_GlitchTex, i.texcoord + _Time.y * _GlitchSpeed).r;
                if (_GlitchIntensity > 0 && glitchEffect > 0.7)
                {
                    // 글리치 컬러 왜곡
                    col.r = lerp(col.r, 1, _GlitchIntensity * glitchEffect);
                    col.g = lerp(col.g, 0, _GlitchIntensity * glitchEffect * 0.8);
                    col.b = lerp(col.b, 1, _GlitchIntensity * glitchEffect);
                }
                
                // 프레넬 효과 (가장자리 강조)
                float3 worldNormal = normalize(cross(ddx(i.worldPos), ddy(i.worldPos)));
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = pow(1 - saturate(dot(worldNormal, viewDir)), _Fresnel);
                
                // 최종 컬러 합성
                col.rgb *= holoEffect * energyPulse;
                col.rgb += _DissolveEdgeColor.rgb * dissolveEdge * _EmissionBoost;
                col.rgb += col.rgb * fresnel * _EmissionBoost * 0.5;
                
                // 알파 계산
                col.a *= holoEffect * (1 - dissolveEdge * 0.5);
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
} 