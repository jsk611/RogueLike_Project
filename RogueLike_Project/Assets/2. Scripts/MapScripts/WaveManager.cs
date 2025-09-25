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
    public int monsterEnforceVar = 0;
    public float HP_enforceRate = 0.15f;
    public float ATK_enforceRate = 0.15f;
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

    [Header("Enhanced Debug System")]
    [SerializeField] bool isDebugMode = false;
    [SerializeField] bool showDebugGUI = true;
    [SerializeField] int debugStage = 1;
    [SerializeField] int debugWave = 1;
    [SerializeField] bool skipToDebugWave = false;
    [SerializeField] bool godMode = false;
    [SerializeField] bool instantKillEnemies = false;
    [SerializeField] bool skipUpgrade = false;
    [SerializeField] GameObject demoEndingUI;
    
    [Header("Debug Quick Actions")]
    [SerializeField] KeyCode nextWaveKey = KeyCode.N;
    [SerializeField] KeyCode prevWaveKey = KeyCode.B;
    [SerializeField] KeyCode nextStageKey = KeyCode.RightArrow;
    [SerializeField] KeyCode prevStageKey = KeyCode.LeftArrow;
    [SerializeField] KeyCode toggleGodModeKey = KeyCode.G;
    [SerializeField] KeyCode killAllEnemiesKey = KeyCode.K;

    // 디버그 상태 추적
    private bool debugInitialized = false;
    private string lastDebugAction = "";
    private float debugActionTime = 0f;

    void Start()
    {
        upgradeManager = FindObjectOfType<UpgradeManager_New>();
        tileManager = FindObjectOfType<TileManager>();
        enemySpawnLogic = FindObjectOfType<EnemySpawnLogic>();
        mapSize = tileManager.GetMapSize;
        currentStage = 1;
        enemyMap = new EnemyType[mapSize, mapSize];
        InitializeEnemyArray();
        
        sp = startPosition.transform.position;

        if (isDebugMode)
        {
            InitializeDebugSystem();
            if (skipToDebugWave)
            {
                StartCoroutine(StartDebugWave());
            }
            else
            {
                StartCoroutine(StartMap());
            }
        }
        else
        {
            StartCoroutine(StartMap());
        }
        InvokeRepeating("UpdateLighting", 0, 0.5f);
    }

    void Update()
    {
        if (isDebugMode)
        {
            HandleDebugInput();
        }
    }

    void InitializeDebugSystem()
    {
        debugInitialized = true;
        Debug.Log($"[DEBUG SYSTEM] Initialized - Target: Stage {debugStage}, Wave {debugWave}");
        
        if (godMode)
        {
            var playerStatus = FindObjectOfType<PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.SetHealth(9999f);
            }
        }
    }

    void HandleDebugInput()
    {
        if (Input.GetKeyDown(nextWaveKey))
        {
            DebugNextWave();
        }
        else if (Input.GetKeyDown(prevWaveKey))
        {
            DebugPreviousWave();
        }
        else if (Input.GetKeyDown(nextStageKey))
        {
            DebugNextStage();
        }
        else if (Input.GetKeyDown(prevStageKey))
        {
            DebugPreviousStage();
        }
        else if (Input.GetKeyDown(toggleGodModeKey))
        {
            ToggleGodMode();
        }
        else if (Input.GetKeyDown(killAllEnemiesKey))
        {
            KillAllEnemies();
        }
    }

    void DebugNextWave()
    {
        if (debugWave < 10)
        {
            debugWave++;
        }
        else
        {
            debugWave = 1;
            if (debugStage < 4) debugStage++;
        }
        
        LogDebugAction($"Next Wave -> Stage {debugStage}, Wave {debugWave}");
        RestartDebugWave();
    }

    void DebugPreviousWave()
    {
        if (debugWave > 1)
        {
            debugWave--;
        }
        else
        {
            debugWave = 10;
            if (debugStage > 1) debugStage--;
        }
        
        LogDebugAction($"Previous Wave -> Stage {debugStage}, Wave {debugWave}");
        RestartDebugWave();
    }

    void DebugNextStage()
    {
        if (debugStage < 4)
        {
            debugStage++;
            debugWave = 1;
            LogDebugAction($"Next Stage -> Stage {debugStage}, Wave {debugWave}");
            RestartDebugWave();
        }
    }

    void DebugPreviousStage()
    {
        if (debugStage > 1)
        {
            debugStage--;
            debugWave = 1;
            LogDebugAction($"Previous Stage -> Stage {debugStage}, Wave {debugWave}");
            RestartDebugWave();
        }
    }

    void ToggleGodMode()
    {
        godMode = !godMode;
        var playerStatus = FindObjectOfType<PlayerStatus>();
        if (playerStatus != null)
        {
            if (godMode)
            {
                playerStatus.SetHealth(9999f);
            }
            else
            {
                playerStatus.SetHealth(100f);
            }
        }
        LogDebugAction($"God Mode: {(godMode ? "ON" : "OFF")}");
    }

    void KillAllEnemies()
    {
        MonsterBase[] monsters = FindObjectsOfType<MonsterBase>();
        foreach (MonsterBase monster in monsters)
        {
            monster.TakeDamage(9999f, false);
        }
        LogDebugAction($"Killed {monsters.Length} enemies");
    }

    void RestartDebugWave()
    {
        StopAllCoroutines();
        StartCoroutine(StartDebugWave());
    }

    void LogDebugAction(string action)
    {
        lastDebugAction = action;
        debugActionTime = Time.time;
        Debug.Log($"[DEBUG] {action}");
    }

    void OnGUI()
    {
        if (!isDebugMode || !showDebugGUI) return;

        GUI.color = Color.white;
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        
        GUILayout.BeginVertical("box");
        GUILayout.Label("DEBUG WAVE MANAGER", GUI.skin.GetStyle("label"));
        GUILayout.Space(10);
        
        // 현재 상태 표시
        GUILayout.Label($"Current: Stage {currentStage}, Wave {currentWave}");
        GUILayout.Label($"Debug Target: Stage {debugStage}, Wave {debugWave}");
        GUILayout.Space(10);
        
        // 디버그 설정
        GUILayout.BeginHorizontal();
        GUILayout.Label("Stage:");
        if (GUILayout.Button("-") && debugStage > 1) { debugStage--; }
        GUILayout.Label(debugStage.ToString());
        if (GUILayout.Button("+") && debugStage < 4) { debugStage++; }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Wave:");
        if (GUILayout.Button("-") && debugWave > 0) { debugWave--; }
        GUILayout.Label(debugWave.ToString());
        if (GUILayout.Button("+") && debugWave < 10) { debugWave++; }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // 빠른 액션 버튼들
        if (GUILayout.Button("Start Debug Wave"))
        {
            RestartDebugWave();
        }
        
        if (GUILayout.Button($"God Mode: {(godMode ? "ON" : "OFF")}"))
        {
            ToggleGodMode();
        }
        
        if (GUILayout.Button("Kill All Enemies"))
        {
            KillAllEnemies();
        }
        
        if (GUILayout.Button("Complete Mission"))
        {
            isMissionEnd = true;
        }
        
        GUILayout.Space(10);
        
        // 단축키 정보
        GUILayout.Label("Shortcuts:", GUI.skin.GetStyle("label"));
        GUILayout.Label($"Next Wave: {nextWaveKey}");
        GUILayout.Label($"Prev Wave: {prevWaveKey}");
        GUILayout.Label($"Next Stage: {nextStageKey}");
        GUILayout.Label($"Prev Stage: {prevStageKey}");
        GUILayout.Label($"God Mode: {toggleGodModeKey}");
        GUILayout.Label($"Kill Enemies: {killAllEnemiesKey}");
        
        GUILayout.Space(10);
        
        // 마지막 액션 표시
        if (!string.IsNullOrEmpty(lastDebugAction) && Time.time - debugActionTime < 3f)
        {
            GUI.color = Color.green;
            GUILayout.Label($"Last Action: {lastDebugAction}");
            GUI.color = Color.white;
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    IEnumerator StartDebugWave()
    {
        // 기본 맵 설정
        tileManager.MakeMapByCSV(startMapPath, 7, 7);
        yield return StartCoroutine(tileManager.MoveTilesByArray(0, 0, 0));
        yield return new WaitForSeconds(0.5f);
        
        UIManager.instance.isStarted = true;
        currentStage = debugStage;
        currentWave = debugWave;
        monsterEnforceVar = (debugStage - 1) * 10 + (debugWave - 1);
        
        ChangeStage();
        
        Debug.Log($"[DEBUG] Starting Wave: Stage {debugStage}, Wave {debugWave}");
 
        // 정비 웨이브인 경우
        if (debugWave == 5 || debugWave == 0)
        {
            yield return StartCoroutine(Maintenance());
        }
        // 보스 웨이브인 경우
        else if (debugWave == 10)
        {
            LoadWaveData($"{debugStage}-boss");
            yield return StartCoroutine(RunWave());
        }
        // 일반 웨이브인 경우
        else
        {
            int mapMaxIdx = stageMapNum[debugStage - 1];
            int randNum = Random.Range(1, mapMaxIdx + 1);
            LoadWaveData($"{debugStage}-{randNum}");
            yield return StartCoroutine(RunWave());
        }
        
        Debug.Log($"[DEBUG] Wave Complete: Stage {debugStage}, Wave {debugWave}");
    }

    #region WaveDataSetting
    void LoadWaveData(string fileTitle)
    {
        string path = Path.Combine(Application.streamingAssetsPath, $"WaveData/wave_{fileTitle}.json");
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

    void ChangeStage(float duration = 3f)
    {
        //하늘 색 변경
        Color skycolor = skyboxMaterials[currentStage - 1].GetColor("_SkyColor");
        Color Horizon = skyboxMaterials[currentStage - 1].GetColor("_HorizonColor");
        float horizonStrength = skyboxMaterials[currentStage - 1].GetFloat("_HorizonStrength");
        float horizonHeight = skyboxMaterials[currentStage - 1].GetFloat("_HorizonHeight");
        defaultSkybox.DOColor(skycolor, "_SkyColor", duration);
        defaultSkybox.DOColor(Horizon, "_HorizonColor", duration);
        defaultSkybox.DOFloat(horizonStrength, "_HorizonStrength", duration);
        defaultSkybox.DOFloat(horizonHeight, "_HorizonHeight", duration);

        if (currentStage == 1 && currentWave == 0)
            ExternSoundManager.instance.StartRandomBGM();
    }

    IEnumerator RunStage()
    {
        yield return new WaitForSeconds(1f);
        int prevWave = -1;
        UIManager.instance.isStarted = true;
        monsterEnforceVar = 0;
        
        for (currentStage = debugStage; currentStage <= 3; currentStage++)
        {
            int mapMaxIdx = stageMapNum[currentStage - 1];
            ChangeStage();
            for (int i = 1; i <= 4; i++)
            {
                currentWave = i;
                int randNum = Random.Range(1, mapMaxIdx + 1);
                int cnt = 0;
                while (prevWave == randNum && cnt++ < 20) randNum = Random.Range(1, mapMaxIdx + 1);

                LoadWaveData($"{currentStage}-{randNum}");
                yield return StartCoroutine(RunWave());
                yield return new WaitForSeconds(0.5f);
                prevWave = randNum;
            }
            //정비 스테이지
            currentWave = 5;
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
                yield return StartCoroutine(RunWave());
                yield return new WaitForSeconds(0.5f);
                prevWave = randNum;
            }

            //보스전
            currentWave = 10;
            LoadWaveData($"{currentStage}-boss");
            yield return StartCoroutine(RunWave());
            yield return new WaitForSeconds(0.5f);

            currentWave = 0;
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(Maintenance());
            yield return new WaitForSeconds(0.5f);
        }

        //데모 버전이므로 3스테이지까지만 진행
        demoEndingUI.SetActive(true);
        yield return new WaitForSeconds(3f);
        demoEndingUI.SetActive(false);
        FindObjectOfType<PlayerStatus>().DecreaseHealth(9999f);
        yield break;
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
        if (ExternSoundManager.instance != null)
            ExternSoundManager.instance.ReduceBGMVolume();
        UIManager.instance.changeWaveText(currentStage.ToString() + "-<color=green>M");

        tileManager.InitializeArray(currentStage, 4);
        Vector2Int playerPos = playerPositionData.playerTilePosition;
        Vector2Int stagePos = tileManager.MakeCenteredMapFromCSV(jeongbiMapPath, playerPos.x, playerPos.y);
        yield return tileManager.MoveTilesByArrayByWave(playerPos.x, playerPos.y, 1.5f,1,0);
        jeongbiStage.SetActive(true);
        jeongbiStage.transform.position = tileManager.GetTiles[stagePos.y, stagePos.x].transform.position;
        jeongbiStage.transform.position = new Vector3(jeongbiStage.transform.position.x, 0, jeongbiStage.transform.position.z);

        monsterEnforceVar++;

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
        if (currentWave == 1 || currentWave == 6)
        {
            if (ExternSoundManager.instance != null)
                ExternSoundManager.instance.RestoreBGMVolume();
        }

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
        List<Coroutine> summonCoroutines = new List<Coroutine>();
        if (waveData.isRandomPos)
        {
            foreach (var enemy in waveData.enemies)
            {
                summonCoroutines.Add(StartCoroutine(SummonRandomPosEnemyCoroutine(basePos, enemy)));
            }
        }
        else
        {
            foreach (var enemy in waveData.enemies)
            {
                summonCoroutines.Add(StartCoroutine(SummonEnemyCoroutine(basePos, enemy)));
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
                case "SinkHole": StartCoroutine(HoleCrisis(ev.repeat, ev.delay, ev.startDelay, ev.count)); break;
                case "Spike": StartCoroutine(SpikeCrisis(ev.repeat, ev.delay, ev.count)); break;
                default: Debug.LogError("Wrong Event Type"); break;
            }
        }
        //임무 완료시 초기화
        while (!isMissionEnd) { yield return new WaitForEndOfFrame(); };

        if(mapChanging != null) StopCoroutine(mapChanging);
        StopAllSpecificCoroutines(summonCoroutines);
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
            Vector2Int randomPos = new Vector2Int(Random.Range(1, mapSize-1), Random.Range(1, mapSize-1));
            while (tileManager.GetTileMap[randomPos.y, randomPos.x] <= 0 || tileManager.GetTileMap[randomPos.y,randomPos.x] >= waveData.wallHeight)
            {
                randomPos = new Vector2Int(Random.Range(1, mapSize-1), Random.Range(1, mapSize - 1));
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
        UIManager.instance.KillingMissionStart();
        while (enemyCountData.enemyCount > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        isMissionEnd = true;
    }
    
    IEnumerator BossMission(int count, bool isBoss = true)
    {
        if (ExternSoundManager.instance != null)
            ExternSoundManager.instance.ReduceBGMVolume();
        UIManager.instance.BossMissionStart();
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
    
    IEnumerator ItemMission(Vector2Int basePos, float time = 4f)
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

    void StopAllSpecificCoroutines(List<Coroutine> coroutines)
    {
        foreach (Coroutine coroutine in coroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
    }
    
    IEnumerator WaveEnd()
    {
        UIManager.instance.MissionComplete();
        StopCoroutine("WallCrisis");
        StopCoroutine("HoleCrisis");
        StopCoroutine("SpikeCrisis");
        StopCoroutine("SummonRandomPosEnemyCoroutine");

        yield return new WaitForSeconds(2f);

        if (!skipUpgrade || !isDebugMode)
        {
            upgradeManager.BasicUpgradeCall();

            yield return null;
            while (upgradeManager.Upgrading)
            {
                yield return null;
            }
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
    
    IEnumerator HoleCrisis(int repeat, float cooltime, float startDelay, int holeCount)
    {
        yield return new WaitForSeconds(startDelay);
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
