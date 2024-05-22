using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    Tile[,] tiles = new Tile[40,40];
    [SerializeField] GameObject tile;
    // Start is called before the first frame update
    void Start()
    {
        GenerateTiles();
        StartCoroutine(RandomChangeHeight(20));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateTiles()
    {
        for (int i = 0; i < 40; i++)
        {
            for (int j = 0; j < 40; j++)
            {
                tiles[i, j] = Instantiate(tile, new Vector3(i * 2, 0, j * 2), Quaternion.identity, this.transform).GetComponent<Tile>();
            }
        }
    }

    IEnumerator RandomChangeHeight(int iteration)
    {
        for(int i = 0; i<iteration; i++)
        {
            //int randNum = Random.Range(10, 20);
            for(int j = 0; j < 30; j++)
            {
                int x = Random.Range(0, 40);
                int y = Random.Range(0, 40);
                tiles[x, y].ChangeHeight(Random.Range(2, 20));
            }
            yield return new WaitForSeconds(4f);
        }
    }
}
