using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [SerializeField] Sprite[] progressImages;
    [SerializeField] Material[] beaconMaterials;
    [SerializeField] MeshRenderer beacon;
    [SerializeField] SpriteRenderer imageRenderer;

    public float maxTime = 6f;
    public float time;
    public bool isPlayerHolding;
    public bool isEnemyHolding;
    SpriteRenderer spriteRenderer;
    HashSet<GameObject> enemiesInZone = new HashSet<GameObject>();

    float progress;
    public bool isDone;

    private void Start()
    {
        time = 0;
        imageRenderer.sprite = progressImages[0];
        isDone = false;
        beacon.material = beaconMaterials[0];
    }
    private void Update()
    {
        if (enemiesInZone.Count == 0) isEnemyHolding = false;
        else isEnemyHolding = true;

        if (isDone) return;
        if (time >= maxTime)
        {
            imageRenderer.color = Color.green;
            imageRenderer.sprite = progressImages[5];
            isDone = true;
            beacon.material = beaconMaterials[3];
            UIManager.instance.ItemMissionUpdate();
            return;
        }
        if (!(isPlayerHolding ^ isEnemyHolding))
        {
            imageRenderer.color = Color.yellow;
            beacon.material = beaconMaterials[0];
        }
        else if (isPlayerHolding)
        {
            time = time < maxTime ? time + Time.deltaTime : maxTime;
            imageRenderer.color = Color.blue;
            beacon.material = beaconMaterials[1];
        }
        else if (isEnemyHolding)
        {
            time = time > 0 ? time - Time.deltaTime / 2 : 0;
            imageRenderer.color = Color.red;
            beacon.material = beaconMaterials[2];
        }

        progress = time / maxTime;
        if(progress > 0.8f)
        {
            imageRenderer.sprite = progressImages[4];
        }
        else if (progress > 0.6f)
        {
            imageRenderer.sprite = progressImages[3];
        }
        else if (progress > 0.4f)
        {
            imageRenderer.sprite = progressImages[2];
        }
        else if (progress > 0.2f)
        {
            imageRenderer.sprite = progressImages[1];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isPlayerHolding = true;
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Creature"))
        {
            enemiesInZone.Add(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isPlayerHolding = false;
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Creature"))
        {
            enemiesInZone.Remove(other.gameObject);
        }
    }
}
