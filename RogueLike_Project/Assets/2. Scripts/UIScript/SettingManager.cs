using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using InfimaGames.LowPolyShooterPack;

public class SettingManager : MonoBehaviour
{
    [SerializeField] GameObject continueButton;
    [SerializeField] Image fade;

    private bool sceneLoading;
    private void Awake()
    {
        Time.timeScale = 1f;
        sceneLoading = false;
        Application.targetFrameRate = 30;
    }
    private void Start()
    {
        if(!PlayerPrefs.HasKey("packet")) continueButton.SetActive(false);
    }
    //continue
    public void InitGame()
    {
        Debug.Log("Start Game");
        StartGame();
    }

    //new game
    public void NewGame()
    {
        //저장 데이터 초기화
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("isNewGame", 1);

        Debug.Log("New Game");
        StartGame();
    }

    #region SceneLoader
    private void StartGame()
    {
        if (!sceneLoading)
        {
            sceneLoading = true;
            fade.gameObject.SetActive(true);
            fade.DOFade(1, 1f).OnComplete(() => LoadToGame());
        }
    }
    private void LoadToGame()
    {
        fade.DOFade(0, 0.1f);
        SceneManager.LoadScene("IngameScene");
    }   
    #endregion

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
