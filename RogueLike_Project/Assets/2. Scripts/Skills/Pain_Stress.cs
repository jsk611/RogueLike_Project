

using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pain_Stress : SkillBehaviour
{
    // Start is called before the first frame update
    PlayerStatus status;
    CharacterBehaviour character;

    private float BasicHealth = 100;
    private void Start()
    {
        character = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        status = character.GetComponent<PlayerStatus>();
    }
    public override void SkillActivation()
    {
        if (!CanActivateSkill()) return;
        if (status.GetHealth() <= BasicHealth) return;
        StartCoroutine(Pain());

    }

    IEnumerator Pain()
    {
        status.IncreaseAttackDamage(status.GetHealth() - BasicHealth);
        float StartHealth = status.GetMaxHealth();
        status.SetHealth(BasicHealth);
        while(StartHealth>BasicHealth)
        {
            status.DecreaseMaxHealth(1);
            StartHealth -= 1;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
