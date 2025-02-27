using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingManager : MonoBehaviour
{
    [SerializeField] GameObject continueButton;
    private void Start()
    {
        if(!PlayerPrefs.HasKey("packet")) continueButton.SetActive(false);
    }
    public void InitGame()
    {
        SceneManager.LoadScene("IngameScene");
    }
    public void NewGame()
    {
        //저장 데이터 초기화
        PlayerPrefs.DeleteAll();

        SceneManager.LoadScene("IngameScene");
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
