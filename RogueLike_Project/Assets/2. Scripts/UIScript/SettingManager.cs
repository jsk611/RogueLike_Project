using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SettingManager : MonoBehaviour
{
    [SerializeField] GameObject continueButton;
    [SerializeField] Image fade;
    private void Start()
    {
        if(!PlayerPrefs.HasKey("packet")) continueButton.SetActive(false);
    }
    public void InitGame()
    {
        fade.gameObject.SetActive(true);
        fade.DOFade(1, 1f);
        Debug.Log("Start Game");
        Invoke("LoadToGame", 1f);
    }
    void LoadToGame()
    {
        fade.DOFade(0, 0.1f);
        SceneManager.LoadScene("IngameScene");
    }
    public void NewGame()
    {
        //저장 데이터 초기화
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("isNewGame", 1);
        fade.gameObject.SetActive(true);

        fade.DOFade(1, 1f);
        Debug.Log("New Game");
        Invoke("LoadToGame", 1f);
    }
    public void GoToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
           UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
