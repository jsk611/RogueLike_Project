using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    private void Awake()
    {
        Instance = this;

        //if (Instance == null)
        //{
        //    Instance = this;
        //    DontDestroyOnLoad(gameObject);
        //}
        //else
        //{
        //    Destroy(gameObject);
        //}
    }
    public event Action MonsterDamagedEvent;
    public event Action EnemyCountReset;
    public event Action<bool> MonsterKilledEvent;

    public void TriggerMonsterDamagedEvent()
    {
        MonsterDamagedEvent.Invoke();
    }
    public void TriggerMonsterKilledEvent(bool isEnemyCounted)
    {
        MonsterKilledEvent.Invoke(isEnemyCounted);
    }
    public void TriggerEnemyCountReset()
    {
        EnemyCountReset.Invoke();
    }

}
