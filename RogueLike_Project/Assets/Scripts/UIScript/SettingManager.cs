using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public GameObject CurDisp, NextDisp;

    public void ButtonActive()
    {
        NextDisp.SetActive(true);
        CurDisp.SetActive(false);
    }
}
