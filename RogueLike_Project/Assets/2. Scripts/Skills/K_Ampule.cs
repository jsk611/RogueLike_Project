using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class K_Ampule : SkillBehaviour
{
    // Start is called before the first frame update
    PlayerStatus status;
    CharacterBehaviour character;

    [SerializeField]
    float ExtraHealth = 100;
    [SerializeField]
    float duration = 7f;
    [SerializeField]

    private void Start()
    {
        character = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        status = character.GetComponent<PlayerStatus>();
    }
    public override void SkillActivation()
    {
        if (!CanActivateSkill()) return;
        recentSKillUsed = Time.time;
        StartCoroutine(AmpuleActivation());
        StartCoroutine(LifeBoost());
    }

    IEnumerator LifeBoost()
    {
        yield return new WaitForSeconds(duration);
        status.DecreaseMaxHealth(ExtraHealth);
    }
    IEnumerator AmpuleActivation()
    {
        status.IncreaseMaxHealth(ExtraHealth);
        float StartHealth = ExtraHealth;
        while(StartHealth>0)
        {
            float healthIncrease = (int)(StartHealth * 0.1f) + 1;
            status.IncreaseHealth(healthIncrease);
            StartHealth -= healthIncrease;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
