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

            int randNum = Random.Range(1, 4);
            enemyMap[y,x] = (EnemyType)randNum;
        }
    }

    IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            yield return StartCoroutine(Wave4());
            yield return StartCoroutine(WaveEnd());
            yield return new WaitForSeconds(3f);
            yield return StartCoroutine(Wave5());
            yield return StartCoroutine(WaveEnd());
            yield return new WaitForSeconds(3f);
            yield return StartCoroutine(Wave1());
            yield return StartCoroutine(WaveEnd());
            yield return new WaitForSeconds(3f);
            yield return StartCoroutine(Wave2());
            yield return StartCoroutine(WaveEnd());
            yield return new WaitForSeconds(3f);
            yield return StartCoroutine(Wave3());
            yield return StartCoroutine(WaveEnd());
            yield return new WaitForSeconds(3f);
        }
    }

    IEnumerator Wave1()
    {
        Debug.Log("Wave 1");
        tileManager.InitializeArray();
        tileManager.MakeCircle(Random.Range(8,14));
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        InitializeEnemyArray();
        MakeRandomEnemyMap(5);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        yield return new WaitForSeconds(5f);
        Debug.Log("Wave End");
    }

    IEnumerator Wave2()
    {
        Debug.Log("Wave 2");
        tileManager.InitializeArray();
        tileManager.MakePyramid(Random.Range(15,31));
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        InitializeEnemyArray();
        MakeRandomEnemyMap(7);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        yield return new WaitForSeconds(5f);
        Debug.Log("Wave End");
    }

    IEnumerator Wave3()
    {
        Debug.Log("Wave 3");
        tileManager.InitializeArray(6);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        InitializeEnemyArray();
        MakeRandomEnemyMap(3);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        yield return new WaitForSeconds(3f);
        for(int i=0; i<10; i++)
        {
            StartCoroutine(tileManager.MakeWave(15, 15, 1, 1.5f, 20));
            yield return new WaitForSeconds(1f);
        }
        Debug.Log("Wave End");
    }
    IEnumerator Wave4()
    {
        Debug.Log("Wave 4");
        //tileManager.InitializeArray(6);
        //yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        for(int i=0; i<5; i++)
        {
            //랜덤으로 벽 생성
            tileManager.InitializeArray(6);
            tileManager.MakeRandomWall(Random.Range(8, 16));
            yield return StartCoroutine(tileManager.MoveTilesByArray());
            yield return new WaitForSeconds(5f);
        }

        //InitializeEnemyArray();
        //MakeRandomEnemyMap(3);
        //enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        Debug.Log("Wave End");
    }

    IEnumerator Wave5()
    {
        Debug.Log("Wave 5");
        //tileManager.InitializeArray(6);
        //yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 5; i++)
        {
            //랜덤으로 구멍 생성
            tileManager.MakeRandomHole(Random.Range(4, 9));
            yield return StartCoroutine(tileManager.MoveTilesByArray());
            yield return new WaitForSeconds(5f);
        }

        //InitializeEnemyArray();
        //MakeRandomEnemyMap(3);
        //enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        Debug.Log("Wave End");
    }

    IEnumerator WaveEnd()
    {
        tileManager.InitializeArray(6);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
    }
}
