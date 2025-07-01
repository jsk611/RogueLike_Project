using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase2_RangedAttackState : State<Ransomware>
{
    private bool isAttackFinished = false;

    public Phase2_RangedAttackState(Ransomware owner) : base(owner)
    {
        owner.SetRangedAttackState(this);
    }

    public override void Enter()
    {
        Debug.Log("[Phase2_RangedAttackState] Enter");
        isAttackFinished = false;
        owner.NmAgent.isStopped = true;

        if (CanExecuteAttack())
        {
            owner.Animator.SetTrigger("RangedAttack2");
            owner.AbilityManager.UseAbility("BasicRangedAttack");
        }
        else
        {
            Debug.LogWarning("Cannot execute attack - missing components");
            isAttackFinished = true; // 공격 불가능한 경우 바로 공격 전환
        }
    }



    public void FireProjectile()
    {
        // 🌀 Phase 2에서는 다중 발사체 곡선 공격!
        owner.StartCoroutine(FireMultipleCurvedProjectiles());
    }
    
    /// <summary>
    /// Phase 2: 다중 곡선 발사체 공격
    /// </summary>
    private IEnumerator FireMultipleCurvedProjectiles()
    {
        Vector3 firePos = owner.FirePoint.position;
        Vector3 baseDirection = (owner.Player.position - firePos).normalized;
        
        // Phase 2에서는 2-3개의 발사체를 연속으로 발사
        int projectileCount = Random.Range(2, 4);
        
        for (int i = 0; i < projectileCount; i++)
        {
            // 각 발사체마다 약간씩 다른 방향
            float angleOffset = (i - projectileCount * 0.5f) * 15f; // -15도 ~ +15도
            Vector3 direction = Quaternion.AngleAxis(angleOffset, Vector3.up) * baseDirection;
            
            GameObject projectile = GameObject.Instantiate(
                owner.DataPacket,
                firePos,
                Quaternion.LookRotation(direction)
            );

            if (projectile.TryGetComponent<MProjectile>(out var mProjectile))
            {
                mProjectile.SetBulletDamage(owner.AbilityManager.GetAbiltiyDmg("BasicRangedAttack"));
                mProjectile.SetDirection(direction);
                
                // 🌀 Phase 2 곡선 공격 설정
                if (projectile.TryGetComponent<DataPacket>(out var dataPacket))
                {
                    SetupPhase2CurvedAttack(dataPacket, i);
                }
            }
            
            // 발사체 간격
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log($"🌀🌀 Phase2 Multiple Curved Attack: {projectileCount} projectiles fired!");
    }
    
    /// <summary>
    /// Phase 2 곡선 공격 설정 - 더 강력하고 복잡한 패턴
    /// </summary>
    private void SetupPhase2CurvedAttack(DataPacket dataPacket, int projectileIndex)
    {
        // Phase 2에서는 모든 곡선 타입 사용 (더 위험함)
        DataPacket.CurveType[] phase2Curves = {
            DataPacket.CurveType.Bezier,     // 예측 불가능한 베지어
            DataPacket.CurveType.Spiral,     // 나선형 추적
            DataPacket.CurveType.Wave3D,     // 3D 파동
            DataPacket.CurveType.SineWave,   // 강화된 사인파
            DataPacket.CurveType.Random      // 완전 랜덤
        };
        
        DataPacket.CurveType selectedCurve = phase2Curves[Random.Range(0, phase2Curves.Length)];
        dataPacket.SetCurveType(selectedCurve);
        
        // Phase 2: 높은 강도의 곡선 (2.5 ~ 4.0)
        float curveIntensity = Random.Range(2.5f, 4.0f);
        
        // 각 발사체마다 다른 강도 적용
        if (projectileIndex == 0)
        {
            // 첫 번째는 가장 강력하게
            curveIntensity *= 1.2f;
            selectedCurve = DataPacket.CurveType.Bezier; // 베지어로 고정
        }
        else if (projectileIndex == 1)
        {
            // 두 번째는 나선형으로
            selectedCurve = DataPacket.CurveType.Spiral;
        }
        
        dataPacket.SetCurveIntensity(curveIntensity);
        
        // Phase 2: 강한 호밍 효과 (0.5 ~ 0.8)
        float homingStrength = Random.Range(0.5f, 0.8f);
        dataPacket.SetHomingStrength(homingStrength);
        
        Debug.Log($"🎯🎯 Phase2 Curve Attack #{projectileIndex}: {selectedCurve}, Intensity: {curveIntensity:F1}, Homing: {homingStrength:F1}");
    }


    public override void Exit()
    {
        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("RangedAttack");
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null &&
               owner.DataPacket != null &&
               owner.FirePoint != null;
    }
    
    public void OnAttackFinished()
    {
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;

    public override void Update()
    {
       
    }
}
