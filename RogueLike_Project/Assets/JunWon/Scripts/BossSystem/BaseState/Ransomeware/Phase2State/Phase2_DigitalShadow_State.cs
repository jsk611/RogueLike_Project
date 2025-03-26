using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Phase2_DigitalShadow_State : BossPhaseBase<Ransomware>
{
    private bool isAttackFinished = false;
    private int shadowCount = 6; // 분열할 조각 수
    [SerializeField] private List<GameObject> activeShadows = new List<GameObject>();
    private float shadowLifetime = 20f; // 분열 조각 지속 시간
    private float summonDistance = 8f;
    private Vector3 originalPosition; // 보스의 원래 위치 저장
    private Quaternion originalRotation; // 보스의 원래 회전 저장
    private Vector3 originalScale; // 보스의 원래 크기 저장
    private bool isLastStandMode = false; // 발악 모드 플래그

    public Phase2_DigitalShadow_State(Ransomware owner) : base(owner)
    {
        owner.SetDigitalShadowState(this);
    }

    

    public override void Enter()
    {
        owner.AbilityManager.SetAbilityActive("SummonShadows");

        isAttackFinished = false;
        Debug.Log("[Phase2 SummonState] Enter");
        owner.NmAgent.isStopped = true;
        originalPosition = owner.transform.position;
        originalRotation = owner.transform.rotation;
        originalScale = owner.transform.localScale;

        if (CanExecuteAttack())
        {
            owner.Animator.SetTrigger("SummonShadow");
            if (owner.AbilityManager.UseAbility("SummonShadow"))
            {
                // 실제 분열은 애니메이션 이벤트를 통해 트리거됨
            }
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

        owner.GetComponent<MonsterStatus>().SetHealth(0);
    }

    public void ActivateLastStandSplit()
    {
        if (owner.Player == null) return;

        // 발악 모드 활성화
        isLastStandMode = true;

        // 기존 그림자 제거
        DestroyAllShadows();

        // 본체를 숨김 (비활성화는 하지 말고 렌더러만 끄기)
        HideOwner();

        // 분열 조각 생성
        SpawnSplitFragments();

        // 모든 그림자 파괴 확인 코루틴 시작
        owner.StartCoroutine(CheckShadowsRoutine());
    }

    private void HideOwner()
    {
        // 본체의 렌더러와 콜라이더를 비활성화
        Renderer[] renderers = owner.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        Collider[] colliders = owner.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // NavMeshAgent 비활성화
        owner.NmAgent.enabled = false;
    }

    private void RestoreOwner()
    {
        // 본체 위치 복원
        owner.transform.position = originalPosition;
        owner.transform.rotation = originalRotation;
        owner.transform.localScale = originalScale;

        // 렌더러와 콜라이더 활성화
        Renderer[] renderers = owner.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }

        Collider[] colliders = owner.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = true;
        }

        // NavMeshAgent 활성화
        owner.NmAgent.enabled = true;
    }

    private void SpawnSplitFragments()
    {
        // 어빌리티 매니저에서 그림자/분열 조각 프리팹 가져오기
        GameObject shadowPrefab = owner.Shadow;
        if (shadowPrefab == null)
        {
            Debug.LogError("분열 조각 프리팹을 찾을 수 없습니다!");
            return;
        }

        // 원형으로 보스 분열 조각 배치
        for (int i = 0; i < shadowCount; i++)
        {
            // 원형으로 위치 계산
            float angle = i * (360f / shadowCount);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            // NavMesh 위에 위치 찾기
            Vector3 targetPos = originalPosition + direction * summonDistance;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, summonDistance, NavMesh.AllAreas))
            {
                // 분열 조각 인스턴스화
                GameObject fragment = GameObject.Instantiate(shadowPrefab, hit.position, Quaternion.identity);

                // 외형 변경 (본체와 유사하지만 약간 다르게)
                CustomizeSplitFragment(fragment);

                // 분열 조각 초기화
                DigitalShadow fragmentComponent = fragment.GetComponent<DigitalShadow>();
                if (fragmentComponent != null)
                {
                    fragmentComponent.Initialize(owner);
                    fragmentComponent.SetAsLastStandFragment(isLastStandMode); // 발악 패턴용 조각임을 표시
                    activeShadows.Add(fragment);

                    // 파괴 이벤트 등록
                    fragmentComponent.OnShadowDestroyed += HandleShadowDestroyed;
                }
            }
        }
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null;
    }

    private void CustomizeSplitFragment(GameObject fragment)
    {
        // 발악 패턴에서의 분열 조각 외형 커스터마이징
        // 예: 색상 변경, 이펙트 추가, 크기 조정 등
        Renderer[] renderers = fragment.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            // 색상을 약간 다르게 (예: 붉은 계열로)
            if (renderer.material != null)
            {
                renderer.material.color = new Color(
                    renderer.material.color.r + 0.2f,
                    renderer.material.color.g - 0.1f,
                    renderer.material.color.b - 0.1f,
                    renderer.material.color.a
                );
            }
        }

        // 크기를 본체보다 약간 작게
        fragment.transform.localScale = originalScale * 0.8f;

        // 파티클 시스템 추가 (있는 경우)
        ParticleSystem particleSystem = fragment.GetComponentInChildren<ParticleSystem>();
        if (particleSystem != null)
        {
            var main = particleSystem.main;
            main.startColor = new Color(1f, 0.5f, 0.5f); // 붉은 계열 파티클
        }
    }

    private void HandleShadowDestroyed(GameObject shadow)
    {
        if (activeShadows.Contains(shadow))
        {
            activeShadows.Remove(shadow);

            // 로그 추가
            Debug.Log($"그림자 파괴됨. 남은 그림자: {activeShadows.Count}, 발악 모드: {isLastStandMode}");
        }

        // 모든 분열 조각이 파괴되었는지 확인
        if (activeShadows.Count == 0)
        {
            if (isLastStandMode)
            {
                // 발악 모드에서 모든 분열 조각이 파괴되면 보스 처치
                Debug.Log("모든 발악 모드 분열 조각이 파괴됨. 보스 처치.");
                MonsterStatus status = owner.GetComponent<MonsterStatus>();
                status.SetHealth(0); // 남은 체력 모두 감소
            }
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

        // 발악 모드인 경우 보스 처치 처리
        if (isLastStandMode)
        {
            Debug.Log("발악 모드 시간 종료. 보스 처치.");
            MonsterStatus status = owner.GetComponent<MonsterStatus>();
            status.SetHealth(0);
        }
        else
        {
            // 일반 모드에서는 공격 종료 처리
            Debug.Log("분열 공격 시간 종료. 본체 복원.");
            RestoreOwner();
            OnAttackFinished();
        }
    }

    public void OnAttackFinished()
    {
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;
}


