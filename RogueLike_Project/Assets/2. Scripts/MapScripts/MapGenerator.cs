using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class MapGenerator : MonoBehaviour
{
    CSVToArray CTA;
    float[,] tileMap;
    [SerializeField] TMP_InputField input;
    [SerializeField] string[] mapPaths;
    [SerializeField] GameObject tile;
    int mapSizeX = 0, mapSizeY = 0;
    GameObject[,] tiles;
    int currentMapIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        CTA = FindObjectOfType<CSVToArray>();
        MakeMapByCSV(mapPaths[0]);
        Make();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return)) {
            CheckCommand(input.text);
        }
    }
    public void MakeMapByCSV(string path)
    {
        float[,] csvMap = CTA.CSVFileToArray(path);
        mapSizeX = csvMap.GetLength(1);
        mapSizeY = csvMap.GetLength(0);
        tileMap = new float[mapSizeX, mapSizeY];
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tileMap[y, x] = csvMap[y, x];
            }
        }

    }
    void Make()
    {
        
        tiles = new GameObject[mapSizeY, mapSizeX];
        Color32[,] baseColors = LoadColorGrid(mapPaths[currentMapIndex] + "_basecolor");
        Color32[,] emissionColors = LoadColorGrid(mapPaths[currentMapIndex] + "_emissioncolor");
        Color32[,] gridColors = LoadColorGrid(mapPaths[currentMapIndex] + "_gridcolor");
        for (int i = 0; i < mapSizeY; i++)
        {
            for (int j = 0; j < mapSizeX; j++)
            {
                tiles[i, j] = Instantiate(tile, new Vector3(i * 2, 0, j * 2), Quaternion.identity, this.transform);
                tiles[i, j].gameObject.name = j.ToString() + "," + i.ToString();
                Vector3 newSize = new Vector3(2, tileMap[i, j], 2);
                Vector3 newPosition = new Vector3(tiles[i,j].transform.position.x, tileMap[i, j]/2, tiles[i, j].transform.position.z);
                tiles[i,j].transform.position = newPosition;
                tiles[i,j].transform.localScale = newSize;
                MeshRenderer mr = tiles[i, j].GetComponent<MeshRenderer>();
                if(baseColors != null) mr.material.SetColor("_BaseColor", baseColors[j, i]);
                if (emissionColors != null) mr.material.SetColor("_EmissionColor", emissionColors[j, i]);
                if (gridColors != null) mr.material.SetColor("GridColor", gridColors[j, i]);
            }
        }
    }

    void DestroyAllTiles()
    {
        for (int i = 0; i < mapSizeY; i++)
        {
            for (int j = 0; j < mapSizeX; j++)
            {
                Destroy(tiles[i, j]);
            }
        }
    }

    void Save()
    {
        //각 타일별 머테리얼 색 저장하기
        Color32[,] baseColorArray = new Color32[mapSizeY, mapSizeX];
        Color32[,] gridColorArray = new Color32[mapSizeY, mapSizeX];
        Color32[,] emissionColorArray = new Color32[mapSizeY, mapSizeX];
        for (int i = 0; i < mapSizeY; i++)
        {
            for (int j = 0; j < mapSizeX; j++)
            {
                baseColorArray[j,i] = tiles[i, j].GetComponent<MeshRenderer>().material.GetColor("_BaseColor");
                gridColorArray[j,i] = tiles[i, j].GetComponent<MeshRenderer>().material.GetColor("GridColor");
                emissionColorArray[j,i] = tiles[i, j].GetComponent<MeshRenderer>().material.GetColor("_EmissionColor");
            }
        }

        SaveColorGrid(baseColorArray, Application.dataPath + "/8. Maps/Resources/" + mapPaths[currentMapIndex] + "_basecolor.csv");
        SaveColorGrid(gridColorArray, Application.dataPath + "/8. Maps/Resources/" + mapPaths[currentMapIndex] + "_gridcolor.csv");
        SaveColorGrid(emissionColorArray, Application.dataPath + "/8. Maps/Resources/" + mapPaths[currentMapIndex] + "_emissioncolor.csv");
    }
    public void SaveColorGrid(Color32[,] colorArray, string path)
    {
        using (StreamWriter writer = new StreamWriter(path))
        {
            int rows = colorArray.GetLength(0);
            int cols = colorArray.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                string line = "";
                for (int j = 0; j < cols; j++)
                {
                    Color32 color = colorArray[i, j];
                    line += $"{color.r}-{color.g}-{color.b}-{color.a}";
                    if (j < cols - 1)
                        line += ",";
                }
                writer.WriteLine(line);
            }
        }
        Debug.Log("2차원 색상 배열 CSV로 저장 완료: " + path);
    }
    Color32[,] LoadColorGrid(string path)
    {
        TextAsset csvFile = Resources.Load<TextAsset>(path);

        if (csvFile == null)
        {
            Debug.LogError("Cannot Find CSV File");
            return null;
        }
        string csvData = csvFile.text;
        string[] lines = csvData.Split('\n');
        int rows = lines.Length - 1;
        int cols = lines[0].Split(',').Length;

        Color32[,] dataArray = new Color32[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            string[] values = lines[i].Split(',');

            for (int j = 0; j < cols; j++)
            {
                string[] color = values[j].Split("-");
                byte r = byte.Parse(color[0]);
                byte g = byte.Parse(color[1]);
                byte b = byte.Parse(color[2]);
                byte a = byte.Parse(color[3]);
                dataArray[i, j] = new Color32(r,g,b,a);
            }
        }
        return dataArray;
    }


    void CheckCommand(string command)
    {
        if (command[0] != '/') return;
        command = command.Substring(1);
        if (command.StartsWith("save"))
        {
            Save();
        }
        else if(int.TryParse(command, out int mapNum))
        {
            DestroyAllTiles();
            currentMapIndex = mapNum;
            MakeMapByCSV(mapPaths[mapNum]);
            Make();
        }
    }
}
