using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnknownVirusBoss;

public class BossCombatContext
{
    // 거리 관련
    public float DistanceToPlayer { get; set; }
    public bool IsPlayerInMeleeRange { get; set; }
    public bool IsPlayerInRangedRange { get; set; }

    // 플레이어 상태
    public float PlayerHealthRatio { get; set; }
    public bool IsPlayerStunned { get; set; }
    public bool IsPlayerUsingRangedWeapon { get; set; }

    // 보스 상태
    public float BossHealthRatio { get; set; }
    public float TimeSinceLastFormChange { get; set; }
    public BossForm CurrentForm { get; set; }

    // 환경 요소
    public bool IsNearWall { get; set; }
    public bool IsOnElevatedGround { get; set; }
    public float ArenaRemaining { get; set; } // 전투 공간 잔여 비율

    // 능력 쿨다운
    public Dictionary<string, float> AbilityCooldowns { get; set; }

    public BossCombatContext()
    {
        AbilityCooldowns = new Dictionary<string, float>();
    }
}
