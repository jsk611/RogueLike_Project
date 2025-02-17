using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TextureGenerator : EditorWindow
{
    private int textureSize = 512;
    private float noiseScale = 20f;
    private int voronoiPoints = 32;
    private float voronoiSharpness = 5f;

    [MenuItem("Tools/Generate Death Reaper Textures")]
    public static void ShowWindow()
    {
        GetWindow<TextureGenerator>("Texture Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Texture Generator Settings", EditorStyles.boldLabel);

        textureSize = EditorGUILayout.IntField("Texture Size", textureSize);
        noiseScale = EditorGUILayout.FloatField("Noise Scale", noiseScale);
        voronoiPoints = EditorGUILayout.IntField("Voronoi Points", voronoiPoints);
        voronoiSharpness = EditorGUILayout.FloatField("Voronoi Sharpness", voronoiSharpness);

        if (GUILayout.Button("Generate Textures"))
        {
            GenerateNoiseTexture();
            GenerateVoronoiTexture();
        }
    }

    private void GenerateNoiseTexture()
    {
        Texture2D noiseTex = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float xCoord = (float)x / textureSize * noiseScale;
                float yCoord = (float)y / textureSize * noiseScale;

                // Fractal Noise 생성
                float noise = 0f;
                float amplitude = 1f;
                float frequency = 1f;

                for (int i = 0; i < 4; i++)
                {
                    noise += amplitude * Mathf.PerlinNoise(xCoord * frequency, yCoord * frequency);
                    amplitude *= 0.5f;
                    frequency *= 2f;
                }

                noise = Mathf.Clamp01(noise);
                pixels[y * textureSize + x] = new Color(noise, noise, noise, 1);
            }
        }

        noiseTex.SetPixels(pixels);
        noiseTex.Apply();

        SaveTexture(noiseTex, "NoiseTexture");
    }

    private void GenerateVoronoiTexture()
    {
        Texture2D voronoiTex = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];

        // Voronoi 포인트 생성
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < voronoiPoints; i++)
        {
            points.Add(new Vector2(
                Random.Range(0f, textureSize),
                Random.Range(0f, textureSize)
            ));
        }

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                Vector2 pixelPos = new Vector2(x, y);
                float minDist = float.MaxValue;
                float secondMinDist = float.MaxValue;

                // 가장 가까운 두 포인트까지의 거리 찾기
                foreach (Vector2 point in points)
                {
                    float dist = Vector2.Distance(pixelPos, point);
                    if (dist < minDist)
                    {
                        secondMinDist = minDist;
                        minDist = dist;
                    }
                    else if (dist < secondMinDist)
                    {
                        secondMinDist = dist;
                    }
                }

                // 엣지 검출을 위한 차이값 계산
                float diff = (secondMinDist - minDist) / voronoiSharpness;
                float value = Mathf.Clamp01(diff);

                // 노이즈 추가
                float noise = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) * 0.1f;
                value = Mathf.Clamp01(value + noise);

                pixels[y * textureSize + x] = new Color(value, value, value, 1);
            }
        }

        voronoiTex.SetPixels(pixels);
        voronoiTex.Apply();

        SaveTexture(voronoiTex, "VoronoiTexture");
    }

    private void SaveTexture(Texture2D texture, string name)
    {
        byte[] bytes = texture.EncodeToPNG();
        string path = EditorUtility.SaveFilePanel(
            "Save Texture",
            "Assets",
            name + ".png",
            "png"
        );

        if (!string.IsNullOrEmpty(path))
        {
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();

            // 텍스처 임포트 설정
            string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }
    }
}