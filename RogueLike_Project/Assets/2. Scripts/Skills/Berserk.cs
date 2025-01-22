using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berserk : SkillBehaviour
{
    // Start is called before the first frame update
    PlayerStatus status;
    CharacterBehaviour character;

    [SerializeField]
    float damageIncreaseRate = 200f;
    [SerializeField]
    float duration = 7f;

    private void Start()
    {
        character = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        status = character.GetComponent<PlayerStatus>();
    }
    public override void SkillActivation()
    {
        if (!CanActivateSkill()) return;
        recentSKillUsed = Time.time;
        float halflife = status.GetHealth() / 2;
        StartCoroutine(BloodPact(halflife));
        StartCoroutine(BerserkMode(halflife));
    }

    IEnumerator BerserkMode(float halflife)
    {
        float addDamage = halflife * damageIncreaseRate / 100f;
        status.IncreaseAttackDamage(addDamage);
        yield return new WaitForSeconds(duration);
        status.DecreaseAttackDamage(addDamage);
    }
    IEnumerator BloodPact(float halflife)
    {
        while ((int)status.GetHealth() > (int)halflife)
        {
            status.DecreaseHealth(0.1f * (status.GetHealth() - halflife));
            yield return null;
        }
    }
}
