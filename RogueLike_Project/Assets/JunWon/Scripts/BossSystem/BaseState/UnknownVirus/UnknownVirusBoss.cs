using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using static UnityEngine.UI.GridLayoutGroup;

public class UnknownVirusBoss : BossBase
{
    public enum BossForm { Basic, Worm, Trojan, Ransomware }

    #region Inspector Settings
    [Header("Form Prefabs")]
    [SerializeField] private WormBossPrime wormFormPrefab;
    [SerializeField] private Troy trojanFormPrefab;
    [SerializeField] private Ransomware ransomwareFormPrefab;

    [Header("Basic Combat")]
    [SerializeField] private GameObject originalAttackPrefab;
    [SerializeField] private float originalAttackRange = 10f;
    [SerializeField] private float originalAttackDamage = 25f;
    [SerializeField] private float originalAttackCooldown = 3f;

    [Header("Map Attack")]
    private float lastMapAttackTime = 15f;
    [SerializeField] private GameObject mapAttackVFX;
    [SerializeField] private float mapAttackCooldown = 15f;
    [Range(0, 1)][SerializeField] private float mapAttackChance = 0.8f;

    [Header("Form Change")]
    [SerializeField] private GameObject transformationVFX;
    [SerializeField] private float transformationTime = 3f;
    [SerializeField] private float formChangeCooldown = 30f;
    [Range(0, 1)][SerializeField] private float formChangeChance = 0.3f;
    #endregion

    #region Components
    [Header("Basic Components")]
    [SerializeField] private AbilityManager abilityManager;
    #endregion

    #region Status & State
    // FSM & States
    [Header("State Machine")]
    [SerializeField] private StateMachine<UnknownVirusBoss> fsm;

    [Header("State")]
    private IntroState_UnknownVirus introState;
    private BasicCombatState_UnknownVirus basicState;
    private MapAttackState_UnknownVirus mapAttackState;
    private TransformState_UnknownVirus transformState;
    private WormCombatState_UnknownVirus wormCombatState;
    private TrojanCombatState_UnknownVirus trojanCombatState;
    private RansomwareCombatState_UnknownVirus ransomwareCombatState;
    private DefeatedState_UnknownVirus deadState;
    #endregion

    #region Animation Event Handlers
    // 기본 원거리 공격 애니메이션 이벤트

    public void OnMapAttackFinished()
    {
        if (mapAttackState != null)
        {
            mapAttackState.OnAttackFinished();
        }
    }
    
    #endregion

    #region State Setters
    public void SetMapAttackState(MapAttackState_UnknownVirus state)
    {
        mapAttackState = state;
    }

    public void TransformState(TransformState_UnknownVirus state)
    {
        transformState = state;
    }

    public void SetDefeatedState(DefeatedState_UnknownVirus state)
    {
        deadState = state;
    }
    #endregion

    #region Public Properties
    public Transform Player => target;
    public NavMeshAgent NmAgent => nmAgent;
    public Animator Animator => anim;
    public BossStatus MonsterStatus => bossStatus;
    public FieldOfView FOV => fov;
    public AbilityManager AbilityManager => abilityManager;
    #endregion

    #region MapAttack
    [Header("Map Attack Settings")]
    [SerializeField] private Collider[] hitColliders;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private int attackAreaSize = 5; // 공격 영역 크기 (5x5)
    [SerializeField] private float tileSearchInterval = 0.1f; // 타일 검색 간격
    [SerializeField] private float shockwavePower = 30f; // 충격파 세기
    [SerializeField] private float searchCooldown = 3f;
    [SerializeField] private LayerMask playerLayer;

    // 맵 공격용 타일
    #endregion




    // Runtime
    private BossForm currentForm = BossForm.Basic;
    private BossBase currentActiveBoss;
    private bool isTransforming = false;

    public BossForm CurrentForm => currentForm;

