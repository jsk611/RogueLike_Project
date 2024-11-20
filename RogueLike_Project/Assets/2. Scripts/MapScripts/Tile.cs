using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    MeshRenderer meshRenderer;
    //[SerializeField] Material defaultMaterial;
    //[SerializeField] Material watchOutMaterial;
    //[SerializeField] Material warningMaterial;
    public bool isSetActive = true;

    [SerializeField] SpriteRenderer minimapTile;
    public float maxHeight;
    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = Instantiate(meshRenderer.material);
    }

    public bool IsSetActive
    {
        get { return isSetActive; }
    }
    public void MovePosition(float pos_y, float duration = 2f)
    {
        StartCoroutine(MoveCoroutine(pos_y, duration));
    }
    public void AlertChanging(float time = 1.6f, bool isWarning = false)
    {
        if(isSetActive) StartCoroutine(AlertChangingCoroutine(time, isWarning));
    }
    IEnumerator AlertChangingCoroutine(float time, bool isWarning)
    {
        Color alertColor = isWarning ? Color.red : Color.yellow;
        alertColor -= new Color(0, 0, 0, 0.5f);

        if (transform.position.y >= 0)
        {
            while (time > 0)
            {
                meshRenderer.material.SetColor("GridColor", alertColor);
                yield return new WaitForSeconds(0.4f);
                meshRenderer.material.SetColor("GridColor", Color.white);
                yield return new WaitForSeconds(0.4f);
                time -= 0.8f;
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

            //미니맵 색 변경
            if (transform.position.y <= 0) minimapTile.color = Color.black;
            else minimapTile.color = Color.white * 1.7f * transform.position.y / maxHeight;
            Color tmp = minimapTile.color;
            tmp.a = 0.6f;
            minimapTile.color = tmp;

            yield return new WaitForFixedUpdate();
        }

        transform.position = newPosition;
        minimapTile.color = pos_y <= 0 ? Color.black : Color.white * 1.7f * transform.position.y / maxHeight;
        Color tmp2 = minimapTile.color;
        tmp2.a = 0.6f;
        minimapTile.color = tmp2;
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
            yield return new WaitForFixedUpdate();
        }

        transform.localScale = newSize;
    }

    public void DestroyTile(float duration = 2f)
    {
        StartCoroutine(SetActiveFalseCoroutine(duration));
    }

    public void CreateTile() //파동형으로 생성시
    {
        gameObject.SetActive(true);
        isSetActive = true;
        transform.position = new Vector3(transform.position.x, -20f, transform.position.z);
    }

    public void CreateTile(float size_y, float duration = 3f) //기본 생성시
    {
        gameObject.SetActive(true);
        isSetActive = true;
        StartCoroutine(TileCreating(size_y, duration));
    }
    IEnumerator TileCreating(float size_y, float duration = 1.5f)
    {

        transform.position = new Vector3(transform.position.x, -20f, transform.position.z);
        StartCoroutine(ChangeSizeCoroutine(size_y, 0));
        if(duration > 0f)
        {
            StartCoroutine(MoveCoroutine(size_y / 2f, 0));
            float tmp = duration;
            while (tmp > 0f)
            {
                tmp -= Time.deltaTime;
                Color newColor = meshRenderer.material.GetColor("GridColor") + new Color(0, 0, 0, Time.deltaTime / duration);
                meshRenderer.material.SetColor("GridColor", newColor);
                yield return new WaitForSeconds(Time.deltaTime);
            }
            meshRenderer.material.SetColor("GridColor", new Color(1,1,1,1));
            //StartCoroutine(MoveCoroutine(size_y / 2f, 0.3f));
        }
        else StartCoroutine(MoveCoroutine(size_y / 2f , duration));

    }
    IEnumerator SetActiveFalseCoroutine(float duration = 1f)
    {
        if(duration > 0) yield return StartCoroutine(MoveCoroutine(transform.position.y + 0.5f, 0.3f));
        StartCoroutine(MoveCoroutine(-20f, duration));

        float tmp = duration;
        while(tmp > 0f)
        {
            tmp -= Time.deltaTime;
            
            Color newColor = meshRenderer.material.GetColor("GridColor");
            if (newColor.a > 0) newColor = newColor - new Color(0, 0, 0, 3 * Time.deltaTime / duration);
            else 
            { 
                isSetActive = false;
                GameObject player = gameObject.GetComponentInChildren<PlayerControl>().gameObject;
                if(player != null) { player.transform.parent = FindObjectOfType<TileManager>().transform; }
                gameObject.SetActive(false);
            }
                meshRenderer.material.SetColor("GridColor", newColor);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //gameObject.SetActive(false);
        //isSetActive = false;
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
    public void WaveToChange(float height, float duration, float targetY)
    {
        StartCoroutine(WaveToChangeCoroutine(height, duration, targetY));
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

    IEnumerator WaveToChangeCoroutine(float height, float durationPerLoop, float targetY)
    {
        //float baseHeight = transform.localScale.y;
        if(targetY > 0 && !IsSetActive)
        {
            CreateTile();
        }

        ChangeHeightWithFixedBase(targetY + height, durationPerLoop / 2f);
        yield return new WaitForSeconds(durationPerLoop / 2.01f);
        ChangeHeightWithFixedBase(targetY, durationPerLoop / 2f);
        yield return new WaitForSeconds(durationPerLoop / 2f);


    }

}
