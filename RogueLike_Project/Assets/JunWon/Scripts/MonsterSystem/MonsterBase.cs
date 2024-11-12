using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public abstract class MonsterBase : MonoBehaviour
{
    [SerializeField] protected Transform target;

    [Header("Preset Fields")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected GameObject splashFx;
    [SerializeField] protected NavMeshAgent nmAgent;
    [SerializeField] protected FieldOfView fov;
    [SerializeField] private Rigidbody playerRigidBody;


    [Header("NormalStats Fields")]
    [SerializeField] protected MonsterStatus monsterStatus;
    protected float attackRange = 1.0f; // ���� ����
    protected float detectionRange = 1.0f; // ���� ����
    protected float attackCooldown = 1.0f; // ���� ����
    protected float hp = 0; // �⺻ ü��
    protected float dmg = 0; // �⺻ ������
    protected float chaseSpeed; // ���� �ӵ�


    [Header("Delay(CoolTime)")]
    [SerializeField] protected float transitionDelay;

    protected Coroutine stateMachineCoroutine;

    protected enum State
    {
        IDLE,
        CHASE,
        ATTACK,
        HIT,
        DIE,
        SEARCH,
        AIM,
        KILL,
    }

    protected State state;

    [SerializeField] EnemyCountData enemyCountData;
    bool isDie = false;
    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        monsterStatus = GetComponent<MonsterStatus>();
        fov = GetComponent<FieldOfView>();


        hp = monsterStatus.GetHealth(); // �⺻ ü��
        dmg = monsterStatus.GetAttackDamage(); // �⺻ ���ݷ�
        chaseSpeed = monsterStatus.GetMovementSpeed(); // �⺻ �̵� �ӵ�
        attackRange = monsterStatus.GetAttackRange(); // �⺻ ���� ����

        //def = monsterStatus.GetDefence();

        state = State.IDLE;
        stateMachineCoroutine = StartCoroutine(StateMachine());
    } // �⺻ ���� ����


    #region Basic Monster Function
    protected virtual IEnumerator StateMachine()
    {
        while (hp > 0)
        {
            Debug.Log(name + "�� ���� state : " + state);
            yield return StartCoroutine(GetStateCoroutine(state));
        }
    }

    private IEnumerator GetStateCoroutine(State state)
    {
        switch (state)
        {
            case State.IDLE:
                return IDLE();
            case State.CHASE:
                return CHASE();
            case State.AIM:
                return AIM();
            case State.ATTACK:
                return ATTACK();
            case State.HIT:
                return HIT();
            case State.DIE:
                return DIE();
            case State.SEARCH:
                return SEARCH();
            case State.KILL:
                return KILL();
            default:
                return null;
        }
    }

    protected virtual IEnumerator IDLE()
    {
        SetAnimatorState(state);// ���¿� ���� �ִϸ����� �Ķ���� ����

        if (fov.visibleTargets.Count > 0)
        {
            target = fov.visibleTargets[0];
            ChangeState(State.CHASE);
        }
        else
        {
            target = null;
        }

        yield return new WaitForSeconds(0.3f);
    }

    protected virtual IEnumerator CHASE()
    {
        SetAnimatorState(state); // ���¿� ���� �ִϸ����� �Ķ���� ����

        if (target == null)
        {
            ChangeState(State.IDLE);
            yield break;
        }

        // �ּ����� �ð� ���� CHASE ���¸� ����
        yield return new WaitForSeconds(0.5f);

        nmAgent.isStopped = false;
        nmAgent.speed = chaseSpeed; // ������ ���� chaseSpeed ���
        nmAgent.SetDestination(target.position);

        if (!nmAgent.pathPending && nmAgent.remainingDistance <= attackRange)
        {
            if (this is RangedMonster)
                ChangeState(State.AIM);
            else
                ChangeState(State.ATTACK);
        }

        yield return new WaitForSeconds(0.3f);
    }

    protected virtual IEnumerator ATTACK()
    {
        SetAnimatorState(state); // ���¿� ���� �ִϸ����� �Ķ���� ����

        nmAgent.isStopped = true; // ���� �� �̵� ����

        if (target == null || Vector3.Distance(transform.position, target.position) > attackRange)
        {
            ChangeState(State.CHASE);
            yield break;
        }

        yield return null; // ���� ��Ÿ�� ���

        //if (target != null && Vector3.Distance(target.transform.position, transform.position) <= attackRange + 1)
        //{
        //    //target.GetComponent<PlayerStatus>().DecreaseHealth(dmg * monsterStatus.CalculateCriticalHit());
        //    //StartCoroutine(Crowd_Control(target));
        //    //StartCoroutine(KnockBack());
        //}
    }

    protected virtual IEnumerator HIT()
    {
        // �̵� �� ���� ����
        nmAgent.isStopped = true;

        // �ǰ� �ִϸ��̼� ���
        SetAnimatorState(state);

        // �̵� �簳
        nmAgent.isStopped = false;

        // ���ϴ� ���·� ��ȯ (��: ���� ����)
        ChangeState(State.CHASE);

        yield return new WaitForSeconds(GetAnimationClipLength("Hit"));
    }

    //protected virtual IEnumerator KnockBack()
    //{
    //    playerRigidBody.isKinematic = false;
    //    playerRigidBody.AddForce((Vector3.up + new Vector3((target.transform.position.x - transform.position.x), 0, (target.transform.position.z - transform.position.z)).normalized) * 10f, ForceMode.Impulse);
    //    yield return new WaitForSeconds(1f);
    //}


    protected virtual IEnumerator AIM()
    {
        if (target == null) yield break;

        SetAnimatorState(state); // ���¿� ���� �ִϸ����� �Ķ���� ����
        nmAgent.isStopped = true;

        ChangeState(State.ATTACK);
        yield return new WaitForSeconds(0.3f);
    }

    protected virtual IEnumerator SEARCH() { yield break; }
    protected virtual IEnumerator KILL() { yield break; }


    public virtual void TakeDamage(float damage)
    {
        // ü�� ���� ó��
        monsterStatus.DecreaseHealth(damage);
        hp = monsterStatus.GetHealth();

        if (hp > 0)
        {
            // �ǰ� ���·� ��ȯ
            ChangeState(State.HIT);
            target = FindObjectOfType<PlayerStatus>().transform;
        }
        else
        {
            // ���� ó��
            isDie = true; // ���� �÷��� ����

            // �ִϸ����� �Ķ���� ����
            if (anim != null)
            {
                anim.SetBool("isDead", true); // �Ǵ� anim.SetTrigger("DieTrigger");
            }

            StartCoroutine(DIE());
        }
    }

    protected virtual IEnumerator DIE()
    {
        // �̵� �� ���� ����
        if (stateMachineCoroutine != null)
        {
            StopCoroutine(stateMachineCoroutine);
        }
        nmAgent.isStopped = true;

        // �ݶ��̴� ��Ȱ��ȭ
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // �� ī��Ʈ ���� (�� ���� ����)
        if (!isDie)
        {
            enemyCountData.enemyCount--;
            Debug.Log("Enemy Died, ���� �� : " + enemyCountData.enemyCount);
            isDie = true;
        }

        // ���� �ִϸ��̼��� ���̸�ŭ ���
        // float deathAnimationLength = GetAnimationClipLength("Die"); // �Ǵ� "Death"
        yield return new WaitForSeconds(1.5f);

        // ������Ʈ ��Ȱ��ȭ �Ǵ� �ı�
        Destroy(gameObject);
    }

    #endregion

    protected void ChangeState(State newState)
    {
        // ���� ��� ���� ��ȯ���� ����
        if (isDie) return;

        Debug.Log(transform.name + " ���� ����: " + state + " �� " + newState);
        state = newState;
    }

    #region AnimationSettings
    protected void SetAnimatorState(State state)
    {
        if (anim != null)
        {
            anim.SetInteger("State", (int)state);
        }
    }
    protected float GetAnimationClipLength(string clipName)
    {
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == clipName)
                {
                    return clip.length;
                }
            }
        }
        // �⺻�� ���� (�ִϸ��̼� Ŭ���� ã�� ���� ���)
        return 1.0f; // �ʿ信 ���� ����
    }
    #endregion



    //protected IEnumerator Crowd_Control(Transform target)
    //{
    //    target.GetComponent<PlayerControl>().enabled = false;
    //    yield return new WaitForSeconds(0.5f);
    //    target.GetComponent <PlayerControl>().enabled = true;
    //}
}
