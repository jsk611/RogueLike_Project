
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.EventSystems.EventTrigger;

public class TileManager : MonoBehaviour
{

    public static int mapSize = 45;
    Tile[,] tiles = new Tile[mapSize, mapSize];
    [SerializeField] GameObject tile;
    [SerializeField] GameObject tilePreview;

    float[,] tileMap = new float[mapSize, mapSize];
    Color32[,] baseColors = new Color32[mapSize,mapSize];
    Color32[,] emissionColors = new Color32[mapSize, mapSize];
    Color32[,] gridColors = new Color32[mapSize, mapSize];

    [SerializeField] Material[] defaultMaterials;
    [SerializeField] private Material tileMakingMaterial;
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
                tiles[i, j] = Instantiate(tile, new Vector3(i * 2, 0,j * 2), Quaternion.identity, this.transform).GetComponent<Tile>();
                tiles[i, j].preview = Instantiate(tilePreview, this.transform);
                tiles[i,j].gameObject.name = j.ToString() + "," + i.ToString();
                tiles[i, j].isSetActive = false;
                tiles[i,j].gameObject.SetActive(false);

                //if (i-1>=0 && tiles[i - 1, j] != null) {
                //    OffMeshLink link = tiles[i,j].jumpPlatForm.AddComponent<OffMeshLink>();
                //    link.startTransform = tiles[i, j].jumpPlatForm;
                //    link.endTransform = tiles[i - 1, j].jumpPlatForm;
                //    link.costOverride = 1;
                //}
                //if (j-1>=0 && tiles[i,j-1] != null) {
                //    OffMeshLink link = tiles[i, j].jumpPlatForm.AddComponent<OffMeshLink>();
                //    link.startTransform = tiles[i, j].jumpPlatForm;
                //    link.endTransform = tiles[i, j-1].jumpPlatForm;
                //    link.costOverride = 1;
                //}
                //if (i+1 < mapSize && tiles[i+1,j] != null) {
                //    OffMeshLink link = tiles[i, j].jumpPlatForm.AddComponent<OffMeshLink>();
                //    link.startTransform = tiles[i, j].jumpPlatForm;
                //    link.endTransform = tiles[i + 1, j].jumpPlatForm;
                //    link.endTransform = tiles[i + 1, j].jumpPlatForm; link.costOverride = 1;

