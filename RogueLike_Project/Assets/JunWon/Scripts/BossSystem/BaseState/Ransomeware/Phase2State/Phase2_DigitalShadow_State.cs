using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Phase2_DigitalShadow_State : BossPhaseBase<Ransomware>
{
    private bool isAttackFinished = false;
    private int shadowCount = 6; // 분열할 조각 수
    [SerializeField] private List<GameObject> activeShadows = new List<GameObject>();
    private float shadowLifetime = 60f; // 분열 조각 지속 시간
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
        GameObject shadowPrefab = owner.Shadow;
        if (shadowPrefab == null) return;

        Debug.Log($"섀도우 생성 시작: 목표 {shadowCount}개");
        float spawnRadius = 5f;

        for (int i = 0; i < shadowCount; i++)
        {
            float angle = (i * (360f / shadowCount));
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            float distance = spawnRadius * 0.8f;
            Vector3 targetPos = originalPosition + direction * distance;

            NavMeshHit hit;
            GameObject fragment;
            if (NavMesh.SamplePosition(targetPos, out hit, distance, NavMesh.AllAreas))
            {
                fragment = Object.Instantiate(shadowPrefab, hit.position, Quaternion.identity);
                Debug.Log($"섀도우 {i + 1} 생성됨: 위치 {hit.position}");
            }
            else
            {
                fragment = Object.Instantiate(shadowPrefab,
                    originalPosition + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f)),
                    Quaternion.identity);
                Debug.Log($"섀도우 {i + 1} 대체 위치에 생성됨");
            }

            DigitalShadow fragmentComponent = fragment.GetComponent<DigitalShadow>();
            if (fragmentComponent != null)
            {
                // 분열 조각이 죽었을 때 보스에게 신호를 전달하도록 이벤트 구독
                fragmentComponent.OnShadowDestroyed += HandleShadowDestroyed;
            }
            activeShadows.Add(fragment);
        }

        owner.ShadowsSpawned = true;
        Debug.Log($"생성 완료: 총 {activeShadows.Count}개 생성됨");
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null;
    }

    private void CustomizeSplitFragment(GameObject fragment)
    {
        // 발악 패턴에서의 분열 조각 외형 커스터마이징
        // 각각의 분열 조각을 약간씩 다르게 만들어 구분하기 쉽게 함
        Renderer[] renderers = fragment.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            // 색상을 약간 다르게 (예: 붉은 계열로 - 각 분열체마다 다른 색조)
            if (renderer.material != null)
            {
                float hueVariation = Random.Range(-0.1f, 0.1f);
                renderer.material.color = new Color(
                    Mathf.Clamp01(renderer.material.color.r + 0.2f + hueVariation),
                    Mathf.Clamp01(renderer.material.color.g - 0.1f - hueVariation),
                    Mathf.Clamp01(renderer.material.color.b - 0.1f + hueVariation),
                    renderer.material.color.a
                );
            }
        }

        // 크기를 약간씩 다르게 (더 자연스러운 분열 느낌)
        float sizeVariation = Random.Range(0.75f, 0.9f);
        fragment.transform.localScale = originalScale * sizeVariation;

        // 파티클 시스템 추가 (있는 경우)
        ParticleSystem particleSystem = fragment.GetComponentInChildren<ParticleSystem>();
        if (particleSystem != null)
        {
            var main = particleSystem.main;

            // 각각 약간 다른 색상의 파티클
            float r = Random.Range(0.8f, 1.0f);
            float g = Random.Range(0.3f, 0.6f);
            float b = Random.Range(0.3f, 0.6f);
            main.startColor = new Color(r, g, b); // 붉은 계열 파티클

            // 파티클 크기도 랜덤화
            float sizeMult = Random.Range(0.9f, 1.1f);
            main.startSize = main.startSize.constant * sizeMult;
        }

        // RangedMonster 컴포넌트에 추가 설정 (다양한 공격 패턴)
        RangedMonster rangedMonster = fragment.GetComponent<RangedMonster>();
        if (rangedMonster != null)
        {
            // 공격 쿨다운, 조준 시간 등을 약간씩 다르게 설정
            //rangedMonster.attack = rangedMonster.attackCooldown * Random.Range(0.8f, 1.2f);
        }
    }

    private void HandleShadowDestroyed(GameObject shadow)
    {
        if (shadow == null) return;

        if (activeShadows.Contains(shadow))
        {
            activeShadows.Remove(shadow);
            Debug.Log($"그림자 파괴됨. 남은 그림자: {activeShadows.Count}");
        }

        activeShadows.RemoveAll(item => item == null);

        if (activeShadows.Count == 0)
        {
            Debug.Log("모든 분열 조각이 파괴됨. 보스에게 신호 전달.");
            // 보스에게 분열 조각 파괴 완료 신호 전달 (예: 다음 페이즈로 전환)
            owner.DigitalShadowsFinished(); // Ransomware 또는 보스 클래스에 이 메서드를 구현
            RestoreOwner();
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


