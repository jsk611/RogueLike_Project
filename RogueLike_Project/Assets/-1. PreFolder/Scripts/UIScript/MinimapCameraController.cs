using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{
    public GameObject player;   

    public float offsetY = 15.0f;
    public float CameraSpeed = 10.0f;
    
    Vector3 PlayerPos;

    void FixedUpdate()
    {
        PlayerPos = new Vector3(
            player.transform.position.x,
            player.transform.position.y + offsetY,
            player.transform.position.z
            );

        transform.position = Vector3.Lerp(transform.position, PlayerPos, Time.deltaTime * CameraSpeed);
    }
}
