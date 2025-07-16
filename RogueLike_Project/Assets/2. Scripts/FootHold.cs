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

    private Vector3 desiredWorldScale;
    void Start()
    {
        time = 0;
        material = GetComponent<MeshRenderer>().material;
        desiredWorldScale = transform.lossyScale;
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
            time = time > 0 ? time - Time.deltaTime/4 : 0;
            material.SetColor("_Color", Color.red);
        }
        progress = time / maxTime;
        material.SetFloat("_progress", progress);
        UIManager.instance.CaptureMissionUpdate(progress);
    }
    void LateUpdate()
    {
        // 현재 부모 스케일 반영하여 localScale을 조정
        Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(
            desiredWorldScale.x / parentScale.x,
            desiredWorldScale.y / parentScale.y,
            desiredWorldScale.z / parentScale.z
        );
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
