using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    TileManager tileManager;
    EnemySpawnLogic enemySpawnLogic;
    EnemyType[,] enemyMap;
    int mapSize;
    bool isGameStarted = false;
    public bool IsGameStarted
    {
        set { isGameStarted = value; }
    }

    [SerializeField] string[] mapPaths;
    [SerializeField] string startMapPath;
    [SerializeField] string jeongbiMapPath;

    [SerializeField] GameObject startStage;
    [SerializeField] GameObject startPosition;
    [SerializeField] GameObject jeongbiStage;
    Vector3 sp;
    void Start()
    {
        tileManager = FindObjectOfType<TileManager>();
        enemySpawnLogic = FindObjectOfType<EnemySpawnLogic>();
        mapSize = tileManager.GetMapSize;
        enemyMap = new EnemyType[mapSize, mapSize];
        InitializeEnemyArray();
        
        sp = startPosition.transform.position;

        StartCoroutine(StartMap());
        //StartCoroutine(RunWaves()); //나중에 GameManager로 옮길 것
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
            
            yield return new WaitForSeconds(3f);
            yield return StartCoroutine(Wave6());
            yield return StartCoroutine(WaveEnd());
            yield return new WaitForSeconds(3f);
            yield return StartCoroutine(Maintenance());
            yield return StartCoroutine(WaveEnd());
            yield return new WaitForSeconds(3f);
            yield return StartCoroutine(Wave8());
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
    IEnumerator StartMap() //향후 게임매니저에서 관리
    {
        yield return new WaitForSeconds(1f);
        startPosition.transform.position = sp;
        tileManager.InitializeArray(-1);
        yield return StartCoroutine(tileManager.MoveTilesByArray(0,0,0));
        startStage.SetActive(true);

        //시작 스테이지에서만 필요로 하는 코드 (영구적 업그레이드, 무기 선택, 훈련장 등등)을 작성

       //플레이어가 게임 시작할때까지 무한 대기
        while (!isGameStarted)
        {
            yield return new WaitForEndOfFrame();
        }

        //게임 시작 시 변이 선택 및 강화, 게임 시작 연출 재생
        if (true)
        {
            Tile[] tiles = startPosition.GetComponentsInChildren<Tile>();
            foreach(Tile t in tiles)
            {
                t.MovePosition(0, 10);
            }
            yield return new WaitForSeconds(9f);

            tileManager.InitializeArray(4);
            yield return StartCoroutine(tileManager.MoveTilesByArrayByWave(15, 12, 3, 1, 0));
            startStage.SetActive(false);
        }
        StartCoroutine(RunWaves());
    }
    IEnumerator Maintenance()
    {
        tileManager.InitializeArray(4);
        tileManager.MakeMapByCSV(jeongbiMapPath);
        yield return tileManager.MoveTilesByArrayByWave(15,15,3,1,0);
        jeongbiStage.SetActive(true);

        yield return new WaitForSeconds(10f); //플레이어 상호작용 코드 필요

        tileManager.InitializeArray(4);
        jeongbiStage.SetActive(false);
        yield return tileManager.MoveTilesByArrayByWave(15,8,3,1,0);
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
    IEnumerator Wave6()
    {
        Debug.Log("Wave 6");
        tileManager.InitializeArray();
        tileManager.MakeMapByCSV(mapPaths[0]);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        InitializeEnemyArray();
        MakeRandomEnemyMap(5);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        yield return new WaitForSeconds(5f);
        Debug.Log("Wave End");
    }
    IEnumerator Wave7()
    {
        Debug.Log("Wave 7");
        tileManager.InitializeArray();
        tileManager.MakeMapByCSV(mapPaths[1]);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        InitializeEnemyArray();
        MakeRandomEnemyMap(5);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        yield return new WaitForSeconds(5f);
        Debug.Log("Wave End");
    }
    IEnumerator Wave8()
    {
        Debug.Log("Wave 8");
        tileManager.InitializeArray();
        tileManager.MakeMapByCSV(mapPaths[2]);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        InitializeEnemyArray();
        MakeRandomEnemyMap(5);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        yield return new WaitForSeconds(5f);
        Debug.Log("Wave End");
    }

    IEnumerator WaveEnd()
    {
        tileManager.InitializeArray(4);
        yield return StartCoroutine(tileManager.MoveTilesByArray(2,2,0));
    }
}
