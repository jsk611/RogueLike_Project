using InfimaGames.LowPolyShooterPack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerControl : MonoBehaviour, ISkillLockable
{
    private float moveSpeed;
    private float moveSpeed_origin;
    private float jumpPower;
    [Range(0, 100)] public float Stamina = 100;

    public float dashCool;
    public bool isGrounded = false;
    public bool dashOver = false;

    // ��ų�� ���� ����
    private Dictionary<SkillType, bool> skillEnabledStates = new Dictionary<SkillType, bool>();

    // �̺�Ʈ ����
    public delegate void SkillLockStateChanged(SkillType skillType, bool isEnabled);
    public static event SkillLockStateChanged OnSkillLockStateChanged;

    Animator playerAnimator;
    Rigidbody playerRigidbody;
    CharacterController character;
    CharacterBehaviour playerCharacter;

    CameraControl cameraController;
    Rigidbody rigidBody;

    PlayerStatus characterStatus;

    RaycastHit hitInfo;

    public Vector3 Movement = Vector3.zero;
    public Vector3 Vertical = Vector3.zero;

    Transform originalParent;
    Vector3 initialWorldScale;
    [SerializeField] PlayerPositionData positionData;

    TileManager tileManager;

    void Awake()
    {
        // ��ų ���� �ʱ�ȭ
        InitializeSkillStates();
    }

    /// <summary>
    /// ��ų ���� �ʱ�ȭ
    /// </summary>
    private void InitializeSkillStates()
    {
        // ��� ��ų Ȱ��ȭ ���·� �ʱ�ȭ
        skillEnabledStates[SkillType.Running] = true;
        skillEnabledStates[SkillType.Jumping] = true;
        skillEnabledStates[SkillType.Dash] = true;
        skillEnabledStates[SkillType.Movement] = true;
        skillEnabledStates[SkillType.Shooting] = true;
        skillEnabledStates[SkillType.WeaponSwitch] = true;
        skillEnabledStates[SkillType.Interaction] = true;
    }

    void Start()
    {
        playerCharacter = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        playerAnimator = playerCharacter.GetPlayerAnimator();
        playerRigidbody = GetComponent<Rigidbody>();
        character = GetComponent<CharacterController>();

        rigidBody = GetComponent<Rigidbody>();

        cameraController = GameObject.Find("ViewCamera").GetComponent<CameraControl>();
        characterStatus = GetComponent<PlayerStatus>();

        moveSpeed = characterStatus.GetMovementSpeed();
        moveSpeed_origin = moveSpeed;

        originalParent = transform.parent;
        initialWorldScale = transform.lossyScale;

        tileManager = FindObjectOfType<TileManager>();
    }

    void Update()
    {
        // �̵� ����� Ȱ��ȭ�Ǿ��� ���� MoveMent() ȣ��
        if (IsSkillEnabled(SkillType.Movement))
        {
            MoveMent();
        }
        else
        {
            // �̵��� ��Ȱ��ȭ�Ǹ� Movement ���͸� 0���� ����
            Movement = Vector3.zero;
        }

        StaminaRegeneration();
        CheckGrounded();

        // ���� ����� Ȱ��ȭ�Ǿ��� ���� Jumping() ȣ��
        if (IsSkillEnabled(SkillType.Jumping))
        {
            Jumping();
        }
    }

    void LateUpdate()
    {
        if (transform.parent != null)
        {
            Transform parentTransform = transform.parent.transform;
            Vector3 parentScale = parentTransform.localScale;

            Vector3 currentWorldScale = transform.lossyScale;
            Vector3 scaleRatio = new Vector3(
                initialWorldScale.x / currentWorldScale.x,
                initialWorldScale.y / currentWorldScale.y,
                initialWorldScale.z / currentWorldScale.z
            );

            transform.localScale = new Vector3(
                transform.localScale.x * scaleRatio.x,
                transform.localScale.y * scaleRatio.y,
                transform.localScale.z * scaleRatio.z
            );
        }
    }

    private void MoveMent()
    {
        moveSpeed = characterStatus.GetMovementSpeed();
        var h = Input.GetAxisRaw("Horizontal") * transform.right;
        var v = Input.GetAxisRaw("Vertical") * transform.forward;
        Movement = h + v;

        character.center = new Vector3(0, 1, 0);

        Movement = Movement.normalized * moveSpeed;

        // �޸��� �ɷ°� ��� �ɷ��� ��� Ȱ��ȭ�� ��쿡�� ��� ���
        if (Input.GetKey(KeyCode.LeftShift) && Stamina >= 100f && Movement.magnitude > Mathf.Epsilon &&
            IsSkillEnabled(SkillType.Running) && IsSkillEnabled(SkillType.Dash))
        {
            StartCoroutine(Dashdd(Movement));
        }

        if (playerCharacter.GetCursorState()) character.Move(Movement * Time.deltaTime);
    }

    IEnumerator Dashdd(Vector3 Movement)
    {
        if (dashCool > 1f && !dashOver)
        {
            Stamina = 0;
            dashCool = 0;
            float t = 0;
            playerCharacter.AnimationCancelReload();
            while (t <= 0.15f)
            {
                character.Move(Movement * 4 * Time.deltaTime);
                t += Time.deltaTime;
                yield return null;
            }
        }
    }

    public bool CheckGrounded()
    {
        isGrounded = Physics.SphereCast(transform.position, character.radius, Vector3.down, out hitInfo, 0.2f, LayerMask.GetMask("Wall"));
        if (!isGrounded)
        {
            rigidBody.isKinematic = false;
            transform.SetParent(tileManager.gameObject.transform);
        }
        if (isGrounded)
        {
            rigidBody.isKinematic = true;
            character.Move(Vector3.down * Physics.gravity.magnitude * Time.deltaTime);

            if (hitInfo.transform != transform.parent)
            {
                transform.SetParent(hitInfo.transform);
                string[] tilePos = hitInfo.transform.name.Split(',');
                if (tilePos.Length == 2) positionData.playerTilePosition = new Vector2Int(int.Parse(tilePos[0]), int.Parse(tilePos[1]));
            }
        }
        if (transform.position.y < -5f)
        {
            characterStatus.DecreaseHealth(60 * Time.deltaTime);
        }
        return isGrounded;
    }

    public IEnumerator AirBorne(Vector3 enemyDirection, float upForce = 10f, float normalForce = 2f)
    {
        int temp = 0;
        while (temp < 3f)
        {
            rigidBody.isKinematic = false;
            rigidBody.AddForce(Vector3.up * upForce + enemyDirection * normalForce, ForceMode.Impulse);
            jumpPower = 4.9f;
            Vertical.y = jumpPower;
            temp++;
            yield return null;
        }
    }

    private void Jumping()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            if (isGrounded)
            {
                isGrounded = false;
                rigidBody.isKinematic = false;
                rigidBody.AddForce(Vector3.up * 11, ForceMode.Impulse);
                jumpPower = 1f;
                Vertical.y = jumpPower;
            }
        }
    }

    private void StaminaRegeneration()
    {
        dashCool += Time.deltaTime;

        if (dashCool > 1.5f)
        {
            if (Stamina < 100) Stamina += characterStatus.GetStaminaRegen() * Time.deltaTime;
            if (Stamina > 100) Stamina = 100f;
        }
    }



    #region ISkillLockable �������̽� ����
    public void SetSkillEnabled(SkillType skillType, bool enabled)
    {
        if (skillEnabledStates.ContainsKey(skillType))
        {
            bool previousState = skillEnabledStates[skillType];
            if (previousState != enabled)
            {
                skillEnabledStates[skillType] = enabled;
                // ���°� ����Ǿ��� ���� �̺�Ʈ �߻�
                OnSkillLockStateChanged?.Invoke(skillType, enabled);
                Debug.Log($"��ų ���� ����: {skillType} - {(enabled ? "Ȱ��ȭ" : "��Ȱ��ȭ")}");
            }
        }
        else
        {
            skillEnabledStates.Add(skillType, enabled);
            OnSkillLockStateChanged?.Invoke(skillType, enabled);
            Debug.Log($"�� ��ų ���� ����: {skillType} - {(enabled ? "Ȱ��ȭ" : "��Ȱ��ȭ")}");
        }
    }

    public bool IsSkillEnabled(SkillType skillType)
    {
        if (skillEnabledStates.TryGetValue(skillType, out bool enabled))
        {
            return enabled;
        }

        // �⺻������ ��ų�� Ȱ��ȭ ����
        return true;
    }

    public void UnlockAllSkills()
    {
        foreach (SkillType skillType in Enum.GetValues(typeof(SkillType)))
        {
            SetSkillEnabled(skillType, true);
        }

        Debug.Log("��� ��ų ��� ������");
    }

    #endregion
}