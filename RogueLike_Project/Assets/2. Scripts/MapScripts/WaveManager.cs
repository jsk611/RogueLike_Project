using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    UpgradeManager upgradeManager;
    TileManager tileManager;
    EnemySpawnLogic enemySpawnLogic;
    EnemyType[,] enemyMap;
    int mapSize;
    int currentStage;

    [Header("Data")]
    [SerializeField] EnemyCountData enemyCountData;
    [SerializeField] PlayerPositionData playerPositionData;
    bool nextWaveTrigger = false;
    public bool NextWaveTrigger
    {
        set { nextWaveTrigger = value; }
    }

    [Header("Map")]
    [SerializeField] string[] mapPaths;
    [SerializeField] string[] stage1MapPath;
    [SerializeField] string[] stage2MapPath;
    [SerializeField] string[] stage3MapPath;
    [SerializeField] string[] stage4MapPath;
    [SerializeField] string startMapPath;
    [SerializeField] string jeongbiMapPath;

    [SerializeField] GameObject startStage;
    [SerializeField] GameObject startPosition;
    [SerializeField] GameObject jeongbiStage; 
    Vector3 sp;

    [Header("Item")]
    Queue<int> earnedCommonItems = new Queue<int>();
    Queue<int> earnedRareItems = new Queue<int>();
    Queue<int> earnedEpicItems = new Queue<int>();
    [SerializeField] GameObject upgradeUI;

    void Start()
    {
        upgradeManager = FindObjectOfType<UpgradeManager>();
        tileManager = FindObjectOfType<TileManager>();
        enemySpawnLogic = FindObjectOfType<EnemySpawnLogic>();
        mapSize = tileManager.GetMapSize;
        currentStage = 1;
        enemyMap = new EnemyType[mapSize, mapSize];
        InitializeEnemyArray();
        
        sp = startPosition.transform.position;

        StartCoroutine(StartMap());
        //StartCoroutine(RunWaves()); //?????? GameManager?? ???? ??
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

            int randNum;
            if (tileManager.IsHighPos(y, x))
            {
                randNum = Random.Range(6, 10);
            }
            else randNum =  Random.Range(1, 10);


            enemyMap[y,x] = (EnemyType)randNum;
        }
    }

    //IEnumerator RunWaves()
    //{
    //    yield return new WaitForSeconds(1f);
    //    while (true)
    //    {
    //        int prevWave = -1;
    //        for (int i=0; i<5; i++)
    //        {
    //            int randNum = Random.Range(0, 4);
    //            while(prevWave == randNum) randNum = Random.Range(0, 4);
    //            yield return StartCoroutine(Stage1Wave(randNum));
    //            yield return StartCoroutine(WaveEnd());
    //            yield return new WaitForSeconds(0.5f);
    //            prevWave = randNum;
    //        }
    //        yield return StartCoroutine(Maintenance());
    //        yield return new WaitForSeconds(0.5f);
    //    }
    //}
    IEnumerator RunStage1()
    {
        currentStage = 1;
        yield return new WaitForSeconds(1f);
        int prevWave = -1;
        int mapMaxIdx = stage1MapPath.Length -1;
        for (int i = 0; i < 1; i++)
        {
            int randNum = Random.Range(0, mapMaxIdx);
            while (prevWave == randNum) randNum = Random.Range(0, mapMaxIdx);
            yield return StartCoroutine(Stage1Wave(randNum));
            yield return StartCoroutine(WaveEnd());
            yield return new WaitForSeconds(0.5f);
            prevWave = randNum;
        }
        yield return StartCoroutine(Stage1Boss());
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(Maintenance());
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(RunStage2());
    }
    IEnumerator RunStage2()
    {
        currentStage = 2;
        yield return new WaitForSeconds(1f);
        int prevWave = -1;
        int mapMaxIdx = stage2MapPath.Length - 1;
        for (int i = 0; i < 5; i++)
        {
            int randNum = Random.Range(0, mapMaxIdx);
            while (prevWave == randNum) randNum = Random.Range(0, mapMaxIdx);
            yield return StartCoroutine(Stage2Wave(randNum));
            yield return StartCoroutine(WaveEnd());
            yield return new WaitForSeconds(0.5f);
            prevWave = randNum;
        }
        yield return StartCoroutine(Maintenance());
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(RunStage2());
    }
    IEnumerator RunStage3() //미구현
    {
        currentStage = 3;
        yield return new WaitForSeconds(1f);
        int prevWave = -1;
        int mapMaxIdx = stage1MapPath.Length - 1;
        for (int i = 0; i < 5; i++)
        {
            int randNum = Random.Range(0, mapMaxIdx);
            while (prevWave == randNum) randNum = Random.Range(0, mapMaxIdx);
            yield return StartCoroutine(Stage1Wave(randNum));
            yield return StartCoroutine(WaveEnd());
            yield return new WaitForSeconds(0.5f);
            prevWave = randNum;
        }
        yield return StartCoroutine(Maintenance());
        yield return new WaitForSeconds(0.5f);
    }
    IEnumerator RunStage4() //미구현
    {
        currentStage = 4;
        yield return new WaitForSeconds(1f);
        int prevWave = -1;
        int mapMaxIdx = stage1MapPath.Length - 1;
        for (int i = 0; i < 5; i++)
        {
            int randNum = Random.Range(0, mapMaxIdx);
            while (prevWave == randNum) randNum = Random.Range(0, mapMaxIdx);
            yield return StartCoroutine(Stage1Wave(randNum));
            yield return StartCoroutine(WaveEnd());
            yield return new WaitForSeconds(0.5f);
            prevWave = randNum;
        }
        yield return StartCoroutine(Maintenance());
        yield return new WaitForSeconds(0.5f);
    }
    IEnumerator StartMap() 
    {
        tileManager.MakeMapByCSV(startMapPath, 7, 7);
        yield return StartCoroutine(tileManager.MoveTilesByArray(0, 0, 0));
        yield return new WaitForSeconds(1f);
        startPosition.transform.position = sp;

        startStage.SetActive(true);


        while (!nextWaveTrigger)
        {
            yield return new WaitForEndOfFrame();
        }

        nextWaveTrigger = false;
        startStage.SetActive(false);

        tileManager.InitializeArray(1,4);
        yield return StartCoroutine(tileManager.MoveTilesByArrayByWave(22, 19, 0, 1, 0));
        startStage.SetActive(false);
        
        StartCoroutine(RunStage1());
    }
    IEnumerator Maintenance()
    {
        tileManager.InitializeArray(currentStage, 4);
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        Vector2Int stagePos = tileManager.MakeCenteredMapFromCSV(jeongbiMapPath, playerPos.x, playerPos.y);
        yield return tileManager.MoveTilesByArrayByWave(playerPos.x, playerPos.y, 1.5f,1,0);
        jeongbiStage.SetActive(true);
        jeongbiStage.transform.position = tileManager.GetTiles[stagePos.y, stagePos.x].transform.position;
        jeongbiStage.transform.position = new Vector3(jeongbiStage.transform.position.x, 0, jeongbiStage.transform.position.z);

        while (!nextWaveTrigger) 
        {
            yield return new WaitForEndOfFrame();
        }

        nextWaveTrigger= false;
        tileManager.InitializeArray(currentStage, 4);
        jeongbiStage.SetActive(false);
        playerPos = playerPositionData.playerTilePosition;
        yield return tileManager.MoveTilesByArrayByWave(playerPos.x, playerPos.y, 1.5f,1,0);
    }
    IEnumerator Stage1Wave(int mapIdx)
    {
        Debug.Log("Stage1_Wave");
        tileManager.InitializeArray(1);
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        tileManager.MakeCenteredMapFromCSV(stage1MapPath[mapIdx], playerPos.x, playerPos.y);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        Debug.Log("Setting Monsters");

        InitializeEnemyArray();
        MakeRandomEnemyMap(8);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        while (enemyCountData.enemyCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Wave End");
    }
    IEnumerator Stage1Boss() //미구현
    {
        Debug.Log("Stage1_Wave");
        tileManager.InitializeArray(1);
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        tileManager.MakeCenteredMapFromCSV(stage1MapPath[stage1MapPath.Length -1], playerPos.x, playerPos.y);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        Debug.Log("Setting Monsters");
        InitializeEnemyArray();
        enemySpawnLogic.SpawnBoss(playerPos.x, playerPos.y, 0);
        enemyCountData.enemyCount = 1;
        while (enemyCountData.enemyCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Wave End");
    }
    IEnumerator Stage2Wave(int mapIdx)
    {
        Debug.Log("Stage1_Wave");
        tileManager.InitializeArray(2);
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        tileManager.MakeCenteredMapFromCSV(stage2MapPath[mapIdx], playerPos.x, playerPos.y);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        Debug.Log("Setting Monsters");

        InitializeEnemyArray();
        MakeRandomEnemyMap(12);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);

        while (enemyCountData.enemyCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Wave End");
    }
    
    
    IEnumerator WaveEnd()
    {
        Item[] items = FindObjectsOfType<Item>();
        foreach(Item item in items) { 
            item.isChasing = true;
            item.velocity *= 2;
        }
        tileManager.InitializeArray(currentStage,4);
        yield return StartCoroutine(tileManager.MoveTilesByArray(0,2,0));

        upgradeManager.RepeatNumSet(earnedCommonItems.Count,earnedRareItems.Count,earnedEpicItems.Count);
        if(earnedCommonItems.Count > 0) upgradeManager.UpgradeDisplay(1);
        else if (earnedRareItems.Count > 0) upgradeManager.UpgradeDisplay(2);
        else if (earnedEpicItems.Count >0) upgradeManager.UpgradeDisplay(3);

        yield return null;
        while (upgradeManager.UIenabled)
        {
            yield return null;
        }
        earnedCommonItems = new Queue<int>();
        earnedRareItems = new Queue<int>();
        earnedEpicItems = new Queue<int>();
        yield return null;
    }

    public void AddItem(int star)
    {
        if (star == 1)
            earnedCommonItems.Enqueue(star);
        else if (star == 2)
            earnedRareItems.Enqueue(star);
        else if (star == 3)
            earnedEpicItems.Enqueue(star);
    }
}
