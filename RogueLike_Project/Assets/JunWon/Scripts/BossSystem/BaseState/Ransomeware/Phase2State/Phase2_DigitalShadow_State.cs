using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Phase2_DigitalShadow_State : BossPhaseBase<Ransomware>
{
    private bool isAttackFinished = false;
    private int shadowCount = 6; // 소환할 그림자 수
    private List<GameObject> activeShadows = new List<GameObject>();
    private float shadowLifetime = 15f; // 그림자 자동 소멸 시간
    private float summonDistance = 5f;

    public Phase2_DigitalShadow_State(Ransomware owner) : base(owner)
    {
        owner.SetDigitalShadowState(this);
    }

    public override void Enter()
    {
        isAttackFinished = false;
        Debug.Log("[Phase2_DigitalShadow_State] Enter");
        owner.NmAgent.isStopped = true;

        if (CanExecuteAttack())
        {
            owner.Animator.SetTrigger("SummonShadows");
            if (owner.AbilityManager.UseAbility("SummonShadows"))
            {
                // 실제 그림자 생성은 애니메이션 이벤트를 통해 트리거됨
            }
        }
        else
        {
            // 공격할 수 없는 경우 즉시 완료
            isAttackFinished = true;
        }
    }

    public override void Exit()
    {
        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("SummonShadows");
    }

    public override void Interrupt()
    {
        base.Interrupt();

        // 인터럽트된 경우 활성화된 그림자 제거
        DestroyAllShadows();
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null;
    }

    // 애니메이션 이벤트에서 호출
    public void SummonShadows()
    {
        if (owner.Player == null) return;

        // 기존 그림자 제거
        DestroyAllShadows();

        // 어빌리티 매니저에서 그림자 프리팹 가져오기
        GameObject shadowPrefab = owner.Shadow;
        if (shadowPrefab == null)
        {
            Debug.LogError("디지털 그림자 프리팹을 찾을 수 없습니다!");
            return;
        }

        // 플레이어 주변에 그림자를 소환할 위치 계산
        for (int i = 0; i < shadowCount; i++)
        {
            // 원형으로 위치 계산
            float angle = i * (360f / shadowCount);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            // NavMesh 위에 위치 찾기
            Vector3 targetPos = owner.Player.position + direction * summonDistance;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, summonDistance, NavMesh.AllAreas))
            {
                // 그림자 인스턴스화
                GameObject shadow = GameObject.Instantiate(shadowPrefab, hit.position, Quaternion.identity);

                // 그림자 초기화
                DigitalShadow shadowComponent = shadow.GetComponent<DigitalShadow>();
                if (shadowComponent != null)
                {
                    shadowComponent.Initialize(owner);
                    activeShadows.Add(shadow);

                    // 파괴 이벤트 등록
                    shadowComponent.OnShadowDestroyed += HandleShadowDestroyed;
                }
            }
        }

        // 모든 그림자 파괴 확인 코루틴 시작
        owner.StartCoroutine(CheckShadowsRoutine());
    }

    private void HandleShadowDestroyed(GameObject shadow)
    {
        if (activeShadows.Contains(shadow))
        {
            activeShadows.Remove(shadow);
        }

        // 타임아웃 전에 모든 그림자가 파괴되면 보스에게 취약 상태 적용
        if (activeShadows.Count == 0)
        {
            owner.ApplyVulnerability(5f);
        }
    }

    private void DestroyAllShadows()
    {
        foreach (var shadow in activeShadows)
        {
            if (shadow != null)
            {
                // 파괴 전 이벤트 등록 해제
                DigitalShadow shadowComponent = shadow.GetComponent<DigitalShadow>();
                if (shadowComponent != null)
                {
                    shadowComponent.OnShadowDestroyed -= HandleShadowDestroyed;
                }

                GameObject.Destroy(shadow);
            }
        }

        activeShadows.Clear();
    }

    private IEnumerator CheckShadowsRoutine()
    {
        // 일정 시간 동안 그림자 활성화 대기
        yield return new WaitForSeconds(shadowLifetime);

        // 여기까지 왔는데 아직 그림자가 있으면 제거
        if (activeShadows.Count > 0)
        {
            DestroyAllShadows();
        }

        // 공격 상태 종료
        OnAttackFinished();
    }

    public void OnAttackFinished()
    {
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;
}


