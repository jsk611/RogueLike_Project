using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            PauseGame();
    }

    public Slider[] Bar;
    public float[] maxValue;
    public float[] currentValue;
    public void BarValueChange(int i)
    {
        Bar[i].value = currentValue[i] / maxValue[i];
    }

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

    public GameObject PauseCanvas;
    public void PauseGame()
    {
        PauseCanvas.SetActive(true);
    }
}
