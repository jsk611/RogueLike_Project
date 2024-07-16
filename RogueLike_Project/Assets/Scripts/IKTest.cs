using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKTest : MonoBehaviour
{
    WeaponControl weapon;
    GameObject current_weapon;

    Animator playerAnimator;
    int shootingLayer;
    // Start is called before the first frame update
    void Start()
    {
        weapon = GameObject.Find("Weapons").GetComponent<WeaponControl>();
        playerAnimator = GetComponent<Animator>();
        shootingLayer = playerAnimator.GetLayerIndex("Shooting");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        current_weapon = weapon.GetCurrentWeapon();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (layerIndex != shootingLayer)
        {
            return;
        }
            //playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            //playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);

        playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand, current_weapon.GetComponentsInChildren<Transform>()[2].position);
        playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand, current_weapon.GetComponentsInChildren<Transform>()[2].rotation);
    }
}
