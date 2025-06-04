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
        sceneLoading = false;
    }
    private void Start()
    {
        if(!PlayerPrefs.HasKey("packet")) continueButton.SetActive(false);
    }
    public void InitGame()
    {
        Debug.Log("Start Game");
        StartCoroutine(StartGame());
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

        Debug.Log("New Game");
        StartCoroutine(StartGame());
    }
    public void GoToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }

    IEnumerator StartGame()
    {
        if (!sceneLoading)
        {
            sceneLoading = true;
            fade.gameObject.SetActive(true);
            fade.DOFade(1, 1f);
            yield return new WaitForSeconds(1f);
            LoadToGame();
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
           UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void StartTest()
    {
        SceneManager.LoadScene("IngameScene");
    }
}
