using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWeapon : MonoBehaviour
{
    BoxCollider boxCollider;
    Character character;
    Inventory inventory;

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
        boxCollider = GetComponent<BoxCollider>();
        character = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<Character>();
        inventory = FindAnyObjectByType<Inventory>();
    }

    // Update is called once per frame
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && !character.IsWeaponExchangeLocked())
            StartCoroutine("ExchangeWeapon");
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
            StopCoroutine("ExchangeWeapon");
    }
    private IEnumerator ExchangeWeapon()
    {
            if (Input.GetKey(KeyCode.F))
                character.OnTryExchangeWeapon(weapon.GetComponent<WeaponBehaviour>(),Position,Rotation);
            
            yield return null;
    }

    

}


