// Copyright 2021, Infima Games. All Rights Reserved.

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

namespace InfimaGames.LowPolyShooterPack
{
    public class Inventory : InventoryBehaviour
    {
        #region FIELDS
        
        /// <summary>
        /// Array of all weapons. These are gotten in the order that they are parented to this object.
        /// </summary>
        private WeaponBehaviour[] weapons;

        /// <summary>
        /// Currently equipped WeaponBehaviour.
        /// </summary>
        private WeaponBehaviour equipped;
        /// <summary>
        /// Currently equipped index.
        /// </summary>
        private int equippedIndex = -1;

        public int maxInventorySize = 2;
        #endregion

        #region METHODS

        public override void Init(int equippedAtStart)
        {
            //Cache all weapons. Beware that weapons need to be parented to the object this component is on!
            weapons = GetComponentsInChildren<WeaponBehaviour>(true);

            //Disable all weapons. This makes it easier for us to only activate the one we need.
            for (int i =0; i<maxInventorySize;i++)
            {
                weapons[i].gameObject.SetActive(false);
            }
            //Equip.
            Equip(equippedAtStart);
        }

        public override WeaponBehaviour Equip(int index)
        {
            //If we have no weapons, we can't really equip anything.
            if (weapons == null)
            {
                return equipped;
            }

            //The index needs to be within the array's bounds.
            if (index > weapons.Length - 1)
                return equipped;

            //No point in allowing equipping the already-equipped weapon.

            //Disable the currently equipped weapon, if we have one.
            if (equipped != null)
                equipped.gameObject.SetActive(false);

            //Update index.
            equippedIndex = index;
            //Update equipped.
            equipped = weapons[equippedIndex];
            //Activate the newly-equipped weapon.
            equipped.gameObject.SetActive(true);

            //Return.
            return equipped;
        }

        public WeaponBehaviour ExcahngeEquip(int index)
        {
            //If we have no weapons, we can't really equip anything.
            if (weapons == null)
            {
                Debug.Log("null equip");
                return equipped;
            }

            if (equipped != null)
                equipped.gameObject.SetActive(false);

            //Update index.
            equippedIndex = index;
            //Update equipped.
            equipped = weapons[equippedIndex];
            //Activate the newly-equipped weapon.
            equipped.gameObject.SetActive(true);

            //Return.
            return equipped;
        }

        #endregion

        #region Getters

        public override int GetLastIndex()
        {
            //Get last index with wrap around.
            int newIndex = equippedIndex - 1;
            if (newIndex < 0)
                newIndex = maxInventorySize - 1;

            //Return.
            return newIndex;
        }

        public override int GetNextIndex()
        {
            //Get next index with wrap around.
            int newIndex = equippedIndex + 1;
            if (newIndex > maxInventorySize - 1)
                newIndex = 0;

            //Return.
            return newIndex;
        }

        public override void SwitchWeapons(int currentindex, WeaponBehaviour deletedWeapon, WeaponBehaviour newWeapon)
        {
            //Cache all weapons. Beware that weapons need to be parented to the object this component is on!
            weapons = GetComponentsInChildren<WeaponBehaviour>(true);
            //Disable all weapons. This makes it easier for us to only activate the one we need.
            foreach (WeaponBehaviour weapon in weapons)
            {
                weapon.gameObject.SetActive(false);
            }
            Destroy(deletedWeapon.gameObject);

            Equip(currentindex);

        }


        public override WeaponBehaviour GetEquipped() => equipped;
        public override int GetEquippedIndex() => equippedIndex;

        public override WeaponBehaviour GetOtherEquipped() => weapons[GetNextIndex()];


        #endregion
    }
}

