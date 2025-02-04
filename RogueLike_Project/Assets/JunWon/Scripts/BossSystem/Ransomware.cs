using System.Buffers;
using System.Xml;
using UnityEngine;
using UnityEngine.AI;

public class Ransomware : MonoBehaviour
{
    public float health = 100f;
    public bool isPlayerDetected = false;
    public bool isPlayerInAttackRange = false;

    private StateMachine<Ransomware> fsm;


    [Header("Components")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected NavMeshAgent nmAgent;
    [SerializeField] protected FieldOfView fov;
    [SerializeField] protected MonsterStatus monsterStatus;
    [SerializeField] private Rigidbody playerRigidBody;



    public bool IsIntroAnimFinished = false;

    void Start()
    {
        var introState = new IntroState_Ransomeware(this);
        var phase1State = new Phase1State_Ransomware(this);
        var deadState = new DefeatedState_Ransomeware(this);
        var HitState = new DefeatedState_Ransomeware(this);

        fsm = new StateMachine<Ransomware>(introState);

        var introToPhase1 = new Transition<Ransomware>(
            introState, 
            phase1State, 
            () => IsIntroAnimFinished);

        var anyToDead = new Transition<Ransomware>(
            null,
            deadState,
            () => monsterStatus.GetHealth() <= 0f);



        fsm.AddTransition(introToPhase1);
        fsm.AddTransition(anyToDead);
    }

    void Update()
    {
        fsm.Update();
    }

    public void TakeDamage(float dmg)
    {
    }
}
