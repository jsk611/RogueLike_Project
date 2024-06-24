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

    [SerializeField] Material first_weapon;
    [SerializeField] Material second_weapon;
    [SerializeField] Material melee_weapon;

    // Start is called before the first frame update
    void Start()
    {

    }
    
    // Update is called once per frame
    void Update()
    {
        switchingWeapon();
    }

    private void switchingWeapon()
    {
        if (Input.GetKey("1")) GetComponent<MeshRenderer>().material = first_weapon;
        else if (Input.GetKey("2")) GetComponent<MeshRenderer>().material = second_weapon;
        else if (Input.GetKey("3")) GetComponent<MeshRenderer>().material = melee_weapon;
    }
}
