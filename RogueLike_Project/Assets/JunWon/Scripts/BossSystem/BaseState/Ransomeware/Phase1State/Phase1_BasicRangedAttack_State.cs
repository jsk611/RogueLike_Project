using System;
using System.Collections;
using UnityEngine;

public class Phase1_BasicRangedAttack_State : State<Ransomware>
{
    private enum RangedPattern
    {
        FocusedCurve,
        SpreadVolley,
        LockOnChase
    }

    private static readonly DataPacket.CurveType[] FocusedCurves =
    {
        DataPacket.CurveType.Spiral,
        DataPacket.CurveType.Wave3D
    };

    private static readonly DataPacket.CurveType[] SpreadCurves =
    {
        DataPacket.CurveType.Spiral,
        DataPacket.CurveType.Zigzag,
        DataPacket.CurveType.SineWave
    };

    private static readonly DataPacket.CurveType[] LockOnCurves =
    {
        DataPacket.CurveType.SineWave,
        DataPacket.CurveType.Bezier
    };

    private bool isAttackFinished = false;
    private RangedPattern selectedPattern;
    private Coroutine volleyRoutine;

    public Phase1_BasicRangedAttack_State(Ransomware owner) : base(owner)
    {
        owner.SetRangedAttackState(this);
    }

    public override void Enter()
    {
        Debug.Log("[Phase1_BasicRangedAttack_State] Enter");
        isAttackFinished = false;
        owner.NmAgent.isStopped = true;
        selectedPattern = SelectPattern();
        AimTowardsPlayer();

        if (CanExecuteAttack())
        {
            Debug.Log($"[Phase1_BasicRangedAttack_State] Pattern: {selectedPattern}");
            owner.Animator.SetTrigger("RangedAttack");
            owner.AbilityManager.UseAbility("BasicRangedAttack");
        }
        else
        {
            Debug.LogWarning("Cannot execute attack - missing components");
            isAttackFinished = true;
        }
    }

    public override void Update()
    {
        if (isInterrupted) return;
        MaintainAim();
    }

    public override void Exit()
    {
        if (volleyRoutine != null)
        {
            owner.StopCoroutine(volleyRoutine);
            volleyRoutine = null;
        }

        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("RangedAttack");
    }

    public void FireProjectile()
    {
        if (owner.Player == null || owner.FirePoint == null || owner.DataPacket == null)
            return;

        if (volleyRoutine != null)
            return;

        volleyRoutine = owner.StartCoroutine(FirePatternRoutine());
    }

