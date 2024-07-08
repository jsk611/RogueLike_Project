using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class WeaponControl : MonoBehaviour
{
    //int remains = 5;
    //float shootSpeed = 0.5f;
    //float reloadSpeed = 2f;
    //float bulletSpeed = 1f;
    //float weight = 5f;

    [SerializeField] GameObject first_weapon;
    [SerializeField] GameObject second_weapon;
    [SerializeField] GameObject melee_weapon;
    [SerializeField] GameObject[] weapons;
    public GameObject currentWeapon;

    Animator playerAnimator;
    public AnimationClip weaponAnimation;
    // Start is called before the first frame update
    void Start()
    {
        
        weaponAnimation = first_weapon.GetComponent<Weapon>().GetAnimation();
        currentWeapon = first_weapon;
        playerAnimator = GetComponent<Animator>();
    }
    
    // Update is called once per frame
    void Update()
    {
        switchingWeapon();
        weaponAnimation = currentWeapon.GetComponent<Weapon>().GetAnimation();
        
    }

    private void switchingWeapon()
    {
        if (Input.GetKey("1"))
        {
            currentWeapon.SetActive(false);
            currentWeapon = first_weapon;
            currentWeapon.SetActive(true);
            weaponAnimation = first_weapon.GetComponent<Weapon>().GetAnimation();
        }
        else if (Input.GetKey("2"))
        {
            currentWeapon.SetActive(false);
            currentWeapon = second_weapon;
            currentWeapon.SetActive(true);
            weaponAnimation = second_weapon.GetComponent<Weapon>().GetAnimation();
        }
        else if (Input.GetKey("3"))
        {
            currentWeapon.SetActive(false);
            currentWeapon = melee_weapon;
            currentWeapon.SetActive(true);
            weaponAnimation = melee_weapon.GetComponent<Weapon>().GetAnimation();
        }
    }
    public GameObject GetCurrentWeapon()
    {
        return currentWeapon;
    }

}
