using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinDisp : MonoBehaviour
{
    public Text coinText;
    int coin = 0;

    void Start()
    {
        CoinReset(2353567);
    }

    public void CoinReset(int getCoin)
    {
        coin += getCoin;
        coinText.text = GetThousandCommaText(coin).ToString();
    }

    public string GetThousandCommaText(int data)
    {
        return string.Format("{0:#,###}", data);
    }
}
