using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathUI : MonoBehaviour
{
    [SerializeField]
    TextMeshPro text;
    [SerializeField]
    RawImage background;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshPro>();
        background = GetComponent<RawImage>();

    
        StartCoroutine(dieMessage());
    }

    // Update is called once per frame
    IEnumerator dieMessage()
    {

        yield return null;
    }
}
