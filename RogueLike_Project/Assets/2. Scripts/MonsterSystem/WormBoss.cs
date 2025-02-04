using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormBoss : MonsterBase
{
    [Header("Body Setting")]
    [SerializeField]
    List<Transform> bodyList;
    [SerializeField]
    Transform chaseTarget;
    [SerializeField]
    float turnInterval = 1f;
    float turnTimer = 0f;
    Transform wormHead;

    //target attaack
    bool isWandering = true;

    [Header("Minion Setting")]
    [SerializeField]
    GameObject summonObject;
    [SerializeField]
    float summonInterval = 5f;
    float summonTimer = 0f;
    List<GameObject> summonedEnemies = new List<GameObject>();


    // Update is called once per frame
    protected override void Start()
    {
        base.Start();
        wormHead = bodyList[0];
        nmAgent.updatePosition = false;
        nmAgent.updateRotation = false;
        StartCoroutine(Move());
    } 

    protected override void UpdateChase()
    {
        isWandering = true;
        summonTimer += Time.deltaTime;
        turnTimer += Time.deltaTime;
        attackTimer += Time.deltaTime;
        if (summonTimer >= summonInterval)
        {
         //   SummonMinion();
            summonTimer = 0f;
        }

        if(attackTimer >= attackCooldown)
        {
           // ChangeState(State.ATTACK);
        }
    }
    protected override void UpdateAttack()
    {
        //플레이어에게 돌진
        isWandering = false;
        attackTimer = 0f;
    }
    void SummonMinion()
    {

        Vector3 randomPosition = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
        GameObject minion = (Instantiate(summonObject,transform.position+randomPosition, Quaternion.identity));
        minion.GetComponent<MonsterBase>().summonedMonster = true;
        minion.GetComponent<MonsterStatus>().SetMaxHealth(20);
   //     minion.GetComponent<MonsterBase>().master = gameObject;
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
        foreach (GameObject minion in summonedEnemies)
        {
            minion.GetComponent<MonsterBase>().TakeDamage(9999, false);
        }
    }
    IEnumerator Move()
    {
        while(true)
        {
            turnTimer += Time.deltaTime;
            if (turnTimer >= turnInterval)
            {
                if (isWandering)
                {
                    turnInterval = Random.Range(1f, 5f);
                    turnTimer = 0f;
                    chaseTarget.position = new Vector3(Random.Range(0,90),4,Random.Range(0,90));
                }
                else
                {
                    chaseTarget.position = target.position;
                    turnTimer = 0f;
                }
            }
            wormHead.rotation = Quaternion.Lerp(wormHead.rotation, Quaternion.LookRotation(chaseTarget.position - wormHead.position), Time.deltaTime * chaseSpeed);
            wormHead.position += wormHead.forward * Time.deltaTime * chaseSpeed;
            for (int i = 1; i < bodyList.Count; i++)
            {
                if (Vector3.Distance(bodyList[i].position, bodyList[i - 1].position) > 2.5f)
                {
                    bodyList[i].rotation = Quaternion.Lerp(bodyList[i].rotation, Quaternion.LookRotation(bodyList[i - 1].position - bodyList[i].position), Time.deltaTime * chaseSpeed);
                    bodyList[i].position += bodyList[i].forward * Time.deltaTime * chaseSpeed;
                }
            }
            yield return null;
        }
    }
}
