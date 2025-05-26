Shader "Custom/CyberFragmentShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        
        [Header(Dissolve)]
        _DissolveMap ("Dissolve Map", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        
        [Header(Edge Glow)]
        _EdgeColor ("Edge Color", Color) = (0,0.5,1,1)
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05
        
        [Header(Grid Effect)]
        _GridTex ("Grid Texture", 2D) = "white" {}
        _GridScale ("Grid Scale", Range(0.1, 10)) = 5
        _GridIntensity ("Grid Intensity", Range(0, 1)) = 0.5
        
        [Header(Emission)]
        _EmissionColor ("Emission Color", Color) = (0,0.5,1,1)
        _EmissionIntensity ("Emission Intensity", Range(0, 5)) = 1
        
        [Header(Scanline)]
        _ScanlineSpeed ("Scanline Speed", Range(0, 10)) = 3
        _ScanlineWidth ("Scanline Width", Range(0, 20)) = 10
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite On
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
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
                float2 dissolveUV : TEXCOORD1;
                float2 gridUV : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
                float3 viewDir : TEXCOORD4;
                float3 normal : NORMAL;
            };
            
            sampler2D _MainTex;
            sampler2D _DissolveMap;
            sampler2D _GridTex;
            float4 _MainTex_ST;
            float4 _DissolveMap_ST;
            float4 _GridTex_ST;
            
            float4 _Color;
            float _DissolveAmount;
            float4 _EdgeColor;
            float _EdgeWidth;
            float _GridScale;
            float _GridIntensity;
            float4 _EmissionColor;
            float _EmissionIntensity;
            float _ScanlineSpeed;
            float _ScanlineWidth;
            float _ScanlineIntensity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.dissolveUV = TRANSFORM_TEX(v.uv, _DissolveMap);
                o.gridUV = v.uv * _GridScale;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 기본 텍스처 색상
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // 디졸브 효과
                float dissolve = tex2D(_DissolveMap, i.dissolveUV).r;
                float dissolveThreshold = _DissolveAmount - 0.0001;
                
                // 엣지 글로우 계산
                float edge = 1 - saturate((dissolve - dissolveThreshold) / _EdgeWidth);
                
                // 그리드 패턴
                fixed4 grid = tex2D(_GridTex, i.gridUV);
                
                // 스캔라인 효과
                float scanline = step(frac(i.worldPos.y * _ScanlineWidth + _Time.y * _ScanlineSpeed), 0.5) * _ScanlineIntensity;
                
                // 프레넬 효과 (가장자리 글로우)
                float fresnel = pow(1.0 - saturate(dot(i.normal, i.viewDir)), 3);
                
                // 최종 색상 계산
                fixed4 finalColor = col;
                
                // 그리드 패턴 혼합
                finalColor.rgb = lerp(finalColor.rgb, grid.rgb * _EmissionColor.rgb, grid.r * _GridIntensity);
                
                // 엣지 글로우 적용
                finalColor.rgb = lerp(finalColor.rgb, _EdgeColor.rgb * 2, edge * edge);
                
                // 스캔라인 혼합
                finalColor.rgb += scanline * _EmissionColor.rgb;
                
                // 이미션 효과 (전체적인 글로우)
                finalColor.rgb += _EmissionColor.rgb * _EmissionIntensity * 0.3;
                
                // 프레넬 효과 적용
                finalColor.rgb += _EdgeColor.rgb * fresnel * 0.7;
                
                // 디졸브 적용 (알파값 계산)
                finalColor.a *= step(dissolveThreshold, dissolve);
                
                // 알파값이 너무 낮은 경우 완전히 투명하게
                if (finalColor.a < 0.05) discard;
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}