using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ExternSoundManager : MonoBehaviour
{
    public static ExternSoundManager instance;
    private AudioSource BGM;
    public float Main_Volume { get; set; }
    public float BGM_Volume { get; set; }
    public float Effect_Volume { get; set; }

    [SerializeField] AudioClip[] BGM_List;
    public Slider BGM_Slider;
    public Slider Effect_Slider;
    public Slider Main_Slider;


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
    }


    public void ChangeBGM(int stage)
    {
        BGM.clip = BGM_List[stage];
    }
    public void PlayBGM()
    {
        BGM.Play();
    }
    public void StopBGM()
    {
        BGM.Stop();
    }
    public void ChangeVolume()
    {
        Main_Volume = Main_Slider.value;
        BGM_Volume = BGM_Slider.value;
        Effect_Volume = Effect_Slider.value;

        BGM.volume = Main_Volume * BGM_Volume;
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
