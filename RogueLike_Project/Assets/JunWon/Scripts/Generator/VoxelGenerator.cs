using UnityEngine;
using System.Collections.Generic;

public class VoxelGenerator : MonoBehaviour
{
    [System.Serializable]
    public class VoxelData
    {
        public Vector3 position;
        public Color color;
        public float glitchIntensity = 0f;
    }

    [Header("Voxel Settings")]
    public Material voxelMaterial;
    public float voxelSize = 1f;
    public Vector3 bossSize = new Vector3(8, 8, 8);

    [Header("Glitch Effects")]
    public Material glitchMaterial;
    public float glitchSpeed = 1f;

    private List<GameObject> voxels = new List<GameObject>();
    private Dictionary<Vector3, VoxelData> voxelMap = new Dictionary<Vector3, VoxelData>();

    public void GeneratePixelatedBoss(BossType bossType)
    {
        ClearExistingVoxels();

        switch (bossType)
        {
            case BossType.Ransomware:
                GenerateRansomwareBoss();
                break;
            case BossType.Trojan:
                GenerateTrojanBoss();
                break;
            case BossType.Worm:
                GenerateWormBoss();
                break;
            case BossType.UnknownVirus:
                GenerateCorruptedBoss();
                break;
        }

        ApplyGlitchEffects();
    }

    private void GenerateRansomwareBoss()
    {
        // 록 모양의 픽셀화된 보스
        Color lockColor = new Color(0.8f, 0.2f, 0.2f); // 빨간색
        Color keyColor = new Color(0.9f, 0.9f, 0.1f);  // 노란색

        // 자물쇠 몸체
        for (int x = 2; x <= 5; x++)
        {
            for (int y = 2; y <= 5; y++)
            {
                for (int z = 2; z <= 5; z++)
                {
                    CreateVoxel(new Vector3(x, y, z), lockColor);
                }
            }
        }

        // 자물쇠 고리
        for (int x = 3; x <= 4; x++)
        {
            CreateVoxel(new Vector3(x, 6, 3), lockColor);
            CreateVoxel(new Vector3(x, 6, 4), lockColor);
            CreateVoxel(new Vector3(x, 7, 3), lockColor);
            CreateVoxel(new Vector3(x, 7, 4), lockColor);
        }

        // 열쇠 구멍
        CreateVoxel(new Vector3(3, 3, 2), keyColor, 0.5f);
        CreateVoxel(new Vector3(4, 3, 2), keyColor, 0.5f);
        CreateVoxel(new Vector3(3, 4, 2), keyColor, 0.5f);
    }

    private void GenerateTrojanBoss()
    {
        // 트로이 목마 모양
        Color horseColor = new Color(0.6f, 0.4f, 0.2f); // 갈색
        Color eyeColor = new Color(1f, 0f, 0f);          // 빨간 눈

        // 말 몸체
        for (int x = 1; x <= 6; x++)
        {
            for (int y = 2; y <= 4; y++)
            {
                for (int z = 2; z <= 5; z++)
                {
                    CreateVoxel(new Vector3(x, y, z), horseColor);
                }
            }
        }

        // 말 목
        for (int y = 4; y <= 6; y++)
        {
            CreateVoxel(new Vector3(1, y, 3), horseColor);
            CreateVoxel(new Vector3(1, y, 4), horseColor);
        }

        // 빨간 눈
        CreateVoxel(new Vector3(0, 5, 3), eyeColor, 1f);
        CreateVoxel(new Vector3(0, 5, 4), eyeColor, 1f);

        // 다리
        for (int leg = 0; leg < 4; leg++)
        {
            int x = (leg < 2) ? 2 : 5;
            int z = (leg % 2 == 0) ? 2 : 5;
            CreateVoxel(new Vector3(x, 0, z), horseColor);
            CreateVoxel(new Vector3(x, 1, z), horseColor);
        }
    }

    private void GenerateWormBoss()
    {
        // 지렁이 형태의 세그먼트
        Color wormColor = new Color(0.2f, 0.8f, 0.2f); // 초록색
        Color segmentColor = new Color(0.1f, 0.6f, 0.1f);

        int segments = 12;
        for (int seg = 0; seg < segments; seg++)
        {
            float angle = seg * 0.5f;
            int x = (int)(4 + Mathf.Sin(angle) * 2);
            int z = (int)(4 + Mathf.Cos(angle) * 2);
            int y = 2 + seg / 3;

            Color segColor = (seg % 2 == 0) ? wormColor : segmentColor;
            float glitch = seg == 0 ? 0.8f : 0.2f; // 머리 부분만 강한 글리치

            // 세그먼트 크기를 머리쪽으로 갈수록 크게
            int size = (seg == 0) ? 2 : 1;

            for (int dx = -size; dx <= size; dx++)
            {
                for (int dy = -size; dy <= size; dy++)
                {
                    for (int dz = -size; dz <= size; dz++)
                    {
                        CreateVoxel(new Vector3(x + dx, y + dy, z + dz), segColor, glitch);
                    }
                }
            }
        }
    }

