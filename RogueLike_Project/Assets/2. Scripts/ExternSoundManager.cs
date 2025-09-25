using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Rendering;
using UnityEngine.UI;
using DG.Tweening;

public class ExternSoundManager : MonoBehaviour
{       
    public static ExternSoundManager instance;
    private AudioSource BGM;
    public float Main_Volume { get; set; }
    public float BGM_Volume { get; set; }
    public float Effect_Volume { get; set; }

    [SerializeField] AudioMixer Main_Mixer;
    public AudioMixerGroup Effect_MixerGroup;

    [SerializeField] AudioClip[] BGM_List;
    public Slider BGM_Slider;
    public Slider Effect_Slider;
    public Slider Main_Slider;

    // 랜덤 BGM 관련 변수
    [SerializeField] private bool isRandomBGMPlaying = false;
    [SerializeField] private float originalBGMVolume;
    [SerializeField] private bool isVolumeReduced = false;
    [SerializeField] private int currentRandomBGMIndex = -1;

    /// SOUNDTYPE
    /// MAIN : 1
    /// BGM : 2
    /// EFFECT : 3

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        BGM = GetComponent<AudioSource>();
        BGM.clip = BGM_List[0];
        Main_Volume = Main_Slider.value;
        BGM_Volume = Main_Volume * BGM_Slider.value;
        Effect_Volume = Main_Volume * Effect_Slider.value;

        BGM.volume = BGM_Volume;
        originalBGMVolume = BGM_Volume;
        ChangeVolume();
    }

    void Update()
    {
            // Update 메서드에서 자동 BGM 전환 로직 제거
        // 한번 선택된 랜덤 BGM은 계속 재생됨
    }

    public void StartRandomBGM()
    {
        if (!isRandomBGMPlaying)
        {
            isRandomBGMPlaying = true;
            PlayNextRandomBGM();
        }
    }

    public void StopRandomBGM()
    {
        isRandomBGMPlaying = false;
        currentRandomBGMIndex = -1;
        BGM.Stop();
    }

    private void PlayNextRandomBGM()
    {
        if (isRandomBGMPlaying && BGM_List.Length > 0)
        {
            int randomIndex = Random.Range(0, BGM_List.Length);
            currentRandomBGMIndex = randomIndex;
            BGM.clip = BGM_List[randomIndex];
            BGM.loop = true; // 선택된 BGM을 계속 반복 재생
            BGM.Play();
        }
    }

    public void ReduceBGMVolume()
    {
        if (isRandomBGMPlaying && !isVolumeReduced)
        {
            isVolumeReduced = true;
            BGM.DOFade(originalBGMVolume * 0.3f, 0.5f);
        }
    }

    public void RestoreBGMVolume()
    {
        if (isRandomBGMPlaying && isVolumeReduced)
        {
            isVolumeReduced = false;
            BGM.DOFade(originalBGMVolume, 0.5f);
        }
    }

    public void ChangeBGM(int stage, float duration)
    {
        // 랜덤 BGM 모드 종료
        if (isRandomBGMPlaying)
        {
            StopRandomBGM();
        }
        
        BGM.DOFade(0, duration).OnComplete(() =>
        {
            BGM.clip = BGM_List[stage];
            BGM.loop = false; // 일반 BGM은 루프하지 않음
            BGM.Play();
            BGM.volume = originalBGMVolume;
        });
    }
    
    public void PlayBGM()
    {
        BGM.Play();
    }
    
    public void StopBGM()
    {
        BGM.Stop();
    }

    private float ConvertVolume(float sliderValue)
    {
        return sliderValue == 0 ? -80 : (sliderValue - 1) * 20;
    } 
    
    public void ChangeVolume()
    {
        Main_Mixer.SetFloat("MasterVolume", ConvertVolume(Main_Slider.value));
        Main_Mixer.SetFloat("BGMVolume", ConvertVolume(BGM_Slider.value));
        Main_Mixer.SetFloat("EffectVolume", ConvertVolume(Effect_Slider.value));
        
        // 원본 볼륨 업데이트
        originalBGMVolume = Main_Volume * BGM_Volume;
    }
    
    public void Mute()
    {
        BGM.mute = true;
    }
    
    public void UnMute()
    {
        BGM.mute = false;
    }
}
