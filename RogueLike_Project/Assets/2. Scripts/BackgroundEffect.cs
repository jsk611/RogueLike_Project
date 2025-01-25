using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundEffect : MonoBehaviour
{
    [SerializeField] EnemyCountData enemyCountData;
    [SerializeField] Material backgroundMT;
    int tmp = 0;
    [SerializeField] float speed;
     // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(enemyCountData.enemyCount < tmp)
        {
            backgroundMT.SetFloat("_HorizonHeight", 0.4f);
        }

        float currentHeight = backgroundMT.GetFloat("_HorizonHeight");
        if(currentHeight < 1f)
        {
            backgroundMT.SetFloat("_HorizonHeight", currentHeight + Time.deltaTime * speed);
        }
        tmp = enemyCountData.enemyCount;
    }
}
