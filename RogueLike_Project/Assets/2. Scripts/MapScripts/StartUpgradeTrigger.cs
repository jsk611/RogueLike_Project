using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUpgradeTrigger : MonoBehaviour
{
    [SerializeField] GameObject upgradeUI;
    [SerializeField] CharacterBehaviour player;

    private bool UIenabled = false;
    private float time = 0;
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == player.gameObject)
        {
            if (Input.GetKeyDown(KeyCode.F) && (Time.time - time > 0.2f))
            {
                time = Time.time;
                if (!UIenabled)
                    UIEnable(true);
                else
                    UIEnable(false);
            }
        }
    }
    public void UIEnable(bool val)
    {
        player.CancelAiming();
        UIenabled = val;
        upgradeUI.SetActive(val);
        player.SetCursorState(!val);
        player.SetInteractingUI(val);
    }
}
