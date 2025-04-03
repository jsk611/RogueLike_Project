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
        Invoke("LoadToGame", 1f);
    }
    void LoadToGame()
    {
        SceneManager.LoadScene("IngameScene");
    }
    public void NewGame()
    {
        //저장 데이터 초기화
        PlayerPrefs.DeleteAll();

        fade.gameObject.SetActive(true);

        fade.DOFade(1, 1f);
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
