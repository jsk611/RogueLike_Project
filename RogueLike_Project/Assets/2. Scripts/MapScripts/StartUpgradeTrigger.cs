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
                {
                    UIenabled = !UIenabled;
                    player.SetCursorState(false);
                    upgradeUI.SetActive(true);
                }
                else
                {
                    UIenabled = !UIenabled;
                    player.SetCursorState(true);
                    upgradeUI.SetActive(false);
                }
            }
        }
    }

}
