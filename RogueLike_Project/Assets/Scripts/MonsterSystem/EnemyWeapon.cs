using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    [SerializeField] Transform firePoint; //발사 위치;
    public GameObject bulletPrefab;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Fire(Quaternion rotation)
    {
        // 지정된 회전으로 총알 생성
        Instantiate(bulletPrefab, firePoint.position, rotation);
    }
}

