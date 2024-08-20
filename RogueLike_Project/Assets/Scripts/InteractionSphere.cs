using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InteractionSphere : MonoBehaviour
{
    // Start is called before the first frame update
    WaveManager waveManager;
    void Start()
    {
        waveManager = FindObjectOfType<WaveManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //플레이어를 감지했을 때 버튼입력을 대기하는 코루틴 실행
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Can Switch Wave");
            StartCoroutine("PressToSwitchWave");
        }

    }

    
    private void OnTriggerExit(Collider other)
    {
        //플레이어가 범위에서 벗어났을 때 입력감지 중단
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Cannot Switch Wave");
            StopCoroutine("PressToSwitchWave");
        }
    }
    
    IEnumerator PressToSwitchWave()
    {
        while (true)
        {
            if(Input.GetKey(KeyCode.F))
            {
                // F 입력했을 때 신호 송신
                Debug.Log("Switching!");
                waveManager.IsGameStarted = true;
                Destroy(gameObject);
            }
            Debug.Log("Press to Switch Wave!");

            yield return null;
        }
    }
}
