using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 보스 타입의 기본 인터페이스
/// </summary>
public interface IBossEntity
{
    float GetBaseDamage();
    float GetDamageMultiplier();
    bool IsInSpecialState();
}