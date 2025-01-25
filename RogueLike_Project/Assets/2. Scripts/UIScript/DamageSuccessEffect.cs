using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageSuccessEffect : MonoBehaviour
{
    float alpha = 0.6f;
    float length = 0.1f;
    Image[] images;
    // Start is called before the first frame update
    void Start()
    {
        images = GetComponentsInChildren<Image>();
        MonsterBase.MonsterDamagedEvent += DamagedSuccess;
    }

    // Update is called once per frame
    void Update()
    {
        if(alpha > 0)
        {
            alpha -= Time.deltaTime;
            if(length > 0.075f) length -= Time.deltaTime*0.15f;
            foreach(Image image in images)
            {
                image.color = new Color(1,1,1,alpha);
                image.gameObject.transform.localScale = new Vector2(0.018f, length);
            }
        }

    }

    private void DamagedSuccess()
    {
        alpha = 0.6f;
        if (length < 0.2f) length += 0.025f;
    }
}
