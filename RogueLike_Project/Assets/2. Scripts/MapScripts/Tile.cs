using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{
    MeshRenderer meshRenderer;
    //[SerializeField] Material defaultMaterial;
    //[SerializeField] Material watchOutMaterial;
    //[SerializeField] Material warningMaterial;
    public bool isSetActive = true;
    public GameObject preview;
    public GameObject warningPreview;

    [SerializeField] SpriteRenderer minimapTile;
    [SerializeField] GameObject spike;
    [SerializeField] GameObject heal;
    [SerializeField] GameObject warningLaser;
    public Transform jumpPlatForm;
    public float maxHeight;
    bool isSpike = false;
    bool isHeal = false;
    public bool canShockWave = true;

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
        //StartCoroutine(MoveCoroutine(pos_y, duration));
        Vector3 newPosition = new Vector3(transform.position.x, pos_y, transform.position.z);
        transform.position = newPosition;
        minimapTile.color = pos_y <= 0 ? Color.black : Color.white * 1.7f * transform.position.y / maxHeight;
        Color tmp2 = minimapTile.color;
        tmp2.a = 0.6f;
        minimapTile.color = tmp2;
        ChangeSpikeMode(false);
        ChangeHealMode(false);
    }
    public void AlertChanging(float newYScale, float time = 1.6f, int warningMode = 0)
    {
        // 0: no warning 1: 주의 2: 가시주의 3: 낙사주의 4: 마법사 디버프 필드
        PreviewSetActiveFalse();
        if (isSetActive)
        {
            StartCoroutine(AlertChangingCoroutine(time, warningMode));
            if (newYScale < 0) newYScale = 0;
            float previewScale = (newYScale - transform.localScale.y);
            if(previewScale > 0.2f && time > 0f)
            {
                preview.SetActive(true);
                preview.transform.localScale = new Vector3(1.99f, previewScale, 1.99f);
                preview.transform.position = this.transform.position + new Vector3(0, previewScale / 2f + transform.localScale.y /2f, 0);
            }
            else if(previewScale < 0.2f && time > 0f)
            {
                warningPreview.SetActive(true);
                warningPreview.transform.localScale = new Vector3(1.99f, -previewScale, 1.99f);
                warningPreview.transform.position = this.transform.position + new Vector3(0, previewScale / 2f + transform.localScale.y / 2f, 0);
            }
        }
    }
    IEnumerator AlertChangingCoroutine(float time, int warningMode)
    {
        Color alertColor = Color.white;
        switch (warningMode)
        {
            case 0: yield break;
            case 1: alertColor = Color.yellow; break;
            case 2: alertColor = new Color(1,0.5f,1); break;
            case 3: alertColor = Color.red; break;
            case 4: alertColor = Color.green; break;
        }

        alertColor -= new Color(0, 0, 0, 0.5f);

        if (transform.position.y >= 0)
        {
            while (time > 0)
            {
                //meshRenderer.material.SetColor("_GridColor", alertColor);
                //yield return new WaitForSeconds(0.4f);
                //meshRenderer.material.SetColor("_GridColor", Color.white);
                //yield return new WaitForSeconds(0.4f);
                meshRenderer.material.DOColor(alertColor, "_GridColor", 0.3f).SetLoops(2, LoopType.Yoyo);
                yield return new WaitForSeconds(0.8f);
                time -= 0.8f;
            }
        }
        else
        {
            yield return new WaitForSeconds(time);
        }
        
    }

    
    public void ChangeHeight(float size_y, float duration = 2f)
    {
        //StartCoroutine(ChangeSizeCoroutine(size_y, duration));
        Vector3 newSize = new Vector3(transform.localScale.x, size_y, transform.localScale.z);
        
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
        ChangeHeight(size_y, 0);
        if(duration > 0f)
        {
            MovePosition(size_y / 2f, 0);
            float tmp = duration;
            while (tmp > 0f)
            {
                tmp -= Time.deltaTime;
                Color newColor = meshRenderer.material.GetColor("_GridColor") + new Color(0, 0, 0, Time.deltaTime / duration);
                meshRenderer.material.SetColor("_GridColor", newColor);
                yield return new WaitForSeconds(Time.deltaTime);
            }
            meshRenderer.material.SetColor("_GridColor", new Color(1,1,1,1));
            //StartCoroutine(MoveCoroutine(size_y / 2f, 0.3f));
        }
        else MovePosition(size_y / 2f, duration);

    }
    IEnumerator SetActiveFalseCoroutine(float duration = 1f)
    {
        MovePosition(-20f, duration);
        ChangeHeight(0, duration);

        isSetActive = false;
        try
        {
            GameObject player = gameObject.GetComponentInChildren<PlayerControl>().gameObject;
            if (player != null) { player.transform.parent = FindObjectOfType<TileManager>().transform; }
            
        }
        catch(NullReferenceException)
        {
        }

        yield return new WaitForSeconds(duration);

        PreviewSetActiveFalse();
        gameObject.SetActive(false);
            
        
        //gameObject.SetActive(false);
        //isSetActive = false;
    }

    public void ChangeHeightWithFixedBase(float size_y, float duration = 3f, bool isSpike = false)
    {
        ChangeSpikeMode(isSpike);
        //StartCoroutine(MoveCoroutine(size_y/2f, duration));
        //StartCoroutine(ChangeSizeCoroutine(size_y, duration));

        MovePosition(size_y / 2f, duration);
        ChangeHeight(size_y, duration);
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

    public void ChangeSpikeMode(bool isSpike)
    {
        this.isSpike = isSpike;
        spike.SetActive(isSpike);
    }
    public IEnumerator MakeSpike()
    {
        GameObject laser = Instantiate(warningLaser, transform.position, quaternion.identity);
        Destroy(laser, 2f);
        yield return StartCoroutine(AlertChangingCoroutine(1.6f, 2));
        ChangeSpikeMode(true);
    }

    public void ChangeHealMode(bool isHeal)
    {
        this.isHeal = isHeal;
        heal.SetActive(isHeal);
    }

    public IEnumerator CreateShockwave()
    {
        if (canShockWave)
        {
            canShockWave = false;
            float duration = 1f;
            float time = 0;
            float PI = Mathf.PI;
            Vector3 origin = transform.position;
            while (time < duration)
            {
                float y = Mathf.Sin(PI * time / duration);
                transform.position = origin + new Vector3(0, y, 0);
                time += Time.deltaTime;
                yield return null;
            }
            transform.position = origin;
            canShockWave = true;
        }
    }
    public void PreviewSetActiveFalse()
    {
        preview.SetActive(false);
        warningPreview.SetActive(false);
    }

}
