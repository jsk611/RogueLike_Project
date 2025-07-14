using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

public class UnknownVirusBoss : BossBase
{
    public enum BossForm { Basic, Worm, Trojan, Ransomware }

    #region 폼 변환 시스템
    [Header("폼 오브젝트")]
    [SerializeField] private GameObject basicFormObject;     // 기본 폼 오브젝트 (자기 자신)
    [SerializeField] private GameObject wormFormObject;      // 웜 폼 오브젝트 (자식)
    [SerializeField] private GameObject trojanFormObject;    // 트로잔 폼 오브젝트 (자식)
    [SerializeField] private GameObject ransomwareFormObject; // 랜섬웨어 폼 오브젝트 (자식)

    // 각 폼 컴포넌트 캐시
    private WormBossPrime wormComponent;
    private Troy trojanComponent;
    private Ransomware ransomwareComponent;

    // 현재 활성화된 폼 정보
    private GameObject currentActiveFormObject;
    private BossBase currentActiveBoss;
    [SerializeField] private BossForm currentForm = BossForm.Basic;

    // 폼 지속 타이머 변수
    private float formStayDuration = 15f; // 변환된 폼이 지속되는 시간
    private float formTimer = 0f;
    #endregion

    #region Effect System

    [SerializeField] VoxelFloatEffect vFE;

    #endregion

    #region 공격 설정
    private float currentRandomValue = 0.9f;
    [Header("기본 공격")]
    [SerializeField] private float baseAttackDamage = 20f;
    [SerializeField] private float baseAttackRange = 10f;
    [SerializeField] private float baseAttackCooldown = 3f;

    [Header("맵 공격")]
    [SerializeField] private GameObject mapAttackVFX;
    [Range(0, 1)][SerializeField] private float mapAttackChance = 0.7f;

    [Header("폼 변환")]
    [SerializeField] private GameObject transformationVFX;
    [Range(0, 1)][SerializeField] private float formChangeChance = 0.3f;

    [Header("매니저")]
    [SerializeField] private AbilityManager abilityManager;
    #endregion

    #region 상태 머신
    [Header("상태 머신")]
    [SerializeField] private StateMachine<UnknownVirusBoss> fsm;

    // 상태들
    private IntroState_UnknownVirus introState;
    private BasicCombatState_UnknownVirus basicState;
    private MapAttackState_UnknownVirus mapAttackState;
    private TransformState_UnknownVirus transformState;
    private DefeatedState_UnknownVirus deadState;
    #endregion

    #region 상태 설정 메서드

    public void SetMapAttackState(MapAttackState_UnknownVirus state)
    {
        mapAttackState = state;
    }

    public void SetTransformState(TransformState_UnknownVirus state)
    {
        transformState = state;
    }
    #endregion


    #region 맵 공격 설정
    [Header("맵 공격 설정")]
    [SerializeField] private TileManager tileManager;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private int attackAreaSize = 5; // 공격 영역 크기 (5x5)
    [SerializeField] private float tileSearchInterval = 0.1f; // 타일 검색 간격
    [SerializeField] private float shockwavePower = 30f; // 충격파 위력
    [SerializeField] private LayerMask playerLayer;
    #endregion

    #region 공개 프로퍼티
    public BossForm CurrentForm => currentForm;
    public Transform Player => target;
    public NavMeshAgent NmAgent => nmAgent;
    public Animator Animator => anim;
    public FieldOfView FOV => fov;
    public AbilityManager AbilityManager => abilityManager;
    public BossBase GetCurrentActiveBoss() => currentActiveBoss;

    public GameObject basic => basicFormObject;
    public GameObject Worm => wormFormObject;
    public GameObject Troy => trojanFormObject;
    public GameObject Ransomware => ransomwareFormObject;

    public VoxelFloatEffect FLOATINGEFFECT => vFE;

    public StateMachine<UnknownVirusBoss> Fsm => fsm;  // 기존 fsm 필드를 public으로 노출

    public float GetFormTimer()
    {
        return formTimer;
    }
    public float GetStayDuration()
    {
        return formStayDuration;
    }

    public void ResetFormTimer()
    {
        formTimer = Time.time;
    }
    #endregion

