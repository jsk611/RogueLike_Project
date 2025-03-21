using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : MonoBehaviour
{
    [SerializeField] Image main;
    [SerializeField] Image mid;

    float ratio = 1f;


    // Update is called once per frame
    void Update()
    {
        ratio = main.fillAmount;

        if (mid.fillAmount > ratio)
        {
            mid.fillAmount -= Time.deltaTime * 0.2f;
        }
        else if (mid.fillAmount < ratio)
        {
            mid.fillAmount = ratio;
        }

    }
}