                //}
                //if (j+1 < mapSize && tiles[i,j+1] != null) {
                //    OffMeshLink link = tiles[i, j].jumpPlatForm.AddComponent<OffMeshLink>();
                //    link.startTransform = tiles[i, j].jumpPlatForm;
                //    link.endTransform = tiles[i, j+1].jumpPlatForm;
                //    link.endTransform = tiles[i + 1, j].jumpPlatForm; link.costOverride = 1;
                //}
            }
        }
    }
    public void InitializeArray(int stage, int initialValue = 0)
    {
        Color[] defaultColor = new Color[4];
        defaultColor[0] = defaultMaterials[stage - 1].GetColor("_BaseColor");
        defaultColor[1] = defaultMaterials[stage - 1].GetColor("_GridColor");
        defaultColor[2] = defaultMaterials[stage - 1].GetColor("_EmissionColor");
        for(int i = 0;i < mapSize; i++)
        {
            for(int j=0; j < mapSize; j++)
            {
                tileMap[i,j] = initialValue;
                tiles[i, j].ChangeSpikeMode(false);
                tiles[i, j].ChangeHealMode(false);
                baseColors[i, j] = defaultColor[0];
                gridColors[i, j] = defaultColor[1];
                emissionColors[i, j] = defaultColor[2];
            }
        }
    }

    public void MakeMapByCSV(string path, int start_x = 0, int start_y = 0)
    {
        float[,] csvMap = CTA.CSVFileToArray(path);
        Color32[,] bc = CTA.LoadColorGrid(path + "_basecolor");
        Color32[,] ec = CTA.LoadColorGrid(path + "_emissioncolor");
        Color32[,] gc = CTA.LoadColorGrid(path + "_gridcolor");
        int csvMapSizeX = csvMap.GetLength(1);
        int csvMapSizeY = csvMap.GetLength(0);
        for(int x = 0; x<csvMapSizeX; x++)
        {
            for (int y = 0; y < csvMapSizeY; y++)
            {
                tileMap[start_y + y,start_x + x] = csvMap[y,x];
                if (bc != null) baseColors[start_y + y,start_x + x] = bc[y,x];
                if (ec != null) emissionColors[start_y + y,start_x + x] = ec[y,x];
                if (gc != null) gridColors[start_y + y,start_x + x] = gc[y,x];
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
            int randomX, randomY;
            do
            {
                randomX = Random.Range(0, mapSize);
                randomY = Random.Range(0, mapSize);
            } while (tileMap[randomY, randomX] <= 0);

            Vector2 pos = new Vector2(randomX, randomY);
            if(posList.Contains(pos))
            {
                i--;
                continue;
            }
            else
            {
                posList.Add(pos);
                tileMap[randomY,randomX] = Random.Range(6,13);
            }
        }
    }
    public void MakeRandomHole(int numOfChanging)
    {
        int cnt = 0;
        for (int i = 0; i < numOfChanging; i++)
        {
            int randomX = Random.Range(0, mapSize);
            int randomY = Random.Range(0, mapSize);
            Vector2 pos = new Vector2(randomX, randomY);
            if (tileMap[randomY, randomX] <= 0)
            {
                i--;
                cnt++;
                if (cnt >= 50) return;
                continue;
            }
            else
            {
                tileMap[randomY, randomX] = -2;
            }
        }
    }
    public void MakeRandomSpike(int numOfChanging)
    {
        List<Vector2> posList = new List<Vector2>();
        for (int i = 0; i < numOfChanging; i++)
        {
            int randomX, randomY;
            do
            {
                randomX = Random.Range(0, mapSize);
                randomY = Random.Range(0, mapSize);
            } while (tileMap[randomY, randomX] <= 0);

            Vector2 pos = new Vector2(randomX, randomY);
            if (posList.Contains(pos))
            {
                i--;
                continue;
            }
            else
            {
                posList.Add(pos);
                StartCoroutine(tiles[randomY, randomX].MakeSpike());
            }
        }
    }

    public bool IsHighPos(int i, int j)
    {
        return false;

    }

    float GetMaxTileHeight()
    {
        float max = tileMap.Cast<float>().Max();
        return max < 10 ? 10 : max;
    }
    public IEnumerator MoveTilesByArray(float durationAboutCoroutine = 1.25f, float durationAboutTile = 1f, float alertTime = 1f)
    {
        tileMakingMaterial.SetFloat("_Progress", -0.6f);
        float maxTileHeight = GetMaxTileHeight();
        if(alertTime > 0)
        {
            //경고 표시
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                
                    tiles[i,j].maxHeight = maxTileHeight;
                    bool isTooHigh = IsHighPos(i,j);

                    if (tileMap[i, j]/2f != tiles[i, j].transform.position.y)
                    {
                        if (tileMap[i, j] <= 0) tiles[i, j].AlertChanging(tileMap[i, j], alertTime, 3);
                        else if (isTooHigh) tiles[i, j].AlertChanging(tileMap[i, j], alertTime, 2);
                        else tiles[i, j].AlertChanging(tileMap[i, j], alertTime, 1);
                    }
                }
            }
            yield return new WaitForSeconds(alertTime);

            tileMakingMaterial.SetFloat("_Progress", -0.6f);
            tileMakingMaterial.DOFloat(0.6f, "_Progress", 4f);
            yield return new WaitForSeconds(4f);
        }

        //타일 위치 변경
        for (int i=0; i < mapSize; i++)
        {
            for(int j=0; j < mapSize; j++)
            {
                tiles[i, j].maxHeight = maxTileHeight;
                if (tileMap[i,j] <= 0)
                {
                    if(tiles[i, j].IsSetActive) tiles[i, j].DestroyTile(1f);
                }
                else
                {
                    MeshRenderer mr = tiles[i, j].gameObject.GetComponent<MeshRenderer>();
                    if (baseColors != null) mr.material.SetColor("_BaseColor", baseColors[j, i]);
                    if (emissionColors != null) mr.material.SetColor("_EmissionColor", emissionColors[j, i]);
                    if (gridColors != null) mr.material.SetColor("_GridColor", gridColors[j, i]);
                    if (!tiles[i, j].IsSetActive) tiles[i, j].CreateTile(tileMap[i, j], durationAboutTile);
                    else 
                    {

                        if (tiles[i, j].transform.position.y == tileMap[i, j]/2) continue;

                        bool isTooHigh = IsHighPos(i,j);
          
                        tiles[i, j].ChangeHeightWithFixedBase(tileMap[i, j], durationAboutTile, isTooHigh);
                        tiles[i, j].PreviewSetActiveFalse();
                        //yield return null;
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
        //경고 표시
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (tileMap[i, j] / 2f != tiles[i, j].transform.position.y)
                {
                    if (tileMap[i, j] <= 0) tiles[i, j].AlertChanging(tileMap[i, j], alertTime, 3);
                    else tiles[i, j].AlertChanging(tileMap[i, j], alertTime,1);
                }
            }
        }
        yield return new WaitForSeconds(alertTime);

        //타일 위치 변경
        float radius = 0;
        float maxRadius = mapSize*Mathf.Sqrt(2);
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
                            tiles[i, j].WaveToChange(height*(1-radius/maxRadius), 0.75f, tileMap[i,j]);
                        }
                        MeshRenderer mr = tiles[i, j].gameObject.GetComponent<MeshRenderer>();
                        if (baseColors != null) mr.material.SetColor("_BaseColor", baseColors[j, i]);
                        if (emissionColors != null) mr.material.SetColor("_EmissionColor", emissionColors[j, i]);
                        if (gridColors != null) mr.material.SetColor("_GridColor", gridColors[j, i]);
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

    public IEnumerator ShowWarningOnTile(Vector3 spawnPos, float duration, float fieldSize, int mode)
    {

        int halfsize = (int)(fieldSize / 2);
        int tileX = (int)(spawnPos.x / 2);
        int tileZ = (int)(spawnPos.z / 2);

        Debug.Log("Field Spawned at [" + tileX + " , " + tileZ + "]");
        float showWarningTime = duration / 4;
        float remainingTime = duration - showWarningTime;
        // 1. 타겟 타일이 실제로 있는지 검사
        if (tileZ < 0 || tileZ >= mapSize || tileX < 0 || tileX >= mapSize)
            yield break;


        for (int i = tileX - halfsize; i <= tileX + halfsize; i++)
        {
            for (int j = tileZ - halfsize; j <= tileZ + halfsize; j++)
            {
                Tile targetTile = tiles[i, j];
                if (targetTile == null || !targetTile.IsSetActive)
                    continue;

                targetTile.AlertChanging(tileMap[i,j], duration, 4);
            }

        }

        yield return new WaitForSeconds(showWarningTime);

        for (int i = tileX - halfsize; i <= tileX + halfsize; i++)
        {
            for (int j = tileZ - halfsize; j <= tileZ + halfsize; j++)
            {
                Tile targetTile = tiles[i, j];
                if (targetTile == null || !targetTile.IsSetActive)
                    continue;

                SetMode(targetTile, mode);
            }

        }

        yield return new WaitForSeconds(remainingTime);

        for (int i = tileX - halfsize; i <= tileX + halfsize; i++)
        {
            for (int j = tileZ - halfsize; j <= tileZ + halfsize; j++)
            {
                Tile targetTile = tiles[i, j];
                if (targetTile == null || !targetTile.IsSetActive)
                    continue;

                ResetMode(targetTile, mode);
            }

        }

    }

    private void SetMode(Tile target, int mode)
    {
        switch (mode)
        {
            case 0:
                target.ChangeSpikeMode(true);
                break;
            case 1:
                target.ChangeHealMode(true);
                break;
        }
    }

    private void ResetMode(Tile target, int mode)
    {
        switch (mode)
        {
            case 0:
                target.ChangeSpikeMode(false);
                break;
            case 1:
                target.ChangeHealMode(false);
                break;
        }
    }

    public IEnumerator CreateShockwave(int i,int j,int chain,float power,float duration = 0.3f)
    {
       
        if (i>=0 && i<=mapSize && j>=0 && j<=mapSize && tiles[i,j].canShockWave && chain>0)
        {
            tiles[i,j].canShockWave = false;
            float time = 0;
            float PI = Mathf.PI;
            Vector3 origin = tiles[i,j].transform.position;
            while (time < duration)
            {
                float y = power/chain* Mathf.Sin(PI * time / duration);
                tiles[i,j].transform.position = origin + new Vector3(0, y, 0);
                time += Time.deltaTime;
                yield return null;
            }
            tiles[i,j].transform.position = origin;
            StartCoroutine(CreateShockwave(i + 1, j, chain - 1,power));
            StartCoroutine(CreateShockwave(i-1,j, chain-1,power));
            StartCoroutine(CreateShockwave(i,j-1,chain-1, power));
            StartCoroutine(CreateShockwave(i,j+1,chain-1,power));
            tiles[i,j].canShockWave = true;
        }
    }

}
