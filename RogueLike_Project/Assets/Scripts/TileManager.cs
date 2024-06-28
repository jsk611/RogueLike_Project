
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class TileManager : MonoBehaviour
{
    static int mapSize = 30;
    Tile[,] tiles = new Tile[mapSize, mapSize];
    [SerializeField] GameObject tile;
    int[,] tileMap = new int[mapSize, mapSize];
    // Start is called before the first frame update
    void Start()
    {
        GenerateTiles();
        

        StartCoroutine(ChangingMapSample());

        
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
    void InitializeArray(int initialValue = 0)
    {
        for(int i = 0;i < mapSize; i++)
        {
            for(int j=0; j < mapSize; j++)
            {
                tileMap[i,j] = initialValue;
            }
        }
    }
    void MakeCircle(float radius)
    {
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
                    tileMap[y, x] = 10;
                }
                else
                {
                    tileMap[y, x] = -2;
                }
            }
        }
    }

    void MakePyramid(int size)
    {
        InitializeArray(size - 30);

        for (int i = 0; i < mapSize/2; i++)
        {
            for(int j = i; j < mapSize-i; j++)
            {
                for(int k = i; k < mapSize-i; k++)
                {
                    tileMap[j, k] += 2;
                }
            }
        }
    }

    void MakeRandomMap(int numOfChanging)
    {
        List<Vector2> posList = new List<Vector2>();
        for(int i = 0; i < numOfChanging; i++)
        {
            int randomX = Random.Range(0, mapSize);
            int randomY = Random.Range(0, mapSize);
            Vector2 pos = new Vector2(randomX, randomY);
            if(posList.Contains(pos))
            {
                i--;
                continue;
            }
            else
            {
                posList.Add(pos);
                tileMap[randomY,randomX] = Random.Range(1, 4);
            }
        }
    }
    IEnumerator ChangingMapSample()
    {
        yield return new WaitForSeconds(1f);
        InitializeArray(2);
        yield return StartCoroutine(MoveTilesByArray(tileMap, 1f, 0.5f, 0f));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(MakeWave(15, 15, 6, 1f, 15f));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(MakeWave(15, 15, 6, 2f, 30f));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(MakeWave(15, 15, 6, 3f, 30f));

        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(MakeWave(15, 15, 4, 3f, 30f));
        }
        yield return StartCoroutine(MakeWave(15, 15, 4, 3f, 30f));
        for (int i=0; i<10; i++)
        {
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(MakeWave(Random.Range(0,30), Random.Range(0, 30), Random.Range(4,6), Random.Range(1f,3f), Random.Range(5, 30)));
        }
        //for (int i = 0; i < mapSize; i++)
        //{
        //    for (int j = 0; j < mapSize; j++)
        //    {
        //        tileMap[i, j] = i;
        //    }
        //}
        //StartCoroutine(MoveTilesByArray(tileMap));
        //yield return new WaitForSeconds(7f);

        //MakeCircle(10);
        //StartCoroutine(MoveTilesByArray(tileMap));
        //yield return new WaitForSeconds(7f);

        //MakePyramid(24);
        //StartCoroutine(MoveTilesByArray(tileMap));
        //yield return new WaitForSeconds(7f);

        //for(int i = 0; i < 10; i++)
        //{
        //    MakeCircle(Random.Range(4,15));
        //    StartCoroutine(MoveTilesByArray(tileMap));
        //    yield return new WaitForSeconds(5f);
        //    MakeRandomMap(Random.Range(50, 201));
        //    StartCoroutine(MoveTilesByArray(tileMap));
        //    yield return new WaitForSeconds(5f);
        //    MakePyramid(Random.Range(10, 31));
        //    StartCoroutine(MoveTilesByArray(tileMap));
        //    yield return new WaitForSeconds(5f);
        //    MakeRandomMap(Random.Range(50,201));
        //    StartCoroutine(MoveTilesByArray(tileMap));
        //    yield return new WaitForSeconds(5f);
        //}

        //InitializeArray();
        //StartCoroutine(MoveTilesByArray(tileMap));
    }

    IEnumerator MoveTilesByArray(int[,] tileArray, float durationAboutCoroutine = 1f, float durationAboutTile = 0.5f, float alertTime = 3f)
    {
        //경고 표시
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (tileArray[i, j]/2f != tiles[i, j].transform.position.y)
                {
                    if (tileArray[i, j] < 0) tiles[i, j].AlertChanging(alertTime, true);
                    else tiles[i, j].AlertChanging(alertTime);
                }
            }
        }
        yield return new WaitForSeconds(alertTime);

        //타일 위치 변경
        for (int i=0; i < mapSize; i++)
        {
            for(int j=0; j < mapSize; j++)
            {
                if (tileArray[i,j] < 0)
                {
                    if(tiles[i, j].IsSetActive) tiles[i, j].DestroyTile(1f);
                }
                else
                {
                    if (!tiles[i, j].IsSetActive) tiles[i, j].CreateTile();
                    tiles[i, j].ChangeHeightWithFixedBase(tileArray[i, j], durationAboutTile);
                }
            }
            yield return new WaitForSeconds((float)durationAboutCoroutine / (mapSize) );
        }
    }

    IEnumerator MakeWave(int x, int y, float height, float time, float maxRadius)
    {
        float radius = 0;
        bool[,] eventTriggered = new bool[mapSize, mapSize];
        while (radius < maxRadius)
        {
            
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (eventTriggered[i, j])
                        continue; // 이미 이벤트가 발생한 타일은 무시

                    float distance = Mathf.Sqrt(Mathf.Pow(i - y, 2) + Mathf.Pow(j - x, 2));

                    if(Mathf.Abs(distance-radius) < 0.5f)
                    {
                        eventTriggered[i, j] = true;
                        tiles[i, j].Wave(height);
                    }

                }
            }
            radius += maxRadius/100f;
            yield return new WaitForSeconds(time/100);
        }
    }
}
