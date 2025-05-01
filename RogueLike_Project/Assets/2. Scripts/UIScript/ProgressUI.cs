using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class ProgressUI : MonoBehaviour
{
    [SerializeField] RectTransform progressBar;
    [SerializeField] RectTransform[] waveIcons;
    [SerializeField] Vector2 movingTarget;
    RectTransform rt;
    static public ProgressUI instance = new ProgressUI();
    private void Awake()
    {
        instance = this;
        rt = GetComponent<RectTransform>();
    }

    public IEnumerator ChangeWaveProgress()
    {
        rt.DOAnchorPos(movingTarget, 1f).SetEase(Ease.OutQuad);
        progressBar.DOScale(progressBar.localScale * 2, 1f);
        foreach (var icon in waveIcons)
        {
            icon.DOScale(icon.localScale * 2, 1f);
        }

        yield return new WaitForSeconds(1.5f);

        rt.DOAnchorPos(Vector2.zero, 1f).SetEase(Ease.OutQuad);
        progressBar.DOScale(progressBar.localScale / 2, 1f);
        foreach(var icon in waveIcons)
        {
            icon.DOScale(icon.localScale / 2, 1f);
        }
    }
}
