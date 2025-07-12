using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverScene : MonoBehaviour
{
    public TMP_Text tx;
    [SerializeField] TMP_Text time;
    [SerializeField] TMP_Text stage;


    void Start()
    {
        StartCoroutine(GameOverTyping());
        time.text = "Time: " + PlayerPrefs.GetString("Time");
        stage.text = "Stage: " + PlayerPrefs.GetString("Stage");
    }

    IEnumerator GameOverTyping()
    {
        string temptx = tx.text;
        tx.text = "";

        for (int i = 0; i <= temptx.Length; i++)
        {
            tx.text = temptx.Substring(0, i);
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene("IngameScene");
    }
}