    private void GenerateCorruptedBoss()
    {
        // 손상된 데이터 형태
        Color[] corruptColors = {
            new Color(1f, 0f, 1f), // 마젠타
            new Color(0f, 1f, 1f), // 시안
            new Color(1f, 1f, 0f), // 노란색
            Color.white,
            Color.black
        };

        // 랜덤하게 깨진 형태
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int z = 0; z < 8; z++)
                {
                    if (Random.value < 0.6f) // 60% 확률로 복셀 생성
                    {
                        Color randomColor = corruptColors[Random.Range(0, corruptColors.Length)];
                        float glitchIntensity = Random.Range(0.3f, 1f);
                        CreateVoxel(new Vector3(x, y, z), randomColor, glitchIntensity);
                    }
                }
            }
        }
    }

    private void CreateVoxel(Vector3 gridPos, Color color, float glitchIntensity = 0f)
    {
        GameObject voxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        voxel.transform.parent = transform;
        voxel.transform.localPosition = gridPos * voxelSize;
        voxel.transform.localScale = Vector3.one * voxelSize;

        Renderer renderer = voxel.GetComponent<Renderer>();

        if (glitchIntensity > 0.3f && glitchMaterial != null)
        {
            renderer.material = new Material(glitchMaterial);
            renderer.material.SetFloat("_GlitchIntensity", glitchIntensity);
        }
        else
        {
            renderer.material = new Material(voxelMaterial);
        }

        renderer.material.color = color;

        voxels.Add(voxel);
        voxelMap[gridPos] = new VoxelData { position = gridPos, color = color, glitchIntensity = glitchIntensity };
    }

    private void ApplyGlitchEffects()
    {
        StartCoroutine(GlitchAnimation());
    }

    private System.Collections.IEnumerator GlitchAnimation()
    {
        while (true)
        {
            foreach (var voxel in voxels)
            {
                if (voxel == null) continue;

                Renderer renderer = voxel.GetComponent<Renderer>();
                if (renderer.material.HasProperty("_GlitchIntensity"))
                {
                    float noise = Mathf.PerlinNoise(Time.time * glitchSpeed, voxel.transform.position.x);
                    renderer.material.SetFloat("_GlitchIntensity", noise);

                    // 랜덤 위치 이동 (글리치 효과)
                    if (Random.value < 0.1f)
                    {
                        Vector3 glitchOffset = new Vector3(
                            Random.Range(-0.1f, 0.1f),
                            Random.Range(-0.1f, 0.1f),
                            Random.Range(-0.1f, 0.1f)
                        );
                        voxel.transform.localPosition += glitchOffset;
                    }
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void ClearExistingVoxels()
    {
        foreach (var voxel in voxels)
        {
            if (voxel != null) DestroyImmediate(voxel);
        }
        voxels.Clear();
        voxelMap.Clear();
    }
}

// 2. 픽셀 아트 스타일 설정
[CreateAssetMenu(fileName = "PixelArtSettings", menuName = "Boss/Pixel Art Settings")]
public class PixelArtSettings : ScriptableObject
{
    [Header("Pixel Art Settings")]
    public int pixelsPerUnit = 16;
    public FilterMode filterMode = FilterMode.Point;
    public bool generateMipMaps = false;

    [Header("Camera Settings")]
    public bool usePixelPerfectCamera = true;
    public int referenceResolution = 320;

    [Header("Shader Settings")]
    public Shader pixelShader;
    public Shader glitchShader;

    public void ApplySettings()
    {
        // 모든 텍스처에 픽셀 아트 설정 적용
        var textures = Resources.FindObjectsOfTypeAll<Texture2D>();
        foreach (var texture in textures)
        {
            ApplyPixelArtSettings(texture);
        }
    }

    private void ApplyPixelArtSettings(Texture2D texture)
    {
        if (texture == null) return;

        string path = UnityEditor.AssetDatabase.GetAssetPath(texture);
        if (string.IsNullOrEmpty(path)) return;

        var importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
        if (importer == null) return;

        importer.textureType = UnityEditor.TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = filterMode;
        importer.mipmapEnabled = generateMipMaps;

        UnityEditor.AssetDatabase.ImportAsset(path);
    }
}

// 3. 보스 변신 시 픽셀화 적용

// 4. 컴퓨터 바이러스 테마 글리치 셰이더 (ShaderLab)
/*
Shader "Custom/VirusGlitch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _GlitchIntensity ("Glitch Intensity", Range(0,1)) = 0.5
        _GlitchSpeed ("Glitch Speed", Float) = 1.0
        _PixelSize ("Pixel Size", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _GlitchIntensity;
            float _GlitchSpeed;
            float _PixelSize;

            v2f vert (appdata v)
            {
                v2f o;
                
                // 글리치 효과로 버텍스 위치 약간 이동
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float glitch = sin(_Time.y * _GlitchSpeed + worldPos.x * 10) * _GlitchIntensity * 0.1;
                v.vertex.xyz += float3(glitch, glitch * 0.5, 0);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = worldPos;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 픽셀화 효과
                float2 pixelUV = floor(i.uv * _PixelSize) / _PixelSize;
                
                // 글리치 색상 효과
                float glitchR = sin(_Time.y * _GlitchSpeed * 2 + i.worldPos.x) * _GlitchIntensity;
                float glitchG = cos(_Time.y * _GlitchSpeed * 1.5 + i.worldPos.y) * _GlitchIntensity;
                float glitchB = sin(_Time.y * _GlitchSpeed * 3 + i.worldPos.z) * _GlitchIntensity;
                
                fixed4 col = tex2D(_MainTex, pixelUV) * _Color;
                
                // 글리치 색상 추가
                col.r += glitchR * 0.3;
                col.g += glitchG * 0.3;
                col.b += glitchB * 0.3;
                
                // 랜덤 노이즈
                float noise = frac(sin(dot(pixelUV, float2(12.9898, 78.233))) * 43758.5453);
                if (noise < _GlitchIntensity * 0.1)
                {
                    col.rgb = 1 - col.rgb; // 색상 반전
                }
                
                return col;
            }
            ENDCG
        }
    }
}
*/

public enum BossType
{
    UnknownVirus,
    Ransomware,
    Trojan,
    Worm
}