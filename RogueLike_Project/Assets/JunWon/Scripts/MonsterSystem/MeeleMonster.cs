using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using InfimaGames.LowPolyShooterPack;

public class MeeleMonster : MonsterBase
{
    protected override void Start() { base.Start(); }

    protected override IEnumerator IDLE() { return base.IDLE(); }
    protected override IEnumerator CHASE() { return base.CHASE(); }
    protected override IEnumerator ATTACK() { return base.ATTACK(); }
    protected override IEnumerator HIT() { return base.HIT(); }
    protected override IEnumerator DIE() { return base.DIE(); }
}
