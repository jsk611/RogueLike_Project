using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : MonoBehaviour
{
    Volume volume;
    Vignette vignette;
    ChromaticAberration chromatic;
    ColorAdjustments colorAdjustments;
    // Start is called before the first frame update
    void Start()
    {
        volume = GetComponent<Volume>();
        Vignette tmp;
        if (volume.profile.TryGet<Vignette>(out tmp))
        {
            vignette = tmp;
        }
        ChromaticAberration tmp1;
        if (volume.profile.TryGet<ChromaticAberration>(out tmp1))
        {
            chromatic = tmp1;
        }
        ColorAdjustments tmp2;
        if (volume.profile.TryGet<ColorAdjustments>(out tmp2))
        {
            colorAdjustments = tmp2;
        }

        StartCoroutine(VignetteAnimation());
    }

    IEnumerator VignetteAnimation()
    {
        while (true)
        {

            while(vignette.intensity.value < 0.4f)
            {
                vignette.intensity.value += Time.deltaTime * 0.2f;
                yield return null;
            }
            while (vignette.intensity.value > 0.2f)
            {
                vignette.intensity.value -= Time.deltaTime * 0.2f;
                yield return null;
            }

            yield return null;
        }
    }

    public void ChangeVignetteColor(Color color)
    {
        vignette.color.value = color;
    }

    public void ChangeChromaticAberrationActive(bool active)
    {
        chromatic.active = active;
    }

    Coroutine coroutine;
    public void DamagedEffect()
    {
        if(coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(DamagedEffectCoroutine());
    }

    IEnumerator DamagedEffectCoroutine()
    {
        float colorVal = 0.8f;
        colorAdjustments.colorFilter.value = new Color(1, colorVal, colorVal);

        while(colorVal < 1)
        {
            colorVal += Time.deltaTime*0.8f;
            colorAdjustments.colorFilter.value = new Color(1, colorVal, colorVal);
            yield return null;
        }
    }
}
