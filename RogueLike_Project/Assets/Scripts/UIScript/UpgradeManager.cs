using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public GameObject[] commonButtons, rareButtons, epicButtons;
    GameObject[] curUpgreadeButtons;
    public int rarepoint = 94, epicpoint = 99;

    public void UpgradeDisplay()
    {
        for(int i=0; i<3; i++)
        {
            int point = Random.Range(0, 100);
            bool[] epicnumused, rarenumused, commonnumused;
            int epicnum, rarenum, commonnum;

            if (point >= epicpoint)
            {
                epicnumused = new bool[5];
                do
                {
                    epicnum = Random.Range(0, 5);
                } while (epicnumused[epicnum] == false);
                curUpgreadeButtons[i] = epicButtons[i];
            }
            else if (point < epicpoint && point >= rarepoint)
            {
                rarenumused = new bool[10];
                do
                {
                    rarenum = Random.Range(0, 5);
                } while (rarenumused[rarenum] == false);
                rarenum = Random.Range(0, 10);
                curUpgreadeButtons[i] = rareButtons[i];
            }
            else
            {
                commonnumused = new bool[20];
                do
                {
                    commonnum = Random.Range(0, 5);
                } while (commonnumused[commonnum] == false);
                commonnum = Random.Range(0, 20);
                curUpgreadeButtons[i] = commonButtons[i];
            }
        }
    }
}
