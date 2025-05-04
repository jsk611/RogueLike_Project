using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    public class PlayMonsterSound : StateMachineBehaviour
    {
        // Start is called before the first frame update
        private enum SoundType
        {
            Attack,
            Hit,
            Extra1,
            Extra2,
            Extra3,
            Extra4,
        }
        private AudioClip audioClip;
        private IAudioManagerService audioManagerService;

        [SerializeField] private MonsterBase monster;
        [SerializeField] private SoundType soundType;
        [SerializeField] private AudioSettings soundSettings = new AudioSettings(1.0f,0.0f,true);
        [SerializeField] private bool playOnEnter = true;
        [SerializeField] private float delay;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!playOnEnter) return;

            audioManagerService ??= ServiceLocator.Current.Get<IAudioManagerService>();

            audioClip = soundType switch
            {
                SoundType.Attack => monster.AttackSound,
                SoundType.Hit => monster.HitSound,
                SoundType.Extra1 => monster.ExtraSounds[0],
                SoundType.Extra2 => monster.ExtraSounds[1],
                SoundType.Extra3 => monster.ExtraSounds[2],
                SoundType.Extra4 => monster.ExtraSounds[3],
                _ => default
            };
            Debug.Log(audioClip.name);
            audioManagerService.PlayOneShotDelayed(audioClip, soundSettings, delay);
        }
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (playOnEnter) return;
            audioManagerService ??= ServiceLocator.Current.Get<IAudioManagerService>();

            audioClip = soundType switch
            {
                SoundType.Attack => monster.AttackSound,
                SoundType.Hit => monster.HitSound,
                SoundType.Extra1 => monster.ExtraSounds[0],
                SoundType.Extra2 => monster.ExtraSounds[1],
                SoundType.Extra3 => monster.ExtraSounds[2],
                SoundType.Extra4 => monster.ExtraSounds[3],
                _ => default
            };

            audioManagerService.PlayOneShotDelayed(audioClip, soundSettings, delay);
        }
    }
}
