using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HPBar : MonoBehaviour
{
    public Slider hpBar;

    public float maxHealth;
    public float currentHealth;

    void Update()
    {
        hpBar.value = currentHealth / maxHealth;
    }
}
