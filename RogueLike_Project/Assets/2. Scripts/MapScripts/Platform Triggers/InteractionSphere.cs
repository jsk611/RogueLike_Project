using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class InteractionSphere : MonoBehaviour
{
    // Start is called before the first frame update
    WaveManager waveManager;
    private Coroutine uiCoroutine;

    [SerializeField] TMP_Text helpUI;
    string uiText = "Next Wave";

    bool isActived;
    void Start()
    {
        waveManager = FindObjectOfType<WaveManager>();
        isActived = false;
    }
    private void OnEnable()
    {
        isActived = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //�÷��̾ �������� �� ��ư�Է��� ����ϴ� �ڷ�ƾ ����
        if (other.gameObject.tag == "Player")
        {
            helpUI.text = uiText;
            helpUI.enabled = true;
            helpUI.color = Color.red;
            uiCoroutine = StartCoroutine("PressToSwitchWave");
        }

    }

    
    private void OnTriggerExit(Collider other)
    {
        //�÷��̾ �������� ����� �� �Է°��� �ߴ�
        if (other.gameObject.tag == "Player")
        {
            helpUI.enabled = false;
            StopCoroutine(uiCoroutine);
        }
    }
    
    IEnumerator PressToSwitchWave()
    {
        while (!isActived)
        {
            if(Input.GetKey(KeyCode.F))
            {
                // F �Է����� �� ��ȣ �۽�
                //   Debug.Log("Switching!");
                helpUI.enabled = false;
                waveManager.NextWaveTrigger = true;
                isActived=true;
            }
        //    Debug.Log("Press to Switch Wave!");

            yield return null;
        }
    }

    private void OnDisable()
    {
        helpUI.enabled = false;
    }
}
