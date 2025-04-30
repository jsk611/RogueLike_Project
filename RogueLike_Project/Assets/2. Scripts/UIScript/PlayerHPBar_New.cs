using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

public class PlayerHPBar_New : MonoBehaviour
{
    [SerializeField] Image[] Bars;
    [SerializeField] float[] barValues;

    int currentBarIdx;
    Tween barHighlight;
    public void ChangeBarValue(float currHP, float maxHp)
    {
        float ratio = currHP / maxHp;
        if (ratio > barValues[0])
        {
            currentBarIdx = 0;
        }
        else if (ratio > barValues[1])
        {
            currentBarIdx = 1;
        }
        else if (ratio > barValues[2])
        {
            currentBarIdx = 2;
        }
        else
        {
            currentBarIdx = 3;
        }


        for(int i=0; i<4; i++)
        {
            if (i < currentBarIdx)
            {
                Bars[i].color = new Color(0.4f, 0.4f, 0.4f, 0.4f);
                continue;
            }
            else Bars[i].color = new Color(0f, 0f, 0f);

            if (currentBarIdx >= 2) {

                Bars[i].color = Color.red;
            }
            else if (currentBarIdx == 1)
            {
                Bars[i].color = Color.yellow;
            }
            else
            {
                Bars[i].color = Color.white;
            }
            
        }
    }
}
