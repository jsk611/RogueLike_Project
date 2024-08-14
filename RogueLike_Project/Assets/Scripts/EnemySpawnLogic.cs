using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnLogic : MonoBehaviour
{
    TileManager tileManager;
    int mapSize;

    [SerializeField] GameObject[] enemyPrefabs;

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
            Vector3 spawnVec = tileManager.GetTiles[y, x].transform.position + new Vector3(0, tileManager.GetTiles[y, x].transform.localScale.y, 0); 
            Instantiate(enemyPrefab, spawnVec, Quaternion.identity, this.transform);
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
