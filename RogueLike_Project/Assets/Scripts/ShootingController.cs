using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingController : MonoBehaviour
{
    Animator playerAnimator;

    WeaponControl weaponController;
    [SerializeField] GameObject weapon;
    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        shooting();
        weaponController = weapon.GetComponent<WeaponControl>();
    }
    private void shooting()
    {
        if (Input.GetMouseButton(0))
        {
            playerAnimator.SetTrigger(weaponController.GetCurrentWeapon().GetComponent<Weapon>().GetShootingName());
            Debug.Log("shooting");
            //playerAnimator.Play();
        }
        if (Input.GetMouseButton(1))
        {
            Debug.Log("targeting");
            playerAnimator.SetTrigger("targeting");
        }
        if (Input.GetKey(KeyCode.R))
        {
            Debug.Log("Reroad");
            playerAnimator.Play("Reload");
        }

    }
}
