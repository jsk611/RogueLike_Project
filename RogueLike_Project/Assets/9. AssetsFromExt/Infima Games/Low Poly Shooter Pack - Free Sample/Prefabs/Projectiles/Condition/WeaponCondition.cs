using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponCondition : MonoBehaviour
{
    // Start is called before the first frame update
    public float effect = 0;
    public float duration = 0;
    public float interval = 0;

    //초기화 및 수정도 포함
    public virtual void StateInitializer(float eff, float dur, float itv)
    {
        effect += eff;
        duration += dur;
        interval += itv;
    }

    public virtual void Succession(WeaponCondition bulletCondition)
    {
        bulletCondition.StateInitializer(effect, duration, interval);
    }
    // Update is called once per frame

}
