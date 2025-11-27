using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    public class PlayMonsterSound : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] List<AudioClip> Attack;
        [SerializeField] List<AudioClip> Hit;
        [SerializeField] List<AudioClip> Extra;
    
        private AudioClip audioClip;
        private IAudioManagerService audioManagerService;

        [SerializeField] private AudioSettings soundSettings = new AudioSettings(1.0f,0.0f,true);
        [SerializeField] private float delay;

        private void Start()
        {
            audioManagerService ??= ServiceLocator.Current.Get<IAudioManagerService>();
        }
        public void PlayAttackSound(int idx)
        {
            audioManagerService.PlayOneShotDelayed(Attack[idx], soundSettings, delay);
        }
        public void PlayHitSound(int idx)
        {
            audioManagerService.PlayOneShotDelayed(Hit[idx],soundSettings, delay);
        }
        public void PlayExtraSound(int dix)
        {
            audioManagerService.PlayOneShotDelayed(Extra[dix], soundSettings, delay);
        }
    }
}
