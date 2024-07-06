using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSwap : MonoBehaviour
{
    public GameObject[] weapon;
    public RectTransform[] rectTrans;

    /*void Start()
    {
        Swapping(1);
    }*/

    void Swapping(int index)
    {
        for(int i = 0; i < 2; i++)
        {
            if(index == i)
            {
                weapon[i].transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                rectTrans[i].anchoredPosition = new Vector3(0, 60 * i, 0);
            }
            else if(index > i)
            {
                weapon[i].transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                rectTrans[i].anchoredPosition = new Vector3(0, 60 * i - 20, 0);
            }
            else
            {
                weapon[i].transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                rectTrans[i].anchoredPosition = new Vector3(0, 60 * i + 20, 0);
            }
        }
    }
}
