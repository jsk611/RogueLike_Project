using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundEffect : MonoBehaviour
{
    [SerializeField] EnemyCountData enemyCountData;
    [SerializeField] Material backgroundMT;
    int tmp = 0;
    [SerializeField] float speed;
    float maxSkyHeight = 1f;
    int maxEnemy;
     // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.MonsterKilledEvent += RealEnemyKilled;
        EventManager.Instance.EnemyCountReset += ResetMaxEnemy;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<MonsterStatus>() != null) 
            collision.gameObject.GetComponent<MonsterBase>().TakeDamage(9999, false);
    }

    // Update is called once per frame
    void Update()
    {
        float currentHeight = backgroundMT.GetFloat("_HorizonHeight");
        if(currentHeight < maxSkyHeight)
        {
            backgroundMT.SetFloat("_HorizonHeight", currentHeight + Time.deltaTime * speed);
        }
        tmp = enemyCountData.enemyCount;
    }
    private void OnDisable()
    {
        EventManager.Instance.MonsterKilledEvent -= RealEnemyKilled;
        EventManager.Instance.EnemyCountReset -= ResetMaxEnemy;
    }

    void RealEnemyKilled(bool isCounted)
    {
        if (!isCounted) return;
        maxSkyHeight = 0.25f + 0.6f * (enemyCountData.enemyCount / (float)maxEnemy);
        backgroundMT.SetFloat("_HorizonHeight", maxSkyHeight - 0.2f);
    }
    void ResetMaxEnemy()
    {
        maxEnemy = enemyCountData.enemyCount;
        maxSkyHeight = 0.25f + 0.6f * (enemyCountData.enemyCount / (float)maxEnemy);
    }
}
