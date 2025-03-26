using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeeleWeapon : BaseWeapon
{
    [Header("근접 무기 설정")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private bool applyKnockback = true;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        // 추가적인 근접 무기 효과 (예: 넉백)
        if (applyKnockback && isCollisionEnabled && other.CompareTag("Player"))
        {
            if (applyKnockback)
            {
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 direction = (other.transform.position - transform.position).normalized;
                    direction.y = 0.2f;
                    rb.AddForce(direction * knockbackForce, ForceMode.Impulse);

                }
            }
        }
    }

    // 근접 무기 특화 기능 추가 가능
}
