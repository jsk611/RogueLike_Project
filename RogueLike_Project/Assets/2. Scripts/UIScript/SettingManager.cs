using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingManager : MonoBehaviour
{
    public void InitGame()
    {
        SceneManager.LoadScene("MapScene");
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
