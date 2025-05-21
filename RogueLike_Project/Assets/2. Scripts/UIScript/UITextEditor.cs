using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarTextEditor : MonoBehaviour
{
    private Text text;
    private Slider slider;

    private void Start()
    {
        slider = GetComponent<Slider>();
        text = GetComponentInChildren<Text>();
        text.text = slider.value.ToString();
    }
    public void TextEdit()
    {
        text.text = slider.value.ToString();
    }
}
