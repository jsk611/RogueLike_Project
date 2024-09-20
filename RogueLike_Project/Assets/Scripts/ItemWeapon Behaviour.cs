using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWeaponBehaviour : MonoBehaviour
{
    

    protected bool canExchangeWeapon = true;
    protected IEnumerator ExchangeCoolTime()
    {
        yield return new WaitForSeconds(2);
        canExchangeWeapon = true;
    }

    public void CanSwitchWeapon()
    {
        canExchangeWeapon = true;
    }
}
