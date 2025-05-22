// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Bootstraper.
    /// </summary>
    public static class Bootstraper
    {
        
        /// <summary>
        /// Initialize.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            //Initialize default service locator.
            ServiceLocator.Initialize();
            
            //Game Mode Service.
            ServiceLocator.Current.Register<IGameModeService>(new GameModeService());

            #region Sound Manager Service

            //Create an object for the sound manager, and add the component!

            if (GameObject.Find("Sound Manager") == null)
            {
                GameObject soundManagerObject = new GameObject("Sound Manager");
                AudioManagerService soundManagerService = soundManagerObject.AddComponent<AudioManagerService>();
                //Make sure that we never destroy our SoundManager. We need it in other scenes too!
                Object.DontDestroyOnLoad(soundManagerObject);
            
                //Register the sound manager service!
                ServiceLocator.Current.Register<IAudioManagerService>(soundManagerService);
            }


            #endregion
        }
    }
}