    public void OnAttackFinished()
    {
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;

    private IEnumerator FirePatternRoutine()
    {
        yield return new WaitForEndOfFrame();

        switch (selectedPattern)
        {
            case RangedPattern.FocusedCurve:
                FireFocusedShot();
                break;
            case RangedPattern.SpreadVolley:
                yield return FireSpreadVolley();
                break;
            case RangedPattern.LockOnChase:
                yield return FireLockOnChase();
                break;
        }

        volleyRoutine = null;
        if (!isAttackFinished)
        {
            isAttackFinished = true;
        }
    }

    private void FireFocusedShot()
    {
        Vector3 direction = GetLeadDirection(0.2f, 0.15f, 0.55f);
        SpawnProjectile(direction, packet =>
        {
            ConfigureProjectile(packet, FocusedCurves, new Vector2(1.4f, 2.1f), new Vector2(0.45f, 0.6f), 0.55f, 13f);
        });
        Debug.Log("🌀 Curved ranged attack projectile fired!");
    }

    private IEnumerator FireSpreadVolley()
    {
        Vector3 baseDirection = GetLeadDirection(0.28f, 0.2f, 0.45f);
        float[] angleOffsets = { -10f, 0f, 10f };

        for (int i = 0; i < angleOffsets.Length; i++)
        {
            Vector3 direction = Quaternion.AngleAxis(angleOffsets[i], Vector3.up) * baseDirection;
            SpawnProjectile(direction, packet =>
            {
                ConfigureProjectile(packet, SpreadCurves, new Vector2(1.0f, 1.8f), new Vector2(0.25f, 0.4f), 0.65f, 11.5f, false, 0.8f);
            });

            if (i < angleOffsets.Length - 1)
                yield return new WaitForSeconds(0.08f);
        }
    }

    private IEnumerator FireLockOnChase()
    {
        Vector3 primaryDirection = GetLeadDirection(0.22f, 0.15f, 0.7f);
        SpawnProjectile(primaryDirection, packet =>
        {
            ConfigureProjectile(packet, LockOnCurves, new Vector2(1.9f, 2.8f), new Vector2(0.55f, 0.75f), 0.6f, 12.5f);
        });

        yield return new WaitForSeconds(0.15f);

        Vector3 followUpDirection = GetLeadDirection(0.3f, 0.18f, 0.85f);
        SpawnProjectile(followUpDirection, packet =>
        {
            ConfigureProjectile(packet, new[] { DataPacket.CurveType.Spiral }, new Vector2(2.2f, 3.2f), new Vector2(0.5f, 0.7f), 0.6f, 13f);
        });
    }

    private DataPacket SpawnProjectile(Vector3 direction, Action<DataPacket> configure)
    {
        Vector3 firePos = owner.FirePoint.position;
        GameObject projectile = GameObject.Instantiate(owner.DataPacket, firePos, Quaternion.LookRotation(direction));

        if (projectile.TryGetComponent<MProjectile>(out var mProjectile))
        {
            mProjectile.SetBulletDamage(owner.AbilityManager.GetAbiltiyDmg("BasicRangedAttack"));
            mProjectile.SetDirection(direction);
        }

        DataPacket packet = null;
        if (projectile.TryGetComponent<DataPacket>(out var dataPacket))
        {
            configure?.Invoke(dataPacket);
            packet = dataPacket;
        }

        return packet;
    }

    private void ConfigureProjectile(
        DataPacket dataPacket,
        DataPacket.CurveType[] allowedCurves,
        Vector2 intensityRange,
        Vector2 homingRange,
        float scaleMultiplier,
        float speedOverride,
        bool showPath = false,
        float pathWidthMultiplier = 1f)
    {
        if (allowedCurves != null && allowedCurves.Length > 0)
        {
            var selectedCurve = allowedCurves[UnityEngine.Random.Range(0, allowedCurves.Length)];
            dataPacket.SetCurveType(selectedCurve);
        }

        dataPacket.SetCurveIntensity(UnityEngine.Random.Range(intensityRange.x, intensityRange.y));
        dataPacket.SetHomingStrength(UnityEngine.Random.Range(homingRange.x, homingRange.y));
        dataPacket.ConfigureVisualProfile(scaleMultiplier, showPath, pathWidthMultiplier);

        if (speedOverride > 0f)
        {
            dataPacket.SetSpeed(speedOverride);
        }
    }

    private Vector3 GetLeadDirection(float lateralScatter, float verticalScatter, float leadTime)
    {
        Vector3 firePos = owner.FirePoint.position;
        Vector3 targetPos = owner.Player.position;
        Rigidbody playerRb = owner.Player.GetComponent<Rigidbody>();

        if (playerRb != null)
        {
            targetPos += playerRb.velocity * leadTime;
        }
        else
        {
            targetPos += owner.Player.forward * leadTime * 2f;
        }

        targetPos += owner.Player.right * UnityEngine.Random.Range(-lateralScatter, lateralScatter);
        targetPos += owner.Player.up * UnityEngine.Random.Range(-verticalScatter, verticalScatter);

        Vector3 direction = targetPos - firePos;
        if (direction.sqrMagnitude < 0.001f)
            direction = owner.Player.position - firePos;

        return direction.normalized;
    }

    private void AimTowardsPlayer()
    {
        if (owner.Player == null) return;

        Vector3 toPlayer = owner.Player.position - owner.transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(toPlayer.normalized);
        owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, targetRotation, 0.6f);
    }

    private void MaintainAim()
    {
        if (owner.Player == null) return;

        Vector3 toPlayer = owner.Player.position - owner.transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(toPlayer.normalized);
        owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, targetRotation, Time.deltaTime * 6f);
    }

    private RangedPattern SelectPattern()
    {
        if (owner.Player == null)
            return RangedPattern.FocusedCurve;

        float distance = Vector3.Distance(owner.transform.position, owner.Player.position);
        float roll = UnityEngine.Random.value;

        if (distance > owner.RangedAttackRange * 0.8f)
            return RangedPattern.FocusedCurve;

        if (distance < owner.MeleeAttackRange * 1.2f)
            return roll < 0.6f ? RangedPattern.SpreadVolley : RangedPattern.LockOnChase;

        if (roll < 0.4f)
            return RangedPattern.SpreadVolley;
        if (roll < 0.7f)
            return RangedPattern.LockOnChase;
        return RangedPattern.FocusedCurve;
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null &&
               owner.DataPacket != null &&
               owner.FirePoint != null;
    }
}




