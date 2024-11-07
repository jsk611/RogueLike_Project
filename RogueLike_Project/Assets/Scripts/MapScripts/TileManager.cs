
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class TileManager : MonoBehaviour
{

    public static int mapSize = 60;
    Tile[,] tiles = new Tile[mapSize, mapSize];
    [SerializeField] GameObject tile;
    float[,] tileMap = new float[mapSize, mapSize];
    CSVToArray CTA;
    public int GetMapSize
    {
        get { return mapSize; }
    }
    public float[,] GetTileMap
    { 
        get { return tileMap; }
    }
    public Tile[,] GetTiles
    {
        get { return tiles; }
    }

    NavMeshSurface navMeshSurface;

    // Start is called before the first frame update
    void Awake()
    {
        GenerateTiles();
        CTA = FindObjectOfType<CSVToArray>();

        //StartCoroutine(ChangingMapSample());    
    }

    private void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        BuildNavMesh();
    }

    public void BuildNavMesh()
    {
        // NavMesh를 빌드합니다.
        navMeshSurface.BuildNavMesh();
    }

    void GenerateTiles()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                tiles[i, j] = Instantiate(tile, new Vector3(i * 2, 0, j * 2), Quaternion.identity, this.transform).GetComponent<Tile>();
                tiles[i,j].gameObject.name = j.ToString() + "," + i.ToString();
                tiles[i, j].isSetActive = false;
                tiles[i,j].gameObject.SetActive(false);

            }
        }
    }
    public void InitializeArray(int initialValue = 0)
    {
        for(int i = 0;i < mapSize; i++)
        {
            for(int j=0; j < mapSize; j++)
            {
                tileMap[i,j] = initialValue;
            }
        }
    }
    public void MakeCircle(float radius)
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
                    tileMap[y, x] = 5;
                }
                else
                {
                    tileMap[y, x] = -2;
                }
            }
        }
    }

    public void MakePyramid(int size)
    {
        InitializeArray(size-20);

        for (int i = 0; i < mapSize/2; i++)
        {
            for(int j = i; j < mapSize-i; j++)
            {
                for(int k = i; k < mapSize-i; k++)
                {
                    tileMap[j, k] += 1;
                }
            }
        }
    }
    public void MakeMapByCSV(string path, int start_x = 0, int start_y = 0)
    {
        float[,] csvMap = CTA.CSVFileToArray(path);
        int csvMapSizeX = csvMap.GetLength(1);
        int csvMapSizeY = csvMap.GetLength(0);
        for(int x = 0; x<csvMapSizeX; x++)
        {
            for (int y = 0; y < csvMapSizeY; y++)
            {
                tileMap[start_y + y,start_x + x] = csvMap[y,x];
            }
        }

    }

    public Vector2Int MakeCenteredMapFromCSV(string path, int player_x, int player_y)
    {
        int size = CTA.CSVFileToArray(path).GetLength(0);
        
        int improved_x = player_x - size/2 >= 0 ? player_x - size / 2 : 0;
        int improved_y = player_y - size/2 >= 0 ? player_y - size / 2 : 0;

        if(improved_x > mapSize - size) improved_x = mapSize - size;
        if(improved_y > mapSize - size) improved_y = mapSize - size;
        Debug.Log(size);
        MakeMapByCSV(path, improved_x, improved_y);

        return new Vector2Int(improved_x, improved_y);
    }


    public void MakeRandomWall(int numOfChanging)
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
                tileMap[randomY,randomX] += Random.Range(2,4);
            }
        }
    }
    public void MakeRandomHole(int numOfChanging)
    {
        for (int i = 0; i < numOfChanging; i++)
        {
            int randomX = Random.Range(0, mapSize);
            int randomY = Random.Range(0, mapSize);
            Vector2 pos = new Vector2(randomX, randomY);
            if (tileMap[randomY, randomX] <= 0)
            {
                i--;
                continue;
            }
            else
            {
                tileMap[randomY, randomX] = -2;
            }
        }
    }

    float GetMaxTileHeight()
    {
        float max = tileMap.Cast<float>().Max();
        return max < 10 ? 10 : max;
    }
    public IEnumerator MoveTilesByArray(float durationAboutCoroutine = 1.25f, float durationAboutTile = 1f, float alertTime = 1f)
    {
        float maxTileHeight = GetMaxTileHeight();
        //경고 표시
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                tiles[i,j].maxHeight = maxTileHeight;
                if (tileMap[i, j]/2f != tiles[i, j].transform.position.y)
                {
                    if (tileMap[i, j] <= 0) tiles[i, j].AlertChanging(alertTime, true);
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
                if (tileMap[i,j] <= 0)
                {
                    if(tiles[i, j].IsSetActive) tiles[i, j].DestroyTile(1f);
                }
                else
                {
                    if (!tiles[i, j].IsSetActive) tiles[i, j].CreateTile(tileMap[i, j], durationAboutTile);
                    else 
                    {
                        if (tiles[i, j].transform.position.y == tileMap[i, j]/2) continue;
                        tiles[i, j].ChangeHeightWithFixedBase(tileMap[i, j], durationAboutTile);
                    } 
                }
            }
            yield return new WaitForSeconds((float)durationAboutCoroutine / (mapSize) );
        }
        yield return new WaitForSeconds(durationAboutTile + 0.5f);
        BuildNavMesh();
    }
    public IEnumerator MoveTilesByArrayByWave(int x, int y, float height, float time, float alertTime = 1f)
    {
        float maxTileHeight = GetMaxTileHeight();
        //경고 표시
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                tiles[i, j].maxHeight = maxTileHeight;
                if (tileMap[i, j] / 2f != tiles[i, j].transform.position.y)
                {
                    if (tileMap[i, j] <= 0) tiles[i, j].AlertChanging(alertTime, true);
                    else tiles[i, j].AlertChanging(alertTime);
                }
            }
        }
        yield return new WaitForSeconds(alertTime);

        //타일 위치 변경
        float radius = 0;
        float maxRadius = mapSize*1.5f;
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

                    if (Mathf.Abs(distance - radius) < 0.5f)
                    {
                        eventTriggered[i, j] = true;
                        if (tileMap[i, j] <= 0)
                        {
                            if (tiles[i, j].IsSetActive) tiles[i, j].DestroyTile(1f);
                        }
                        else
                        {
                            if (!tiles[i, j].IsSetActive) tiles[i, j].CreateTile();
                            tiles[i, j].WaveToChange(height, 0.5f, tileMap[i,j]);
                        }
                    }

                }
            }
            radius += maxRadius / 100;
            yield return new WaitForSeconds(time / 100);
        }
        yield return new WaitForSeconds(1f);
        BuildNavMesh();
    }
    public IEnumerator MakeWave(int x, int y, int height, float time, float maxRadius)
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
