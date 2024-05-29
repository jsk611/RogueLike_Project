using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class StaminaBar : MonoBehaviour
{
    public Slider staminaBar;

    public float maxStamina;
    public float currentStamina;

    void Update()
    {
        staminaBar.value = currentStamina / maxStamina;
    }
}
