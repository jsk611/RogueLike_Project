using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    int[,] tile = new int[11, 11];
    GameObject[,] tileObject = new GameObject[11, 11];
    public int min, max;
    bool isMove = false;
    bool isOver = true;
    Renderer objectColor;

    void Start()
    {
        StartSetting();
    }

    void Update()
    {
        MoveSetting();
        Move();
    }

    void StartSetting()
    {
        for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < 11; j++)
            {
                tile[i, j] = 0;
                tileObject[i, j] = transform.GetChild(i).gameObject.transform.GetChild(j).gameObject;
            }
        }
    }

    void MoveSetting()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Reset();
            int temp = Random.Range(0, 10);
            if (temp < 7)
                RandomSetting();
            else
                PyramidSetting();
        }
    }

    void RandomSetting()
    {
        int repeat = Random.Range(min, max + 1);
        for (int p = 0; p < repeat; p++)
        {
            int x = Random.Range(0, 11);
            int y = Random.Range(0, 11);
            if (tile[x, y] != 0)
            {
                p--;
                continue;
            }
            else
            {
                tile[x, y] = Random.Range(-2, 3);
                if (tile[x, y] == 0)
                {
                    p--;
                    continue;
                }
                objectColor = tileObject[x, y].GetComponent<Renderer>();
                objectColor.material.color = Color.red;
            }
        }
        isOver = false;
        isMove = true;
    }

    void PyramidSetting()
    {
        for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < 11; j++)
            {
                objectColor = tileObject[i, j].GetComponent<Renderer>();
                objectColor.material.color = Color.gray;
                if (i <= 5)
                    tile[i, j] = i;
                else
                    tile[i, j] = 10 - i;
            }
        }

        int[,] temptile = new int[11, 11];

        for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < 11; j++)
            {
                if (i <= 5)
                {
                    temptile[j, i] = i;
                    if (temptile[j, i] <= tile[j, i])
                        tile[j, i] = temptile[j, i];
                }
                else
                {
                    temptile[j, i] = 10 - i;
                    if (temptile[j, i] <= tile[j, i])
                        tile[j, i] = temptile[j, i];
                }
            }
        }

        isOver = false;
        isMove = true;
    }

    void Reset()
    {
        for(int i = 0; i < 11; i++)
        {
            for(int j = 0; j < 11; j++)
            {
                objectColor = tileObject[i, j].GetComponent<Renderer>();
                objectColor.material.color = Color.white;
                tile[i, j] = 0;
            }
        }
    }

    void Move()
    {
        if (isMove)
        {
            if (isOver)
                isMove = false;
            else
            {
                isOver = true;
                for(int i = 0; i < 11; i++)
                {
                    for(int j = 0; j < 11; j++)
                    {
                        if(tileObject[i,j].transform.position.y != tile[i, j])
                        {
                            isOver = false;
                            
                            Vector3 destination = new Vector3(tileObject[i, j].transform.position.x,
                                tile[i, j], tileObject[i, j].transform.position.z);
                            tileObject[i, j].transform.position = Vector3.MoveTowards
                                (tileObject[i, j].transform.position, destination, 0.015f);
                        }
                    }
                }
            }
        }
    }
}
