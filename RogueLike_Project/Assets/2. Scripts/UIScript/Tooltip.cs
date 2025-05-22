using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public Vector2 offset = new Vector2(10f, -10f); // 마우스에서 약간 떨어진 위치
    TMP_Text text;
    private void Start()
    {
        text = GetComponentInChildren<TMP_Text>(true);
        gameObject.SetActive(false);
    }
    void Update()
    {
        transform.position = Input.mousePosition + (Vector3)offset;
    }

    public void isNewGameBtn()
    {
        if(text == null) text = GetComponentInChildren<TMP_Text>(true);
        text.text = "New Game";
    }
    public void isSettingBtn()
    {
        if (text == null) text = GetComponentInChildren<TMP_Text>(true);
        text.text = "Setting";
    }
    public void isQuitBtn()
    {
        if (text == null) text = GetComponentInChildren<TMP_Text>(true);
        text.text = "Quit";
    }

}
