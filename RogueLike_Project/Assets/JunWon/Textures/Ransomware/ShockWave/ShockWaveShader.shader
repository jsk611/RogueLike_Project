Shader "Custom/AdvancedExplosionShader"
{
   Properties
   {
        _MainTex ("Base Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DataTex ("Data Pattern", 2D) = "white" {}
        
        _ExplosionRadius ("Explosion Radius", Range(0, 20)) = 0
        _Distortion ("Distortion Amount", Range(0, 1)) = 0.2
        
        _CoreColor ("Core Color", Color) = (0,1,1,1)
        _RimColor ("Rim Color", Color) = (1,0,1,1)
        _DataColor ("Data Color", Color) = (0,1,0,1)
        
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.5
        _GlitchSpeed ("Glitch Speed", Range(0, 10)) = 5
        _DataFlowSpeed ("Data Flow Speed", Range(0, 10)) = 2
        
        _DissolveEdge ("Dissolve Edge", Range(0, 1)) = 0.1
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
   }
    
SubShader
{
    Tags { "RenderType"="Transparent" "Queue"="Transparent" }
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
            float3 worldNormal : TEXCOORD1;
            float3 worldPos : TEXCOORD2;
            float3 viewDir : TEXCOORD3;
        };
            
        sampler2D _MainTex, _NoiseTex, _DataTex;
        float4 _MainTex_ST;
        float _ExplosionRadius, _Distortion;
        float4 _CoreColor, _RimColor, _DataColor;
        float _GlitchIntensity, _GlitchSpeed, _DataFlowSpeed;
        float _DissolveEdge, _DissolveAmount;
            
        v2f vert (appdata v)
        {
            v2f o;
                
            // 폭발에 따른 버텍스 변위
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            float dist = length(worldPos);
            float explosionFactor = saturate(1 - dist / _ExplosionRadius);
            v.vertex.xyz += v.normal * explosionFactor * _Distortion;
                
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            o.worldNormal = UnityObjectToWorldNormal(v.normal);
            o.worldPos = worldPos;
            o.viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                
            return o;
        }
            
        float random(float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
        }
            
        float3 glitch(float2 uv)
        {
            float2 gridUV = floor(uv * 50) / 50;
            float randomVal = random(gridUV + _Time.y * _GlitchSpeed);
                
            float glitchLine = step(0.98, randomVal);
            float lineOffset = (randomVal - 0.5) * 0.1 * _GlitchIntensity;
                
            float2 glitchedUV = uv + float2(lineOffset * glitchLine, 0);
            return tex2D(_MainTex, glitchedUV).rgb;
        }
            
        fixed4 frag (v2f i) : SV_Target
        {
            // 노이즈 텍스처 샘플링 (디졸브 효과용)
            float2 noiseUV = i.uv + _Time.y * 0.1;
            float noise = tex2D(_NoiseTex, noiseUV).r;
                
            // 데이터 패턴 애니메이션
            float2 dataUV = i.uv + _Time.y * _DataFlowSpeed;
            float dataPattern = tex2D(_DataTex, dataUV).r;
                
            // 림 라이트 계산
            float rim = 1.0 - saturate(dot(i.viewDir, i.worldNormal));
            rim = pow(rim, 3);
                
            // 글리치 효과
            float3 glitchColor = glitch(i.uv);
                
            // 디졸브 효과
            float dissolveEdge = step(noise, _DissolveAmount) - 
                                step(noise, _DissolveAmount - _DissolveEdge);
                
            // 최종 색상 계산
            float3 finalColor = lerp(_CoreColor.rgb, _RimColor.rgb, rim);
            finalColor = lerp(finalColor, _DataColor.rgb, dataPattern * 0.5);
            finalColor = lerp(finalColor, glitchColor, _GlitchIntensity * 0.5);
                
            // 폭발 반경에 따른 알파값
            float alpha = 1 - _DissolveAmount;
            alpha *= 1 - dissolveEdge;
                
            // 코어 부분 발광 효과
            float corePulse = 0.5 + 0.5 * sin(_Time.y * 5);
            finalColor += _CoreColor.rgb * corePulse * (1 - _DissolveAmount);
                
            return float4(finalColor, alpha);
        }
        ENDCG
    }
}
}
