using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    public void MovePosition(float pos_y, float duration = 2f)
    {
        StartCoroutine(MoveCoroutine(pos_y, duration));
    }

    IEnumerator MoveCoroutine(float pos_y, float duration)
    {
        Vector3 startPosition = transform.position;
        Vector3 newPosition = new Vector3(transform.position.x, pos_y, transform.position.z);

        float elapsed = 0;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, newPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = newPosition;
    }
    public void ChangeHeight(float size_y, float duration = 2f)
    {
        StartCoroutine(ChangeSizeCoroutine(size_y, duration));
    }
    IEnumerator ChangeSizeCoroutine(float size_y, float duration)
    {
        Vector3 startSize = transform.localScale;
        Vector3 newSize = new Vector3(transform.localScale.x, size_y, transform.localScale.z);

        float elapsed = 0;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(startSize, newSize, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = newSize;
    }
}
