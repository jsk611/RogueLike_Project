using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWeapon : MonoBehaviour
{
    BoxCollider boxCollider;
    Character character;
    Inventory inventory;
    bool canExchangeWeapon = true;

    [Header("Weapon Prefab")]
    [SerializeField] GameObject weapon;
    // Start is called before the first frame update    
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        character = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<Character>();
        inventory = FindAnyObjectByType<Inventory>();
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
            StartCoroutine("ExchangeWeapon");
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
            StopCoroutine("ExchangeWeapon");    
    }
    private IEnumerator ExchangeWeapon()
    {
        while (canExchangeWeapon)
        {
            if (Input.GetKey(KeyCode.F) )
            {
                canExchangeWeapon = false;
                Debug.Log(character.name);
                Instantiate(weapon, inventory.transform);
                character.Exchange(weapon.GetComponent<WeaponBehaviour>());
                Destroy(gameObject);
            }
            Debug.Log("Switch weapon?");
            yield return null;
        }
    }
}
