using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponCondition : MonoBehaviour
{
    // Start is called before the first frame update
    public float damage= 1;
    public float duration = 1;
    public float probability= 1;
    public float interval = 1;
    public float effect = 1;

    //초기화 및 수정도 포함

    public virtual void StateInitializer(float dmg,float dur, float prob, float itv, float eff)
    {
        damage = dmg;
        duration = dur;
        probability = prob;
        interval = itv;
        effect = eff;

        int index = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetInventory().GetEquippedIndex();
        UIManager.instance.ElementImageSwap(index, this);
    }

    public virtual void Succession(WeaponCondition bulletCondition)
    {
        bulletCondition.StateInitializer(damage, duration, probability,interval,effect);
    }
    public virtual void Upgrade(float dmg, float dur, float prob, float itv, float eff) 
    {
        damage += dmg;
        duration += dur;
        probability += prob;
        interval += itv;
        effect += eff;
    }
    // Update is called once per frame

}
