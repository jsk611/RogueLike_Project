using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(TeleportEffectController))]
public class TeleportEffectControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TeleportEffectController controller = (TeleportEffectController)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Test Dissolve Effect"))
        {
            controller.TestDissolveEffect();
        }

        if (GUILayout.Button("Test Teleport To Random Position"))
        {
            controller.TestRandomTeleport();
        }

        if (GUILayout.Button("Reset Effect"))
        {
            controller.ResetEffect();
        }
    }
}
#endif


 
