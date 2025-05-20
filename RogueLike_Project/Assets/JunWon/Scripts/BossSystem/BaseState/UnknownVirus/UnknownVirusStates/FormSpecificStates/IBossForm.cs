using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnknownVirusBoss;

public interface IBossForm
{
    // 식별자 및 기본 정보
    string FormName { get; }
    BossForm FormType { get; }

    // 라이프사이클 메서드
    void Initialize(UnknownVirusBoss controller);
    void Activate();
    void Deactivate();

    // 상태 관리
    void SaveState();
    void LoadState();

    // 전투 관련
    void HandleAttack();
    void HandleSpecialAbility(string abilityName);
    void HandleMovement(Vector3 targetPosition);

    // 데미지 및 체력 관련
    void TakeDamage(float damage, bool showEffect);
    float GetCurrentHealthRatio();

    // AI 결정
    float EvaluateFormEffectiveness(BossCombatContext context);
    BossForm SuggestNextForm(BossCombatContext context);
}
