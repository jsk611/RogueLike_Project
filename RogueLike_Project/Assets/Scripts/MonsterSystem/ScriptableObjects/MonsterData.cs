using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "MonsterScriptable/CreateMonsterData", order = int.MaxValue)]
public class MonsterData : ScriptableObject
{
    [Header("Settings")]
    [SerializeField]
    private string monsterName;
    public string Name { get { return monsterName; } set { monsterName = value; } }

    [SerializeField]
    private string monsterID;
    public string ID { get { return monsterID; } set { monsterID = value; } }

    [Header("Monster Basic stats")]
    [SerializeField]
    private float hp;
    public float HP { get { return hp; } set { hp = value; } }

    [SerializeField]
    private float def;
    public float Def { get { return def; } set { def = value; } }

    [SerializeField]
    private float damage;
    public float Damage { get { return damage; } set { damage = value; } }




    [Header("Monster CombatSystem stats")]
    [SerializeField]
    private float attackRange;
    public float AttackRange { get { return attackRange; } set { attackRange = value; } }

    private float closeRange;
    public float CloaseRange { get { return closeRange; } set { closeRange = value; } }

    [SerializeField]
    private float coolDown;
    public float CoolDown { get { return coolDown; } set { coolDown = value; } }

    [SerializeField]
    private float walkSpeed;
    public float Walkspeed { get { return walkSpeed; } set { walkSpeed = value; } }

}
