using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicMonsterVisual : MonoBehaviour
{
    private MaterialPropertyBlock propBlock;
    private Renderer[] renderers;

    [Header("의태 몬스터 색상 설정")]
    [SerializeField] private Color mimicTintColor = new Color(0.6f, 0.2f, 1f, 0.75f); // RGB(153, 51, 255)
    [SerializeField] private float emissionIntensity = 1.0f;

    void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        renderers = GetComponentsInChildren<Renderer>();
    }

    void OnEnable()
    {
        ApplyMimicVisual();
    }

    void ApplyMimicVisual()
    {
        foreach (var renderer in renderers)
        {
            renderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", mimicTintColor);
            propBlock.SetColor("_BaseColor", mimicTintColor); // URP용
            propBlock.SetColor("_GridColor", mimicTintColor);

            // 약간의 발광 효과 추가
            Color emissionColor = mimicTintColor * emissionIntensity;
            propBlock.SetColor("_EmissionColor", emissionColor);

            renderer.SetPropertyBlock(propBlock);
        }
    }
}
