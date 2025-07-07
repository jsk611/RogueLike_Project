using System.Collections;
using System.Collections.Generic;
using Unity.Play.Publisher.Editor;
using UnityEngine;
using UnityEngine.AI;

public class DefeatedState_Ransomeware : State<Ransomware>
{
    private bool isAnimationFinished = false;
    private float deathEffectDelay = 0.5f;
    private float despawnDelay = 5f;

    public DefeatedState_Ransomeware(Ransomware owner) : base(owner)
    {
        owner.SetDefeatedState(this);
    }

    public override void Enter()
    {
        Debug.Log("[DefeatedState_Ransomeware] Enter");

        // NavMeshAgent 정지
        if (owner.NmAgent != null)
        {
            owner.NmAgent.isStopped = true;
            owner.NmAgent.enabled = false;
        }

        // 콜라이더 비활성화 (선택적)
        Collider[] colliders = owner.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // 애니메이션 설정
        owner.Animator.SetLayerWeight(owner.Animator.GetLayerIndex("Dead"), 1.0f);
        owner.Animator.SetTrigger("Dead");

        // 죽음 이펙트 재생 (딜레이 적용)
        owner.StartCoroutine(PlayDeathEffectWithDelay());

        // 보상 아이템 드롭 (선택적)
        DropRewards();
    }

    public override void Update()
    {
        // 필요한 경우 업데이트 로직
    }

    public override void Exit()
    {
        Debug.Log("[DefeatedState_Ransomeware] Exit");
    }

    public void OnDeathAnimationFinished()
    {
        Debug.Log("[DefeatedState_Ransomeware] 죽음 애니메이션 완료");
        isAnimationFinished = true;

        // 보상 드롭
        DropRewards();

        // 오브젝트 비활성화
        DisableRansomware();
    }

    private IEnumerator PlayDeathEffectWithDelay()
    {
        yield return new WaitForSeconds(deathEffectDelay);

        // 죽음 이펙트 재생
        //if (owner.DeathEffect != null)
        //{
        //    GameObject effect = GameObject.Instantiate(owner.DeathEffect, owner.transform.position, Quaternion.identity);
        //    // 선택적으로 이펙트 파티클 방향 및 위치 조정
        //}

        // 데이터 삭제 효과음 재생
        AudioSource audioSource = owner.GetComponent<AudioSource>();
        //if (audioSource != null && owner.DeathSound != null)
        //{
        //    audioSource.PlayOneShot(owner.DeathSound);
        //}
    }

    private IEnumerator DespawnAfterDelay()
    {
        yield return new WaitForSeconds(despawnDelay);

        // 랜섬웨어 보스 제거
        GameObject.Destroy(owner.gameObject);
    }

    private void DropRewards()
    {
        // 보상 드롭 로직
        // 데이터 회복 아이템, 업그레이드 키 등 드롭
    }

    private void DisableRansomware()
    {
        Debug.Log("[DefeatedState_Ransomeware] 랜섬웨어 비활성화");

        // 랜섬웨어 보스 비활성화
        GameObject.Destroy(owner.gameObject);

        // 필요한 경우 추가 클린업 작업
        // 예: 특정 매니저에 보스 처치 알림 등
    }

    public bool IsAnimationFinished() => isAnimationFinished;
}