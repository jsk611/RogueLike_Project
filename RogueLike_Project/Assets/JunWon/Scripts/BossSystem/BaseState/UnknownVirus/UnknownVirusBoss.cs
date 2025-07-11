using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

public class UnknownVirusBoss : BossBase
{
    public enum BossForm { Basic, Worm, Trojan, Ransomware }

    #region �� ���� ����
    [Header("�� ������Ʈ")]
    [SerializeField] private GameObject basicFormObject;     // �⺻ �� ������Ʈ (�ڱ� �ڽ�)
    [SerializeField] private GameObject wormFormObject;      // �� �� ������Ʈ (�ڽ�)
    [SerializeField] private GameObject trojanFormObject;    // Ʈ���� �� �� ������Ʈ (�ڽ�)
    [SerializeField] private GameObject ransomwareFormObject; // �������� �� ������Ʈ (�ڽ�)

    // �� �� ������Ʈ ĳ��
    private WormBossPrime wormComponent;
    private Troy trojanComponent;
    private Ransomware ransomwareComponent;

    // ���� Ȱ��ȭ�� �� ����
    private GameObject currentActiveFormObject;
    private BossBase currentActiveBoss;
    [SerializeField] private BossForm currentForm = BossForm.Basic;

    // �� ���� ��Ÿ�� ����
    private float formStayDuration = 15f; // ������ ���� �ӹ��� �ð�
    private float formTimer = 0f;
    #endregion

    #region Effect System

    [SerializeField] VoxelFloatEffect vFE;

    #endregion

    #region ���� ����
    private float currentRandomValue = 0.9f;
    [Header("���� ����")]
    [SerializeField] private float baseAttackDamage = 20f;
    [SerializeField] private float baseAttackRange = 10f;
    [SerializeField] private float baseAttackCooldown = 3f;

    [Header("�� ����")]
    [SerializeField] private GameObject mapAttackVFX;
    [Range(0, 1)][SerializeField] private float mapAttackChance = 0.7f;

    [Header("�� ����")]
    [SerializeField] private GameObject transformationVFX;
    [Range(0, 1)][SerializeField] private float formChangeChance = 0.3f;

    [Header("������Ʈ")]
    [SerializeField] private AbilityManager abilityManager;
    #endregion

    #region ���� �ӽ�
    [Header("���� �ӽ�")]
    [SerializeField] private StateMachine<UnknownVirusBoss> fsm;

    // ���µ�
    private IntroState_UnknownVirus introState;
    private BasicCombatState_UnknownVirus basicState;
    private MapAttackState_UnknownVirus mapAttackState;
    private TransformState_UnknownVirus transformState;
    private DefeatedState_UnknownVirus deadState;
    #endregion

    #region ���� ���� �޼���

    public void SetMapAttackState(MapAttackState_UnknownVirus state)
    {
        mapAttackState = state;
    }

    public void SetTransformState(TransformState_UnknownVirus state)
    {
        transformState = state;
    }
    #endregion


    #region �� ���� ����
    [Header("�� ���� ����")]
    [SerializeField] private TileManager tileManager;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private int attackAreaSize = 5; // ���� ���� ũ�� (5x5)
    [SerializeField] private float tileSearchInterval = 0.1f; // Ÿ�� �˻� ����
    [SerializeField] private float shockwavePower = 30f; // ����� ����
    [SerializeField] private LayerMask playerLayer;
    #endregion

    #region ���� ������Ƽ
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

    public StateMachine<UnknownVirusBoss> Fsm => fsm;  // ���� fsm �ʵ带 public���� ����

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

    #region ����Ƽ ����������Ŭ
    private void Start()
    {
        InitializeComponents();
        InitiailzeEffects();
        InitializeFormHierarchy();
        InitializeAbilities();
        InitializeStates();
        InitializeFSM();

        Debug.Log("[UnknownVirusBoss] �ʱ�ȭ �Ϸ�");
    }

