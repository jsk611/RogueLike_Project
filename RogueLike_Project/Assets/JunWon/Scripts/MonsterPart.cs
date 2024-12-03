using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPart : MonoBehaviour
{
    [SerializeField] private MonsterStatus mStat;
    [SerializeField] private MonsterBase mBase;
    [SerializeField] private string partName; // ���� �̸�
    [SerializeField] private float damageMultiplier = 1.0f; // ������ ����

    private void Start()
    {
        mStat = GetComponentInParent<MonsterStatus>();
        mBase = GetComponentInParent<MonsterBase>();
    }

}
