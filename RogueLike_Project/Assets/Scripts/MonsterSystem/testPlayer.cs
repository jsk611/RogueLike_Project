using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testPlayer : MonoBehaviour
{
    private RangedMonster rangedMonster; // RangedMonster를 참조할 수 있는 변수
    private MeeleMonster meeleMonster; // RangedMonster를 참조할 수 있는 변수
    [SerializeField] bool attack;

    void Start()
    {
        attack = false;
        // RangedMonster 객체를 찾습니다.
        rangedMonster = FindObjectOfType<RangedMonster>();
        meeleMonster = FindObjectOfType<MeeleMonster>();

        if (rangedMonster == null)
        {
            Debug.LogError("No RangedMonster found in the scene.");
        }
        if (meeleMonster == null)
        {
            Debug.LogError("No MeeleMonster found in the scene.");

        }
    }

    void Update()
    {
        // 예를 들어, 키보드 입력에 따라 데미지를 줄 수 있습니다.
        if (Input.GetKeyDown(KeyCode.N))
        {
            // RangedMonster가 할당되어 있으면 TakeDamage 메서드를 호출합니다.
            rangedMonster.TakeDamage(5, gameObject.transform); // 예: 5의 데미지를 줍니다.
            meeleMonster.TakeDamage(5, gameObject.transform);
            Debug.Log("Damage dealt to RangedMonster.");
            Debug.Log("Damage dealt to meeleMonster.");
        }
    }
}
