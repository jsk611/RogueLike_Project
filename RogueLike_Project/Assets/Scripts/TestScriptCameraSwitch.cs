using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScriptCameraSwitch : MonoBehaviour
{
    // Start is called before the first frame update
    private Camera maincamera;
    private void Start()
    {
        maincamera = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetCameraWorld();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            maincamera.fieldOfView = 30;
        }
        if (Input.GetKeyUp(KeyCode.L))
        {
            maincamera.fieldOfView = 67;
        }
    }
}
