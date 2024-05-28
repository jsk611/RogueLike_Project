
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    static int mapSize = 30;
    Tile[,] tiles = new Tile[mapSize, mapSize];
    [SerializeField] GameObject tile;
    // Start is called before the first frame update
    void Start()
    {
        GenerateTiles();
        int[,] testArray = new int[mapSize, mapSize];
        for(int i = 0; i < mapSize; i++)
        {
            for(int j = 0; j < mapSize; j++)
            {
                testArray[i,j] = i - 2;
            }
        }
        Debug.Log(testArray[0, 0]);
        //StartCoroutine(MoveTilesByArray(testArray));


        double radius = 7.5;
        int centerX = mapSize / 2;
        int centerY = mapSize / 2;

        // Iterate over every point in the 30x30 grid
        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                // Calculate the distance from the center
                double distance = Mathf.Sqrt(Mathf.Pow(x - centerX, 2) + Mathf.Pow(y - centerY, 2));

                // If the distance is less than or equal to the radius, mark the point as part of the circle
                if (distance <= radius)
                {
                    testArray[y, x] = 10;
                }
                else
                {
                    testArray[y, x] = -2;
                }
            }
        }

        StartCoroutine(MoveTilesByArray(testArray));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateTiles()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                tiles[i, j] = Instantiate(tile, new Vector3(i * 2, 0, j * 2), Quaternion.identity, this.transform).GetComponent<Tile>();
            }
        }
    }

    IEnumerator MoveTilesByArray(int[,] tileArray, float durationAboutCoroutine = 1f, float durationAboutTile = 0.5f)
    {
        for (int i=0; i < mapSize; i++)
        {
            for(int j=0; j < mapSize; j++)
            {
                if (tileArray[i,j] < 0)
                {
                    tiles[i, j].DestroyTile(1f);
                }
                else
                {
                    if (!tiles[i, j].IsSetActive) tiles[i, j].CreateTile();
                    tiles[i, j].MovePosition(tileArray[i,j]/2f, durationAboutTile);
                    tiles[i, j].ChangeHeight(tileArray[i,j]+1, durationAboutTile);
                }
            }
            yield return new WaitForSeconds((float)durationAboutCoroutine / (mapSize) );
        }
    }

    IEnumerator RandomChangeHeight(int iteration)
    {
        for(int i = 0; i<iteration; i++)
        {
            //int randNum = Random.Range(10, 20);
            for(int j = 0; j < 300; j++)
            {
                int x = Random.Range(0, mapSize);
                int y = Random.Range(0, mapSize);
                tiles[x, y].ChangeHeight(Random.Range(2, 20),0.5f);
            }
            yield return new WaitForSeconds(0.6f);
        }
    }
}
