using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBar : MonoBehaviour
{
    [SerializeField] Image inside;
    [SerializeField] Image mid;
    [SerializeField] float speed = 0.6f;

    float ratio = 1f;


    // Update is called once per frame
    void Update()
    {
        inside.fillAmount = ratio;
        
        if(mid.fillAmount > ratio )
        {
            mid.fillAmount -= Time.deltaTime*speed;
        }
        else mid.fillAmount = ratio;

        //if (mid.fillAmount == 0) Destroy(gameObject);
    }

    public void SetRatio(float current, float max)
    {
        ratio = current / max;
    }
}
