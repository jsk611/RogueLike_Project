Shader "Custom/MimicPlantDissolve"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        
        [Header(Dissolve)]
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _DissolveTex ("Dissolve Texture", 2D) = "white" {}
        _DissolveEdgeWidth ("Edge Width", Range(0, 1)) = 0.1
        _DissolveEdgeColor ("Edge Color", Color) = (1, 0.5, 0, 1)
        _DissolveEdgeEmission ("Edge Emission", Range(0, 10)) = 2
        
        [Header(Emergence Animation)]
        _EmergenceHeight ("Emergence Height", Float) = 2.0
        _EmergenceNoise ("Emergence Noise", 2D) = "white" {}
        _EmergenceSpeed ("Emergence Speed", Float) = 1.0
        _GroundLevel ("Ground Level", Float) = 0.0
        
        [Header(Glitch Effects)]
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0
        _GlitchSpeed ("Glitch Speed", Float) = 10
        _GlitchTex ("Glitch Texture", 2D) = "black" {}
        
        [Header(Hologram)]
        _HologramAlpha ("Hologram Alpha", Range(0, 1)) = 1
        _ScanlineFrequency ("Scanline Frequency", Float) = 50
        _ScanlineSpeed ("Scanline Speed", Float) = 2
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard alpha:fade vertex:vert
        #pragma target 3.0
        
        sampler2D _MainTex;
        sampler2D _DissolveTex;
        sampler2D _EmergenceNoise;
        sampler2D _GlitchTex;
        
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_DissolveTex;
            float3 worldPos;
            float4 screenPos;
        };
        
        fixed4 _Color;
        float _DissolveAmount;
        float _DissolveEdgeWidth;
        fixed4 _DissolveEdgeColor;
        float _DissolveEdgeEmission;
        
        float _EmergenceHeight;
        float _EmergenceSpeed;
        float _GroundLevel;
        
        float _GlitchIntensity;
        float _GlitchSpeed;
        
        float _HologramAlpha;
        float _ScanlineFrequency;
        float _ScanlineSpeed;
        
        void vert(inout appdata_full v)
        {
            // 글리치 효과로 버텍스 왜곡
            if (_GlitchIntensity > 0)
            {
                float glitchNoise = tex2Dlod(_GlitchTex, float4(v.texcoord.xy + _Time.y * _GlitchSpeed, 0, 0)).r;
                v.vertex.xyz += v.normal * glitchNoise * _GlitchIntensity * 0.1;
                
                // 랜덤 글리치 오프셋
                float3 glitchOffset = float3(
                    sin(_Time.y * _GlitchSpeed + v.vertex.x) * _GlitchIntensity * 0.05,
                    cos(_Time.y * _GlitchSpeed + v.vertex.y) * _GlitchIntensity * 0.05,
                    sin(_Time.y * _GlitchSpeed + v.vertex.z) * _GlitchIntensity * 0.05
                );
                v.vertex.xyz += glitchOffset;
            }
        }
        
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // 기본 텍스처
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            
            // 디졸브 효과
            float dissolveNoise = tex2D(_DissolveTex, IN.uv_DissolveTex).r;
            float dissolveThreshold = _DissolveAmount;
            
            // 등장 효과 (월드 Y 좌표 기반)
            float emergenceNoise = tex2D(_EmergenceNoise, IN.uv_MainTex + _Time.y * _EmergenceSpeed).r;
            float emergenceHeight = IN.worldPos.y - _GroundLevel;
            float emergenceFactor = saturate((emergenceHeight + emergenceNoise * _EmergenceHeight) / _EmergenceHeight);
            
            // 디졸브와 등장 효과 결합
            float combinedDissolve = lerp(dissolveNoise, emergenceFactor, _DissolveAmount);
            
            // 엣지 효과 계산
            float edge = 1 - smoothstep(dissolveThreshold - _DissolveEdgeWidth, dissolveThreshold, combinedDissolve);
            float dissolveEdge = edge * (1 - step(dissolveThreshold, combinedDissolve));
            
            // 홀로그램 스캔라인 효과
            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float scanline = sin(screenUV.y * _ScanlineFrequency + _Time.y * _ScanlineSpeed);
            float hologramEffect = saturate(scanline * 0.5 + 0.5);
            
            // 최종 색상 계산
            o.Albedo = c.rgb;
            o.Emission = _DissolveEdgeColor.rgb * dissolveEdge * _DissolveEdgeEmission;
            
            // 알파 계산
            float finalAlpha = 1 - step(dissolveThreshold, combinedDissolve);
            finalAlpha *= _HologramAlpha * hologramEffect;
            
            // 글리치 효과
            if (_GlitchIntensity > 0)
            {
                float glitchEffect = tex2D(_GlitchTex, IN.uv_MainTex + _Time.y * _GlitchSpeed).r;
                o.Albedo = lerp(o.Albedo, float3(1, 0, 1), glitchEffect * _GlitchIntensity);
                finalAlpha *= lerp(1, glitchEffect, _GlitchIntensity * 0.5);
            }
            
            o.Alpha = finalAlpha;
        }
        ENDCG
    }
    
    FallBack "Transparent/Diffuse"
} 