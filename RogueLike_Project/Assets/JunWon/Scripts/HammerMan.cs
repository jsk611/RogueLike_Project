using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerMan : MonsterBase
{

    protected override void Start() { base.Start(); }

    protected override IEnumerator IDLE() { return base.IDLE(); }
    protected override IEnumerator CHASE() { return base.CHASE(); }
    protected override IEnumerator ATTACK() { 
        yield return base.ATTACK();

        MakeShockWave(dmg);
        yield return null;
    }
    protected override IEnumerator HIT() { return base.HIT(); }
    protected override IEnumerator DIE() { return base.DIE(); }

    void MakeShockWave(float damage)
    {

    }
}
