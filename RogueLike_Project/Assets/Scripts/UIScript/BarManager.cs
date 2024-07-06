using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BarManager : MonoBehaviour
{
    public Slider Bar;

    public float maxValue;
    public float currentValue;

    void Update()
    {
        Bar.value = currentValue / maxValue;
    }
}
