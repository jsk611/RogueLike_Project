using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    Tile[,] tiles = new Tile[20,20];
    [SerializeField] GameObject tile;
    // Start is called before the first frame update
    void Start()
    {
        //tile.MovePosition(2.5f);
        //tile.ChangeHeight(5f, 4);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
