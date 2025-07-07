using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHPBar : MonoBehaviour
{
    [SerializeField] Image inside;
    [SerializeField] Image mid;
    [SerializeField] float speed = 0.2f;
    [SerializeField] TMP_Text bossName;
    [SerializeField] TMP_Text percent;
    BossStatus bossStatus;

    float ratio = 1f;


    // Update is called once per frame
    void Update()
    {
        SetRatio(bossStatus.GetHealth(), bossStatus.GetMaxHealth());

        inside.fillAmount = ratio;

        if (mid.fillAmount > ratio)
        {
            mid.fillAmount -= Time.deltaTime * speed;
        }
        else mid.fillAmount = ratio;

        //if (mid.fillAmount == 0) Destroy(gameObject);
    }

    void SetRatio(float current, float max)
    {
        ratio = current / max;
        percent.text = $"{(int)(ratio * 100)}%"; 
    }

    public void SetBoss(BossStatus boss)
    {
        bossStatus = boss;
        bossName.text = boss.bossName;
    }
}