    private void Update()
    {
        // FSM ������Ʈ
        UpdateRandomValue();
        fsm.Update();

        // ��� ���� Ȯ��
        if (bossStatus.GetHealth() <= 0 && !(fsm.CurrentState is DefeatedState_UnknownVirus))
        {
            HandleDeath();
        }
    }
    #endregion

    #region �ʱ�ȭ �޼���

    private void InitiailzeEffects()
    {
        vFE = GetComponentInChildren<VoxelFloatEffect>();
    }
    private void InitializeComponents()
    {
        // ���� ã��
        tileManager = FindObjectOfType<TileManager>();
        target = GameObject.FindWithTag("Player").transform;
        vFE = GetComponentInChildren<VoxelFloatEffect>();

        // ������Ʈ ��������
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        fov = GetComponent<FieldOfView>();

        Debug.Log("[UnknownVirusBoss] ������Ʈ �ʱ�ȭ �Ϸ�");
    }

    private void InitializeFormHierarchy()
    {
        // �� ������Ʈ ��ȿ�� �˻�
        if (basicFormObject == null)
        {
            Debug.LogError("[UnknownVirusBoss] �⺻ �� ������Ʈ�� �����ϴ�!");
            return;
        }

        // �� ������Ʈ ĳ��
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

        // �ʱ� ���� - �⺻ ���� Ȱ��ȭ
        ActivateBasicFormOnly();

        Debug.Log("[UnknownVirusBoss] �� ���� �ʱ�ȭ �Ϸ�");
    }

    private void InitializeAbilities()
    {
        // �� ���� �ɷ� Ȱ��ȭ
        abilityManager.SetAbilityActive("MapAttack");
        abilityManager.SetMaxCoolTime("MapAttack");

        abilityManager.SetAbilityActive("Transform");
        abilityManager.SetMaxCoolTime("Transform");

        Debug.Log("[UnknownVirusBoss] �ɷ� �ʱ�ȭ �Ϸ�");
    }

    private void InitializeStates()
    {
        introState = new IntroState_UnknownVirus(this);
        basicState = new BasicCombatState_UnknownVirus(this);
        mapAttackState = new MapAttackState_UnknownVirus(this);
        transformState = new TransformState_UnknownVirus(this);
        deadState = new DefeatedState_UnknownVirus(this);

        Debug.Log("[UnknownVirusBoss] ���� �ʱ�ȭ �Ϸ�");
    }

