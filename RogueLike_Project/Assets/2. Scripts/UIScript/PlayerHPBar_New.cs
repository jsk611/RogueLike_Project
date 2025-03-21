using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

public class PlayerHPBar_New : MonoBehaviour
{
    [SerializeField] Image[] Bars;
    [SerializeField] Image[] Backs;
    [SerializeField] float[] barValues;

    int currentBarIdx;
    Tween barHighlight;
    public void ChangeBarValue(float currHP, float maxHp)
    {
        float ratio = currHP / maxHp;
        if (ratio > barValues[0])
        {
            float barRatio = (ratio - barValues[0]) / (1 - barValues[0]);
            Bars[0].fillAmount = barRatio;
            Bars[1].fillAmount = 1;
            Bars[2].fillAmount = 1;
            Bars[3].fillAmount = 1;

            currentBarIdx = 0;

            Debug.Log("BarRatio : " + barRatio.ToString());
        }
        else if (ratio > barValues[1])
        {
            float barRatio = (ratio - barValues[1]) / (barValues[0] - barValues[1]);
            Bars[0].fillAmount = 0;
            Bars[1].fillAmount = barRatio;
            Bars[2].fillAmount = 1;
            Bars[3].fillAmount = 1;

            currentBarIdx = 1;
        }
        else if (ratio > barValues[2])
        {
            float barRatio = (ratio - barValues[2]) / (barValues[1] - barValues[2]);
            Bars[0].fillAmount = 0;
            Bars[1].fillAmount = 0;
            Bars[2].fillAmount = barRatio;
            Bars[3].fillAmount = 1;

            currentBarIdx = 2;
        }
        else
        {
            float barRatio = (ratio - barValues[3]) / (barValues[2] - barValues[3]);
            Bars[0].fillAmount = 0;
            Bars[1].fillAmount = 0;
            Bars[2].fillAmount = 0;
            Bars[3].fillAmount = barRatio;

            currentBarIdx = 3;
        }

        if(barHighlight != null) barHighlight.Kill();

        for(int i=0; i<4; i++)
        {
            if (i == currentBarIdx) Backs[i].color = new Color(0.4f, 0.4f, 0.4f, 0.4f);
            else Backs[i].color = new Color(0f, 0f, 0f, 0.4f);

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
        barHighlight = Bars[currentBarIdx].DOColor(Bars[currentBarIdx].color * 0.7f, 1f).SetLoops(-1, LoopType.Yoyo).OnKill(() => Bars[currentBarIdx].color = Color.white); 
    }
}
