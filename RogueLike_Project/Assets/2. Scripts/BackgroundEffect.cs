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
        
    }

    // Update is called once per frame
    void Update()
    {
        if(enemyCountData.enemyCount < tmp)
        {
            maxSkyHeight = 0.25f + 0.75f * (enemyCountData.enemyCount / (float)maxEnemy);
            backgroundMT.SetFloat("_HorizonHeight", maxSkyHeight - 0.2f);
        }
        else if(enemyCountData.enemyCount > tmp)
        {
            maxEnemy = enemyCountData.enemyCount;
        }

        float currentHeight = backgroundMT.GetFloat("_HorizonHeight");
        if(currentHeight < maxSkyHeight)
        {
            backgroundMT.SetFloat("_HorizonHeight", currentHeight + Time.deltaTime * speed);
        }
        tmp = enemyCountData.enemyCount;
    }
}
