using JetBrains.Annotations;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Unity.Mathematics;
using DG.Tweening;
using static UnityEngine.EventSystems.EventTrigger;

public class WaveManager : MonoBehaviour
{
    UpgradeManager_New upgradeManager;
    TileManager tileManager;
    EnemySpawnLogic enemySpawnLogic;
    EnemyType[,] enemyMap;
    int mapSize;
    public int currentStage;
    public int currentWave;

    [Header("Data")]
    [SerializeField] EnemyCountData enemyCountData;
    [SerializeField] PlayerPositionData playerPositionData;
    [SerializeField] Material[] skyboxMaterials;
    [SerializeField] Material defaultSkybox;
    [SerializeField] GameObject footHold;
    [SerializeField] GameObject item;
    public float HP_enforceRate = 1.0f;
    public float ATK_enforceRate = 1.0f;
    bool nextWaveTrigger = false;
    public bool NextWaveTrigger
    {
        set { nextWaveTrigger = value; }
    }

    [Header("Map")]
    [SerializeField] int[] stageMapNum;
    [SerializeField] string startMapPath;
    [SerializeField] string jeongbiMapPath;

    [SerializeField] GameObject startStage;
    [SerializeField] GameObject startPosition;
    [SerializeField] GameObject jeongbiStage; 
    Vector3 sp;

    [Header("Item")]
    [SerializeField] GameObject upgradeUI;

    WaveData waveData;
    bool isMissionEnd;

    [Header("Debug")]
    [SerializeField] int debugStage;

