using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnLogic : MonoBehaviour
{
    TileManager tileManager;
    int mapSize;

    private void Start()
    {
        tileManager = FindObjectOfType<TileManager>();
        mapSize = tileManager.GetMapSize;
    }


    void SpawnEnemy(int x, int y, EnemyType enemyType)
    {
        GameObject enemyPrefab = GetEnemyPrefab(enemyType);
        //Debug.Log(enemyType);
        Debug.Log(Resources.Load<GameObject>("Assets/Prefabs/Enemy1"));
        if (enemyPrefab != null)
        {
            Vector3 spawnVec = tileManager.GetTiles[y, x].transform.position + Vector3.up; 
            Instantiate(enemyPrefab, spawnVec, Quaternion.identity);
            Debug.Log("Instantiate ¿€µøµ ");
        }
    }

    public void SpawnEnemyByArray(EnemyType[,] enemyMap)
    {
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                SpawnEnemy(x, y, enemyMap[y,x]);

            }
        }
    }

    GameObject GetEnemyPrefab(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.Type1:
                return Resources.Load<GameObject>("Assets/Prefabs/Enemy1.prefab");
            case EnemyType.Type2:
                return Resources.Load<GameObject>("Assets/Prefabs/Enemy1.prefab");
            case EnemyType.Type3:
                return Resources.Load<GameObject>("Assets/Prefabs/Enemy1.prefab");
            default: return null;
        }
    }

}
