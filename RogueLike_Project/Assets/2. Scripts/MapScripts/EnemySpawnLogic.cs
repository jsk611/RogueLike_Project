using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnLogic : MonoBehaviour
{
    TileManager tileManager;
    int mapSize;

    [SerializeField] GameObject[] enemyPrefabs;
    [SerializeField] GameObject[] bossPrefabs;

    [SerializeField] EnemyCountData enemyCountData;

    private void Start()
    {
        tileManager = FindObjectOfType<TileManager>();
        mapSize = tileManager.GetMapSize;
    }


    void SpawnEnemy(int x, int y, EnemyType enemyType)
    {
        GameObject enemyPrefab = GetEnemyPrefab(enemyType);
        if (enemyPrefab != null)
        {
            Transform tileTransform = tileManager.GetTiles[y, x].transform;
            Debug.Log(transform.name + ": " + "[" + tileTransform.position.x + " " + tileTransform.position.z + "]" + "/ height: " + tileTransform.position.y);
            Vector3 spawnVec = tileTransform.position + new Vector3(0, tileTransform.localScale.y / 2.0f + 0.5f, 0); 
            Instantiate(enemyPrefab, spawnVec, Quaternion.identity, this.transform);
            enemyCountData.enemyCount++;
        }
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
    GameObject GetEnemyPrefab(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.None: return null;
            default: return enemyPrefabs[(int)enemyType-1];
        }
        //switch (enemyType)
        //{
        //    case EnemyType.Type1:
        //        return Resources.Load<GameObject>("Assets/Prefabs/Enemy1.prefab");
        //    case EnemyType.Type2:
        //        return Resources.Load<GameObject>("Assets/Prefabs/Enemy1.prefab");
        //    case EnemyType.Type3:
        //        return Resources.Load<GameObject>("Assets/Prefabs/Enemy1.prefab");
        //    default: return null;
        //}
    }
}