    void Start()
    {
        upgradeManager = FindObjectOfType<UpgradeManager_New>();
        tileManager = FindObjectOfType<TileManager>();
        enemySpawnLogic = FindObjectOfType<EnemySpawnLogic>();
        mapSize = tileManager.GetMapSize;
        currentStage = 1;
        debugStage = 1;
        enemyMap = new EnemyType[mapSize, mapSize];
        InitializeEnemyArray();
        
        sp = startPosition.transform.position;

        StartCoroutine(StartMap());
        InvokeRepeating("UpdateLighting", 0, 0.5f);
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

    void UpdateLighting()
    {
        DynamicGI.UpdateEnvironment();
        
       //if (currentStage == 1) RenderSettings.ambientIntensity = Mathf.PingPong(Time.time, 1);
    }

    void ChangeSkyBox(float duration = 3f)
    {
        Color skycolor = skyboxMaterials[currentStage - 1].GetColor("_SkyColor");
        Color Horizon = skyboxMaterials[currentStage - 1].GetColor("_HorizonColor");
        float horizonStrength = skyboxMaterials[currentStage - 1].GetFloat("_HorizonStrength");
        float horizonHeight = skyboxMaterials[currentStage - 1].GetFloat("_HorizonHeight");
        defaultSkybox.DOColor(skycolor, "_SkyColor", duration);
        defaultSkybox.DOColor(Horizon, "_HorizonColor", duration);
        defaultSkybox.DOFloat(horizonStrength, "_HorizonStrength", duration);
        defaultSkybox.DOFloat(horizonHeight, "_HorizonHeight", duration);
    }

    IEnumerator RunStage()
    {
        yield return new WaitForSeconds(1f);
        int prevWave = -1;
        UIManager.instance.isStarted = true;
        for(currentStage = debugStage; currentStage <= 4; currentStage++)
        {
            int mapMaxIdx = stageMapNum[currentStage - 1];
            ChangeSkyBox();
            for (int i = 1; i <= 5; i++)
            {
                currentWave = i;
                int randNum = Random.Range(1, mapMaxIdx + 1);
                int cnt = 0;
                while (prevWave == randNum && cnt++ < 20) randNum = Random.Range(1, mapMaxIdx + 1);

                LoadWaveData($"{currentStage}-{randNum}");
                LoadWaveData($"{currentStage}-boss");
                yield return StartCoroutine(RunWave());
                yield return new WaitForSeconds(0.5f);
                prevWave = randNum;
            }
            //정비 스테이지
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(Maintenance());
            yield return new WaitForSeconds(0.5f);

            for (int i = 6; i <= 9; i++)
            {
                currentWave = i;
                int randNum = Random.Range(1, mapMaxIdx + 1);
                int cnt = 0;
                while (prevWave == randNum && cnt++ < 20) randNum = Random.Range(1, mapMaxIdx + 1);

                LoadWaveData($"{currentStage}-{randNum}");
                //LoadWaveData($"{2}-{7}");
                yield return StartCoroutine(RunWave());
                yield return new WaitForSeconds(0.5f);
                prevWave = randNum;
            }
            //보스전
            LoadWaveData($"{currentStage}-boss");
            yield return StartCoroutine(RunWave());
            yield return new WaitForSeconds(0.5f);

            yield return new WaitForSeconds(0.5f);
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

        tileManager.InitializeArray(1,4);
        yield return StartCoroutine(tileManager.MoveTilesByArrayByWave(22, 19, 0, 1, 0));
        startStage.SetActive(false);
        
        StartCoroutine(RunStage());
        yield break;
    }
    IEnumerator Maintenance()
    {
        UIManager.instance.changeWaveText(currentStage.ToString() + "-<color=green>M");

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
  
    IEnumerator RunWave()
    {
        isMissionEnd = false;
        //UI작업
        if(waveData.mission.type.CompareTo("Boss") == 0) UIManager.instance.changeWaveText(currentStage.ToString() + "-<color=red>X");
        else UIManager.instance.changeWaveText(currentStage.ToString() + "-" + currentWave.ToString());
        //맵 불러오기
        tileManager.InitializeArray(currentStage);
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        Vector2Int tmp = new Vector2Int(playerPos.x, playerPos.y);
        Vector2Int basePos = tileManager.MakeCenteredMapFromCSV(waveData.maps[0].file, playerPos.x, playerPos.y);
        yield return StartCoroutine(tileManager.MoveTilesByArray());
        yield return new WaitForSeconds(1f);
        //적 소환
        if (waveData.isRandomPos)
        {
            foreach (var enemy in waveData.enemies)
            {
                StartCoroutine(SummonRandomPosEnemyCoroutine(basePos, enemy));
            }
        }
        else
        {
            foreach (var enemy in waveData.enemies)
            {
                StartCoroutine(SummonEnemyCoroutine(basePos, enemy));
            }
        }
        
        //임무
        switch (waveData.mission.type)
        {
            case "Killing": StartCoroutine(KillingMission(waveData.mission.count)); break;
            case "Boss": StartCoroutine(BossMission(waveData.mission.count, true)); break;
            case "Survive": StartCoroutine(SurviveMission(waveData.mission.time)); break;
            case "Capture": StartCoroutine(CaptureMission(waveData.mission.time, basePos)); break;
            case "Item": StartCoroutine(ItemMission(basePos)); break;
        }

        //멀티 맵일시 맵 변경
        Coroutine mapChanging = null;
        if (waveData.isMultiMap) mapChanging = StartCoroutine(MultiMapsChangingCoroutine(tmp, waveData.isRepeating));
        //이벤트
        foreach (var ev in waveData.events)
        {
            switch (ev.type)
            {
                case "Building": StartCoroutine(WallCrisis(ev.repeat, ev.delay, ev.count)); break;
                case "SinkHole": StartCoroutine(HoleCrisis(ev.repeat, ev.delay, ev.count)); break;
                case "Spike": StartCoroutine(SpikeCrisis(ev.repeat, ev.delay, ev.count)); break;
                default: Debug.LogError("Wrong Event Type"); break;
            }
        }
        //임무 완료시 초기화
        while (!isMissionEnd) { yield return new WaitForEndOfFrame(); };

        if(mapChanging != null) StopCoroutine(mapChanging);
        StopCoroutine("SummonEnemyCoroutine");
        MonsterBase[] monsterBases = FindObjectsOfType<MonsterBase>();
        foreach (MonsterBase monster in monsterBases)
        {
            monster.summonedMonster = true;
            monster.TakeDamage(9999f, false);
        }
        yield return new WaitForSeconds(2);
        monsterBases = FindObjectsOfType<MonsterBase>();
        foreach (MonsterBase monster in monsterBases)
        {
            monster.summonedMonster = true;
            monster.TakeDamage(9999f, false);
        }
        yield return StartCoroutine(WaveEnd());
        yield break;
    }

    IEnumerator MultiMapsChangingCoroutine(Vector2Int basePos, bool isRepeating)
    {
        yield return new WaitForSeconds(waveData.maps[0].duration);
        do
        {
            for (int i = 1; i < waveData.maps.Count; i++)
            {
                tileManager.MakeCenteredMapFromCSV(waveData.maps[i].file, basePos.x, basePos.y);
                yield return StartCoroutine(tileManager.MoveTilesByArray());
                yield return new WaitForSeconds(waveData.maps[i].duration);
            }
        } while (isRepeating);
        
    }
    IEnumerator SummonEnemyCoroutine(Vector2Int basePos, EnemyInfo enemy)
    {
        yield return new WaitForSeconds(enemy.firstDelay); //첫 스폰 지연시간

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
            if (isMissionEnd) yield break;
            foreach(Vector2Int spawnpoint in spawnPoints)
            {
                enemySpawnLogic.SpawnEnemy(spawnpoint.x, spawnpoint.y, enemyType);
                count--;
                if(count == 0) break;
            }
            yield return new WaitForSeconds(enemy.spawnDelay);
        }
    }
    IEnumerator SummonRandomPosEnemyCoroutine(Vector2Int basePos, EnemyInfo enemy)
    {
        //적 스폰 위치 표시
        int count = enemy.count;
        EnemyType enemyType = enemy.type;
        while (count > 0)
        {
            if (isMissionEnd) yield break;
            Vector2Int randomPos = new Vector2Int(Random.Range(0, mapSize), Random.Range(0, mapSize));
            while (tileManager.GetTileMap[randomPos.y, randomPos.x] <= 0)
            {
                randomPos = new Vector2Int(Random.Range(0, mapSize), Random.Range(0, mapSize));
            }
            
            enemySpawnLogic.SpawnEnemy(randomPos.x, randomPos.y, enemyType);
            count--;
            if (count == 0) break;
            
            yield return new WaitForSeconds(enemy.spawnDelay);
        }
    }
    IEnumerator KillingMission(int count, bool isBoss = false)
    {
        enemyCountData.enemyCount = count;
        UIManager.instance.KillingMissionStart(isBoss);
        while (enemyCountData.enemyCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        isMissionEnd = true;
    }
    IEnumerator BossMission(int count, bool isBoss = true)
    {
        UIManager.instance.KillingMissionStart(isBoss);
        while (!UIManager.instance.isAllBossKilled)
        {
            yield return new WaitForEndOfFrame();
        }
        isMissionEnd = true;
    }
    IEnumerator SurviveMission(float time)
    {
        UIManager.instance.SurviveMissionStart(time);
        while (UIManager.instance.time > 0f)
        {
            yield return new WaitForEndOfFrame();
        }
        isMissionEnd = true;
    }
    IEnumerator CaptureMission(float time, Vector2Int basePos)
    {
        MissionInfo info = waveData.mission;
        UIManager.instance.CaptureMissionStart();
        Vector2Int pos = basePos + new Vector2Int(info.footholdPoint.x, info.footholdPoint.y);
        Vector3 realPos = tileManager.GetTiles[pos.y, pos.x].transform.position;
        Vector2 size = new Vector2(info.footholdSize.x, info.footholdSize.y);
        FootHold fh = Instantiate(footHold, new Vector3(realPos.x, info.footholdHeight, realPos.z), quaternion.identity).GetComponent<FootHold>();
        fh.gameObject.transform.localScale = new Vector3(size.x, 0.5f, size.y);
        if(tileManager.GetTiles[pos.y, pos.x].gameObject.activeSelf) fh.transform.parent = tileManager.GetTiles[pos.y, pos.x].transform;
        fh.maxTime = time;
        while (fh.progress < 1)
        {
            yield return new WaitForEndOfFrame();
        }
        isMissionEnd = true;
        Destroy(fh.gameObject, 2f);
    }
    IEnumerator ItemMission(Vector2Int basePos, float time = 6f)
    {
        MissionInfo info = waveData.mission;
        UIManager.instance.ItemMissionStart(info.count);
        Stack<Item> itemStk = new Stack<Item>();

        foreach(var posData in info.itemPoints)
        {
            Vector2Int pos = basePos + new Vector2Int(posData.x, posData.y);
            Vector3 realPos = tileManager.GetTiles[pos.y, pos.x].transform.position + new Vector3(0, tileManager.GetTiles[pos.y, pos.x].transform.localScale.y /2 + 1.5f, 0);
            Item it = Instantiate(item,realPos, quaternion.identity).GetComponent<Item>();
            it.maxTime = time;
            itemStk.Push(it);
        }

        while (UIManager.instance.itemCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        isMissionEnd = true;
        int c = itemStk.Count;
        for(int i=0; i< c; i++)
        {
            Destroy(itemStk.Pop().gameObject, 2f);
        }
    }
    //IEnumerator Stage1Boss()
    //{
    //    Debug.Log("Stage1_Wave");
    //    UIManager.instance.changeWaveText(currentStage.ToString() + "- <color=red>B</color>");

    //    tileManager.InitializeArray(1);
    //    Vector2Int playerPos = playerPositionData.playerTilePosition;
    //    tileManager.MakeCenteredMapFromCSV(stage1MapPath[stage1MapPath.Length -1], playerPos.x, playerPos.y);
    //    yield return StartCoroutine(tileManager.MoveTilesByArray());
    //    yield return new WaitForSeconds(1f);

    //    Debug.Log("Setting Monsters");
    //    InitializeEnemyArray();
    //    enemySpawnLogic.SpawnBoss(playerPos.x, playerPos.y, 0);
    //    enemyCountData.enemyCount = 1;
    //    UIManager.instance.KillingMissionStart();
    //    while (enemyCountData.enemyCount > 0)
    //    {
    //        yield return new WaitForEndOfFrame();
    //    }

    //    Debug.Log("Wave End");
    //}


    IEnumerator WaveEnd()
    {
        UIManager.instance.MissionComplete();
        StopCoroutine("WallCrisis");
        StopCoroutine("HoleCrisis");
        StopCoroutine("SpikeCrisis");

        //Item[] items = FindObjectsOfType<Item>();
        //foreach(Item item in items) { 
        //    item.isChasing = true;
        //    item.velocity *= 2;
        //}
        tileManager.InitializeArray(currentStage,4);
        yield return StartCoroutine(tileManager.MoveTilesByArray(0,2,0));

        //upgradeManager.RepeatNumSet(earnedCommonItems.Count,earnedRareItems.Count,earnedEpicItems.Count);
        //if(earnedCommonItems.Count > 0) upgradeManager.UpgradeDisplay(1);
        //else if (earnedRareItems.Count > 0) upgradeManager.UpgradeDisplay(2);
        //else if (earnedEpicItems.Count >0) upgradeManager.UpgradeDisplay(3);
        //StartCoroutine(upgradeManager.UpgradeDisplay(UpgradeTier.common));
        upgradeManager.BasicUpgradeCall();

        yield return null;
        while (upgradeManager.Upgrading)
        {
            yield return null;
        }

        yield return null;
    }

    #region CrisisEvent
    IEnumerator WallCrisis(int repeat, float cooltime, int wallCount)
    {
        for(int i = 0; i < repeat; i++)
        {
            if (isMissionEnd) break;

            tileManager.MakeRandomWall(wallCount);
            yield return StartCoroutine(tileManager.MoveTilesByArray());
            yield return new WaitForSeconds(cooltime);
        }
    }
    IEnumerator HoleCrisis(int repeat, float cooltime, int holeCount)
    {
        for (int i = 0; i < repeat; i++)
        {
            if (isMissionEnd) break;

            tileManager.MakeRandomHole(holeCount);
            yield return StartCoroutine(tileManager.MoveTilesByArray());
            yield return new WaitForSeconds(cooltime);
        }
    }
    IEnumerator SpikeCrisis(int repeat, float cooltime, int holeCount)
    {
        for (int i = 0; i < repeat; i++)
        {
            if (isMissionEnd) break;

            tileManager.MakeRandomSpike(holeCount);
            yield return StartCoroutine(tileManager.MoveTilesByArray());
            yield return new WaitForSeconds(cooltime);
        }
    }
    #endregion
    
}