    private void InitializeFSM()
    {
        // ���� �ν��Ͻ� ����
        var states = CreateStates();

        // �ʱ� ���¸� ��Ʈ�η� ������ FSM ����
        fsm = new StateMachine<UnknownVirusBoss>(states.introState);

        // ���� ����
        SetupTransitions(states);

        Debug.Log("[UnknownVirusBoss] FSM �ʱ�ȭ �Ϸ�");
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
        // ��Ʈ�� �� �⺻ ����
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.introState, s.basicState, () => true));

        // 기본 상태 → 맵 공격 (Base 활성화 상태 체크 추가)
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.basicState, s.mapAttackState,
            () => abilityManager.GetAbilityRemainingCooldown("MapAttack") == 0 &&
                 currentRandomValue < mapAttackChance &&
                 basicFormObject != null && basicFormObject.activeInHierarchy));

        // �� ���� �� �⺻ ����
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.mapAttackState, s.basicState,
            () => mapAttackState.IsAnimationFinished()
        ));

        // �⺻ ���� �� ����
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.basicState, s.transformState,
            () => abilityManager.GetAbilityRemainingCooldown("Transform") == 0 &&
                 currentRandomValue > mapAttackChance));

        // �� �� ���� �� �⺻ ����
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.transformState, s.basicState,
            () => transformState.IsAnimationFinished()
        ));


        // ���� ��� ���� ���� (��Ʈ�� ����)
        List<State<UnknownVirusBoss>> exceptStates = new List<State<UnknownVirusBoss>> { s.introState };
        fsm.AddGlobalTransition(s.deadState, () => bossStatus.GetHealth() <= 0, exceptStates);
    }
    #endregion



    #region �� ���� �޼���
    private void ActivateBasicFormOnly()
    {
        // �⺻ ���� Ȱ��ȭ�ϰ� �������� ��Ȱ��ȭ
        if (basicFormObject != null)
            basicFormObject.SetActive(true);

        if (wormFormObject != null)
            wormFormObject.SetActive(false);

        if (trojanFormObject != null)
            trojanFormObject.SetActive(false);

        if (ransomwareFormObject != null)
            ransomwareFormObject.SetActive(false);

        // ���� �� ����
        currentForm = BossForm.Basic;
        currentActiveFormObject = basicFormObject;
        currentActiveBoss = null;
    }

    public void ApplyForm(BossForm form)
    {
        // �� ��ȯ ����
        if (form == currentForm) return;

        // ���� Ȱ��ȭ�� �� ��Ȱ��ȭ
        DeactivateCurrentForm();

        // �� �� Ȱ��ȭ
        ActivateForm(form);

        // �� Ÿ�̸� ����
        formTimer = Time.time;

        // ���� �� ������Ʈ
        currentForm = form;

        Debug.Log($"[UnknownVirusBoss] {form} ������ ���� �Ϸ�");
    }

    private void DeactivateCurrentForm()
    {
        // ���� Ȱ��ȭ�� �� ������Ʈ ��Ȱ��ȭ
        if (currentActiveFormObject != null)
        {
            // �� ������Ʈ ��Ȱ��ȭ
            currentActiveFormObject.SetActive(false);
        }

        if (currentActiveBoss != null)
        {
            currentActiveBoss.ResetBoss();
        }

        // ���� Ȱ�� ���� ���� �ʱ�ȭ
        currentActiveBoss = null;
    }

    private void ActivateForm(BossForm form)
    {
        GameObject targetFormObject = null;

        // ���� ���� ��� ������Ʈ ����

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

        // ���� �� ��ġ�� �������� ����ȭ
        formObject.transform.position = currentActiveFormObject.transform.position;
        formObject.transform.rotation = currentActiveFormObject.transform.rotation;
    }

    private void SyncHealthToActiveForm(GameObject formObject, BossForm form)
    {
        // ���� ü�� ���� ���
        float healthRatio = bossStatus.GetHealth() / bossStatus.GetMaxHealth();

        // ��� ���� ü�� ������Ʈ ��������
        BossStatus targetStatus = formObject.GetComponent<BossStatus>();


        // ��� ü�� ���� (���� �����ϰ�)
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

        // ���� Ȱ�� ���� ü�� ���� ���
        BossStatus formStatus = currentActiveBoss.GetComponent<BossStatus>();
        if (formStatus == null)
            return;

        float healthRatio = formStatus.GetHealth() / formStatus.GetMaxHealth();

        // ��ü ü�� ���� ����ȭ
        bossStatus.SetHealth(bossStatus.GetMaxHealth() * healthRatio);

        // UI ����
        HPBar?.SetRatio(bossStatus.GetHealth(), bossStatus.GetMaxHealth());
    }

    private void DeactivateAllForms()
    {
        // ��� �� ��Ȱ��ȭ (����� ���)
        if (wormFormObject != null)
            wormFormObject.SetActive(false);

        if (trojanFormObject != null)
            trojanFormObject.SetActive(false);

        if (ransomwareFormObject != null)
            ransomwareFormObject.SetActive(false);

        // ���� �ʱ�ȭ
        basicFormObject.SetActive(true);
        currentActiveFormObject = basicFormObject;
        currentActiveBoss = null;
    }
    #endregion

    #region ������Ʈ ����
  

    public void PrepareToReturnToBasicForm()
    {
        fsm.ForcedTransition(transformState);
    }

    private void HandleDeath()
    {
        StopAllCoroutines();

        // ��� ó��
        DeactivateAllForms();

        // ��� ���·� ��ȯ
        fsm.ForcedTransition(deadState);

        Debug.Log("[UnknownVirusBoss] ���� ���");
    }

    public void RequestFormChange(BossForm newForm)
    {
        // ���� ȿ�� Ȱ��ȭ
        if (transformationVFX != null)
            transformationVFX.SetActive(true);
    }

    #endregion

    #region ������ ó��
    public override void TakeDamage(float damage, bool showDamage = true)
    {
        // ���� Ȱ��ȭ�� ���� ������ ����
        if (currentForm != BossForm.Basic && currentActiveBoss != null)
        {
            // ���� ���� ������ ����
            currentActiveBoss.TakeDamage(damage, showDamage);

            // �⺻ ������ ������ ����ȭ
            SyncHealthFromActiveBoss();
        }
        else
        {
            // �⺻ �� ������ ó��
            bossStatus.DecreaseHealth(damage);

            // ���� �̺�Ʈ �� UI ǥ��
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

            // UI ����
            HPBar?.SetRatio(bossStatus.GetHealth(), bossStatus.GetMaxHealth());
        }

        // ��� üũ
        if (bossStatus.GetHealth() <= 0 && !(fsm.CurrentState is DefeatedState_UnknownVirus))
        {
            HandleDeath();
        }
    }
    #endregion

    #region �ִϸ��̼� �̺�Ʈ �ڵ鷯
    // �� ���� �ִϸ��̼� �Ϸ� �̺�Ʈ
    public void OnMapAttackFinished()
    {
        if (mapAttackState != null)
        {
            mapAttackState.OnAttackFinished();
        }
    }

    // �⺻ ���� �ִϸ��̼� �Ϸ� �̺�Ʈ
    public void OnBasicAttackFinished()
    {
        // �⺻ ���� �Ϸ� ó�� (�ʿ��)
    }
    #endregion

    #region �� ���� ����
    // �� ���� Ʈ���� �޼���
    public void TriggerMapAttack()
    {
        try
        {
            if (tileManager == null)
            {
                Debug.LogError("�� ���� ���� �Ұ�: TileManager�� null�Դϴ�");
                mapAttackState?.OnAttackFinished();
                return;
            }

            // �˻� �˰����� ���� (����)
            int searchMethod = UnityEngine.Random.Range(0, 3);
            StartCoroutine(ExecuteMapAttack(searchMethod));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TriggerMapAttack ����: {e.Message}\n{e.StackTrace}");
            mapAttackState?.OnAttackFinished();
        }
    }

    private IEnumerator ExecuteMapAttack(int searchMethod)
    {
        // �÷��̾� �ֺ� ��ǥ ���
        Vector3 playerPos = target.position;

        // ���� ��ǥ�� Ÿ�� �׸��� ��ǥ�� ��ȯ
        int centerX = Mathf.RoundToInt(playerPos.x / 2);
        int centerZ = Mathf.RoundToInt(playerPos.z / 2);

        Debug.Log($"�׸��� ��ġ [{centerX}, {centerZ}]���� �� ���� ����");

        // ��ǥ Ÿ�� ������ ���� (���� ���� ��)
        int targetX = centerX + UnityEngine.Random.Range(-attackAreaSize / 2, attackAreaSize / 2 + 1);
        int targetZ = centerZ + UnityEngine.Random.Range(-attackAreaSize / 2, attackAreaSize / 2 + 1);

        // ��ȿ�� �� ���� ���� ����
        targetX = Mathf.Clamp(targetX, 0, tileManager.GetMapSize - 1);
        targetZ = Mathf.Clamp(targetZ, 0, tileManager.GetMapSize - 1);

        // �˻� ����� ���� ȿ�� ����
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

        // ���� �Ϸ� �� ���
        yield return new WaitForSeconds(1f);

        // �� ���� ���� �Ϸ� �˸�
        mapAttackState?.OnAttackFinished();
    }

    // ���� Ÿ�� �˻� ����
    private IEnumerator LinearTileSearch(int centerX, int centerZ, int targetX, int targetZ)
    {
        int halfSize = attackAreaSize / 2;

        // �˻� ���� ��� (�÷��̾� �߽����� attackAreaSize x attackAreaSize ����)
        int minX = Mathf.Max(0, centerX - halfSize);
        int maxX = Mathf.Min(tileManager.GetMapSize - 1, centerX + halfSize);
        int minZ = Mathf.Max(0, centerZ - halfSize);
        int maxZ = Mathf.Min(tileManager.GetMapSize - 1, centerZ + halfSize);

        Debug.Log($"���� �˻� ����: [{minX},{minZ}] ���� [{maxX},{maxZ}], ��ǥ: [{targetX},{targetZ}]");

        // ��� Ÿ���� ���������� �˻�
        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                // ���� �˻� Ÿ�� ǥ��
                HighlightTile(x, z, Color.red);

                // ���� ��ǥ�� ��ȯ
                Vector3 tilePos = new Vector3(x * 2, 0, z * 2);

                // ������ ȿ��
                if (laserPrefab != null)
                {
                    GameObject laser = Instantiate(laserPrefab,
                       tilePos + Vector3.up * 0.2f, // Ÿ�� �ٷ� ��
                       Quaternion.identity);

                    // ������ ������ ���� (VirusLaser ������Ʈ�� �ִٰ� ����)
                    var virusLaser = laser.GetComponent<VirusLaser>();
                    if (virusLaser != null)
                    {
                        virusLaser.SetDamage(abilityManager.GetAbiltiyDmg("MapAttack"));
                    }
                }

                yield return new WaitForSeconds(tileSearchInterval);

                // Ÿ�� Ÿ���� ã���� ����� ȿ��
                if (x == targetX && z == targetZ)
                {
                    // Ÿ�� Ÿ�� ǥ��
                    HighlightTile(x, z, Color.green);

                    // ����� ���� (TileManager�� CreateShockwave �ڷ�ƾ ȣ��)
                    StartCoroutine(tileManager.CreateShockwave(x, z, halfSize, shockwavePower));

                    yield break;
                }

                // �˻� �Ϸ�� Ÿ�� ����
                ResetTileColor(x, z);
            }
        }
    }

    // ���� Ÿ�� �˻� ����
    private IEnumerator BinaryTileSearch(int centerX, int centerZ, int targetX, int targetZ)
    {
        int halfSize = attackAreaSize / 2;

        // �˻� ���� ���
        int leftX = Mathf.Max(0, centerX - halfSize);
        int rightX = Mathf.Min(tileManager.GetMapSize - 1, centerX + halfSize);
        int topZ = Mathf.Max(0, centerZ - halfSize);
        int bottomZ = Mathf.Min(tileManager.GetMapSize - 1, centerZ + halfSize);

        Debug.Log($"���� �˻� ����: [{leftX},{topZ}] ���� [{rightX},{bottomZ}], ��ǥ: [{targetX},{targetZ}]");

        int iterations = 0;
        int maxIterations = 10; // ���� ���� ����

        while (leftX <= rightX && topZ <= bottomZ && iterations < maxIterations)
        {
            iterations++;
            int midX = (leftX + rightX) / 2;
            int midZ = (topZ + bottomZ) / 2;

            // ���� �˻� ���� ����
            for (int x = leftX; x <= rightX; x++)
            {
                for (int z = topZ; z <= bottomZ; z++)
                {
                    HighlightTile(x, z, Color.red);

                    // ��� Ÿ�Ͽ��� ������ ȿ��
                    if (x == leftX || x == rightX || z == topZ || z == bottomZ)
                    {
                        if (laserPrefab != null)
                        {
                            Vector3 tilePos = new Vector3(x * 2, 0, z * 2);
                            GameObject laser = Instantiate(laserPrefab,
                                tilePos + Vector3.up * 0.2f,
                                Quaternion.identity);

                            // ������ ������ ����
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

            // ���� ȿ�� �ʱ�ȭ
            for (int x = leftX; x <= rightX; x++)
            {
                for (int z = topZ; z <= bottomZ; z++)
                {
                    ResetTileColor(x, z);
                }
            }

            yield return new WaitForSeconds(0.2f);

            // Ÿ���� ã�Ҵ��� Ȯ��
            if (midX == targetX && midZ == targetZ)
            {
                HighlightTile(midX, midZ, Color.green);
                yield break;
            }

            // ���� �˻� ����
            if (targetX < midX)
                rightX = midX - 1;
            else
                leftX = midX + 1;

            if (targetZ < midZ)
                bottomZ = midZ - 1;
            else
                topZ = midZ + 1;
        }

        // Ÿ�� ���� ���� �� ȿ�� ���� (���� �˻��� �������� ���)
        HighlightTile(targetX, targetZ, Color.green);
    }

    // ���� Ÿ�� �˻� ����
    private IEnumerator RandomTileSearch(int centerX, int centerZ, int targetX, int targetZ)
    {
        int halfSize = attackAreaSize / 2;

        // �˻� ���� ���
        int minX = Mathf.Max(0, centerX - halfSize);
        int maxX = Mathf.Min(tileManager.GetMapSize - 1, centerX + halfSize);
        int minZ = Mathf.Max(0, centerZ - halfSize);
        int maxZ = Mathf.Min(tileManager.GetMapSize - 1, centerZ + halfSize);

        Debug.Log($"���� �˻� ����: [{minX},{minZ}] ���� [{maxX},{maxZ}], ��ǥ: [{targetX},{targetZ}]");

        HashSet<Vector2Int> searchedTiles = new HashSet<Vector2Int>();
        int maxAttempts = Mathf.Min(20, (maxX - minX + 1) * (maxZ - minZ + 1));

        for (int i = 0; i < maxAttempts; i++)
        {
            // �˻� ���� ������ ���� Ÿ�� ����
            int x = UnityEngine.Random.Range(minX, maxX + 1);
            int z = UnityEngine.Random.Range(minZ, maxZ + 1);
            Vector2Int tilePos = new Vector2Int(x, z);

            // �̹� �˻��� Ÿ���̸� �ٽ� ���� (�ִ� 3��)
            int attempts = 0;
            while (searchedTiles.Contains(tilePos) && attempts < 3)
            {
                x = UnityEngine.Random.Range(minX, maxX + 1);
                z = UnityEngine.Random.Range(minZ, maxZ + 1);
                tilePos = new Vector2Int(x, z);
                attempts++;
            }

            searchedTiles.Add(tilePos);

            // Ÿ�� ���� �� ������ ȿ��
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

            // Ÿ�� Ÿ���� ã�Ҵ��� Ȯ��
            if (x == targetX && z == targetZ)
            {
                HighlightTile(x, z, Color.green);
                yield break;
            }

            // �˻� �Ϸ�� Ÿ�� ����
            ResetTileColor(x, z);
        }

        // �ִ� �õ� Ƚ���� �ʰ��ص� Ÿ���� ã�� ���� ���
        HighlightTile(targetX, targetZ, Color.green);
    }

    // Ÿ�� ���� ���� �޼���
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

    // Ÿ�� ���� ���� ���� �޼���
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

    // ���� ���� �� ���� �޼���
    public void CleanupMapAttack()
    {
    }
    #endregion

    void UpdateRandomValue()
    {
        currentRandomValue = UnityEngine.Random.value;
    }

}