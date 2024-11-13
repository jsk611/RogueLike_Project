using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    [SerializeField] Transform firePoint; //발사 위치;
    public GameObject bulletPrefab;
    public SkinnedMeshRenderer smRenderer;
    public MonsterStatus monsterStatus;

    // Start is called before the first frame update
    void Start()
    {
        firePoint = smRenderer.GetComponent<SkinnedMeshRenderer>().bones[0].transform;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Fire()
    {

        // 지정된 회전으로 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, transform.rotation);
        bullet.GetComponent<MProjectile>().SetBulletDamage(monsterStatus.GetAttackDamage()*monsterStatus.CalculateCriticalHit());
    }
}

