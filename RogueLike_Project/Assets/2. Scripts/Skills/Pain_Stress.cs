

using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pain_Stress : SkillBehaviour
{
    // Start is called before the first frame update
    PlayerStatus status;
    CharacterBehaviour character;

    [SerializeField]
    float additionalAttackDamage = 38;

    private float BasicHealth = 100;
    private void Start()
    {
        character = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        status = character.GetComponent<PlayerStatus>();
    }
    public override void SkillActivation()
    {
        if (!CanActivateSkill()) return;
        
        if (status.GetMaxHealth() <= BasicHealth) return;
        
        recentSKillUsed = Time.time;
        StartCoroutine(Pain());

    }

    IEnumerator Pain()
    {
        status.IncreaseAttackDamage(status.GetMaxHealth() - BasicHealth + additionalAttackDamage);
        float StartHealth = status.GetMaxHealth();
        while(StartHealth>BasicHealth)
        {
            status.DecreaseMaxHealth(1);
            StartHealth -= 1;
            yield return new WaitForSeconds(0.1f);
        }
        
    }
}
