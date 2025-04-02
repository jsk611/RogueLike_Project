// Copyright 2021, Infima Games. All Rights Reserved.

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Game Mode Service.
    /// </summary>
    public class GameModeService : IGameModeService
    {
        #region FIELDS
        
        /// <summary>
        /// The Player Character.
        /// </summary>
        private CharacterBehaviour playerCharacter;
        private UpgradeManager_New upgradeManager;
        private KillingEffect KillingEffect;
        
        #endregion
        
        #region FUNCTIONS
        
        public CharacterBehaviour GetPlayerCharacter()
        {
            //Make sure we have a player character that is good to go!
            if (playerCharacter == null)
                playerCharacter = UnityEngine.Object.FindObjectOfType<CharacterBehaviour>();
            
            //Return.
            return playerCharacter;
        }
        public UpgradeManager_New GetUpgradeManager()
        {
            if (upgradeManager == null)
                upgradeManager = UnityEngine.Object.FindObjectOfType<UpgradeManager_New>();

            return upgradeManager;
        }
        public KillingEffect GetKillingEffect()
        {
            if (KillingEffect == null)
                KillingEffect = UnityEngine.Object.FindObjectOfType<KillingEffect>();
            return UnityEngine.Object.FindObjectOfType<KillingEffect>();
        }

        #endregion
    }
}