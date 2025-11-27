using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy Definition", fileName = "Enemy_XXX")]
public class EnemyDefinition : ScriptableObject
{
    [Header("Identity")]
    public EnemyType type;
    public GameObject prefab;

    [Header("Base Stats")]
    public float maxHealth = 100f;
    public float attackPower = 10f;
    public float moveSpeed = 3.5f;
    public float attackInterval = 1.0f;
    public float detectionRange = 10f;
    // 필요하면 방어력, 탄속 등 추가

    [Header("Spawn Settings")]
    [Tooltip("이 라운드 인덱스부터 풀에 등장 (예: 1-1=1, 1-2=2, ... 2-1=11 이런 식)")]
    public int unlockRound = 1;
    public float baseWeight = 1.0f;
}
