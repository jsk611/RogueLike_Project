using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class WeaponControl : MonoBehaviour
{
    int remains = 5;
    float shootSpeed = 0.5f;
    float reloadSpeed = 2f;
    float bulletSpeed = 1f;
    float weight = 5f;

    [SerializeField] GameObject first_weapon;
    [SerializeField] GameObject second_weapon;
    [SerializeField] GameObject melee_weapon;

    Animator playerAnimator;
    Animation weaponAnimation;
    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        weaponAnimation = first_weapon.GetComponent<Animation>();
    }
    
    // Update is called once per frame
    void Update()
    {
        switchingWeapon();
        shooting();
    }

    private void switchingWeapon()
    {
        if (Input.GetKey("1"))
        {
            GetComponent<MeshRenderer>().material = first_weapon.GetComponent<MeshRenderer>().material;
            weaponAnimation = first_weapon.GetComponent<Animation>();
        }
        else if (Input.GetKey("2"))
        {
            GetComponent<MeshRenderer>().material = second_weapon.GetComponent<MeshRenderer>().material;
            weaponAnimation = second_weapon.GetComponent<Animation>();
        }
        else if (Input.GetKey("3"))
        {
            GetComponent<MeshRenderer>().material = melee_weapon.GetComponent<MeshRenderer>().material;
            weaponAnimation = melee_weapon.GetComponent<Animation>();
        }
    }
    private void shooting()
    {
        if (Input.GetMouseButton(0))
        {
            playerAnimator.SetTrigger("shooting");
            Debug.Log(weaponAnimation.name);
            playerAnimator.Play(weaponAnimation.name);
        }
        if (Input.GetMouseButton(1)) Debug.Log("targeting");
        if (Input.GetKey(KeyCode.R))
        {
            Debug.Log("Reroad");
            playerAnimator.Play("Reload");
        }

    }
}
