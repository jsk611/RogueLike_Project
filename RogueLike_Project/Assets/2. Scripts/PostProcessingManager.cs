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
    bool isCritical = false;
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


        StartCoroutine(VignetteAnimation());
    }

    IEnumerator VignetteAnimation()
    {
        while (true)
        {
            if (isCritical)
            {
                while (vignette.intensity.value < 0.4f)
                {
                    vignette.intensity.value += Time.deltaTime * 0.2f;
                    yield return null;
                }
                while (vignette.intensity.value > 0.2f)
                {
                    vignette.intensity.value -= Time.deltaTime * 0.2f;
                    yield return null;
                }
            }
            else
            {
                while (vignette.intensity.value > 0f)
                {
                    vignette.intensity.value -= Time.deltaTime * 0.2f;
                    yield return null;
                }
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
        isCritical = active;
    }

    public void DamagedEffect(float intensity)
    {
        if (vignette.color.value == Color.white)
        {
            vignette.color.value = new Color(1f, 0.3f, 0.3f);
        }
        if (vignette.intensity.value <= 0.6f) vignette.intensity.value += intensity;
    }

}
