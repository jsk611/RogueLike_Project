using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    static public UIManager instance = new UIManager();

    public GameObject[] weapon;


    public Text[] curAmmo, maxAmmo;
    Text curammo, maxammo;


    private void Awake()
    {
        instance = this;

    }

    private void Start()
    {
        BarValueChange(0, 100, 100);
        BarValueChange(1, 100, 100);
        CoinReset(0);
        Swapping(0);
    }

    //Controlling bars value
    public Slider[] Bar;
    public void BarValueChange(int i, float maxValue, float curValue)
    {
        Bar[i].value = curValue / maxValue;
    }

    //Swapping Weapons
 
    public void Swapping(int index)
    {
        
        curammo = curAmmo[index];
        maxammo = maxAmmo[index];
        for (int i = 0; i < 2; i++)
        {
            if (index == i)
            {
                weapon[i].transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            }
            else
            {
                weapon[i].transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            }
        }
    }
    public void AmmoTextReset(bool knifeActive,int cur, int max)
    {
        if (!knifeActive)
        {
            curammo.text = cur.ToString();
            maxammo.text = max.ToString();
        }
        else
        {
            curammo.text = "-";
            maxammo.text = "-";
        }    

    }



    public Text dnaText;
    public void CoinReset(int dna)
    {
        if (dna == 0)
            dnaText.text = "0";
        else
            dnaText.text = GetThousandCommaText(dna).ToString();
    }   
    public string GetThousandCommaText(int data)
    {
        return string.Format("{0:#,###}", data);
    }

    public Image deactivateImage;
    public IEnumerator OnCooltime(float cool)
    {
        float curcool = cool;
        while (curcool > 0)
        {
            curcool -= Time.deltaTime;
            deactivateImage.fillAmount = (curcool / cool);
            yield return null;
        }
    }
}
