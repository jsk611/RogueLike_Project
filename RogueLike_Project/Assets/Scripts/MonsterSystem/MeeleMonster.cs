using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using InfimaGames.LowPolyShooterPack;

public class MeeleMonster : MonsterBase
{
    [Header("Flying Monster Settings")]
    private float chaseSpeed; // 추적 속도
    public float attackRange = 1.0f; // 공격 범위
    public float attackCooldown = 0.1f; // 공격 간격
    private float damage; // 공격력

    private FieldOfView fov; // 시야 각도 컴포넌트

    private Rigidbody playerRigidBody;

    protected override void Start()
    {
        fov = GetComponent<FieldOfView>(); // 시야 각도 컴포넌트 가져오기
        target = GameObject.FindGameObjectWithTag("Player").transform; // 타겟 지정
        base.Start();

        chaseSpeed = monsterStatus.GetMovementSpeed();
        damage = monsterStatus.GetAttackDamage();

        playerRigidBody = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<Rigidbody>();
        
    }

    protected override IEnumerator StateMachine()
    {
        
        while (hp > 0)
        {
            Debug.Log(state + " state Melee");
            switch (state)
            {
                case State.IDLE:
                    yield return StartCoroutine(IDLE());
                    break;
                case State.CHASE:
                    yield return StartCoroutine(CHASE());
                    break;
                case State.ATTACK:
                    yield return StartCoroutine(ATTACK());
                    break;
            }
        }
        
    }

    private IEnumerator IDLE()
    {
        if (fov.visibleTargets.Count > 0)
        {
            target = fov.visibleTargets[0];
            ChangeState(State.CHASE);
        }
        else
        {
            target = null;
        }

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator CHASE()
    {
        if (target == null)
        {
            ChangeState(State.IDLE);
            yield break;
        }

        nmAgent.speed = chaseSpeed; // 이전에 계산된 chaseSpeed 사용
        nmAgent.SetDestination(target.position);

        // Debug.Log($"Remaining Distance: {nmAgent.remainingDistance}, Stopping Distance: {nmAgent.stoppingDistance}");

        if (nmAgent.remainingDistance <= nmAgent.stoppingDistance)
        {
            ChangeState(State.ATTACK);
        }

        yield return null;
    }

    private IEnumerator ATTACK()
    {

        if (target != null && Vector3.Distance(target.transform.position,transform.position) <= attackRange+1)
        {
            target.GetComponent<PlayerStatus>().DecreaseHealth(damage * monsterStatus.CalculateCriticalHit());
            StartCoroutine(Crowd_Control(target));
            StartCoroutine(KnockBack());
        }

        yield return new WaitForSeconds(attackCooldown); // 공격 쿨타임 대기

        if (target == null || Vector3.Distance(transform.position, target.position) > attackRange)
        {
            ChangeState(State.CHASE);
        }
    }

    private IEnumerator KnockBack()
    {
        playerRigidBody.isKinematic = false;
        playerRigidBody.AddForce((Vector3.up+new Vector3((target.transform.position.x-transform.position.x),0,(target.transform.position.z-transform.position.z)).normalized)*10f,ForceMode.Impulse);
        yield return new WaitForSeconds(1f) ;
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        if (hp <= 0)
        {
            Die();
        }
    }

    public override void Die()
    {
        base.Die();
        // 사망 애니메이션 재생 및 추가 로직
    }
}
