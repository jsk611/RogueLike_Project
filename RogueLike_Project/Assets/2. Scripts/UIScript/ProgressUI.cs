using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class ProgressUI : MonoBehaviour
{
    [SerializeField] RectTransform progressBar;
    [SerializeField] RectTransform[] waveIcons; //0:past 1:current 2:next
    [SerializeField] TMP_Text waveText;
    [SerializeField] Vector2 movingTarget;
    RectTransform rt;
    WaveManager waveManager;

    static public ProgressUI instance = new ProgressUI();
    private void Awake()
    {
        instance = this;
        rt = GetComponent<RectTransform>();
        waveManager = FindObjectOfType<WaveManager>();
    }
    public static void SetAnchorAndPivotPreservingPosition(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        // 1. ���� ���� ��ġ�� ���� ��ġ ����
        Vector3 worldPos = rt.position;
        Vector3 localPos = rt.localPosition;

        // 2. anchor�� pivot ����
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;

        // 3. ���ο� localPosition�� ����Ͽ� ����
        rt.localPosition = localPos;

        // 4. ���� ��ġ ����
        rt.position = worldPos;
    }
    public IEnumerator ChangeWaveProgress(string title)
    {
        //�� ��ġ�̵�
        rt.DOAnchorPos(movingTarget, 1f).SetEase(Ease.OutQuad);
        progressBar.DOScale(progressBar.localScale * 2, 1f);

        yield return new WaitForSeconds(1f);
        //���̺� ��ȯ �ִϸ��̼�
        Vector2[] pivots = new Vector2[3];
        Vector2[] anchorMaxs = new Vector2[3];
        Vector2[] anchorMins = new Vector2[3];
        for(int i=0; i<3; i++) 
        {
            pivots[i] = waveIcons[i].pivot;
            anchorMaxs[i] = waveIcons[i].anchorMax;
            anchorMins[i] = waveIcons[i].anchorMin;
        }

        //���� ������ �����
        waveIcons[0].GetComponent<Image>().DOFade(0, 0.45f);
        waveIcons[0].DOAnchorPos(new Vector2(-2, 0), 0.45f);

        //�߾� ������ ���� �̵�
        SetAnchorAndPivotPreservingPosition(waveIcons[1], anchorMins[0], anchorMaxs[0], pivots[0]);
        waveIcons[1].anchoredPosition = new Vector2(22.5f, 0f);
        waveIcons[1].DOAnchorPos(new Vector2(2, 0), 0.9f);
        waveIcons[1].DOScale(new Vector2(0.5f,0.5f), 0.9f);

        //������ ������ �߾����� �̵�
        SetAnchorAndPivotPreservingPosition(waveIcons[2], anchorMins[1], anchorMaxs[1], pivots[1]);
        waveIcons[2].DOAnchorPos(Vector2.zero, 0.9f);
        waveIcons[2].DOScale(new Vector2(1f, 1f), 0.9f);

        waveText.DOFade(0f, 0.5f);

        yield return new WaitForSeconds(0.45f);
        //���ο� ������ �̵�
        switch(waveManager.currentWave)
        {
            // ���� ������
            case 4:
            case 10:
                waveIcons[0].GetComponent<Image>().color = Color.green;
                break;
            // ������ ������
            case 9:
                waveIcons[0].GetComponent<Image>().color = Color.red;
                break;
            default:
                waveIcons[0].GetComponent<Image>().color = Color.white;
                break;
        }

        waveIcons[0].pivot = pivots[2];
        waveIcons[0].anchorMax = anchorMaxs[2];
        waveIcons[0].anchorMin = anchorMins[2];
        waveIcons[0].anchoredPosition = new Vector2(2,0);
        waveIcons[0].GetComponent<Image>().DOFade(1, 0.45f);
        waveIcons[0].DOAnchorPos(new Vector2(-2, 0), 0.45f);

        yield return new WaitForSeconds(0.25f);
        waveText.text = title;
        waveText.DOFade(1f, 0.5f);

        yield return new WaitForSeconds(1f);

        RectTransform tmp = waveIcons[0];
        waveIcons[0] = waveIcons[1];
        waveIcons[1] = waveIcons[2];
        waveIcons[2] = tmp;

        //�� ��ġ �������
        rt.DOAnchorPos(Vector2.zero, 1f).SetEase(Ease.OutQuad);
        progressBar.DOScale(progressBar.localScale / 2, 1f);

    }
}
