Shader "Skybox/GridSky" 
{
     Properties
    {
        _GridColor ("Grid Color", Color) = (0,1,1,1) // 네온 블루
        _BackgroundColor ("Background Color", Color) = (0.18, 0.02, 0.33,1) // 다크 퍼플
        _GridScale ("Grid Scale", Float) = 20.0
        _LineThickness ("Line Thickness", Range(0,1)) = 0.02
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Skybox" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _GridColor;
            fixed4 _BackgroundColor;
            float _GridScale;
            float _LineThickness;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldDir : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldDir = normalize(mul(unity_ObjectToWorld, v.vertex).xyz);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.worldDir);
                float theta = atan2(dir.z, dir.x); // -PI to PI
                float phi = acos(dir.y); // 0 to PI

                // Normalize theta and phi to [0,1]
                float u = (theta + UNITY_PI) / (2.0 * UNITY_PI);
                float v = phi / UNITY_PI;

                // Compute the grid lines for longitude (theta) and latitude (phi)
                float gridU = frac(u * _GridScale);
                float gridV = frac(v * _GridScale);

                // Calculate distance to nearest grid line
                float distU = min(gridU, 1.0 - gridU);
                float distV = min(gridV, 1.0 - gridV);

                // Determine if within line thickness with smoothstep for anti-aliasing
                float lineU = 1.0 - smoothstep(_LineThickness, _LineThickness + 0.001, distU);
                float lineV = 1.0 - smoothstep(_LineThickness, _LineThickness + 0.001, distV);

                // Combine grid lines
                float grid = max(lineU, lineV);

                // Blend colors based on grid
                fixed4 color = lerp(_BackgroundColor, _GridColor, grid);
                return color;
            }
            ENDCG
        }
    }
}