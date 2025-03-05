using JetBrains.Annotations;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class WaveManager : MonoBehaviour
{
    UpgradeManager upgradeManager;
    TileManager tileManager;
    EnemySpawnLogic enemySpawnLogic;
    EnemyType[,] enemyMap;
    int mapSize;
    int currentStage;
    int currentWave;

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

    WaveData waveData;
    bool isMissionEnd;
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

        LoadWaveData("1-1");

        StartCoroutine(StartMap());
        //StartCoroutine(RunWaves()); //?????? GameManager?? ???? ??
    }

    #region WaveDataSetting
    void LoadWaveData(string fileTitle)
    {
        string path = Path.Combine(Application.streamingAssetsPath, $"WaveData/Wave_{fileTitle}.json");
        if (File.Exists(path))
        {
            string jsonText = File.ReadAllText(path);
            waveData = JsonUtility.FromJson<WaveData>(jsonText);
            ApplyWaveSettings();
        }
        else
        {
            Debug.LogError("파일 찾기 실패!: " + path);
        }
    }
    void ApplyWaveSettings()
    {
        Debug.Log($"Wave {waveData.waveIndex} Loaded.");

        if (waveData.isMultiMap)
        {
            Debug.Log("multiple maps.");
            foreach (var map in waveData.maps)
            {
                Debug.Log($"Map: {map.file}, Duration: {map.duration}");
            }
        }
        else
        {
            Debug.Log("single map: " + waveData.maps[0].file);
        }

        foreach (var enemy in waveData.enemies)
        {
            Debug.Log($"Spawn {enemy.count} {enemy.type}(s) at {string.Join(", ", enemy.spawnPoints)}");
        }
    }
    #endregion

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
        Array allEnemy = Enum.GetValues(typeof(EnemyType));
        List<int> enemyPool1 = new List<int>(); 
        List<int> enemyPool2 = new List<int>();
        foreach (int enemyNum in allEnemy)
        {
            if(enemyNum/100 <= currentStage)
            {
                if(enemyNum%100/10 <= 1) enemyPool1.Add(enemyNum);
                if(enemyNum%100/10 >= 1) enemyPool2.Add(enemyNum);
            }
        }
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
                randNum = enemyPool2[Random.Range(0, enemyPool2.Count)];
            }
            else randNum = enemyPool1[Random.Range(0, enemyPool1.Count)];


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
        for (int i = 0; i < 4; i++)
        {
            currentWave = i + 1;
            int randNum = Random.Range(0, mapMaxIdx);
            while (prevWave == randNum) randNum = Random.Range(0, mapMaxIdx);
            yield return StartCoroutine(Stage1Wave(randNum));
            yield return StartCoroutine(WaveEnd());
            yield return new WaitForSeconds(0.5f);
            prevWave = randNum;
        }
        yield return StartCoroutine(Stage1Boss());
        yield return StartCoroutine(WaveEnd());
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
        for (int i = 0; i < 4; i++)
        {
            currentWave = i + 1;
            int randNum = Random.Range(0, mapMaxIdx);
            while (prevWave == randNum) randNum = Random.Range(0, mapMaxIdx);
            yield return StartCoroutine(Stage2Wave(randNum));
            yield return StartCoroutine(WaveEnd());
            yield return new WaitForSeconds(0.5f);
            prevWave = randNum;
        }
        yield return StartCoroutine(Maintenance());
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(RunStage3());
    }
    IEnumerator RunStage3() //미구현
    {
        currentStage = 3;
        yield return new WaitForSeconds(1f);
        int prevWave = -1;
        int mapMaxIdx = stage3MapPath.Length - 1;
        for (int i = 0; i < 5; i++)
        {
            currentWave = i + 1;
            int randNum = Random.Range(0, mapMaxIdx);
            while (prevWave == randNum) randNum = Random.Range(0, mapMaxIdx);
            yield return StartCoroutine(Stage3Wave(randNum));
            yield return StartCoroutine(WaveEnd());
            yield return new WaitForSeconds(0.5f);
            prevWave = randNum;
        }
        yield return StartCoroutine(Maintenance());
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(RunStage3());
    }
    IEnumerator RunStage4() //미구현
    {
        currentStage = 4;
        yield return new WaitForSeconds(1f);
        int prevWave = -1;
        int mapMaxIdx = stage1MapPath.Length - 1;
        for (int i = 0; i < 5; i++)
        {
            currentWave = i + 1;
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
        
        StartCoroutine(RunWave());
        yield break;
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
        UIManager.instance.changeWaveText(currentStage.ToString() + "-" + currentWave.ToString());

        tileManager.InitializeArray(1);
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        tileManager.MakeCenteredMapFromCSV(stage1MapPath[mapIdx], playerPos.x, playerPos.y);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        Debug.Log("Setting Monsters");

        InitializeEnemyArray();
        MakeRandomEnemyMap(8);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);
        UIManager.instance.KillingMissionStart();

        while (enemyCountData.enemyCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Wave End");
    }
    IEnumerator RunWave()
    {
        //UI작업
        UIManager.instance.changeWaveText(currentStage.ToString() + "-" + currentWave.ToString());
        //맵 불러오기
        tileManager.InitializeArray(1);
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        Vector2Int basePos = tileManager.MakeCenteredMapFromCSV(waveData.maps[0].file, playerPos.x, playerPos.y);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);
        //적 소환
        foreach(var enemy in waveData.enemies)
        {
            StartCoroutine(SummonEnemyCoroutine(basePos, enemy));
        }
        //임무
        isMissionEnd = false;
        switch (waveData.mission.type)
        {
            case "Killing": StartCoroutine(KillingMission(waveData.mission.count)); break;
        }

        //멀티 맵일시 맵 변경
        Coroutine mapChanging = null;
        if (waveData.isMultiMap) mapChanging = StartCoroutine(MultiMapsChangingCoroutine(basePos));
        //이벤트
        foreach (var ev in waveData.events)
        {
            switch (ev.type)
            {
                case "Building": StartCoroutine(WallCrisis(6, 10f, 100)); break;
                default: Debug.LogError("Wrong Event Type"); break;
            }
        }
        //임무 완료시 초기화
        while (!isMissionEnd) { yield return new WaitForEndOfFrame(); };
        StopCoroutine(mapChanging);
        StopCoroutine("SummonEnemyCoroutine");
        StartCoroutine(WaveEnd());
        yield break;
    }

    IEnumerator MultiMapsChangingCoroutine(Vector2Int basePos)
    {
        yield return new WaitForSeconds(waveData.maps[0].duration);
        for(int i=1; i<waveData.maps.Count; i++)
        {
            tileManager.MakeCenteredMapFromCSV(waveData.maps[i].file, basePos.x, basePos.y);
            yield return StartCoroutine(tileManager.MoveTilesByArray());
            yield return new WaitForSeconds(waveData.maps[i].duration);
        }
    }
    IEnumerator SummonEnemyCoroutine(Vector2Int basePos, EnemyInfo enemy)
    {
        //적 스폰 위치 표시
        
        List<Vector2Int> spawnPoints = new List<Vector2Int>();
        foreach(var spawnpoint in enemy.spawnPoints)
        {
            spawnPoints.Add(basePos + new Vector2Int(spawnpoint.x, spawnpoint.y));
        }

        int count = enemy.count;
        EnemyType enemyType = enemy.type;
        while(count > 0)
        {
            foreach(Vector2Int spawnpoint in spawnPoints)
            {
                enemySpawnLogic.SpawnEnemy(spawnpoint.x, spawnpoint.y, enemyType);
                count--;
                if(count == 0) break;
            }
            yield return new WaitForSeconds(enemy.spawnDelay);
        }
    }
    IEnumerator KillingMission(int count)
    {
        enemyCountData.enemyCount = count;
        UIManager.instance.KillingMissionStart();
        while (enemyCountData.enemyCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator Stage1Boss()
    {
        Debug.Log("Stage1_Wave");
        UIManager.instance.changeWaveText(currentStage.ToString() + "- <color=red>B</color>");

        tileManager.InitializeArray(1);
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        tileManager.MakeCenteredMapFromCSV(stage1MapPath[stage1MapPath.Length -1], playerPos.x, playerPos.y);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        Debug.Log("Setting Monsters");
        InitializeEnemyArray();
        enemySpawnLogic.SpawnBoss(playerPos.x, playerPos.y, 0);
        enemyCountData.enemyCount = 1;
        UIManager.instance.KillingMissionStart();
        while (enemyCountData.enemyCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Wave End");
    }
    IEnumerator Stage2Wave(int mapIdx)
    {
        Debug.Log("Stage2_Wave");
        UIManager.instance.changeWaveText(currentStage.ToString() + "-" + currentWave.ToString());

        tileManager.InitializeArray(2);
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        tileManager.MakeCenteredMapFromCSV(stage2MapPath[mapIdx], playerPos.x, playerPos.y);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        Debug.Log("Setting Monsters");

        InitializeEnemyArray();
        MakeRandomEnemyMap(12);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);
        UIManager.instance.KillingMissionStart();
        while (enemyCountData.enemyCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Wave End");
    }
    IEnumerator Stage3Wave(int mapIdx)
    {
        Debug.Log("Stage3_Wave");
        UIManager.instance.changeWaveText(currentStage.ToString() + "-" + currentWave.ToString());

        tileManager.InitializeArray(3);
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        tileManager.MakeCenteredMapFromCSV(stage3MapPath[mapIdx], playerPos.x, playerPos.y);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);

        Debug.Log("Setting Monsters");

        InitializeEnemyArray();
        MakeRandomEnemyMap(12);
        enemySpawnLogic.SpawnEnemyByArray(enemyMap);
        UIManager.instance.KillingMissionStart();

        //랜덤 위기 시스템
        int randnum = Random.Range(1, 4);
        switch (randnum)
        {
            case 1: StartCoroutine(WallCrisis(6, 10f, 100)); break;
            case 2: StartCoroutine(HoleCrisis(6, 8f, 120)); break;
            case 3: StartCoroutine(SpikeCrisis(6, 8f, 50)); break;
        }

        while (enemyCountData.enemyCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Wave End");
    }

    IEnumerator WaveEnd()
    {
        UIManager.instance.MissionComplete();
        StopCoroutine("WallCrisis");
        StopCoroutine("HoleCrisis");
        StopCoroutine("SpikeCrisis");
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

    IEnumerator WallCrisis(int repeat, float cooltime, int wallCount)
    {
        for(int i = 0; i < repeat; i++)
        {
            if (enemyCountData.enemyCount <= 0) break;

            tileManager.MakeRandomWall(wallCount);
            yield return StartCoroutine(tileManager.MoveTilesByArray());
            yield return new WaitForSeconds(cooltime);
        }
    }
    IEnumerator HoleCrisis(int repeat, float cooltime, int holeCount)
    {
        for (int i = 0; i < repeat; i++)
        {
            if (enemyCountData.enemyCount <= 0) break;

            tileManager.MakeRandomHole(holeCount);
            yield return StartCoroutine(tileManager.MoveTilesByArray());
            yield return new WaitForSeconds(cooltime);
        }
    }
    IEnumerator SpikeCrisis(int repeat, float cooltime, int holeCount)
    {
        for (int i = 0; i < repeat; i++)
        {
            if (enemyCountData.enemyCount <= 0) break;

            tileManager.MakeRandomSpike(holeCount);
            yield return StartCoroutine(tileManager.MoveTilesByArray());
            yield return new WaitForSeconds(cooltime);
        }
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