    #region 유니티 라이프사이클
    private void Start()
    {
        InitializeComponents();
        InitiailzeEffects();
        InitializeFormHierarchy();
        InitializeAbilities();
        InitializeStates();
        InitializeFSM();

        Debug.Log("[UnknownVirusBoss] 초기화 완료");
    }

    private void Update()
    {
        // FSM 업데이트
        UpdateRandomValue();
        fsm.Update();

        // 죽음 상태 확인
        if (bossStatus.GetHealth() <= 0 && !(fsm.CurrentState is DefeatedState_UnknownVirus))
        {
            HandleDeath();
        }
    }
    #endregion

    #region 초기화 메서드

    private void InitiailzeEffects()
    {
        vFE = GetComponentInChildren<VoxelFloatEffect>();
    }
    private void InitializeComponents()
    {
        // 매니저 찾기
        tileManager = FindObjectOfType<TileManager>();
        target = GameObject.FindWithTag("Player").transform;
        vFE = GetComponentInChildren<VoxelFloatEffect>();

        // 컴포넌트 가져오기
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        fov = GetComponent<FieldOfView>();

        Debug.Log("[UnknownVirusBoss] 컴포넌트 초기화 완료");
    }

    private void InitializeFormHierarchy()
    {
        // 폼 오브젝트 유효성 검사
        if (basicFormObject == null)
        {
            Debug.LogError("[UnknownVirusBoss] 기본 폼 오브젝트가 없습니다!");
            return;
        }

        // 폼 컴포넌트 캐시
        if (wormFormObject != null)
        {
            wormComponent = wormFormObject.GetComponent<WormBossPrime>();
            wormFormObject.SetActive(false);
        }

        if (trojanFormObject != null)
        {
            trojanComponent = trojanFormObject.GetComponent<Troy>();
            trojanFormObject.SetActive(false);
        }

        if (ransomwareFormObject != null)
        {
            ransomwareComponent = ransomwareFormObject.GetComponent<Ransomware>();
            ransomwareFormObject.SetActive(false);
        }

        // 초기 상태 - 기본 상태 활성화
        ActivateBasicFormOnly();

        Debug.Log("[UnknownVirusBoss] 폼 구조 초기화 완료");
    }

    private void InitializeAbilities()
    {
        // 맵 공격 능력 활성화
        abilityManager.SetAbilityActive("MapAttack");
        abilityManager.SetMaxCoolTime("MapAttack");

        abilityManager.SetAbilityActive("Transform");
        abilityManager.SetMaxCoolTime("Transform");

        Debug.Log("[UnknownVirusBoss] 능력 초기화 완료");
    }

    private void InitializeStates()
    {
        introState = new IntroState_UnknownVirus(this);
        basicState = new BasicCombatState_UnknownVirus(this);
        mapAttackState = new MapAttackState_UnknownVirus(this);
        transformState = new TransformState_UnknownVirus(this);
        deadState = new DefeatedState_UnknownVirus(this);

        Debug.Log("[UnknownVirusBoss] 상태 초기화 완료");
    }

    private void InitializeFSM()
    {
        // 상태 인스턴스 생성
        var states = CreateStates();

        // 초기 상태를 인트로로 설정한 FSM 생성
        fsm = new StateMachine<UnknownVirusBoss>(states.introState);

        // 전환 설정
        SetupTransitions(states);

        Debug.Log("[UnknownVirusBoss] FSM 초기화 완료");
    }

    private (
        IntroState_UnknownVirus introState,
        BasicCombatState_UnknownVirus basicState,
        MapAttackState_UnknownVirus mapAttackState,
        TransformState_UnknownVirus transformState,
        DefeatedState_UnknownVirus deadState
    ) CreateStates()
    {
        return (
            new IntroState_UnknownVirus(this),
            new BasicCombatState_UnknownVirus(this),
            new MapAttackState_UnknownVirus(this),
            new TransformState_UnknownVirus(this),
            new DefeatedState_UnknownVirus(this)
        );
    }

