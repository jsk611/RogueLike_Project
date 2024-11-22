using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DummyMonster : MonsterBase
{

    [Header("Settings")]
    [SerializeField] float attackRange = 10f;
    [SerializeField] float fireRate = 2f;
    [SerializeField] float rotationSpeed = 2f;

    public EnemyWeapon gun;
    public Transform firePoint;

    private FieldOfView fov;
    private float searchTargetDelay = 0.2f;

    private Quaternion initialWatchDirection;

    protected override void Start()
    {
        fov = GetComponent<FieldOfView>();
        initialWatchDirection = transform.rotation; // 몬스터의 초기 방향을 저장
        hp = 999999999; // 기본 체력 설정
        state = State.IDLE;
        base.Start();
    }

    protected override IEnumerator StateMachine()
    {
        yield return null;
    }

    private void Update()
    {
       
    }

   
}
