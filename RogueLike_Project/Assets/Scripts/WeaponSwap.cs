using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSwap : MonoBehaviour
{
    public GameObject[] weapon;
    public RectTransform[] rectTrans;

    void Start()
    {
        Swapping(2);
    }

    void Swapping(int index)
    {
        for(int i = 0; i < 3; i++)
        {
            if(index == i)
            {
                weapon[i].transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                rectTrans[i].anchoredPosition = new Vector3(0, 100 * i, 0);
            }
            else if(index > i)
            {
                weapon[i].transform.localScale = new Vector3(1f, 1f, 1f);
                rectTrans[i].anchoredPosition = new Vector3(0, 100 * i - 30, 0);
            }
            else
            {
                weapon[i].transform.localScale = new Vector3(1f, 1f, 1f);
                rectTrans[i].anchoredPosition = new Vector3(0, 100 * i + 30, 0);
            }
        }
    }
}
