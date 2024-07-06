using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cooltime : MonoBehaviour
{
    public Image onCoolImage;

    void Start()
    {
        StartCoroutine(OnCooltime(5));
    }

    public IEnumerator OnCooltime(float cool)
    {
        float curcool = cool;

        while(curcool > 0)
        {
            curcool -= Time.deltaTime;
            onCoolImage.fillAmount = (curcool / cool);
            yield return new WaitForSeconds(0.01f);
        }
    }
}
