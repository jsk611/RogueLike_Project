using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    TileManager tileManager;
    EnemySpawnLogic enemySpawnLogic;
    EnemyType[,] enemyMap;
    int mapSize;

    [SerializeField] EnemyCountData enemyCountData;
    [SerializeField] PlayerPositionData playerPositionData;
    bool nextWaveTrigger = false;
    public bool NextWaveTrigger
    {
        set { nextWaveTrigger = value; }
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

            if (enemyMap[y,x] != EnemyType.None || tileManager.GetTileMap[y,x] <= 0)
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
            int prevWave = -1;
            for(int i=0; i<5; i++)
            {
                int randNum = Random.Range(1, 9);
                while(prevWave == randNum) randNum = Random.Range(1, 9);
                yield return StartCoroutine("Wave" + randNum.ToString());
                yield return StartCoroutine(WaveEnd());
                yield return new WaitForSeconds(0.5f);
                prevWave = randNum;
            }
            yield return StartCoroutine(Maintenance());
            yield return new WaitForSeconds(0.5f);
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
        while (!nextWaveTrigger)
        {
            yield return new WaitForEndOfFrame();
        }

        nextWaveTrigger = false;
        //게임 시작 시 변이 선택 및 강화, 게임 시작 연출 재생
        tileManager.MakeMapByCSV(startMapPath, 7,7);
        yield return StartCoroutine(tileManager.MoveTilesByArray(0, 0, 0));
        startStage.SetActive(false);
        
        //yield return new WaitForSeconds(9f);
        yield return new WaitForSeconds(0.1f);
        tileManager.InitializeArray(4);
        yield return StartCoroutine(tileManager.MoveTilesByArrayByWave(22, 19, 1.5f, 1, 0));
        startStage.SetActive(false);
        
        StartCoroutine(RunWaves());
    }
    IEnumerator Maintenance()
    {
        tileManager.InitializeArray(4);
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        Vector2Int stagePos = tileManager.MakeCenteredMapFromCSV(jeongbiMapPath, playerPos.x, playerPos.y);
        yield return tileManager.MoveTilesByArrayByWave(playerPos.x, playerPos.y, 1.5f,1,0);
        jeongbiStage.SetActive(true);
        jeongbiStage.transform.position = tileManager.GetTiles[stagePos.y, stagePos.x].transform.position;
        jeongbiStage.transform.position = new Vector3(jeongbiStage.transform.position.x, 0, jeongbiStage.transform.position.z);
        //플레이어 상호작용 코드 필요

        while (!nextWaveTrigger) 
        {
            yield return new WaitForEndOfFrame();
        }

        nextWaveTrigger= false;
        tileManager.InitializeArray(4);
        jeongbiStage.SetActive(false);
        playerPos = playerPositionData.playerTilePosition;
        yield return tileManager.MoveTilesByArrayByWave(playerPos.x, playerPos.y, 1.5f,1,0);
    }
    IEnumerator Wave1()
    {
        Debug.Log("Wave 1");
        tileManager.InitializeArray();
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        tileManager.MakeCenteredMapFromCSV(mapPaths[0], playerPos.x, playerPos.y);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        Debug.Log("Setting Monsters");

        InitializeEnemyArray();
        MakeRandomEnemyMap(5);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        //적이 다 처치될 때까지 대기
        while (enemyCountData.enemyCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Wave End");
    }
    IEnumerator Wave2()
    {
        Debug.Log("Wave 2");
        tileManager.InitializeArray();
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        tileManager.MakeCenteredMapFromCSV(mapPaths[1], playerPos.x, playerPos.y);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        InitializeEnemyArray();
        MakeRandomEnemyMap(5);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        while (enemyCountData.enemyCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Wave End");
    }
    IEnumerator Wave3()
    {
        Debug.Log("Wave 3");
        tileManager.InitializeArray();
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        tileManager.MakeCenteredMapFromCSV(mapPaths[2], playerPos.x, playerPos.y);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        InitializeEnemyArray();
        MakeRandomEnemyMap(5);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        while (enemyCountData.enemyCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Wave End");
    }
    IEnumerator Wave4()
    {
        Debug.Log("Wave 4");
        tileManager.InitializeArray();
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        tileManager.MakeCenteredMapFromCSV(mapPaths[3], playerPos.x, playerPos.y);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        InitializeEnemyArray();
        MakeRandomEnemyMap(5);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        while (enemyCountData.enemyCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Wave End");
    }
    IEnumerator Wave5()
    {
        Debug.Log("Wave 5");
        tileManager.InitializeArray();
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        tileManager.MakeCenteredMapFromCSV(mapPaths[4], playerPos.x, playerPos.y);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        InitializeEnemyArray();
        MakeRandomEnemyMap(5);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        while (enemyCountData.enemyCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Wave End");
    }

    IEnumerator Wave6()
    {
        Debug.Log("Wave 6");
        tileManager.InitializeArray();
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        tileManager.MakeCenteredMapFromCSV(mapPaths[5], playerPos.x, playerPos.y);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        InitializeEnemyArray();
        MakeRandomEnemyMap(7);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        while (enemyCountData.enemyCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Wave End");
    }

    IEnumerator Wave7()
    {
        InitializeEnemyArray();
        MakeRandomEnemyMap(7);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        while (enemyCountData.enemyCount > 0)
        {
            //랜덤으로 벽 생성
            tileManager.InitializeArray(4);
            tileManager.MakeRandomWall(Random.Range(250, 400));
            yield return StartCoroutine(tileManager.MoveTilesByArray(0,1,0));
            yield return new WaitForSeconds(2f);
            if (enemyCountData.enemyCount == 0) break;
            yield return new WaitForSeconds(2f);
            if (enemyCountData.enemyCount == 0) break;
            yield return new WaitForSeconds(2f);
        }

        Debug.Log("Wave End");
    }

    IEnumerator Wave8()
    {
        Debug.Log("Wave 8");
        yield return new WaitForSeconds(1f);

        InitializeEnemyArray();
        MakeRandomEnemyMap(7);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);
        while(enemyCountData.enemyCount > 0)
        {
            //랜덤으로 구멍 생성
            tileManager.MakeRandomHole(Random.Range(100,200));
            yield return StartCoroutine(tileManager.MoveTilesByArray());
            yield return new WaitForSeconds(5f);
        }


        Debug.Log("Wave End");
    }
    
    IEnumerator WaveEnd()
    {
        tileManager.InitializeArray(4);
        yield return StartCoroutine(tileManager.MoveTilesByArray(0,2,0));
        yield return new WaitForSeconds(2f);
    }
}
