using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleButtonManager : MonoBehaviour
{
    public GameObject MainDisp;
    public GameObject SettingDisp;

    public void InitGame()
    {
        SceneManager.LoadScene("Prototype1");
    }

    public void SettingOn()
    {
        SettingDisp.SetActive(true);
        MainDisp.SetActive(false);
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
