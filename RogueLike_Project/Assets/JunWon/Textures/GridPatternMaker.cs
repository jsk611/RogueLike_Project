using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridPatternMaker : EditorWindow
{
    private int textureSize = 512;
    private int blockSize = 4;  // DXT5 스타일의 블록 크기
    private float noiseScale = 20f;
    private float contrast = 1.5f;
    private float threshold = 0.5f;
    private bool useCompression = true;

    [MenuItem("Tools/Digital Noise Generator")]
    public static void ShowWindow()
    {
        GetWindow<GridPatternMaker>("Digital Noise");
    }

    private void OnGUI()
    {
        GUILayout.Label("Digital Noise Settings", EditorStyles.boldLabel);

        textureSize = EditorGUILayout.IntField("Texture Size", textureSize);
        blockSize = EditorGUILayout.IntField("Block Size", blockSize);
        noiseScale = EditorGUILayout.FloatField("Noise Scale", noiseScale);
        contrast = EditorGUILayout.FloatField("Contrast", contrast);
        threshold = EditorGUILayout.Slider("Threshold", threshold, 0f, 1f);
        useCompression = EditorGUILayout.Toggle("Use DXT5 Compression", useCompression);

        if (GUILayout.Button("Generate Noise Texture"))
        {
            GenerateNoiseTexture();
        }
    }

    private float Random2D(Vector2 st)
    {
        // Frac는 소수점 부분만 반환하는 함수입니다.
        // x - floor(x)와 동일한 결과를 얻을 수 있습니다.
        float value = Mathf.Sin(Vector2.Dot(st, new Vector2(12.9898f, 78.233f))) * 43758.5453123f;
        return value - Mathf.Floor(value);
    }

    private void GenerateNoiseTexture()
    {
        Texture2D texture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];

        // 블록 단위로 노이즈 생성
        for (int y = 0; y < textureSize; y += blockSize)
        {
            for (int x = 0; x < textureSize; x += blockSize)
            {
                // 블록의 기본 노이즈 값 생성
                float blockNoiseBase = Random2D(new Vector2(
                    x / (float)textureSize * noiseScale,
                    y / (float)textureSize * noiseScale
                ));

                // 명암 대비 적용
                blockNoiseBase = Mathf.Pow(blockNoiseBase, contrast);

                // 임계값 적용
                float blockValue = blockNoiseBase > threshold ? 1 : 0;

                // 블록 내의 모든 픽셀에 적용
                for (int by = 0; by < blockSize && (y + by) < textureSize; by++)
                {
                    for (int bx = 0; bx < blockSize && (x + bx) < textureSize; bx++)
                    {
                        // 블록 내 개별 픽셀에 약간의 변화 추가
                        float pixelNoise = Random2D(new Vector2(bx, by)) * 0.1f;
                        float finalValue = Mathf.Clamp01(blockValue + pixelNoise);

                        pixels[(y + by) * textureSize + (x + bx)] = new Color(finalValue, finalValue, finalValue, 1);
                    }
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        // 저장
        string path = EditorUtility.SaveFilePanel(
            "Save Noise Texture",
            "Assets",
            "DigitalNoise.png",
            "png"
        );

        if (!string.IsNullOrEmpty(path))
        {
            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();

            // 텍스처 임포트 설정
            string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Point; // 픽셀화된 느낌을 위해

                if (useCompression)
                {
                    importer.textureCompression = TextureImporterCompression.CompressedHQ;
                    importer.compressionQuality = 50; // 압축 품질 조절
                }
                else
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                }

                importer.SaveAndReimport();
            }
        }
    }
}
