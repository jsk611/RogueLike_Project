using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    TileManager tileManager;
    EnemySpawnLogic enemySpawnLogic;
    EnemyType[,] enemyMap;
    int mapSize;
    void Start()
    {
        tileManager = FindObjectOfType<TileManager>();
        enemySpawnLogic = FindObjectOfType<EnemySpawnLogic>();
        mapSize = tileManager.GetMapSize;
        enemyMap = new EnemyType[mapSize, mapSize];
        InitializeEnemyArray();

        StartCoroutine(RunWaves()); //나중에 GameManager로 옮길 것
    }


    void InitializeEnemyArray()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                enemyMap[i, j] = EnemyType.None;
            }
        }
    }

    void MakeRandomEnemyMap(int num)
    {
        for(int i=0; i<num; i++)
        {
            int x = Random.Range(0, mapSize);
            int y = Random.Range(0, mapSize);

            if (enemyMap[y,x] != EnemyType.None || tileManager.GetTileMap[y,x] < 0)
            {
                i--;
                continue;
            }

            int randNum = Random.Range(1, 3);
            enemyMap[y,x] = (EnemyType)randNum;
        }
    }

    IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            yield return StartCoroutine(Wave1());
            yield return new WaitForSeconds(5f);
        }
    }

    IEnumerator Wave1()
    {
        tileManager.InitializeArray();
        tileManager.MakeCircle(12);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        Debug.Log("테스트");
        InitializeEnemyArray();
        MakeRandomEnemyMap(10);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        yield return new WaitForSeconds(10f);
    }
}
