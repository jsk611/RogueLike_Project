// 1. Editor 폴더에 넣을 에디터 스크립트
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class VoxelBossEditorWindow : EditorWindow
{
    private VoxelGenerator selectedGenerator;
    private BossType selectedBossType = BossType.Ransomware;
    private Material voxelMaterial;
    private Material glitchMaterial;
    private float voxelSize = 1f;
    private Vector3 bossSize = new Vector3(8, 8, 8);

    // 미리보기용
    private bool showPreview = true;
    private Color previewColor = Color.white;

    [MenuItem("Tools/Voxel Boss Generator")]
    public static void ShowWindow()
    {
        VoxelBossEditorWindow window = GetWindow<VoxelBossEditorWindow>("복셀 보스 생성기");
        window.minSize = new Vector2(300, 400);
    }

    void OnGUI()
    {
        GUILayout.Label("복셀 보스 생성 도구", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // 기본 설정
        GUILayout.Label("기본 설정", EditorStyles.boldLabel);
        selectedGenerator = (VoxelGenerator)EditorGUILayout.ObjectField("복셀 생성기", selectedGenerator, typeof(VoxelGenerator), true);

        if (selectedGenerator == null)
        {
            EditorGUILayout.HelpBox("Scene에 VoxelGenerator가 있는 오브젝트를 선택하세요.", MessageType.Warning);

            if (GUILayout.Button("새 복셀 생성기 만들기"))
            {
                CreateNewVoxelGenerator();
            }
        }

        EditorGUILayout.Space();

        // 보스 타입 선택
        GUILayout.Label("보스 타입", EditorStyles.boldLabel);
        selectedBossType = (BossType)EditorGUILayout.EnumPopup("보스 종류", selectedBossType);

        EditorGUILayout.Space();

        // 복셀 설정
        GUILayout.Label("복셀 설정", EditorStyles.boldLabel);
        voxelSize = EditorGUILayout.FloatField("복셀 크기", voxelSize);
        bossSize = EditorGUILayout.Vector3Field("보스 크기", bossSize);

        voxelMaterial = (Material)EditorGUILayout.ObjectField("기본 머티리얼", voxelMaterial, typeof(Material), false);
        glitchMaterial = (Material)EditorGUILayout.ObjectField("글리치 머티리얼", glitchMaterial, typeof(Material), false);

        EditorGUILayout.Space();

        // 미리보기
        showPreview = EditorGUILayout.Toggle("미리보기 표시", showPreview);
        if (showPreview)
        {
            previewColor = EditorGUILayout.ColorField("미리보기 색상", previewColor);
        }

        EditorGUILayout.Space();

        // 버튼들
        GUI.enabled = selectedGenerator != null;

        if (GUILayout.Button("복셀 보스 생성", GUILayout.Height(30)))
        {
            GenerateVoxelBoss();
        }

        if (GUILayout.Button("기존 복셀 지우기"))
        {
            ClearExistingVoxels();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("프리팹으로 저장"))
        {
            SaveAsPrefab();
        }

        GUI.enabled = true;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("1. 복셀 생성기를 선택하세요\n2. 보스 타입을 골라주세요\n3. '복셀 보스 생성' 버튼을 누르세요", MessageType.Info);
    }

    private void CreateNewVoxelGenerator()
    {
        GameObject newObj = new GameObject("VoxelBossGenerator");
        VoxelGenerator generator = newObj.AddComponent<VoxelGenerator>();

        // 기본값 설정
        generator.voxelSize = voxelSize;
        generator.bossSize = bossSize;

        selectedGenerator = generator;
        Selection.activeGameObject = newObj;

        Debug.Log("새 복셀 생성기가 생성되었습니다!");
    }

    private void GenerateVoxelBoss()
    {
        if (selectedGenerator == null) return;

        // 설정 적용
        selectedGenerator.voxelSize = voxelSize;
        selectedGenerator.bossSize = bossSize;
        selectedGenerator.voxelMaterial = voxelMaterial;
        selectedGenerator.glitchMaterial = glitchMaterial;

        // 생성
        selectedGenerator.GeneratePixelatedBoss(selectedBossType);

        // Undo 지원
        Undo.RegisterCompleteObjectUndo(selectedGenerator.gameObject, "Generate Voxel Boss");

        Debug.Log($"{selectedBossType} 복셀 보스가 생성되었습니다!");
    }

    private void ClearExistingVoxels()
    {
        if (selectedGenerator == null) return;

        Undo.RegisterCompleteObjectUndo(selectedGenerator.gameObject, "Clear Voxels");

        Debug.Log("기존 복셀들이 제거되었습니다.");
    }

    private void SaveAsPrefab()
    {
        if (selectedGenerator == null) return;

        string path = EditorUtility.SaveFilePanelInProject(
            "프리팹 저장",
            $"VoxelBoss_{selectedBossType}",
            "prefab",
            "복셀 보스 프리팹을 저장할 위치를 선택하세요");

        if (!string.IsNullOrEmpty(path))
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(selectedGenerator.gameObject, path);
            Debug.Log($"복셀 보스 프리팹이 저장되었습니다: {path}");

            // 프로젝트 창에서 하이라이트
            EditorGUIUtility.PingObject(prefab);
        }
    }
}