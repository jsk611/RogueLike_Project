using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private void Start()
    {
        CoinReset(23456);
        StartCoroutine(OnCooltime(5));
        Swapping(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            PauseGame();
    }

    //Controlling bars value
    public Slider[] Bar;
    public float[] maxValue;
    public float[] currentValue;
    public void BarValueChange(int i)
    {
        Bar[i].value = currentValue[i] / maxValue[i];
    }

    //TitleScene(Start, Quit)
    public void InitGame()
    {
        SceneManager.LoadScene("Prototype1");
    }
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    //Pausing game
    public GameObject PauseCanvas;
    public void PauseGame()
    {
        PauseCanvas.SetActive(true);
    }

    //Swapping Weapons
    public GameObject[] weapon;
    public void Swapping(int index)
    {
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

    public Text dnaText;
    int dna = 0;
    public void CoinReset(int getDNA)
    {
        dna += getDNA;
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
