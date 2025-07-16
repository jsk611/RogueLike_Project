using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWeapon : MonoBehaviour
{
    BoxCollider boxCollider;
    Character character;
    Inventory inventory;

    [SerializeField] WeaponType type;

    [Header("Weapon Prefab")]
    [SerializeField] GameObject weapon;

    [Header("Weapon Position")]
    [SerializeField]
    Vector3 Position;

    [Header("Weapon Rotation")]
    [SerializeField]
    Quaternion Rotation;

    // Start is called before the first frame update    
    void Start()
    {
        CheckUnlocked();

        boxCollider = GetComponent<BoxCollider>();
        character = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<Character>();
        inventory = FindAnyObjectByType<Inventory>();

    }

    // Update is called once per frame
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && character.CanExchangeWeapon())
        {
            if (Input.GetKey(KeyCode.F)) character.OnTryExchangeWeapon(weapon,Position,Rotation);
        }
           // StartCoroutine("ExchangeWeapon");
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
            StopCoroutine("ExchangeWeapon");
    }
    private IEnumerator ExchangeWeapon()
    {
            if (Input.GetKey(KeyCode.F))
                character.OnTryExchangeWeapon(weapon,Position,Rotation);
            
            yield return null;
    }
    public void CheckUnlocked()
    {
        if (PermanentUpgradeManager.instance.weaponLockData.GetWeaponLock(type) == false)
        {
            gameObject.SetActive(false);
            return;
        }
        else gameObject.SetActive(true);
    }
    

}


