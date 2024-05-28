using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockTile : Tile
{
    [SerializeField] GameObject block;
    List<Block> blocks;

    int blockCount = 0;
    float currentSize_y = 0f;
    // Start is called before the first frame update
    void Start()
    {
        GameObject g = Instantiate(block, transform.position + Vector3.up * 50, Quaternion.identity, this.transform);
        blocks.Append(g.GetComponent<Block>());
    }

    public void ChangeHeight(int size_y)
    {
        StartCoroutine(GenerateBlockCoroutine(size_y));
    }

    IEnumerator GenerateBlockCoroutine(int size_y)
    {

        if(size_y > currentSize_y)
        {
            currentSize_y = size_y;
            while (blocks.Count() * 2 < size_y)
            {
                GameObject g = Instantiate(block, transform.position + Vector3.up * 50, Quaternion.identity, this.transform);
                blocks.Add(g.GetComponent<Block>());
                blocks[-1].MovePosition(transform.position.y + blocks.Count()*2 , 1f);
                yield return new WaitForSeconds(0.2f);
            }
        }
        else
        {
            currentSize_y = size_y;


        }


    }
}
