using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

using UnityEngine;

public class Phase1_BasicRangedAttack_State : State<Ransomware>
{
    private bool isAttackFinished = false;

    public Phase1_BasicRangedAttack_State(Ransomware owner) : base(owner) {
        owner.SetRangedAttackState(this);
    }

    public override void Enter()
    {
        Debug.Log("[Phase1_BasicRangedAttack_State] Enter");
        isAttackFinished = false;
        owner.NmAgent.isStopped = true;

        if (CanExecuteAttack())
        {
            owner.Animator.SetTrigger("RangedAttack");
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
        Vector3 firePos = owner.FirePoint.position;
        Vector3 directionToPlayer = (owner.Player.position - firePos).normalized;

        GameObject projectile = GameObject.Instantiate(
            owner.DataPacket,
            firePos,
            Quaternion.LookRotation(directionToPlayer)
        );

        if (projectile.TryGetComponent<MProjectile>(out var mProjectile))
        {
            mProjectile.SetBulletDamage(owner.AbilityManager.GetAbiltiyDmg("BasicRangedAttack"));
            mProjectile.SetDirection(directionToPlayer);
            
            // 🌀 곡선 공격 설정
            if (projectile.TryGetComponent<DataPacket>(out var dataPacket))
            {
                SetupCurvedAttack(dataPacket);
            }
            
            Debug.Log("🌀 Curved ranged attack projectile fired!");
        }
    }
    
    /// <summary>
    /// 곡선 공격 설정 - Phase 1에서는 비교적 간단한 곡선
    /// </summary>
    private void SetupCurvedAttack(DataPacket dataPacket)
    {
        // Phase 1에서는 랜덤하지만 비교적 예측 가능한 곡선들 사용
        DataPacket.CurveType[] phase1Curves = {
            DataPacket.CurveType.Spiral,
            DataPacket.CurveType.Zigzag,
        };
        
        DataPacket.CurveType selectedCurve = phase1Curves[Random.Range(0, phase1Curves.Length)];
        dataPacket.SetCurveType(selectedCurve);
        
        // Phase 1: 중간 강도의 곡선 (1.5 ~ 2.5)
        float curveIntensity = Random.Range(1.5f, 2.5f);
        dataPacket.SetCurveIntensity(curveIntensity);
        
        // Phase 1: 약간의 호밍 효과 (0.2 ~ 0.4)
        float homingStrength = Random.Range(0.2f, 0.4f);
        dataPacket.SetHomingStrength(homingStrength);
        
        Debug.Log($"🎯 Phase1 Curve Attack: {selectedCurve}, Intensity: {curveIntensity:F1}, Homing: {homingStrength:F1}");
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

    // �ִϸ��̼� �̺�Ʈ���� ȣ��� �޼���
    public void OnAttackFinished()
    {
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;

    public override void Update()
    {
        
    }
}
