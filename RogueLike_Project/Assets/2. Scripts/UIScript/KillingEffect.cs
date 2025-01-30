using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillingEffect : MonoBehaviour
{
    float alpha = 0.75f;
    float length = 0.24f;
    Image[] images;
    int tmp = 0;
    [SerializeField] EnemyCountData enemyCountData;
    // Start is called before the first frame update
    void Start()
    {
        images = GetComponentsInChildren<Image>();
        EventManager.Instance.MonsterKilledEvent += KillingSuccess;
    }

    // Update is called once per frame
    void Update()
    {
        if (alpha > 0)
        {
            alpha -= Time.deltaTime;
            if (length > 0.075f) length -= Time.deltaTime * 0.2f;
            foreach (Image image in images)
            {
                image.color = new Color(1, 0, 0, alpha);
                image.gameObject.transform.localScale = new Vector2(0.025f, length);
            }
        }
        //if (enemyCountData.enemyCount < tmp)
        //{
        //    KillingSuccess();
        //}
        //tmp = enemyCountData.enemyCount;

    }


    private void OnDisable()
    {
        EventManager.Instance.MonsterKilledEvent -= KillingSuccess;
    }
    void KillingSuccess(bool tmp)
    {
        alpha = 0.75f;
        length = 0.34f;
    }
}
