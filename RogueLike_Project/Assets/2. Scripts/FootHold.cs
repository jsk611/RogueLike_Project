using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootHold : MonoBehaviour
{
    public float maxTime;
    public float time;
    public bool isPlayerHolding;
    public bool isEnemyHolding;

    Material material;
    public float progress;

    HashSet<GameObject> enemiesInZone = new HashSet<GameObject>();
    void Start()
    {
        time = 0;
        material = GetComponent<MeshRenderer>().material;
    }
    private void Update()
    {
        if (enemiesInZone.Count == 0) isEnemyHolding = false;
        else isEnemyHolding=true;

        if(time >= maxTime)
        {
            material.SetColor("_Color", Color.green);
            return;
        }
        if (!(isPlayerHolding^isEnemyHolding))
        {
            material.SetColor("_Color", Color.yellow);
        }
        else if (isPlayerHolding)
        {
            time = time < maxTime ? time + Time.deltaTime : maxTime;
            material.SetColor("_Color", Color.blue);
        }
        else if(isEnemyHolding)
        {
            time = time > 0 ? time - Time.deltaTime/2 : 0;
            material.SetColor("_Color", Color.red);
        }
        progress = time / maxTime;
        material.SetFloat("_progress", progress);
        UIManager.instance.CaptureMissionUpdate(progress);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isPlayerHolding = true;
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Creature"))
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
