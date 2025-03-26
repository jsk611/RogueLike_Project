using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 기본 무기 인터페이스
/// </summary>
public interface IWeapon
{
    void EnableCollision();
    void DisableCollision();
    void SetDamage(float damage);
    void UpdateDamageFromSource();
    void ApplyHitEffect(Vector3 hitPoint, GameObject target);
    GameObject GetGameObject();
}
