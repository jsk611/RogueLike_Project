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
                randNum = Random.Range(4, 7);
            }
            else randNum = Random.Range(1, 6);


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

        tileManager.InitializeArray(4);
        yield return StartCoroutine(tileManager.MoveTilesByArrayByWave(22, 19, 0, 1, 0));
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
        //???????? ???????? ???? ????

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

        //???? ?? ?????? ?????? ????
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
            //???????? ?? ????
            tileManager.InitializeArray(4);
            tileManager.MakeRandomWall(Random.Range(200, 300));
            yield return StartCoroutine(tileManager.MoveTilesByArray(0,1,1));
            yield return new WaitForSeconds(2.5f);
            if (enemyCountData.enemyCount == 0) break;
            yield return new WaitForSeconds(2.5f);
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
            tileManager.MakeRandomHole(Random.Range(75,100));
            yield return StartCoroutine(tileManager.MoveTilesByArray());
            yield return new WaitForSeconds(5f);
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
        tileManager.InitializeArray(4);
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
