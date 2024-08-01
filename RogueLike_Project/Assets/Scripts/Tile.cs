using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    MeshRenderer meshRenderer;
    MeshRenderer childMesh;
    //[SerializeField] Material defaultMaterial;
    //[SerializeField] Material watchOutMaterial;
    //[SerializeField] Material warningMaterial;
    bool isSetActive = true;
    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = Instantiate(meshRenderer.material);

        childMesh = GetComponentsInChildren<MeshRenderer>()[1];
        childMesh.material = Instantiate(childMesh.material);
    }

    private void Update()
    {
        meshRenderer.material.mainTextureScale = new Vector2(1, transform.localScale.y / transform.localScale.x);
    }
    public bool IsSetActive
    {
        get { return isSetActive; }
    }
    public void MovePosition(float pos_y, float duration = 2f)
    {
        StartCoroutine(MoveCoroutine(pos_y, duration));
    }
    public void AlertChanging(float time = 3f, bool isWarning = false)
    {
        if(isSetActive) StartCoroutine(AlertChangingCoroutine(time, isWarning));
    }
    IEnumerator AlertChangingCoroutine(float time, bool isWarning)
    {
        Color alertColor = isWarning ? Color.red : Color.yellow;

        if (transform.position.y >= 0)
        {
            while (time > 1f)
            {
                meshRenderer.material.color = alertColor;
                childMesh.material.color = alertColor;
                yield return new WaitForSeconds(0.66f);
                meshRenderer.material.color = Color.white;
                childMesh.material.color = Color.white;
                yield return new WaitForSeconds(0.34f);
                time -= 1f;
            }
            while (time > 0)
            {
                meshRenderer.material.color = alertColor;
                childMesh.material.color = alertColor;
                yield return new WaitForSeconds(0.16f);
                meshRenderer.material.color = Color.white;
                childMesh.material.color = Color.white;
                yield return new WaitForSeconds(0.09f);
                time -= 0.25f;
            }
        }
        else
        {
            yield return new WaitForSeconds(time);
        }
    }

    IEnumerator MoveCoroutine(float pos_y, float duration)
    { 
        //타일 움직이기
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
        //타일 움직이기
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

    public void DestroyTile(float duration = 1f)
    {
        StartCoroutine(SetActiveFalseCoroutine(duration));
    }

    public void CreateTile()
    {
        gameObject.SetActive(true);
        isSetActive = true;
        transform.position = new Vector3(transform.position.x, -20f, transform.position.z);
    }

    IEnumerator SetActiveFalseCoroutine(float duration = 1f)
    {
        yield return StartCoroutine(MoveCoroutine(-20f, duration)); 
        gameObject.SetActive(false);
        isSetActive = false;
    }

    public void ChangeHeightWithFixedBase(float size_y, float duration = 3f)
    {
        StartCoroutine(MoveCoroutine(size_y/2f, duration));
        StartCoroutine(ChangeSizeCoroutine(size_y, duration));
    }

    public void Wave(float height, float duration = 0.25f, int repetition = 1)
    {
        StartCoroutine(WaveCoroutine(height, duration, repetition));
    }
    
    IEnumerator WaveCoroutine(float height, float durationPerLoop, int repetition)
    {
        float baseHeight = transform.localScale.y;
        for(int i=0; i<repetition; i++)
        {
            ChangeHeightWithFixedBase(baseHeight + height, durationPerLoop / 2f);
            yield return new WaitForSeconds(durationPerLoop / 2.01f);
            ChangeHeightWithFixedBase(baseHeight, durationPerLoop / 2f);
            yield return new WaitForSeconds(durationPerLoop / 2f);
        }
    }
}
