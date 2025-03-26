using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderPrime : BossBase
{
    // Start is called before the first frame update
    [SerializeField] private AbilityManager abilityManager;


    public AbilityManager AbilityManager => abilityManager;

    void Start()
    {
        InitializeComponents();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void InitializeComponents()
    {
        nmAgent = GetComponent<NavMeshAgent>();
        bossStatus = GetComponent<BossStatus>();
        fov = GetComponent<FieldOfView>();
        target = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().transform;
    }
    public override void TakeDamage(float damage, bool showDamage = true)
    {
  
        bossStatus.DecreaseHealth(damage);

        EventManager.Instance.TriggerMonsterDamagedEvent();
        Instantiate(UIDamaged, transform.position + new Vector3(0, UnityEngine.Random.Range(0f, height / 2), 0), Quaternion.identity).GetComponent<UIDamage>().damage = damage;
        if (bossStatus.GetHealth() <= 0)
        {
          //add dieState;
        }
    }
}
