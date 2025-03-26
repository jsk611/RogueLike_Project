using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataReaperBasicEffect : MonoBehaviour
{
    [Header("가장자리 효과 설정")]
    [SerializeField] private float edgeWidth = 0.02f;
    [SerializeField] private float edgeGlowIntensity = 1.5f;
    [SerializeField] private Color edgeColor = Color.cyan;

    [Header("디지털 효과 설정")]
    [Range(0, 1)]
    [SerializeField] private float glitchIntensity = 0.2f;
    [SerializeField] private float pulseSpeed = 1.0f;
    [SerializeField] private float noiseScale = 5.0f;

    private Renderer[] renderers;
    private float time;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        // 모든 렌더러의 머티리얼에 초기값 설정
        UpdateShaderProperties();
    }

    void Update()
    {
        time += Time.deltaTime * pulseSpeed;

        // 가장자리 효과 파라미터 업데이트
        float pulseFactor = Mathf.Sin(time) * 0.5f + 0.5f; // 0~1 사이의 맥동값
        float currentEdgeWidth = edgeWidth * (0.8f + pulseFactor * 0.4f);
        float currentGlowIntensity = edgeGlowIntensity * (0.9f + pulseFactor * 0.2f);

        // 움직임이나 애니메이션 상태에 따른 조정도 여기서 가능

        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                if (mat.HasProperty("_EdgeWidth"))
                    mat.SetFloat("_EdgeWidth", currentEdgeWidth);

                if (mat.HasProperty("_EdgeGlowIntensity"))
                    mat.SetFloat("_EdgeGlowIntensity", currentGlowIntensity);

                if (mat.HasProperty("_GlitchIntensity"))
                    mat.SetFloat("_GlitchIntensity", glitchIntensity);

                if (mat.HasProperty("_Time"))
                    mat.SetFloat("_Time", time);
            }
        }
    }

    // 효과 강도 일시적 증가 함수 (애니메이션 이벤트 등에서 호출)
    public void PulseEffect(float intensity = 1.0f)
    {
        StartCoroutine(PulseCoroutine(intensity));
    }

    private System.Collections.IEnumerator PulseCoroutine(float intensity)
    {
        float originalGlitch = glitchIntensity;
        glitchIntensity += intensity * 0.3f;
        glitchIntensity = Mathf.Clamp01(glitchIntensity);

        UpdateShaderProperties();

        yield return new WaitForSeconds(0.2f);

        glitchIntensity = originalGlitch;
        UpdateShaderProperties();
    }

    private void UpdateShaderProperties()
    {
        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                if (mat.HasProperty("_EdgeColor"))
                    mat.SetColor("_EdgeColor", edgeColor);

                if (mat.HasProperty("_EdgeWidth"))
                    mat.SetFloat("_EdgeWidth", edgeWidth);

                if (mat.HasProperty("_EdgeGlowIntensity"))
                    mat.SetFloat("_EdgeGlowIntensity", edgeGlowIntensity);

                if (mat.HasProperty("_GlitchIntensity"))
                    mat.SetFloat("_GlitchIntensity", glitchIntensity);

                if (mat.HasProperty("_NoiseScale"))
                    mat.SetFloat("_NoiseScale", noiseScale);
            }
        }
    }
}
