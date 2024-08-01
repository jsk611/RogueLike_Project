using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Slider[] Bar;
    public float[] maxValue;
    public float[] currentValue;
    public void BarValueChange(int i)
    {
        Bar[i].value = currentValue[i] / maxValue[i];
    }
}
