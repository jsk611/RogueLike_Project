using UnityEditor;
using UnityEngine;

public class ShaderConverter : EditorWindow
{
    [MenuItem("Tools/Convert All Materials to URP")]
    public static void ConvertMaterialsToURP()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        int count = guids.Length;
        for (int i = 0; i < count; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat.shader.name.Contains("Standard"))
            {
                mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                Debug.Log($"Converted: {path}");
            }
        }
        Debug.Log("All materials converted to URP.");
    }
}