    private void Start()
    {
        InitializeComponent(); Debug.Log("→ Component 초기화 완료");
        InitializeAbility(); Debug.Log("→ 기술 초기화 완료");
        InitializeStates(); Debug.Log("→ State 초기화 완료");
        InitializeFSM(); Debug.Log("→ FSM 초기화 완료");
    }

    private void Update()
    {
        fsm.Update();
    }

    private void InitializeComponent()
    {
        // 기본 세팅: Player, Animator, NavMeshAgent 등 세팅
        tileManager = FindObjectOfType<TileManager>();
        target = GameObject.FindWithTag("Player").transform;
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        fov = GetComponent<FieldOfView>();
        // 원본 모델은 이 스크립트가 붙은 오브젝트 자체
    }

    private void InitializeStates()
    {
        introState = new IntroState_UnknownVirus(this);
        basicState = new BasicCombatState_UnknownVirus(this);
        mapAttackState = new MapAttackState_UnknownVirus(this);
        transformState = new TransformState_UnknownVirus(this);
        wormCombatState = new WormCombatState_UnknownVirus(this);
        trojanCombatState = new TrojanCombatState_UnknownVirus(this);
        ransomwareCombatState = new RansomwareCombatState_UnknownVirus(this);
        deadState = new DefeatedState_UnknownVirus(this);
    }

    private void InitializeFSM()
    {
        // 1) 상태 인스턴스 생성
        var states = CreateStates();

        // 2) FSM 생성 (초기 상태는 Intro)
        fsm = new StateMachine<UnknownVirusBoss>(states.introState);

        // 3) 전이 설정
        SetupTransitions(states);
    }

    private (
        IntroState_UnknownVirus introState,
        BasicCombatState_UnknownVirus basicState,
        MapAttackState_UnknownVirus mapAttackState,
        TransformState_UnknownVirus transformState,
        WormCombatState_UnknownVirus wormCombatState,
        TrojanCombatState_UnknownVirus trojanCombatState,
        RansomwareCombatState_UnknownVirus ransomwareCombatState,
        DefeatedState_UnknownVirus deadState
    ) CreateStates()
    {
        return (
            new IntroState_UnknownVirus(this),
            new BasicCombatState_UnknownVirus(this),
            new MapAttackState_UnknownVirus(this),
            new TransformState_UnknownVirus(this),
            new WormCombatState_UnknownVirus(this),
            new TrojanCombatState_UnknownVirus(this),
            new RansomwareCombatState_UnknownVirus(this),
            new DefeatedState_UnknownVirus(this)
        );
    }

