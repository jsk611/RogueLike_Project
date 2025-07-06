// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Manages the spawning and playing of sounds.
    /// </summary>
    public class AudioManagerService : MonoBehaviour,IAudioManagerService
    {
        /// <summary>
        /// Contains data related to playing a OneShot audio.
        /// </summary>
        private readonly struct OneShotCoroutine
        {
            /// <summary>
            /// Audio Clip.
            /// </summary>
            public AudioClip Clip { get; }
            /// <summary>
            /// Audio Settings.
            /// </summary>
            public AudioSettings Settings { get; }
            /// <summary>
            /// Delay.
            /// </summary>
            public float Delay { get; }
            
            /// <summary>
            /// Constructor.
            /// </summary>
            public OneShotCoroutine(AudioClip clip, AudioSettings settings, float delay)
            {
                //Clip.
                Clip = clip;
                //Settings
                Settings = settings;
                //Delay.
                Delay = delay;
            }
        }
     

        /// SOUNDTYPE
        /// MAIN : 1
        /// BGM : 2
        /// EFFECT : 3

        /// <summary>
        /// Destroys the audio source once it has finished playing.
        /// </summary>
        private IEnumerator DestroySourceWhenFinished(AudioSource source)
        {
            if (source == null) yield break;
            //Wait for the audio source to complete playing the clip.
            yield return new WaitForSeconds(source.clip.length);
        
            //Destroy the audio game object, since we're not using it anymore.
            //This isn't really too great for performance, but it works, for now.
            DestroyImmediate(source.gameObject);
        }

        /// <summary>
        /// Waits for a certain amount of time before starting to play a one shot sound.
        /// </summary>
        private IEnumerator PlayOneShotAfterDelay(OneShotCoroutine value, int soundType)
        {
            //Wait for the delay.
            yield return new WaitForSeconds(value.Delay);
            //Play.
            PlayOneShot_Internal(value.Clip, soundType);
        }
        
        /// <summary>
        /// Internal PlayOneShot. Basically does the whole function's name!
        /// </summary>
        private void PlayOneShot_Internal(AudioClip clip, int soundType)
        {
            //No need to do absolutely anything if the clip is null.
            if (clip == null)
                return;
            ExternSoundManager soundManager = ExternSoundManager.instance;
            AudioSettings settings = new AudioSettings(soundManager.Main_Volume*soundManager.Effect_Volume);


            //Spawn a game object for the audio source.
            var newSourceObject = new GameObject($"Audio Source -> {clip.name}");
            //Add an audio source component to that object.
            var newAudioSource = newSourceObject.AddComponent<AudioSource>();

            //Set volume.
            newAudioSource.volume = settings.Volume;
            //Set spatial blend.
            newAudioSource.spatialBlend = settings.SpatialBlend;
            //Set AudioMixer Group.
            newAudioSource.outputAudioMixerGroup = soundManager.Effect_MixerGroup;

            //Play the clip!
            newAudioSource.PlayOneShot(clip);
            Destroy(newSourceObject, clip.length);
        }

        #region Audio Manager Service Interface

        public void PlayOneShot(AudioClip clip, int soundType)
        {
            //Play.
            PlayOneShot_Internal(clip, soundType);
        }

        public void PlayOneShotDelayed(AudioClip clip, AudioSettings settings = default, float delay = 1.0f)
        {
            //Play.
            StartCoroutine(PlayOneShotAfterDelay(new OneShotCoroutine(clip, settings, delay),2));
        }

        #endregion
    }
}