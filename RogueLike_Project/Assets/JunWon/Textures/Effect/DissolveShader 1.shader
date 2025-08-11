Shader "Custom/LitDissolve_Standard"
{
    Properties
    {
        // === 기존 외형 유지용 ===
        _MainTex ("Base Map", 2D) = "white" {}
        _Color   ("Base Color", Color) = (1,1,1,1)
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Metallic ("Metallic", Range(0,1)) = 0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _EmissionColor ("Emission Color", Color) = (0,0,0,0)

        // === Dissolve ===
        _NoiseTex ("Noise", 2D) = "gray" {}
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 1
        _EdgeWidth ("Edge Width", Range(0.001, 0.2)) = 0.05
        _EdgeColor ("Edge Color", Color) = (0,1,1,1)
        _EdgeIntensity ("Edge Intensity", Range(0,10)) = 3

        // === 셰이더 내부 애니메이션 컨트롤 ===
        _StartTime ("Start Time (sec)", Float) = 0
        _Duration  ("Duration (sec)", Float) = 1
        _Direction ("Direction (+1=Disappear, -1=Appear)", Float) = 1
        _Delay     ("Delay (sec)", Float) = 0

        // 방향성 디졸브(선택)
        _DissolveDir ("Dissolve Dir (Object)", Vector) = (0,1,0,0)
        _PlaneOffset ("Plane Offset", Float) = 0
    }

    SubShader
    {
        Tags{ "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow alpha:clip
        #pragma target 3.0

        sampler2D _MainTex, _BumpMap, _NoiseTex;
        fixed4 _Color, _EmissionColor, _EdgeColor;
        half _Metallic, _Glossiness;
        half _NoiseScale, _EdgeWidth, _EdgeIntensity;

        // 애니 파라미터
        half _StartTime, _Duration, _Direction, _Delay;
        float4 _DissolveDir;
        half _PlaneOffset;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 worldPos;
        };

        // 유틸
        float3 ObjPos(float3 worldPos)
        {
            return mul(unity_WorldToObject, float4(worldPos,1)).xyz;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // === 기본 PBR ===
            fixed4 baseCol = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = baseCol.rgb;
            o.Alpha  = 1;

            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Emission = _EmissionColor.rgb;

            // === Dissolve 진행도(셰이더 내부에서 시간 계산) ===
            // _Time.y = seconds. 프로퍼티로 _Time을 만들지 말 것!
            float t = _Time.y;
            float prog = saturate( (t - _StartTime - _Delay) / max(_Duration, 1e-5) );
            // 방향: +1 이면 0->1(사라짐), -1 이면 1->0(나타남)
            float dirStep = step(0.0, _Direction);
            float dissolve = lerp(1.0 - prog, prog, dirStep);

            // === 마스크 계산 ===
            float3 objPos = ObjPos(IN.worldPos);
            float2 uvN = objPos.xy * _NoiseScale;                // 오브젝트 기준 UV
            float n = tex2D(_NoiseTex, uvN).r;

            // 방향성(원하면 grad 가중치 추가)
            float3 dir = normalize(_DissolveDir.xyz);
            float grad = dot(dir, objPos) + _PlaneOffset;

            float mask = n; // + 0.0 * grad;  // 필요 시 가중합

            // === clip & edge ===
            float edge = smoothstep(dissolve - _EdgeWidth, dissolve, mask);
            clip(edge - 1e-4);                                   // 픽셀 버리기(섀도우 포함)

            // 가장자리 발광
            float band = saturate(
                smoothstep(dissolve, dissolve + _EdgeWidth, mask) -
                smoothstep(dissolve - _EdgeWidth, dissolve, mask)
            );
            o.Emission += band * _EdgeColor.rgb * _EdgeIntensity;
        }
        ENDCG
    }
    FallBack "Transparent/Cutout/VertexLit"
}