    private void SetupTransitions((
        IntroState_UnknownVirus introState,
        BasicCombatState_UnknownVirus basicState,
        MapAttackState_UnknownVirus mapAttackState,
        TransformState_UnknownVirus transformState,
        WormCombatState_UnknownVirus wormCombatState,
        TrojanCombatState_UnknownVirus trojanCombatState,
        RansomwareCombatState_UnknownVirus ransomwareCombatState,
        DefeatedState_UnknownVirus deadState
    ) s)
    {
        // Intro → Basic
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.introState, s.basicState, () => true));

        // Basic → MapAttack
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.basicState, s.mapAttackState,
            () => abilityManager.GetAbilityRemainingCooldown("MapAttack") == 0 && UnityEngine.Random.value < mapAttackChance));

        // MapAttack → Basic
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.mapAttackState, s.basicState,
            () => s.mapAttackState.IsAnimationFinished()
        ));

        // Basic → Transform
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.basicState, s.transformState,
            () => UnityEngine.Random.value < formChangeChance));

        // Transform → 각 전투 폼
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.transformState, s.basicState,
            () => !isTransforming && currentForm == BossForm.Basic));
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.transformState, s.wormCombatState,
            () => !isTransforming && currentForm == BossForm.Worm));
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.transformState, s.trojanCombatState,
            () => !isTransforming && currentForm == BossForm.Trojan));
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.transformState, s.ransomwareCombatState,
            () => !isTransforming && currentForm == BossForm.Ransomware));

        // 각 전투 폼 종료 → Basic
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.wormCombatState, s.basicState,
            () => currentActiveBoss == null));
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.trojanCombatState, s.basicState,
            () => currentActiveBoss == null));
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.ransomwareCombatState, s.basicState,
            () => currentActiveBoss == null));

        // 어느 상태에서나 Dead
        List<State<UnknownVirusBoss>> exceptStates = new List<State<UnknownVirusBoss>> { introState };
        fsm.AddGlobalTransition(deadState, () => bossStatus.GetHealth() <= 0, exceptStates);
    }

    #region AttackAbility Helpers
    private void InitializeAbility()
    {
        // 발악 패턴에서 사용할 능력 활성화
        abilityManager.SetAbilityActive("MapAttack");
        abilityManager.SetMaxCoolTime("MapAttack");

        // 다른 모든 능력 비활성화
        //owner.AbilityManager.SetAbilityInactive();
    }
    #endregion

    #region Transform Logic
    /// <summary>TransformState 에서 호출</summary>
    public void RequestFormChange(BossForm newForm)
    {
        if (isTransforming) return;
        isTransforming = true;
        currentForm = newForm;
        StartCoroutine(TransformRoutine());
    }

    private IEnumerator TransformRoutine()
    {
        transformationVFX?.SetActive(true);
        yield return new WaitForSeconds(transformationTime);
        ApplyForm(currentForm);
        transformationVFX?.SetActive(false);
        isTransforming = false;
    }

    public void ApplyForm(BossForm form)
    {
        // 1) 기본 모델 숨기기
        gameObject.SetActive(form == BossForm.Basic);

        // 2) 이전 폼 클린업
        if (currentActiveBoss != null) Destroy(currentActiveBoss.gameObject);

        // 3) 새 폼 생성 & 트래킹
        switch (form)
        {
            case BossForm.Worm:
                currentActiveBoss = Instantiate(wormFormPrefab, transform);
                break;
            case BossForm.Trojan:
                currentActiveBoss = Instantiate(trojanFormPrefab, transform);
                break;
            case BossForm.Ransomware:
                currentActiveBoss = Instantiate(ransomwareFormPrefab, transform);
                break;
            case BossForm.Basic:
            default:
                currentActiveBoss = null;
                gameObject.SetActive(true);
                return;
        }

        // 4) 전투 종료 콜백 등록 (Defeated 시 자동 Basic 복귀)
       
    }
    #endregion

    public override void TakeDamage(float damage, bool showDamage = true)
    {
        // 1) 변환 중엔 피해 무시
        if (isTransforming)
            return;

        // 2) Basic 이 아닌 폼(하위 보스)에 위임
        if (currentForm != BossForm.Basic && currentActiveBoss != null)
        {
            currentActiveBoss.TakeDamage(damage, showDamage);
            return;
        }

        // 3) Basic 폼 처리
        // 3-1) 체력 감소
        bossStatus.DecreaseHealth(damage);

        // 3-2) 피해 이벤트 및 UI 팝업
        EventManager.Instance.TriggerMonsterDamagedEvent();
        if (showDamage && UIDamaged != null)
        {
            var popup = Instantiate(
                UIDamaged,
                transform.position + new Vector3(0, UnityEngine.Random.Range(0f, height * 0.5f), 0),
                Quaternion.identity
            ).GetComponent<UIDamage>();
            popup.damage = damage;
        }
       
    }


    #region MapAttack Func
    // 맵 공격 트리거 메서드
    public void TriggerMapAttack()
    {
        try
        {
            lastMapAttackTime = Time.time;

            if (tileManager == null)
            {
                Debug.LogError("Cannot execute map attack: TileManager is null");
                mapAttackState?.OnAttackFinished();
                return;
            }

            // 검색 알고리즘 선택
            int searchMethod = UnityEngine.Random.Range(0, 3);
            StartCoroutine(ExecuteMapAttack(searchMethod));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TriggerMapAttack error: {e.Message}\n{e.StackTrace}");
            mapAttackState?.OnAttackFinished();
        }
    }

    private IEnumerator ExecuteMapAttack(int searchMethod)
    {
        // 플레이어 주변 좌표 계산
        Vector3 playerPos = target.position;

        // 월드 좌표를 TileManager의 그리드 좌표로 변환
        int centerX = Mathf.RoundToInt(playerPos.x / 2);
        int centerZ = Mathf.RoundToInt(playerPos.z / 2);

        Debug.Log($"Starting map attack at grid position: [{centerX}, {centerZ}]");

        // 목표 타일 무작위 선택 (공격 영역 내)
        int targetX = centerX + UnityEngine.Random.Range(-attackAreaSize / 2, attackAreaSize / 2 + 1);
        int targetZ = centerZ + UnityEngine.Random.Range(-attackAreaSize / 2, attackAreaSize / 2 + 1);

        // 유효한 맵 범위 내로 조정
        targetX = Mathf.Clamp(targetX, 0, tileManager.GetMapSize - 1);
        targetZ = Mathf.Clamp(targetZ, 0, tileManager.GetMapSize - 1);

        // 검색 방법에 따른 효과 실행
        switch (searchMethod)
        {
            case 0:
                yield return StartCoroutine(LinearTileSearch(centerX, centerZ, targetX, targetZ));
                break;
            case 1:
                yield return StartCoroutine(BinaryTileSearch(centerX, centerZ, targetX, targetZ));
                break;
            case 2:
                yield return StartCoroutine(RandomTileSearch(centerX, centerZ, targetX, targetZ));
                break;
        }

        // 공격 완료 후 대기
        yield return new WaitForSeconds(1f);

        // 맵 공격 상태 완료 알림
        mapAttackState?.OnAttackFinished();
    }

    // 선형 타일 검색 공격
    private IEnumerator LinearTileSearch(int centerX, int centerZ, int targetX, int targetZ)
    {
        int halfSize = attackAreaSize / 2;

        // 검색 범위 계산 (플레이어 중심으로 attackAreaSize x attackAreaSize 영역)
        int minX = Mathf.Max(0, centerX - halfSize);
        int maxX = Mathf.Min(tileManager.GetMapSize - 1, centerX + halfSize);
        int minZ = Mathf.Max(0, centerZ - halfSize);
        int maxZ = Mathf.Min(tileManager.GetMapSize - 1, centerZ + halfSize);

        Debug.Log($"Linear search area: [{minX},{minZ}] to [{maxX},{maxZ}], target at: [{targetX},{targetZ}]");

        // 모든 타일을 순차적으로 검색
        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                // 현재 검색 타일 표시
                HighlightTile(x, z, Color.red);

                // 월드 좌표로 변환
                Vector3 tilePos = new Vector3(x * 2, 0, z * 2);

                // 레이저 효과
                if (laserPrefab != null)
                {
                    GameObject laser = Instantiate(laserPrefab,
                       tilePos + Vector3.up * 0.2f, // 타일 바로 위
                       Quaternion.identity);

                    // 레이저 데미지 설정
                    VirusLaser virusLaser = laser.GetComponent<VirusLaser>();
                    if (virusLaser != null)
                    {
                        virusLaser.SetDamage(abilityManager.GetAbiltiyDmg("MapAttack"));
                    }
                }

                yield return new WaitForSeconds(tileSearchInterval);

                // 타겟 타일을 찾으면 충격파 효과
                if (x == targetX && z == targetZ)
                {
                    // 타겟 타일 표시
                    HighlightTile(x, z, Color.green);

                    // 충격파 생성 (TileManager의 CreateShockwave 코루틴 호출)
                    StartCoroutine(tileManager.CreateShockwave(x, z, halfSize, shockwavePower));

                    // 데미지 적용
                    yield break;
                }

                // 검색 완료된 타일 리셋
                ResetTileColor(x, z);
            }
        }
    }

    // 이진 타일 검색 공격
    private IEnumerator BinaryTileSearch(int centerX, int centerZ, int targetX, int targetZ)
    {
        int halfSize = attackAreaSize / 2;

        // 검색 범위 계산
        int leftX = Mathf.Max(0, centerX - halfSize);
        int rightX = Mathf.Min(tileManager.GetMapSize - 1, centerX + halfSize);
        int topZ = Mathf.Max(0, centerZ - halfSize);
        int bottomZ = Mathf.Min(tileManager.GetMapSize - 1, centerZ + halfSize);

        Debug.Log($"Binary search area: [{leftX},{topZ}] to [{rightX},{bottomZ}], target at: [{targetX},{targetZ}]");

        int iterations = 0;
        int maxIterations = 10; // 무한 루프 방지

        while (leftX <= rightX && topZ <= bottomZ && iterations < maxIterations)
        {
            iterations++;
            int midX = (leftX + rightX) / 2;
            int midZ = (topZ + bottomZ) / 2;

            // 현재 검색 영역 강조
            for (int x = leftX; x <= rightX; x++)
            {
                for (int z = topZ; z <= bottomZ; z++)
                {
                    HighlightTile(x, z, Color.red);

                    // 경계 타일에만 레이저 효과
                    if (x == leftX || x == rightX || z == topZ || z == bottomZ)
                    {
                        if (laserPrefab != null)
                        {
                            Vector3 tilePos = new Vector3(x * 2, 0, z * 2);
                            GameObject laser = Instantiate(laserPrefab,
                                tilePos + Vector3.up * 0.2f,
                                Quaternion.identity);

                            // 레이저 데미지 설정
                            VirusLaser virusLaser = laser.GetComponent<VirusLaser>();
                            if (virusLaser != null)
                            {
                                virusLaser.SetDamage(abilityManager.GetAbiltiyDmg("MapAttack"));
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);

            // 강조 효과 초기화
            for (int x = leftX; x <= rightX; x++)
            {
                for (int z = topZ; z <= bottomZ; z++)
                {
                    ResetTileColor(x, z);
                }
            }

            yield return new WaitForSeconds(0.2f);

            // 타겟을 찾았는지 확인
            if (midX == targetX && midZ == targetZ)
            {
                HighlightTile(midX, midZ, Color.green);

                Vector3 targetPos = new Vector3(midX * 2, 0, midZ * 2);

                yield break;
            }

            // 이진 검색 로직
            if (targetX < midX)
                rightX = midX - 1;
            else
                leftX = midX + 1;

            if (targetZ < midZ)
                bottomZ = midZ - 1;
            else
                topZ = midZ + 1;
        }

        // 타겟을 찾지 못한 경우 (이런 일은 없어야 함)
        HighlightTile(targetX, targetZ, Color.green);

        Vector3 finalPos = new Vector3(targetX * 2, 0, targetZ * 2);
    }

    // 랜덤 타일 검색 공격
    private IEnumerator RandomTileSearch(int centerX, int centerZ, int targetX, int targetZ)
    {
        int halfSize = attackAreaSize / 2;

        // 검색 범위 계산
        int minX = Mathf.Max(0, centerX - halfSize);
        int maxX = Mathf.Min(tileManager.GetMapSize - 1, centerX + halfSize);
        int minZ = Mathf.Max(0, centerZ - halfSize);
        int maxZ = Mathf.Min(tileManager.GetMapSize - 1, centerZ + halfSize);

        Debug.Log($"Random search area: [{minX},{minZ}] to [{maxX},{maxZ}], target at: [{targetX},{targetZ}]");

        HashSet<Vector2Int> searchedTiles = new HashSet<Vector2Int>();
        int maxAttempts = Mathf.Min(20, (maxX - minX + 1) * (maxZ - minZ + 1));

        for (int i = 0; i < maxAttempts; i++)
        {
            // 검색 범위 내에서 랜덤 타일 선택
            int x = UnityEngine.Random.Range(minX, maxX + 1);
            int z = UnityEngine.Random.Range(minZ, maxZ + 1);
            Vector2Int tilePos = new Vector2Int(x, z);

            // 이미 검색한 타일이면 다시 선택 (최대 3번)
            int attempts = 0;
            while (searchedTiles.Contains(tilePos) && attempts < 3)
            {
                x = UnityEngine.Random.Range(minX, maxX + 1);
                z = UnityEngine.Random.Range(minZ, maxZ + 1);
                tilePos = new Vector2Int(x, z);
                attempts++;
            }

            searchedTiles.Add(tilePos);

            // 타일 강조 및 레이저 효과
            HighlightTile(x, z, Color.red);

            if (laserPrefab != null)
            {
                Vector3 worldPos = new Vector3(x * 2, 0.1f, z * 2);
                GameObject laser = Instantiate(laserPrefab,
                    worldPos + Vector3.up * 2,
                    Quaternion.identity);
                Destroy(laser, 2f);
            }

            yield return new WaitForSeconds(tileSearchInterval);

            // 타겟 타일을 찾았는지 확인
            if (x == targetX && z == targetZ)
            {
                HighlightTile(x, z, Color.green);
                StartCoroutine(tileManager.CreateShockwave(x, z, halfSize, shockwavePower));
                ApplyDamageAroundPosition(new Vector3(x * 2, 0, z * 2));
                yield break;
            }

            // 검색 완료된 타일 리셋
            ResetTileColor(x, z);
        }

        // 최대 시도 횟수를 초과해도 타겟을 찾지 못한 경우
        HighlightTile(targetX, targetZ, Color.green);
        StartCoroutine(tileManager.CreateShockwave(targetX, targetZ, halfSize, shockwavePower));
        ApplyDamageAroundPosition(new Vector3(targetX * 2, 0, targetZ * 2));
    }

    // 타일 강조 헬퍼 메서드
    private void HighlightTile(int x, int z, Color color)
    {
        if (x < 0 || x >= tileManager.GetMapSize || z < 0 || z >= tileManager.GetMapSize)
            return;

        Tile tile = tileManager.GetTiles[z, x];
        if (tile != null && tile.IsSetActive)
        {
            Renderer renderer = tile.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.SetColor("_BaseColor", color);
            }
        }
    }

    // 타일 색상 리셋 헬퍼 메서드
    private void ResetTileColor(int x, int z)
    {
        if (x < 0 || x >= tileManager.GetMapSize || z < 0 || z >= tileManager.GetMapSize)
            return;

        Tile tile = tileManager.GetTiles[z, x];
        if (tile != null && tile.IsSetActive)
        {
            Renderer renderer = tile.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.SetColor("_BaseColor", Color.white);
            }
        }
    }

    // 지정된 위치 주변에 데미지 적용
    private void ApplyDamageAroundPosition(Vector3 centerPosition)
    {
        float damageRadius = attackAreaSize * 1.0f; // 데미지 반경

        Debug.Log("Apply Damage");
        // 데미지 적용
        hitColliders = Physics.OverlapSphere(centerPosition, damageRadius, playerLayer);
        foreach (var collider in hitColliders)
        {
            if (collider == null) continue;
            Debug.Log(collider + "is hit");

            PlayerStatus playerStatus = collider.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.DecreaseHealth(abilityManager.GetAbiltiyDmg("MapAttack"));

                // 추가 효과 - 넉백
                Rigidbody playerRb = collider.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    Vector3 knockbackDir = (collider.transform.position - centerPosition).normalized;
                    knockbackDir.y = 0.3f; // 약간 위로
                    playerRb.AddForce(knockbackDir * 10f, ForceMode.Impulse);
                }
            }
        }
    }

    // 공격 종료 시 정리 메서드
    public void CleanupMapAttack()
    {
        // 혹시 남아있는 레이저 이펙트 찾아서 제거
        GameObject[] lasers = GameObject.FindGameObjectsWithTag("VirusLaser");
        foreach (var laser in lasers)
        {
            if (laser != null) Destroy(laser);
        }
    }
    #endregion
}
