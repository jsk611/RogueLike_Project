using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPart : MonoBehaviour
{
    [SerializeField] private string partName; // 부위 이름
    [SerializeField] private Collider collider; // 부위의 Collider
    [SerializeField] private float damageMultiplier = 1.0f; // 데미지 배율

    public string PartName => partName;
    public Collider Collider => collider;
    public float DamageMultiplier => damageMultiplier;
}
