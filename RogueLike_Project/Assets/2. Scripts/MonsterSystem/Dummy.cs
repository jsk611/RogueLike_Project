using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Dummy : MonoBehaviour
{
    [SerializeField] GameObject UIDamaged;
    public virtual void TakeDamage(float damage)
    {
        // 체력 감소 처리
        UIDamage uIDamage = Instantiate(UIDamaged, transform.position, Quaternion.identity).GetComponent<UIDamage>();
        uIDamage.damage = damage;
    }
}
