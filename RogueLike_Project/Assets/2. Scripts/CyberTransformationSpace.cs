using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 🏢 15층 건물 캡슐 변신 시스템 - 큐브들이 나선형으로 배치되어 15층 건물 캡슐을 형성하고 층별로 해체되며 몬스터 등장
/// 각 층이 명확하게 구분되어 15층부터 1층까지 순서대로 해체됩니다.
/// </summary>
public class CyberTransformationSpace : MonoBehaviour
{
    [Header("🌀 Spiral Capsule Settings")]
    [SerializeField] private float capsuleRadius = 4f;          // 캡슐 반지름
    [SerializeField] private float capsuleHeight = 8f;          // 캡슐 높이
    [SerializeField] private int buildingFloors = 15;           // 🏢 건물 층수 (15층 건물)
    [SerializeField] private float spiralTightness = 2f;        // 나선 밀도 (높을수록 촘촘)
    [SerializeField] private float goldenAngle = 137.507764f;   // 황금각도
    
    [Header("🎬 Animation Settings")]
    [SerializeField] private float formationTime = 3f;         // 캡슐 형성 시간
    [SerializeField] private float maintainTime = 2f;          // 유지 시간
    [SerializeField] private float dissolutionTime = 4f;       // 해체 시간
    [SerializeField] private float layerDelay = 0.2f;          // 레이어간 딜레이
    [SerializeField] private AnimationCurve spiralCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("👁️ Visibility Settings")]
    [SerializeField] private float innerVisibility = 0.3f;     // 내부 투명도 (0=완전투명, 1=불투명)
    [SerializeField] private Color capsuleColor = new Color(0.2f, 0.8f, 1f, 0.7f);
    [SerializeField] private Color glowColor = new Color(0.4f, 1f, 0.8f, 1f);
    
    [Header("🎭 Monster Reveal")]
    [SerializeField] private GameObject[] monstersToReveal;     // 등장할 몬스터들
    [SerializeField] private ParticleSystem revealEffectPrefab; // 등장 효과
    [SerializeField] private float monsterRevealDelay = 0.5f;   // 몬스터 등장 딜레이
    
    [Header("🔊 Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip formationSound;
    [SerializeField] private AudioClip dissolutionSound;
    [SerializeField] private AudioClip revealSound;
    
    [Header("🛠️ Debug")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool showGizmos = true;
    
    // Core Variables
    private List<Transform> voxelCubes = new List<Transform>();
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Vector3> originalRotations = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, SpiralData> spiralPositions = new Dictionary<Transform, SpiralData>();
    private Dictionary<Transform, int> voxelFloors = new Dictionary<Transform, int>();          // 🏢 각 큐브의 층수 (1~15층)
    private List<List<Transform>> floorGroups = new List<List<Transform>>();                   // 🏢 층별 큐브 그룹
    
    // State
    private bool isTransforming = false;
    private bool isRevealing = false;
    private Coroutine currentTransformation;
    
    // Spiral Data Structure
    [System.Serializable]
    private struct SpiralData
    {
        public Vector3 position;        // 나선 위치
        public float angle;             // 회전 각도
        public int floor;               // 🏢 건물 층수 (1~15층)
        public float height;            // 높이
        public float radius;            // 반지름
        public float spiralProgress;    // 나선 진행도 (0-1)
    }
    
    void Start()
    {
        InitializeSystem();
    }
    
    void Update()
    {
        if (debugMode)
        {
            HandleDebugInput();
        }
    }
    
    #region Initialization
    
    private void InitializeSystem()
    {
        FindAllVoxels();
        StoreOriginalTransforms();
        CalculateSpiralFormation();
        HideAllMonsters();
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        
        Debug.Log($"[BuildingCapsule] 🏢 시스템 초기화 완료 - {voxelCubes.Count}개 큐브, {buildingFloors}층 건물");
    }
    
    private void FindAllVoxels()
    {
        voxelCubes.Clear();
        
        // 자식 객체에서 모든 큐브 찾기
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child != transform && IsVoxelCube(child))
            {
                voxelCubes.Add(child);
            }
        }
        
        Debug.Log($"[BuildingCapsule] {voxelCubes.Count}개 큐브 발견");
    }
    
    private bool IsVoxelCube(Transform obj)
    {
        // 큐브 식별 조건 (이름, 태그, 컴포넌트 등)
        return obj.name.ToLower().Contains("cube") ||
               obj.name.ToLower().Contains("voxel") ||
               obj.name.ToLower().Contains("block") ||
               obj.GetComponent<MeshRenderer>() != null;
    }
    
    private void StoreOriginalTransforms()
    {
        originalPositions.Clear();
        originalRotations.Clear();
        originalScales.Clear();
        
        foreach (Transform voxel in voxelCubes)
        {
            originalPositions[voxel] = voxel.localPosition;
            originalRotations[voxel] = voxel.localEulerAngles;
            originalScales[voxel] = voxel.localScale;
        }
    }
    
    #endregion
    
    #region Spiral Formation Calculation
    
    private void CalculateSpiralFormation()
    {
        spiralPositions.Clear();
        voxelFloors.Clear();
        floorGroups.Clear();
        
        // 🏢 15층 건물 그룹 초기화 (1층~15층)
        for (int i = 0; i < buildingFloors; i++)
        {
            floorGroups.Add(new List<Transform>());
        }
        
        int voxelCount = voxelCubes.Count;
        
        for (int i = 0; i < voxelCount; i++)
        {
            Transform voxel = voxelCubes[i];
            
            // 나선 진행도 (0-1)
            float spiralProgress = (float)i / (voxelCount - 1);
            
            // 🏢 건물 층별 나선 데이터 계산
            SpiralData spiralData = CalculateBuildingFloorPosition(i, voxelCount, spiralProgress);
            
            spiralPositions[voxel] = spiralData;
            voxelFloors[voxel] = spiralData.floor;
            floorGroups[spiralData.floor - 1].Add(voxel); // 0-based index (1층 = index 0)
        }
        
        // 🏢 층별 통계 출력 (15층부터 1층까지)
        for (int floor = buildingFloors; floor >= 1; floor--)
        {
            int floorIndex = floor - 1; // 0-based
            if (floorGroups[floorIndex].Count > 0)
            {
                Debug.Log($"[BuildingCapsule] {floor}층: {floorGroups[floorIndex].Count}개 큐브");
            }
        }
        
        Debug.Log($"[BuildingCapsule] 🏢 {buildingFloors}층 건물 완성! 총 {voxelCount}개 큐브");
    }
    
    private SpiralData CalculateBuildingFloorPosition(int index, int totalVoxels, float spiralProgress)
    {
        SpiralData data = new SpiralData();
        data.spiralProgress = spiralProgress;
        
        // 🏢 건물 층수 계산 (1층~15층)
        data.floor = Mathf.FloorToInt(spiralProgress * buildingFloors) + 1; // 1층부터 시작
        data.floor = Mathf.Clamp(data.floor, 1, buildingFloors);
        
        // 🏢 층별 고정 높이 계산 (각 층이 명확하게 구분됨)
        data.height = CalculateBuildingFloorHeight(data.floor);
        
        // 🏢 층별 반지름 계산 (캡슐 모양 유지)
        float normalizedFloorHeight = (float)(data.floor - 1) / (buildingFloors - 1); // 0~1
        data.radius = CalculateCapsuleRadius(normalizedFloorHeight);
        
        // 나선 각도 계산 (황금비율 기반)
        data.angle = (index * goldenAngle) % 360f;
        
        // 최종 3D 위치 계산
        float angleRad = data.angle * Mathf.Deg2Rad;
        data.position = new Vector3(
            Mathf.Cos(angleRad) * data.radius,
            data.height,
            Mathf.Sin(angleRad) * data.radius
        );
        
        return data;
    }
    
    /// <summary>
    /// 🏢 건물 층별 고정 높이 계산 (각 층이 명확하게 구분)
    /// </summary>
    private float CalculateBuildingFloorHeight(int floor)
    {
        // 1층~15층을 캡슐 높이에 균등 분배
        float floorHeight = capsuleHeight / buildingFloors;
        
        // 바닥(1층)부터 위(15층)까지 배치
        float bottomY = -capsuleHeight * 0.5f;
        float currentFloorY = bottomY + (floor - 0.5f) * floorHeight; // 층 중앙에 배치
        
        return currentFloorY;
    }
    
    private float CalculateCapsuleHeight(float normalizedHeight)
    {
        // 캡슐 중심을 0으로 하는 높이 (-height/2 ~ +height/2)
        return (normalizedHeight - 0.5f) * capsuleHeight;
    }
    
    private float CalculateCapsuleRadius(float normalizedHeight)
    {
        float halfHeight = capsuleHeight * 0.5f;
        float cylinderHeight = capsuleHeight - (capsuleRadius * 2f);
        float halfCylinderHeight = cylinderHeight * 0.5f;
        
        float currentHeight = CalculateCapsuleHeight(normalizedHeight);
        
        // 중간 원통 부분
        if (Mathf.Abs(currentHeight) <= halfCylinderHeight)
        {
            return capsuleRadius;
        }
        
        // 위아래 둥근 캡 부분
        float capHeight = Mathf.Abs(currentHeight) - halfCylinderHeight;
        float radius = Mathf.Sqrt(Mathf.Max(0, capsuleRadius * capsuleRadius - capHeight * capHeight));
        return Mathf.Max(0.1f, radius);
    }
    
    #endregion
    
    #region Public API
    
    public void StartTransformation()
    {
        if (isTransforming) return;
        
        if (currentTransformation != null)
            StopCoroutine(currentTransformation);
        
        currentTransformation = StartCoroutine(TransformationSequence());
    }
    
    public void StopTransformation()
    {
        if (currentTransformation != null)
        {
            StopCoroutine(currentTransformation);
            currentTransformation = null;
        }
        
        StartCoroutine(RestoreOriginalState());
    }
    
    public bool IsTransforming => isTransforming;
    public bool IsRevealing => isRevealing;
    
    #endregion
    
    #region Main Transformation Sequence
    
    private IEnumerator TransformationSequence()
    {
        isTransforming = true;
        
        Debug.Log("[BuildingCapsule] 🏢 15층 건물 캡슐 변신 시작!");
        
        // Phase 1: 🏢 15층 건물 캡슐 형성
        yield return StartCoroutine(FormBuildingCapsule());
        
        // Phase 2: 캡슐 유지 (내부 살짝 보이게)
        yield return StartCoroutine(MaintainCapsule());
        
        // Phase 3: 🏢 층별 해체 및 몬스터 등장 (15층→1층)
        yield return StartCoroutine(DissolveCapsuleAndRevealMonster());
        
        // Phase 4: 원래 상태 복원
        yield return StartCoroutine(RestoreOriginalState());
        
        isTransforming = false;
        Debug.Log("[BuildingCapsule] ✨ 15층 건물 변신 완료!");
    }
    
    #endregion
    
    #region Phase 1: Spiral Capsule Formation
    
    private IEnumerator FormBuildingCapsule()
    {
        Debug.Log("[BuildingCapsule] 🏢 Phase 1: 15층 건물 캡슐 형성");
        
        PlaySound(formationSound);
        
        float timer = 0f;
        
        while (timer < formationTime)
        {
            float progress = timer / formationTime;
            float curvedProgress = spiralCurve.Evaluate(progress);
            
            // 🏢 모든 큐브를 층별 나선 위치로 이동
            foreach (Transform voxel in voxelCubes)
            {
                if (voxel != null && spiralPositions.ContainsKey(voxel))
                {
                    AnimateVoxelToBuildingPosition(voxel, curvedProgress);
                }
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        // 최종 위치 보정
        foreach (Transform voxel in voxelCubes)
        {
            if (voxel != null && spiralPositions.ContainsKey(voxel))
            {
                SpiralData data = spiralPositions[voxel];
                voxel.localPosition = data.position;
                voxel.localRotation = Quaternion.Euler(0, data.angle, 0);
            }
        }
        
        Debug.Log("[BuildingCapsule] ✅ 15층 건물 캡슐 형성 완료");
    }
    
    private void AnimateVoxelToBuildingPosition(Transform voxel, float progress)
    {
        if (!originalPositions.ContainsKey(voxel) || !spiralPositions.ContainsKey(voxel))
            return;
        
        Vector3 startPos = originalPositions[voxel];
        SpiralData targetData = spiralPositions[voxel];
        
        // 나선형 경로 계산
        Vector3 spiralPath = CalculateSpiralPath(startPos, targetData, progress);
        voxel.localPosition = spiralPath;
        
        // 회전 애니메이션
        float targetAngle = targetData.angle * progress;
        voxel.localRotation = Quaternion.Euler(0, targetAngle, 0);
        
        // 스케일 효과
        float scale = Mathf.Lerp(1f, 0.9f, progress * 0.5f);
        voxel.localScale = originalScales[voxel] * scale;
        
        // 투명도 조절 (내부 가시성)
        SetVoxelTransparency(voxel, progress, targetData.floor);
    }
    
    private Vector3 CalculateSpiralPath(Vector3 startPos, SpiralData targetData, float progress)
    {
        // 기본 선형 보간
        Vector3 linearPath = Vector3.Lerp(startPos, targetData.position, progress);
        
        // 나선형 곡선 추가
        float spiralOffset = Mathf.Sin(progress * Mathf.PI * spiralTightness) * 0.5f;
        Vector3 spiralDirection = new Vector3(
            Mathf.Sin(targetData.angle * Mathf.Deg2Rad),
            0,
            Mathf.Cos(targetData.angle * Mathf.Deg2Rad)
        );
        
        return linearPath + spiralDirection * spiralOffset;
    }
    
    private void SetVoxelTransparency(Transform voxel, float formationProgress, int floor)
    {
        var renderer = voxel.GetComponent<Renderer>();
        if (renderer == null) return;
        
        // 🏢 층수에 따른 투명도 계산 (낮은 층은 더 투명하게 - 내부 가시성)
        float floorProgress = (float)(floor - 1) / (buildingFloors - 1); // 0~1 (1층=0, 15층=1)
        float targetAlpha = Mathf.Lerp(innerVisibility, 1f, floorProgress); // 1층은 투명, 15층은 불투명
        
        // 형성 과정에서의 투명도
        float currentAlpha = Mathf.Lerp(1f, targetAlpha, formationProgress);
        
        // 머티리얼 투명도 적용
        ApplyTransparency(renderer, currentAlpha);
    }
    
    private void ApplyTransparency(Renderer renderer, float alpha)
    {
        Material[] materials = renderer.materials;
        
        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];
            
            // 투명 모드 설정
            if (alpha < 1f)
            {
                mat.SetFloat("_Mode", 3); // Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.renderQueue = 3000;
            }
            
            // 색상과 투명도 적용
            Color color = mat.color;
            color.a = alpha;
            mat.color = color;
        }
    }
    
    #endregion
    
    #region Phase 2: Maintain Capsule
    
    private IEnumerator MaintainCapsule()
    {
        Debug.Log("[BuildingCapsule] 🏢 Phase 2: 15층 건물 캡슐 유지 (내부 살짝 보이게)");
        
        float timer = 0f;
        
        while (timer < maintainTime)
        {
            // 부드러운 회전 효과
            float rotationSpeed = 30f; // 도/초
            float currentRotation = (timer / maintainTime) * rotationSpeed * maintainTime;
            
            foreach (Transform voxel in voxelCubes)
            {
                if (voxel != null && spiralPositions.ContainsKey(voxel))
                {
                    SpiralData data = spiralPositions[voxel];
                    
                    // 부드러운 회전
                    float totalAngle = data.angle + currentRotation;
                    voxel.localRotation = Quaternion.Euler(0, totalAngle, 0);
                    
                    // 🏢 층별 살짝 위아래로 움직이는 효과
                    float wave = Mathf.Sin(Time.time * 2f + data.spiralProgress * Mathf.PI) * 0.05f;
                    Vector3 pos = data.position;
                    pos.y += wave;
                    voxel.localPosition = pos;
                }
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        Debug.Log("[BuildingCapsule] ✅ 15층 건물 캡슐 유지 완료");
    }
    
    #endregion
    
    #region Phase 3: Dissolution and Monster Reveal
    
    private IEnumerator DissolveCapsuleAndRevealMonster()
    {
        Debug.Log("[BuildingCapsule] 🎭 Phase 3: 15층 건물 해체 및 몬스터 등장 (15층→1층)");
        
        isRevealing = true;
        PlaySound(dissolutionSound);
        
        // 몬스터 등장 준비
        PrepareMonsterReveal();
        
        // 🏢 15층 건물 방식으로 층별 해체 (15층→1층)
        yield return StartCoroutine(DissolveByFloors());
        
        // 몬스터 완전 등장
        yield return StartCoroutine(CompleteMonsterReveal());
        
        isRevealing = false;
        Debug.Log("[BuildingCapsule] ✅ 몬스터 등장 완료");
    }
    
    private IEnumerator DissolveByFloors()
    {
        Debug.Log($"[BuildingCapsule] 🏢 {buildingFloors}층 건물 해체 시작 (15층→1층)");
        
        // 15층부터 1층까지 순차적으로 해체
        for (int floor = buildingFloors; floor >= 1; floor--)
        {
            int floorIndex = floor - 1; // 0-based index
            List<Transform> floorVoxels = floorGroups[floorIndex];
            
            if (floorVoxels.Count > 0)
            {
                yield return StartCoroutine(DissolveFloor(floor, floorVoxels));
                yield return new WaitForSeconds(layerDelay);
                
                // 몬스터 점진적 등장 진행도 계산 (15층부터 해체되므로)
                float dissolvedFloors = buildingFloors - floor + 1; // 해체된 층 수
                float revealProgress = dissolvedFloors / buildingFloors;
                RevealMonstersGradually(revealProgress);
            }
        }
        
        Debug.Log("[BuildingCapsule] 🏢 모든 층 해체 완료!");
    }
    

    
    private IEnumerator DissolveFloor(int floor, List<Transform> floorVoxels)
    {
        Debug.Log($"[BuildingCapsule] 🏢 {floor}층 해체 시작 - {floorVoxels.Count}개 큐브");
        
        // 해당 층의 모든 큐브를 동시에 해체 시작
        foreach (Transform voxel in floorVoxels)
        {
            if (voxel != null)
            {
                StartCoroutine(DissolveFloorVoxel(voxel, floor));
            }
        }
        
        yield return new WaitForSeconds(0.4f); // 층 해체 완료 대기
        Debug.Log($"[BuildingCapsule] ✅ {floor}층 해체 완료!");
    }
    
    private void PrepareMonsterReveal()
    {
        Debug.Log("[SpiralCapsule] 몬스터 등장 준비");
        
        foreach (GameObject monster in monstersToReveal)
        {
            if (monster != null)
            {
                // 몬스터를 투명하게 시작
                SetMonsterTransparency(monster, 0f);
                monster.SetActive(true);
            }
        }
    }
    
    private IEnumerator DissolveFloorVoxel(Transform voxel, int floor)
    {
        float dissolveDuration = 0.8f;
        float timer = 0f;
        
        Vector3 startPos = voxel.localPosition;
        Vector3 startScale = voxel.localScale;
        
        // 🏢 층별 개별 딜레이 (같은 층 내에서도 약간의 시차로 자연스럽게)
        float randomDelay = Random.Range(0f, layerDelay * 0.3f);
        yield return new WaitForSeconds(randomDelay);
        
        while (timer < dissolveDuration && voxel != null)
        {
            float progress = timer / dissolveDuration;
            
            // 🏢 층별 떨어지는 효과 (위층일수록 더 높은 곳에서 떨어짐)
            Vector3 currentPos = startPos;
            
            // Y축 아래로 떨어지는 효과 (위층일수록 더 멀리 떨어짐)
            float fallDistance = -2f - (floor * 0.2f); // 위층일수록 더 멀리
            currentPos.y += Mathf.Lerp(0f, fallDistance, progress * progress); // 가속도 효과
            
            // 약간의 바깥쪽 퍼짐 효과 (층수에 비례)
            Vector3 outwardDirection = new Vector3(startPos.x, 0, startPos.z).normalized;
            float spreadStrength = 0.3f + (floor * 0.05f); // 위층일수록 더 퍼짐
            currentPos += outwardDirection * (progress * spreadStrength);
            
            // 🏢 층별 회전 효과 (위층일수록 더 빠르게 회전)
            float rotationMultiplier = Mathf.Lerp(1f, 2f + floor * 0.1f, progress);
            voxel.Rotate(Vector3.up * Time.deltaTime * 180f * rotationMultiplier);
            voxel.Rotate(Vector3.right * Time.deltaTime * 90f * rotationMultiplier);
            
            voxel.localPosition = currentPos;
            
            // 스케일 축소 (떨어지면서 작아짐)
            float scale = Mathf.Lerp(1f, 0.1f, progress);
            voxel.localScale = startScale * scale;
            
            // 투명도 감소 (서서히 사라짐)
            var renderer = voxel.GetComponent<Renderer>();
            if (renderer != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, progress * progress); // 빠르게 투명해짐
                ApplyTransparency(renderer, alpha);
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        // 큐브 비활성화
        if (voxel != null)
        {
            voxel.gameObject.SetActive(false);
        }
    }
    
    private void RevealMonstersGradually(float revealProgress)
    {
        foreach (GameObject monster in monstersToReveal)
        {
            if (monster != null)
            {
                SetMonsterTransparency(monster, revealProgress);
                
                // 등장 효과
                if (revealProgress > 0.5f && revealEffectPrefab != null)
                {
                    SpawnRevealEffect(monster.transform.position);
                }
            }
        }
    }
    
    private IEnumerator CompleteMonsterReveal()
    {
        Debug.Log("[SpiralCapsule] 몬스터 완전 등장");
        
        PlaySound(revealSound);
        
        foreach (GameObject monster in monstersToReveal)
        {
            if (monster != null)
            {
                yield return StartCoroutine(FinalMonsterReveal(monster));
                yield return new WaitForSeconds(monsterRevealDelay);
            }
        }
    }
    
    private IEnumerator FinalMonsterReveal(GameObject monster)
    {
        float revealDuration = 1f;
        float timer = 0f;
        
        Vector3 originalPos = monster.transform.position;
        Vector3 startPos = originalPos + Vector3.up * 0.5f;
        monster.transform.position = startPos;
        
        while (timer < revealDuration)
        {
            float progress = timer / revealDuration;
            
            // 위에서 아래로 등장
            monster.transform.position = Vector3.Lerp(startPos, originalPos, progress);
            
            // 투명도 증가
            SetMonsterTransparency(monster, progress);
            
            // 스케일 효과
            float scale = Mathf.Lerp(0.8f, 1f, progress);
            monster.transform.localScale = Vector3.one * scale;
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        // 최종 설정
        monster.transform.position = originalPos;
        monster.transform.localScale = Vector3.one;
        SetMonsterTransparency(monster, 1f);
        
        // 등장 효과
        SpawnRevealEffect(monster.transform.position);
    }
    
    private void SetMonsterTransparency(GameObject monster, float alpha)
    {
        Renderer[] renderers = monster.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            ApplyTransparency(renderer, alpha);
        }
    }
    
    private void SpawnRevealEffect(Vector3 position)
    {
        if (revealEffectPrefab != null)
        {
            var effect = Instantiate(revealEffectPrefab, position, Quaternion.identity);
            Destroy(effect.gameObject, 3f);
        }
    }
    
    #endregion
    
    #region Phase 4: Restore Original State
    
    private IEnumerator RestoreOriginalState()
    {
        Debug.Log("[BuildingCapsule] 🔄 원래 상태 복원");
        
        float restoreDuration = 1f;
        float timer = 0f;
        
        // 비활성화된 큐브들 다시 활성화
        foreach (Transform voxel in voxelCubes)
        {
            if (voxel != null && !voxel.gameObject.activeInHierarchy)
            {
                voxel.gameObject.SetActive(true);
            }
        }
        
        while (timer < restoreDuration)
        {
            float progress = timer / restoreDuration;
            
            foreach (Transform voxel in voxelCubes)
            {
                if (voxel != null)
                {
                    // 원래 위치로 복원
                    if (originalPositions.ContainsKey(voxel))
                    {
                        Vector3 currentPos = voxel.localPosition;
                        Vector3 targetPos = originalPositions[voxel];
                        voxel.localPosition = Vector3.Lerp(currentPos, targetPos, progress);
                    }
                    
                    // 원래 회전으로 복원
                    if (originalRotations.ContainsKey(voxel))
                    {
                        Vector3 targetRotation = originalRotations[voxel];
                        voxel.localEulerAngles = Vector3.Lerp(voxel.localEulerAngles, targetRotation, progress);
                    }
                    
                    // 원래 스케일로 복원
                    if (originalScales.ContainsKey(voxel))
                    {
                        Vector3 targetScale = originalScales[voxel];
                        voxel.localScale = Vector3.Lerp(voxel.localScale, targetScale, progress);
                    }
                    
                    // 투명도 복원
                    var renderer = voxel.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        ApplyTransparency(renderer, 1f);
                    }
                }
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        Debug.Log("[BuildingCapsule] ✅ 원래 상태 복원 완료");
    }
    
    #endregion
    
    #region Monster Management
    
    private void HideAllMonsters()
    {
        foreach (GameObject monster in monstersToReveal)
        {
            if (monster != null)
            {
                monster.SetActive(false);
            }
        }
    }
    
    #endregion
    
    #region Audio
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    #endregion
    
    #region Debug & Testing
    
    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("[Debug] 🏢 15층 건물 캡슐 변신 시작! (15층→1층 해체)");
            StartTransformation();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("[Debug] ⏹️ 변신 중단!");
            StopTransformation();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[Debug] 🔄 15층 건물 구조 재계산!");
            CalculateSpiralFormation();
        }
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("[Debug] 👁️ 몬스터 숨기기/보이기 토글!");
            ToggleMonsterVisibility();
        }
        
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("[Debug] 🏢 15층 건물 구조 미리보기!");
            PreviewBuildingFloors();
        }
    }
    
    private void PreviewBuildingFloors()
    {
        if (!Application.isPlaying) return;
        
        Debug.Log($"=== 🏢 {buildingFloors}층 건물 구조 미리보기 ===");
        
        // 15층부터 1층까지 해체 순서로 출력
        for (int floor = buildingFloors; floor >= 1; floor--)
        {
            int floorIndex = floor - 1; // 0-based
            int cubeCount = floorGroups[floorIndex].Count;
            float floorHeight = CalculateBuildingFloorHeight(floor);
            
            Debug.Log($"해체 순서 {buildingFloors - floor + 1}: {floor}층 (높이 {floorHeight:F2}) - {cubeCount}개 큐브");
        }
        
        Debug.Log($"💡 총 {buildingFloors}개 층으로 구성된 캡슐 건물입니다!");
    }
    
    private void ToggleMonsterVisibility()
    {
        foreach (GameObject monster in monstersToReveal)
        {
            if (monster != null)
            {
                monster.SetActive(!monster.activeInHierarchy);
            }
        }
    }
    
    #endregion
    
    #region Context Menu Tests
    
    [ContextMenu("🏢 Test Building Capsule (15F→1F Dissolution)")]
    private void TestBuildingCapsuleTransformation()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("플레이 모드에서만 테스트 가능합니다!");
            return;
        }
        
        Debug.Log("[ContextMenu] 🏢 15층 건물 캡슐 변신 시작 (15층→1층 층별 해체)");
        StartTransformation();
    }
    
    [ContextMenu("⏹️ Stop All")]
    private void TestStopAll()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("플레이 모드에서만 테스트 가능합니다!");
            return;
        }
        
        StopTransformation();
    }
    
    [ContextMenu("🔄 Recalculate Spiral")]
    private void TestRecalculateSpiral()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("플레이 모드에서만 테스트 가능합니다!");
            return;
        }
        
        InitializeSystem();
    }
    
    [ContextMenu("📊 Show Statistics")]
    private void ShowStatistics()
    {
        Debug.Log($"=== 🏢 15층 건물 캡슐 변신 시스템 통계 ===");
        Debug.Log($"총 큐브 수: {voxelCubes.Count}");
        Debug.Log($"건물 층수: {buildingFloors}층");
        Debug.Log($"캡슐 크기: 반지름 {capsuleRadius}, 높이 {capsuleHeight}");
        Debug.Log($"등장할 몬스터: {monstersToReveal.Length}개");
        Debug.Log($"현재 상태: 변신 중 = {isTransforming}, 등장 중 = {isRevealing}");
        Debug.Log($"해체 방식: 🏢 {buildingFloors}층→1층 순서대로 층별 해체");
        
        // 층별 분포 정보
        if (Application.isPlaying && voxelCubes.Count > 0)
        {
            Debug.Log("\n--- 🏢 건물 층별 구조 ---");
            for (int floor = buildingFloors; floor >= 1; floor--)
            {
                int floorIndex = floor - 1; // 0-based
                int cubeCount = floorGroups[floorIndex].Count;
                float floorHeight = CalculateBuildingFloorHeight(floor);
                
                Debug.Log($"{floor}층: {cubeCount}개 큐브 (높이: {floorHeight:F2})");
            }
            
            Debug.Log("\n--- 🎬 해체 순서 ---");
            for (int floor = buildingFloors; floor >= 1; floor--)
            {
                int dissolveOrder = buildingFloors - floor + 1;
                int floorIndex = floor - 1;
                int cubeCount = floorGroups[floorIndex].Count;
                
                Debug.Log($"해체 {dissolveOrder}단계: {floor}층 - {cubeCount}개 큐브");
            }
            
            Debug.Log($"\n💡 총 {buildingFloors}개 층으로 구성된 명확한 층별 해체 시스템입니다!");
        }
    }
    
    #endregion
    
    #region Gizmos
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // 캡슐 외곽선 그리기
        Gizmos.color = capsuleColor;
        Gizmos.DrawWireSphere(transform.position, capsuleRadius);
        
        // 캡슐 높이 표시
        Vector3 topPoint = transform.position + Vector3.up * (capsuleHeight * 0.5f);
        Vector3 bottomPoint = transform.position + Vector3.down * (capsuleHeight * 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(topPoint, bottomPoint);
        
        // 🏢 나선 위치들 표시 (층별 색상 구분)
        if (Application.isPlaying && spiralPositions.Count > 0)
        {
            foreach (var kvp in spiralPositions)
            {
                if (kvp.Key != null)
                {
                    Vector3 worldPos = transform.TransformPoint(kvp.Value.position);
                    
                    // 🏢 층수에 따른 색상 (15층=빨강/먼저 해체, 1층=파랑/나중에 해체)
                    float floorProgress = (float)(kvp.Value.floor - 1) / (buildingFloors - 1); // 0~1
                    
                    // 해체 순서에 따른 색상 (빨강=먼저 사라짐, 파랑=나중에 사라짐)
                    Gizmos.color = Color.Lerp(Color.blue, Color.red, floorProgress);
                    Gizmos.DrawWireSphere(worldPos, 0.12f);
                    
                    // 🏢 층별 해체 순서 표시 (작은 큐브로)
                    Gizmos.color = Color.Lerp(Color.cyan, Color.magenta, floorProgress);
                    Gizmos.DrawCube(worldPos, Vector3.one * 0.08f);
                    
                    // 원래 위치에서 나선 위치로의 연결선
                    if (originalPositions.ContainsKey(kvp.Key))
                    {
                        Vector3 originalWorldPos = transform.TransformPoint(originalPositions[kvp.Key]);
                        Gizmos.color = new Color(1, 1, 1, 0.2f);
                        Gizmos.DrawLine(originalWorldPos, worldPos);
                    }
                }
            }
            
            // 🏢 층별 구분선 그리기 (각 층을 명확히 표시)
            for (int floor = 1; floor <= buildingFloors; floor++)
            {
                float floorHeight = CalculateBuildingFloorHeight(floor);
                Vector3 floorCenter = transform.position + Vector3.up * floorHeight;
                
                // 층별 구분 원 그리기 (해체 순서 색상으로)
                float floorProgress = (float)(floor - 1) / (buildingFloors - 1);
                Gizmos.color = Color.Lerp(new Color(0, 0, 1, 0.3f), new Color(1, 0, 0, 0.3f), floorProgress);
                DrawGizmosCircle(floorCenter, capsuleRadius, Vector3.up);
                
                // 층수 표시
                Gizmos.color = Color.white;
                //Gizmos.DrawSphere(floorCenter, 0.1f); // 층 중심점
            }
        }
        
        // 몬스터 위치 표시
        Gizmos.color = Color.green;
        foreach (GameObject monster in monstersToReveal)
        {
            if (monster != null)
            {
                Gizmos.DrawWireCube(monster.transform.position, Vector3.one * 0.5f);
                
                // 몬스터 등장 방향 표시 (위에서 아래로)
                Vector3 startPos = monster.transform.position + Vector3.up * 0.5f;
                Gizmos.color = new Color(0, 1, 0, 0.6f);
                Gizmos.DrawLine(startPos, monster.transform.position);
                Gizmos.DrawSphere(startPos, 0.1f);
            }
        }
    }
    
    // Gizmos용 원 그리기 헬퍼 함수
    private void DrawGizmosCircle(Vector3 center, float radius, Vector3 normal)
    {
        Vector3 forward = Vector3.Slerp(Vector3.forward, -normal, 0.5f);
        Vector3 right = Vector3.Cross(normal, forward).normalized * radius;
        forward = Vector3.Cross(right, normal).normalized * radius;
        
        Matrix4x4 matrix = new Matrix4x4();
        matrix[0] = right.x; matrix[1] = right.y; matrix[2] = right.z;
        matrix[4] = normal.x; matrix[5] = normal.y; matrix[6] = normal.z;
        matrix[8] = forward.x; matrix[9] = forward.y; matrix[10] = forward.z;
        matrix[12] = center.x; matrix[13] = center.y; matrix[14] = center.z;
        matrix[15] = 1;
        
        Vector3 lastPoint = center + right;
        for (int i = 1; i <= 32; i++)
        {
            float angle = (float)i / 32f * 2f * Mathf.PI;
            Vector3 newPoint = center + right * Mathf.Cos(angle) + forward * Mathf.Sin(angle);
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint = newPoint;
        }
    }
    
    #endregion
}
