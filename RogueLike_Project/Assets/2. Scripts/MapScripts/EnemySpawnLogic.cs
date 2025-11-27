using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnLogic : MonoBehaviour
{
    static public EnemySpawnLogic instance = new EnemySpawnLogic();
    TileManager tileManager;
    int mapSize;

    [SerializeField] GameObject[] enemyPrefabs;
    [SerializeField] GameObject[] bossPrefabs;

    private readonly List<(EnemyType type, float weight)> currentEnemyPool = new();
    private float currentTotalWeight = 0f;


    [SerializeField] EnemyCountData enemyCountData;

    private void Start()
    {
        instance = this;
        tileManager = FindObjectOfType<TileManager>();
        mapSize = tileManager.GetMapSize;
    }

    public void BuildPoolForRound(int roundIndex)
    {
        currentEnemyPool.Clear();
        currentTotalWeight = 0f;

        // foreach (var def in enemyDefinitions)
        // {
        //     if (def == null) continue;
        //     if (roundIndex < def.unlockRound) // 아직 안 풀린 몹
        //         continue;

        //     float w = def.baseWeight;
        //     if (w <= 0f) continue;

        //     currentEnemyPool.Add((def.type, w));
        //     currentTotalWeight += w;
        // }

        if (currentEnemyPool.Count == 0)
        {
            Debug.LogWarning($"[EnemySpawnLogic] Round {roundIndex} 에 사용 가능한 몬스터가 없습니다.");
        }
    }

    private EnemyType GetRandomTypeFromPool()
    {
        if (currentEnemyPool.Count == 0)
        {
            Debug.LogError("[EnemySpawnLogic] currentEnemyPool 이 비어 있음. EnemyType.None 반환.");
            return EnemyType.None;
        }

        float r = Random.value * currentTotalWeight;
        float acc = 0f;

        foreach (var entry in currentEnemyPool)
        {
            acc += entry.weight;
            if (r <= acc)
                return entry.type;
        }

        return currentEnemyPool[currentEnemyPool.Count - 1].type; // float 오차 대비
    }
    public GameObject SpawnEnemy(int x, int y, EnemyType enemyType)
    {
        GameObject enemyPrefab = GetEnemyPrefab(enemyType);
        GameObject enemySpawned = null;
        if (enemyPrefab != null)
        {
            Transform tileTransform = tileManager.GetTiles[y, x].transform;
            Debug.Log(transform.name + ": " + "[" + tileTransform.position.x + " " + tileTransform.position.z + "]" + "/ height: " + tileTransform.position.y);
            Vector3 spawnVec = tileTransform.position + new Vector3(0, tileTransform.localScale.y / 2.0f + 0.5f, 0); 
            enemySpawned = Instantiate(enemyPrefab, spawnVec, Quaternion.identity, this.transform);
            //enemyCountData.enemyCount++;
        }
        return enemySpawned;
    }

    public void SpawnEnemyByArray(EnemyType[,] enemyMap)
    {
        enemyCountData.enemyCount = 0;
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                SpawnEnemy(x, y, enemyMap[y,x]);
            }
        }
        EventManager.Instance.TriggerEnemyCountReset();
        Debug.Log(enemyCountData.enemyCount);
    }
    public void SpawnBoss(int x, int y, int bossIdx)
    {
        Transform tileTransform = tileManager.GetTiles[y, x].transform;
        Debug.Log(transform.name + ": " + "[" + tileTransform.position.x + " " + tileTransform.position.z + "]" + "/ height: " + tileTransform.position.y);
        Vector3 spawnVec = tileTransform.position + new Vector3(0, tileTransform.localScale.y / 2.0f + 0.5f, 0);
        Instantiate(bossPrefabs[bossIdx], spawnVec, Quaternion.identity, this.transform);
        enemyCountData.enemyCount++;
        
    }
    public GameObject GetEnemyPrefab(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.None: return null;
            case EnemyType.MeeleeSoldier: return enemyPrefabs[0];
            case EnemyType.Golem: return enemyPrefabs[1];
            case EnemyType.RangedSoldier: return enemyPrefabs[2];
            case EnemyType.Turret: return enemyPrefabs[3];
            case EnemyType.Hoverbot: return enemyPrefabs[4];
            case EnemyType.Sniper: return enemyPrefabs[5];
            case EnemyType.HammerMan: return enemyPrefabs[6];
            case EnemyType.Thrower: return enemyPrefabs[7];
            case EnemyType.Summoner: return enemyPrefabs[8];
            case EnemyType.FieldMage: return enemyPrefabs[9];
            case EnemyType.SpiderMinion: return enemyPrefabs[10];
            case EnemyType.Wormboss: return bossPrefabs[0];
            case EnemyType.Troyboss: return bossPrefabs[1];
            case EnemyType.Ransomware: return bossPrefabs[2];
            case EnemyType.Spider: return bossPrefabs[3];
            case EnemyType.Unknown: return bossPrefabs[4];
            default: return null;
        }
    }
}
