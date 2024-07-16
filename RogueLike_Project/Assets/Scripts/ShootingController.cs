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
            Debug.Log(weaponController.weaponAnimation.name);
            playerAnimator.Play(weaponController.weaponAnimation.name); 
            
            //playerAnimator.Play();
        }
        if (Input.GetMouseButton(1))
        {
            playerAnimator.SetTrigger("targeting");
        }
        if (Input.GetKey(KeyCode.R))
        {
            playerAnimator.SetTrigger("reloading");
            playerAnimator.Play("Reload");
        }

    }
}