    private void SetupTransitions((
        IntroState_UnknownVirus introState,
        BasicCombatState_UnknownVirus basicState,
        MapAttackState_UnknownVirus mapAttackState,
        TransformState_UnknownVirus transformState,
        DefeatedState_UnknownVirus deadState
    ) s)
    {
        // 인트로 → 기본 상태
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.introState, s.basicState, () => true));

        // 기본 상태 → 맵 공격 (Base 활성화 상태 체크 추가)
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.basicState, s.mapAttackState,
            () => abilityManager.GetAbilityRemainingCooldown("MapAttack") == 0 &&
                 currentRandomValue < mapAttackChance &&
                 basicFormObject != null && basicFormObject.activeInHierarchy));

        // 맵 공격 → 기본 상태
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.mapAttackState, s.basicState,
            () => mapAttackState.IsAnimationFinished()
        ));

        // 기본 상태 → 변환
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.basicState, s.transformState,
            () => abilityManager.GetAbilityRemainingCooldown("Transform") == 0 &&
                 currentRandomValue > mapAttackChance));

        // 폼 변환 상태 → 기본 상태
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.transformState, s.basicState,
            () => transformState.IsAnimationFinished()
        ));


        // 글로벌 죽음 상태 전환 (인트로 제외)
        List<State<UnknownVirusBoss>> exceptStates = new List<State<UnknownVirusBoss>> { s.introState };
        fsm.AddGlobalTransition(s.deadState, () => bossStatus.GetHealth() <= 0, exceptStates);
    }
    #endregion



    #region 폼 관리 메서드
    private void ActivateBasicFormOnly()
    {
        // 기본 상태 활성화하고 나머지는 비활성화
        if (basicFormObject != null)
            basicFormObject.SetActive(true);

        if (wormFormObject != null)
            wormFormObject.SetActive(false);

        if (trojanFormObject != null)
            trojanFormObject.SetActive(false);

        if (ransomwareFormObject != null)
            ransomwareFormObject.SetActive(false);

        // 현재 폼 설정
        currentForm = BossForm.Basic;
        currentActiveFormObject = basicFormObject;
        currentActiveBoss = null;
    }

    public void ApplyForm(BossForm form)
    {
        // 폼 전환 방지
        if (form == currentForm) return;

        // 현재 활성화된 폼 비활성화
        DeactivateCurrentForm();

        // 새 폼 활성화
        ActivateForm(form);

        // 폼 타이머 리셋
        formTimer = Time.time;

        // 현재 폼 업데이트
        currentForm = form;

        Debug.Log($"[UnknownVirusBoss] {form} 폼으로 변환 완료");
    }

    private void DeactivateCurrentForm()
    {
        // 현재 활성화된 폼 오브젝트 비활성화
        if (currentActiveFormObject != null)
        {
            // 폼 오브젝트 비활성화
            currentActiveFormObject.SetActive(false);
        }

        if (currentActiveBoss != null)
        {
            currentActiveBoss.ResetBoss();
        }

        // 현재 활성 상태 변수 초기화
        currentActiveBoss = null;
    }

    private void ActivateForm(BossForm form)
    {
        GameObject targetFormObject = null;

        // 대상 폼에 따른 오브젝트 선택

        switch (form)
        {
            case BossForm.Basic:
                targetFormObject = basicFormObject;
                currentActiveBoss = null;
                break;
            case BossForm.Worm:
                targetFormObject = wormFormObject;
                currentActiveBoss = wormComponent;
                targetFormObject.GetComponent<VirusDissolveEffect>().ResetDissolve();
                break;
            case BossForm.Trojan:
                targetFormObject = trojanFormObject;
                currentActiveBoss = trojanComponent;
                targetFormObject.GetComponent<VirusDissolveEffect>().ResetDissolve();
                break;
            case BossForm.Ransomware:
                targetFormObject = ransomwareFormObject;
                currentActiveBoss = ransomwareComponent;
                targetFormObject.GetComponent<VirusDissolveEffect>().ResetDissolve();
                break;
        }

        // 새로운 폼 오브젝트 활성화
        if (targetFormObject != null)
        {
            targetFormObject.SetActive(true);

            // 위치/회전 동기화
            SyncFormTransform(targetFormObject);

            // 체력 동기화
            SyncHealthToActiveForm(targetFormObject, form);

            // 현재 활성 폼 오브젝트 업데이트
            currentActiveFormObject = targetFormObject;
        }
    }

    private void SyncFormTransform(GameObject formObject)
    {
        if (formObject == null || formObject == basicFormObject)
            return;

        // 현재 폼 위치와 회전을 동기화
        formObject.transform.position = currentActiveFormObject.transform.position;
        formObject.transform.rotation = currentActiveFormObject.transform.rotation;
    }

    private void SyncHealthToActiveForm(GameObject formObject, BossForm form)
    {
        // 현재 체력 비율 계산
        float healthRatio = bossStatus.GetHealth() / bossStatus.GetMaxHealth();

        // 새로 생성 체력 컴포넌트 가져오기
        BossStatus targetStatus = formObject.GetComponent<BossStatus>();


        // 새로 체력 설정 (비율 유지하며)
        if (targetStatus != null)
        {
            float newHealth = targetStatus.GetMaxHealth() * healthRatio;
            targetStatus.SetHealth(newHealth);
        }
    }

    private void SyncHealthFromActiveBoss()
    {
        if (currentForm == BossForm.Basic || currentActiveBoss == null)
            return;

        // 현재 활성 보스 체력 비율 계산
        BossStatus formStatus = currentActiveBoss.GetComponent<BossStatus>();
        if (formStatus == null)
            return;

        float healthRatio = formStatus.GetHealth() / formStatus.GetMaxHealth();

        // 전체 체력 비율 동기화
        bossStatus.SetHealth(bossStatus.GetMaxHealth() * healthRatio);

        // UI 업데이트
        HPBar?.SetRatio(bossStatus.GetHealth(), bossStatus.GetMaxHealth());
    }

    private void DeactivateAllForms()
    {
        // 모든 폼 비활성화 (기본폼 제외)
        if (wormFormObject != null)
            wormFormObject.SetActive(false);

        if (trojanFormObject != null)
            trojanFormObject.SetActive(false);

        if (ransomwareFormObject != null)
            ransomwareFormObject.SetActive(false);

        // 상태 초기화
        basicFormObject.SetActive(true);
        currentActiveFormObject = basicFormObject;
        currentActiveBoss = null;
    }
    #endregion

    #region 이벤트 핸들
  

    public void PrepareToReturnToBasicForm()
    {
        fsm.ForcedTransition(transformState);
    }

    private void HandleDeath()
    {
        StopAllCoroutines();

        // 폼들 처리
        DeactivateAllForms();

        // 죽음 상태로 전환
        fsm.ForcedTransition(deadState);

        Debug.Log("[UnknownVirusBoss] 보스 사망");
    }

    public void RequestFormChange(BossForm newForm)
    {
        // 변환 효과 활성화
        if (transformationVFX != null)
            transformationVFX.SetActive(true);
    }

    #endregion

    #region 데미지 처리
    public override void TakeDamage(float damage, bool showDamage = true)
    {
        // 현재 활성화된 보스가 있으면 전달
        if (currentForm != BossForm.Basic && currentActiveBoss != null)
        {
            // 활성 보스 데미지 처리
            currentActiveBoss.TakeDamage(damage, showDamage);

            // 기본 보스와 데미지 동기화
            SyncHealthFromActiveBoss();
        }
        else
        {
            // 기본 폼 데미지 처리
            bossStatus.DecreaseHealth(damage);

            // 데미지 이벤트 및 UI 표시
            EventManager.Instance.TriggerMonsterDamagedEvent();
            if (showDamage && UIDamaged != null)
            {
                var popup = Instantiate(
                    UIDamaged,
                    transform.position + new Vector3(0, UnityEngine.Random.Range(0f, height / 2), 0),
                    Quaternion.identity
                ).GetComponent<UIDamage>();

                popup.damage = damage;
            }

            // UI 업데이트
            HPBar?.SetRatio(bossStatus.GetHealth(), bossStatus.GetMaxHealth());
        }

        // 죽음 체크
        if (bossStatus.GetHealth() <= 0 && !(fsm.CurrentState is DefeatedState_UnknownVirus))
        {
            HandleDeath();
        }
    }
    #endregion

    #region 애니메이션 이벤트 핸들러
    // 맵 공격 애니메이션 완료 이벤트
    public void OnMapAttackFinished()
    {
        if (mapAttackState != null)
        {
            mapAttackState.OnAttackFinished();
        }
    }

    // 기본 공격 애니메이션 완료 이벤트
    public void OnBasicAttackFinished()
    {
        // 기본 공격 완료 처리 (필요시)
    }
    #endregion

    #region 맵 공격 구현
    // 맵 공격 트리거 메서드
    public void TriggerMapAttack()
    {
        try
        {
            if (tileManager == null)
            {
                Debug.LogError("맵 공격 실행 불가: TileManager가 null입니다");
                mapAttackState?.OnAttackFinished();
                return;
            }

            // 검색 알고리즘 선택 (랜덤)
            int searchMethod = UnityEngine.Random.Range(0, 3);
            StartCoroutine(ExecuteMapAttack(searchMethod));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TriggerMapAttack 오류: {e.Message}\n{e.StackTrace}");
            mapAttackState?.OnAttackFinished();
        }
    }

    private IEnumerator ExecuteMapAttack(int searchMethod)
    {
        // 플레이어 주변 좌표 계산
        Vector3 playerPos = target.position;

        // 월드 좌표를 타일 그리드 좌표로 변환
        int centerX = Mathf.RoundToInt(playerPos.x / 2);
        int centerZ = Mathf.RoundToInt(playerPos.z / 2);

        Debug.Log($"그리드 위치 [{centerX}, {centerZ}]에서 맵 공격 시작");

        // 목표 타일 랜덤하게 선택 (공격 영역 내)
        int targetX = centerX + UnityEngine.Random.Range(-attackAreaSize / 2, attackAreaSize / 2 + 1);
        int targetZ = centerZ + UnityEngine.Random.Range(-attackAreaSize / 2, attackAreaSize / 2 + 1);

        // 유효한 맵 범위 내로 제한
        targetX = Mathf.Clamp(targetX, 0, tileManager.GetMapSize - 1);
        targetZ = Mathf.Clamp(targetZ, 0, tileManager.GetMapSize - 1);

        // 검색 방식에 따른 효과 실행
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

    // 선형 타일 검색 방식
    private IEnumerator LinearTileSearch(int centerX, int centerZ, int targetX, int targetZ)
    {
        int halfSize = attackAreaSize / 2;

        // 검색 영역 계산 (플레이어 중심으로 attackAreaSize x attackAreaSize 영역)
        int minX = Mathf.Max(0, centerX - halfSize);
        int maxX = Mathf.Min(tileManager.GetMapSize - 1, centerX + halfSize);
        int minZ = Mathf.Max(0, centerZ - halfSize);
        int maxZ = Mathf.Min(tileManager.GetMapSize - 1, centerZ + halfSize);

        Debug.Log($"선형 검색 영역: [{minX},{minZ}] 부터 [{maxX},{maxZ}], 목표: [{targetX},{targetZ}]");

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

                    // 레이저 데미지 설정 (VirusLaser 컴포넌트가 있다고 가정)
                    var virusLaser = laser.GetComponent<VirusLaser>();
                    if (virusLaser != null)
                    {
                        virusLaser.SetDamage(abilityManager.GetAbiltiyDmg("MapAttack"));
                    }
                }

                yield return new WaitForSeconds(tileSearchInterval);

                // 타일 타겟을 찾았으면 충격파 효과
                if (x == targetX && z == targetZ)
                {
                    // 타겟 타일 표시
                    HighlightTile(x, z, Color.green);

                    // 충격파 생성 (TileManager의 CreateShockwave 코루틴 호출)
                    StartCoroutine(tileManager.CreateShockwave(x, z, halfSize, shockwavePower));

                    yield break;
                }

                // 검색 완료된 타일 복원
                ResetTileColor(x, z);
            }
        }
    }

    // 이진 타일 검색 방식
    private IEnumerator BinaryTileSearch(int centerX, int centerZ, int targetX, int targetZ)
    {
        int halfSize = attackAreaSize / 2;

        // 검색 영역 설정
        int leftX = Mathf.Max(0, centerX - halfSize);
        int rightX = Mathf.Min(tileManager.GetMapSize - 1, centerX + halfSize);
        int topZ = Mathf.Max(0, centerZ - halfSize);
        int bottomZ = Mathf.Min(tileManager.GetMapSize - 1, centerZ + halfSize);

        Debug.Log($"이진 검색 영역: [{leftX},{topZ}] 부터 [{rightX},{bottomZ}], 목표: [{targetX},{targetZ}]");

        int iterations = 0;
        int maxIterations = 10; // 최대 반복 제한

        while (leftX <= rightX && topZ <= bottomZ && iterations < maxIterations)
        {
            iterations++;
            int midX = (leftX + rightX) / 2;
            int midZ = (topZ + bottomZ) / 2;

            // 현재 검색 영역 표시
            for (int x = leftX; x <= rightX; x++)
            {
                for (int z = topZ; z <= bottomZ; z++)
                {
                    HighlightTile(x, z, Color.red);

                    // 경계 타일에서 레이저 효과
                    if (x == leftX || x == rightX || z == topZ || z == bottomZ)
                    {
                        if (laserPrefab != null)
                        {
                            Vector3 tilePos = new Vector3(x * 2, 0, z * 2);
                            GameObject laser = Instantiate(laserPrefab,
                                tilePos + Vector3.up * 0.2f,
                                Quaternion.identity);

                            // 레이저 데미지 설정
                            var virusLaser = laser.GetComponent<VirusLaser>();
                            if (virusLaser != null)
                            {
                                virusLaser.SetDamage(abilityManager.GetAbiltiyDmg("MapAttack"));
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);

            // 색상 효과 초기화
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
                yield break;
            }

            // 검색 영역 축소
            if (targetX < midX)
                rightX = midX - 1;
            else
                leftX = midX + 1;

            if (targetZ < midZ)
                bottomZ = midZ - 1;
            else
                topZ = midZ + 1;
        }

        // 타겟 강제 표시 (최대 검색을 수행해도 못찾은 경우)
        HighlightTile(targetX, targetZ, Color.green);
    }

    // 랜덤 타일 검색 방식
    private IEnumerator RandomTileSearch(int centerX, int centerZ, int targetX, int targetZ)
    {
        int halfSize = attackAreaSize / 2;

        // 검색 영역 설정
        int minX = Mathf.Max(0, centerX - halfSize);
        int maxX = Mathf.Min(tileManager.GetMapSize - 1, centerX + halfSize);
        int minZ = Mathf.Max(0, centerZ - halfSize);
        int maxZ = Mathf.Min(tileManager.GetMapSize - 1, centerZ + halfSize);

        Debug.Log($"랜덤 검색 영역: [{minX},{minZ}] 부터 [{maxX},{maxZ}], 목표: [{targetX},{targetZ}]");

        HashSet<Vector2Int> searchedTiles = new HashSet<Vector2Int>();
        int maxAttempts = Mathf.Min(20, (maxX - minX + 1) * (maxZ - minZ + 1));

        for (int i = 0; i < maxAttempts; i++)
        {
            // 검색 영역 내에서 랜덤 타일 선택
            int x = UnityEngine.Random.Range(minX, maxX + 1);
            int z = UnityEngine.Random.Range(minZ, maxZ + 1);
            Vector2Int tilePos = new Vector2Int(x, z);

            // 이미 검색한 타일이면 다시 선택 (최대 3회)
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

            // 타겟 타겟을 찾았는지 확인
            if (x == targetX && z == targetZ)
            {
                HighlightTile(x, z, Color.green);
                yield break;
            }

            // 검색 완료된 타일 복원
            ResetTileColor(x, z);
        }

        // 최대 시도 횟수를 초과해도 타겟을 찾지 못한 경우
        HighlightTile(targetX, targetZ, Color.green);
    }

    // 타일 색상 변경 메서드
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

    // 타일 색상 원래 복원 메서드
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

    // 공격 정리 후 처리 메서드
    public void CleanupMapAttack()
    {
    }
    #endregion

    void UpdateRandomValue()
    {
        currentRandomValue = UnityEngine.Random.value;
    }

}