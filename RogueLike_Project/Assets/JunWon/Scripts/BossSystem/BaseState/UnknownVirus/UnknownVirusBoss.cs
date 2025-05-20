using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnknownVirusBoss : BossBase
{
    public enum BossForm { Basic, Worm, Trojan, Ransomware }

    #region 폼 계층 관리
    [Header("폼 오브젝트")]
    [SerializeField] private GameObject basicFormObject;     // 기본 폼 오브젝트 (자기 자신)
    [SerializeField] private GameObject wormFormObject;      // 웜 폼 오브젝트 (자식)
    [SerializeField] private GameObject trojanFormObject;    // 트로이 목마 폼 오브젝트 (자식)
    [SerializeField] private GameObject ransomwareFormObject; // 랜섬웨어 폼 오브젝트 (자식)

    // 각 폼 컴포넌트 캐싱
    private WormBossPrime wormComponent;
    private Troy trojanComponent;
    private Ransomware ransomwareComponent;

    // 현재 활성화된 폼 참조
    private GameObject currentActiveFormObject;
    private BossBase currentActiveBoss;
    private bool isTransforming = false;
    private BossForm currentForm = BossForm.Basic;

    // 폼 변경 쿨타임 관리
    private float lastFormChangeTime = 0f;
    private float formStayDuration = 15f; // 변신한 폼에 머무는 시간
    private float formTimer = 0f;
    #endregion

    #region 보스 설정
    [Header("전투 설정")]
    [SerializeField] private float baseAttackDamage = 20f;
    [SerializeField] private float baseAttackRange = 10f;
    [SerializeField] private float baseAttackCooldown = 3f;

    [Header("맵 공격")]
    [SerializeField] private GameObject mapAttackVFX;
    [SerializeField] private float mapAttackCooldown = 15f;
    [Range(0, 1)][SerializeField] private float mapAttackChance = 0.8f;
    private float lastMapAttackTime = 0f;

    [Header("폼 변경")]
    [SerializeField] private GameObject transformationVFX;
    [SerializeField] private float transformationTime = 3f;
    [SerializeField] private float formChangeCooldown = 30f;
    [Range(0, 1)][SerializeField] private float formChangeChance = 0.3f;

    [Header("컴포넌트")]
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
    private WormCombatState_UnknownVirus wormCombatState;
    private TrojanCombatState_UnknownVirus trojanCombatState;
    private RansomwareCombatState_UnknownVirus ransomwareCombatState;
    private DefeatedState_UnknownVirus deadState;
    #endregion

    #region 상태 세팅 메서드

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
    [SerializeField] private float shockwavePower = 30f; // 충격파 세기
    [SerializeField] private LayerMask playerLayer;
    #endregion

    #region 공개 프로퍼티
    public BossForm CurrentForm => currentForm;
    public Transform Player => target;
    public NavMeshAgent NmAgent => nmAgent;
    public Animator Animator => anim;
    public BossStatus MonsterStatus => bossStatus;
    public FieldOfView FOV => fov;
    public AbilityManager AbilityManager => abilityManager;
    public BossBase GetCurrentActiveBoss() => currentActiveBoss;
    #endregion

    #region 유니티 라이프사이클
    private void Start()
    {
        InitializeComponents();
        InitializeFormHierarchy();
        InitializeAbilities();
        InitializeStates();
        InitializeFSM();

        Debug.Log("[UnknownVirusBoss] 초기화 완료");
    }

    private void Update()
    {
        // FSM 업데이트
        fsm.Update();

        // 현재 활성화된 폼의 체력을 메인 보스에 동기화
        if (currentForm != BossForm.Basic && currentActiveBoss != null)
        {
            SyncHealthFromActiveBoss();
        }

        // 폼별 로직 업데이트
        UpdateActiveFormLogic();

        // 폼 변경 타이머 체크 (변신 폼에 머무는 시간)
        CheckFormTransformationTimer();

        // 사망 상태 확인
        if (bossStatus.GetHealth() <= 0 && !(fsm.CurrentState is DefeatedState_UnknownVirus))
        {
            HandleDeath();
        }
    }
    #endregion

    #region 초기화 메서드
    private void InitializeComponents()
    {
        // 참조 찾기
        tileManager = FindObjectOfType<TileManager>();
        target = GameObject.FindWithTag("Player").transform;

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

        // 폼 컴포넌트 캐싱
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

        // 초기 상태 - 기본 폼만 활성화
        ActivateBasicFormOnly();

        Debug.Log("[UnknownVirusBoss] 폼 계층 초기화 완료");
    }

    private void InitializeAbilities()
    {
        // 맵 공격 능력 활성화
        abilityManager.SetAbilityActive("MapAttack");
        abilityManager.SetMaxCoolTime("MapAttack");

        Debug.Log("[UnknownVirusBoss] 능력 초기화 완료");
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

        Debug.Log("[UnknownVirusBoss] 상태 초기화 완료");
    }

    private void InitializeFSM()
    {
        // 상태 인스턴스 생성
        var states = CreateStates();

        // 초기 상태를 인트로로 설정해 FSM 생성
        fsm = new StateMachine<UnknownVirusBoss>(states.introState);

        // 전이 설정
        SetupTransitions(states);

        Debug.Log("[UnknownVirusBoss] FSM 초기화 완료");
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
        // 인트로 → 기본 전투
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.introState, s.basicState, () => true));

        // 기본 전투 → 맵 공격
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.basicState, s.mapAttackState,
            () => abilityManager.GetAbilityRemainingCooldown("MapAttack") == 0 &&
                 UnityEngine.Random.value < mapAttackChance));

        // 맵 공격 → 기본 전투
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.mapAttackState, s.basicState,
            () => mapAttackState.IsAnimationFinished()
        ));

        // 기본 전투 → 변신
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.basicState, s.transformState,
            () => Time.time - lastFormChangeTime >= formChangeCooldown &&
                 UnityEngine.Random.value < formChangeChance));

        // 변신 → 각 폼 전투 상태
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

        // 각 폼 전투 → 기본 전투
        // 참고: 변신 폼에서 기본 폼으로 돌아오는 것은 Update에서 타이머로 처리함
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.wormCombatState, s.transformState,
            () => formTimer >= formStayDuration));
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.trojanCombatState, s.transformState,
            () => formTimer >= formStayDuration));
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.ransomwareCombatState, s.transformState,
            () => formTimer >= formStayDuration));

        // 전역 사망 상태 전이 (인트로 제외)
        List<State<UnknownVirusBoss>> exceptStates = new List<State<UnknownVirusBoss>> { s.introState };
        fsm.AddGlobalTransition(s.deadState, () => bossStatus.GetHealth() <= 0, exceptStates);
    }
    #endregion



    #region 폼 관리 메서드
    private void ActivateBasicFormOnly()
    {
        // 기본 폼만 활성화하고 나머지는 비활성화
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
        // 폼 전환 로직
        if (form == currentForm) return;

        // 현재 활성화된 폼 비활성화
        DeactivateCurrentForm();

        // 새 폼 활성화
        ActivateForm(form);

        // 폼 타이머 리셋
        formTimer = 0f;

        // 현재 폼 업데이트
        currentForm = form;

        // 폼 변경 후 능력 업데이트
        UpdateFormSpecificAbilities();

        Debug.Log($"[UnknownVirusBoss] {form} 폼으로 변신 완료");
    }

    private void DeactivateCurrentForm()
    {
        // 현재 활성화된 폼 오브젝트 비활성화
        if (currentActiveFormObject != null)
        {
            // 체력 상태 동기화
            SyncHealthFromActiveBoss();

            // 폼 오브젝트 비활성화
            currentActiveFormObject.SetActive(false);
        }

        // 현재 활성 보스 참조 초기화
        currentActiveBoss = null;
    }

    private void ActivateForm(BossForm form)
    {
        GameObject targetFormObject = null;

        // 폼에 따라 대상 오브젝트 선택
        switch (form)
        {
            case BossForm.Basic:
                targetFormObject = basicFormObject;
                currentActiveBoss = null;
                break;

            case BossForm.Worm:
                targetFormObject = wormFormObject;
                currentActiveBoss = wormComponent;
                break;

            case BossForm.Trojan:
                targetFormObject = trojanFormObject;
                currentActiveBoss = trojanComponent;
                break;

            case BossForm.Ransomware:
                targetFormObject = ransomwareFormObject;
                currentActiveBoss = ransomwareComponent;
                break;
        }

        // 대상 폼 오브젝트 활성화
        if (targetFormObject != null)
        {
            // 위치/회전 동기화
            SyncFormTransform(targetFormObject);

            // 체력 동기화
            SyncHealthToActiveForm(targetFormObject, form);

            // 활성화
            targetFormObject.SetActive(true);

            // 현재 활성 폼 오브젝트 업데이트
            currentActiveFormObject = targetFormObject;
        }
    }

    private void SyncFormTransform(GameObject formObject)
    {
        if (formObject == null || formObject == basicFormObject)
            return;

        // 기본 폼 위치를 기준으로 동기화
        formObject.transform.position = transform.position;
        formObject.transform.rotation = transform.rotation;
    }

    private void SyncHealthToActiveForm(GameObject formObject, BossForm form)
    {
        // 현재 체력 비율 계산
        float healthRatio = bossStatus.GetHealth() / bossStatus.GetMaxHealth();

        // 대상 폼의 체력 컴포넌트 가져오기
        BossStatus targetStatus = null;

        switch (form)
        {
            case BossForm.Worm:
                if (wormComponent != null)
                    targetStatus = wormComponent.BossStatus;
                break;

            case BossForm.Trojan:
                if (trojanComponent != null)
                    targetStatus = trojanComponent.BossStatus;
                break;

            case BossForm.Ransomware:
                if (ransomwareComponent != null)
                    targetStatus = ransomwareComponent.BossStatus;
                break;
        }

        // 대상 체력 설정 (비율 동일하게)
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

        // 현재 활성 폼의 체력 비율 계산
        BossStatus formStatus = currentActiveBoss.GetComponent<BossStatus>();
        if (formStatus == null)
            return;

        float healthRatio = formStatus.GetHealth() / formStatus.GetMaxHealth();

        // 본체 체력 비율 동기화
        bossStatus.SetHealth(bossStatus.GetMaxHealth() * healthRatio);

        // UI 갱신
        HPBar?.SetRatio(bossStatus.GetHealth(), bossStatus.GetMaxHealth());
    }

    private void UpdateFormSpecificAbilities()
    {
        // 현재 폼에 따라 특정 능력 활성화/비활성화
        switch (currentForm)
        {
            case BossForm.Basic:
                abilityManager.SetAbilityActive("BasicAttack");
                abilityManager.SetAbilityActive("MapAttack");
                break;

            case BossForm.Worm:
                // 웜 폼 특화 능력 설정 (필요시)
                break;

            case BossForm.Trojan:
                // 트로이 목마 폼 특화 능력 설정 (필요시)
                break;

            case BossForm.Ransomware:
                // 랜섬웨어 폼 특화 능력 설정 (필요시)
                break;
        }
    }

    private void DeactivateAllForms()
    {
        // 모든 폼 비활성화 (사망시 사용)
        if (wormFormObject != null)
            wormFormObject.SetActive(false);

        if (trojanFormObject != null)
            trojanFormObject.SetActive(false);

        if (ransomwareFormObject != null)
            ransomwareFormObject.SetActive(false);

        // 참조 초기화
        currentActiveFormObject = basicFormObject;
        currentActiveBoss = null;
    }
    #endregion

    #region 업데이트 로직
    private void UpdateActiveFormLogic()
    {
        // 현재 폼에 따른 특정 로직 업데이트
        switch (currentForm)
        {
            case BossForm.Basic:
                // 기본 폼에서의 로직
                UpdateBasicFormLogic();
                break;

            case BossForm.Worm:
                // 웜 폼에서의 로직
                UpdateWormFormLogic();
                break;

            case BossForm.Trojan:
                // 트로이 목마 폼에서의 로직
                UpdateTrojanFormLogic();
                break;

            case BossForm.Ransomware:
                // 랜섬웨어 폼에서의 로직
                UpdateRansomwareFormLogic();
                break;
        }
    }

    private void UpdateBasicFormLogic()
    {
        // 기본 폼에서의 전투 및 의사결정 처리
        if (fsm.CurrentState is BasicCombatState_UnknownVirus)
        {
            // 기본 공격 및 전투 로직 처리 (필요시)

            // 폼 변경 결정 로직 (주기적으로 체크)
            if (Time.time - lastFormChangeTime >= formChangeCooldown &&
                UnityEngine.Random.value < formChangeChance * Time.deltaTime * 5f) // 확률 조정
            {
                DecideFormTransformation();
            }
        }
    }

    private void UpdateWormFormLogic()
    {
        // 웜 폼 특화 로직 (필요시)
        if (wormComponent != null && wormComponent.isActiveAndEnabled)
        {
            // 추가 로직이 필요하면 여기에 구현
        }
    }

    private void UpdateTrojanFormLogic()
    {
        // 트로이 목마 폼 특화 로직 (필요시)
        if (trojanComponent != null && trojanComponent.isActiveAndEnabled)
        {
            // 추가 로직이 필요하면 여기에 구현
        }
    }

    private void UpdateRansomwareFormLogic()
    {
        // 랜섬웨어 폼 특화 로직 (필요시)
        if (ransomwareComponent != null && ransomwareComponent.isActiveAndEnabled)
        {
            // 추가 로직이 필요하면 여기에 구현
        }
    }

    private void CheckFormTransformationTimer()
    {
        // 변신 폼에 머무는 시간 체크
        if (currentForm != BossForm.Basic && !isTransforming)
        {

            // 지정된 시간 이상 지났으면 기본 폼으로 복귀 준비
            if (formTimer >= formStayDuration &&
                !(fsm.CurrentState is TransformState_UnknownVirus))
            {
                PrepareToReturnToBasicForm();
            }
        }
    }

    private void PrepareToReturnToBasicForm()
    {
        // 기본 폼으로 복귀 전환 시작
        isTransforming = true;
        lastFormChangeTime = Time.time;

        // 변신 상태로 전환
        fsm.ForcedTransition(transformState);

        // 기본 폼으로 변신 요청
        RequestFormChange(BossForm.Basic);
    }

    private void DecideFormTransformation()
    {
        // 다음 변신 폼 결정 (랜덤)
        int formIndex = UnityEngine.Random.Range(1, 4); // 1~3 (Worm, Trojan, Ransomware)
        BossForm nextForm = (BossForm)formIndex;

        // 선택한 폼 오브젝트가 없으면 다른 폼 선택
        switch (nextForm)
        {
            case BossForm.Worm:
                if (wormFormObject == null) nextForm = BossForm.Trojan;
                break;
            case BossForm.Trojan:
                if (trojanFormObject == null) nextForm = BossForm.Ransomware;
                break;
            case BossForm.Ransomware:
                if (ransomwareFormObject == null) nextForm = BossForm.Worm;
                break;
            default:
                if (basicFormObject == null) nextForm = BossForm.Basic;
                break;
        }

        // 선택한 폼도 없으면 기본 폼 유지
        switch (nextForm)
        {
            case BossForm.Worm:
                if (wormFormObject == null) return;
                break;
            case BossForm.Trojan:
                if (trojanFormObject == null) return;
                break;
            case BossForm.Ransomware:
                if (ransomwareFormObject == null) return;
                break;
            default:
                if (basicFormObject == null) return;
                break;
        }

        // 변신 시작
        lastFormChangeTime = Time.time;

        // 변신 상태로 전환
        fsm.ForcedTransition(transformState);

        // 변신 요청
        RequestFormChange(nextForm);
    }

    private void HandleDeath()
    {
        // 사망 처리
        DeactivateAllForms();

        // 사망 상태로 전환
        fsm.ForcedTransition(deadState);

        Debug.Log("[UnknownVirusBoss] 보스 사망");
    }
    #endregion

    #region 변신 로직
    /// <summary>TransformState에서 호출</summary>
    public void RequestFormChange(BossForm newForm)
    {
        if (isTransforming) return;

        isTransforming = true;
        StartCoroutine(TransformRoutine(newForm));
    }

    private IEnumerator TransformRoutine(BossForm newForm)
    {
        // 변신 효과 활성화
        if (transformationVFX != null)
            transformationVFX.SetActive(true);

        Debug.Log($"[UnknownVirusBoss] {newForm} 폼으로 변신 시작");

        // 변신 시간 대기
        yield return new WaitForSeconds(transformationTime);

        // 해당 폼 적용
        ApplyForm(newForm);

        // 변신 효과 비활성화
        if (transformationVFX != null)
            transformationVFX.SetActive(false);

        // 변신 완료
        isTransforming = false;

        // TransformState에 변신 완료 알림
        if (transformState != null)
            transformState.OnTransformationComplete();

        Debug.Log($"[UnknownVirusBoss] {newForm} 폼으로 변신 완료");
    }
    #endregion

    #region 데미지 처리
    public override void TakeDamage(float damage, bool showDamage = true)
    {
        // 변신 중엔 데미지 무시
        if (isTransforming)
            return;

        // 현재 활성화된 폼에 데미지 전달
        if (currentForm != BossForm.Basic && currentActiveBoss != null)
        {
            // 변신 폼에 데미지 적용
            currentActiveBoss.TakeDamage(damage, showDamage);

            // 기본 폼에도 데미지 동기화
            SyncHealthFromActiveBoss();
        }
        else
        {
            // 기본 폼 데미지 처리
            bossStatus.DecreaseHealth(damage);

            // 피해 이벤트 및 UI 표시
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

            // UI 갱신
            HPBar?.SetRatio(bossStatus.GetHealth(), bossStatus.GetMaxHealth());
        }

        // 사망 체크
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

    #region 맵 공격 로직
    // 맵 공격 트리거 메서드
    public void TriggerMapAttack()
    {
        try
        {
            lastMapAttackTime = Time.time;

            if (tileManager == null)
            {
                Debug.LogError("맵 공격 실행 불가: TileManager가 null입니다");
                mapAttackState?.OnAttackFinished();
                return;
            }

            // 검색 알고리즘 선택 (랜덤)
            int searchMethod = UnityEngine.Random.Range(0, 3);
            StartCoroutine(ExecuteMapAttack(searchMethod));

            mapAttackState?.OnAttackFinished();
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

        Debug.Log($"선형 검색 영역: [{minX},{minZ}] 에서 [{maxX},{maxZ}], 목표: [{targetX},{targetZ}]");

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

                // 타겟 타일을 찾으면 충격파 효과
                if (x == targetX && z == targetZ)
                {
                    // 타겟 타일 표시
                    HighlightTile(x, z, Color.green);

                    // 충격파 생성 (TileManager의 CreateShockwave 코루틴 호출)
                    StartCoroutine(tileManager.CreateShockwave(x, z, halfSize, shockwavePower));

                    // 해당 위치 주변에 데미지 적용
                    ApplyDamageAroundPosition(new Vector3(x * 2, 0, z * 2));

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

        Debug.Log($"이진 검색 영역: [{leftX},{topZ}] 에서 [{rightX},{bottomZ}], 목표: [{targetX},{targetZ}]");

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
                StartCoroutine(tileManager.CreateShockwave(midX, midZ, halfSize, shockwavePower));
                ApplyDamageAroundPosition(new Vector3(midX * 2, 0, midZ * 2));
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

        // 타겟 영역 강조 및 효과 적용 (이진 검색이 실패했을 경우)
        HighlightTile(targetX, targetZ, Color.green);
        StartCoroutine(tileManager.CreateShockwave(targetX, targetZ, halfSize, shockwavePower));
        ApplyDamageAroundPosition(new Vector3(targetX * 2, 0, targetZ * 2));
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

        Debug.Log($"랜덤 검색 영역: [{minX},{minZ}] 에서 [{maxX},{maxZ}], 목표: [{targetX},{targetZ}]");

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

        Debug.Log("데미지 적용");

        // 데미지 적용
        Collider[] hitColliders = Physics.OverlapSphere(centerPosition, damageRadius, playerLayer);
        foreach (var collider in hitColliders)
        {
            if (collider == null) continue;

            Debug.Log(collider + "가 맞았습니다");

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