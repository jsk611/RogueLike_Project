using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MouseCursor : MonoBehaviour
{
    static public MouseCursor cursor = new MouseCursor();
    // Start is called before the first frame update
    void Start()
    {
        enabled = false;
    }
    public void CursorControl(bool signal)
    {
        if (signal)
        {
            enabled = true;
            StartCoroutine(cursorMove());
        }
        else
        {
            enabled = false;
            StopCoroutine(cursorMove());
        }
    }
    IEnumerator cursorMove()
    {
            Vector2 mousePos = Input.mousePosition;
            gameObject.transform.position = mousePos;
        yield return null;
    }
}
