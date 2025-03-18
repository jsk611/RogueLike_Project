using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LegIKManager : MonoBehaviour
{
    public static LegIKManager instance;
    [SerializeField] float stepInterval = 3f;
    public enum LEGS
    {
        leftFront,
        rightFront,
        leftBack,
        rightBack
    }
    public Dictionary<LEGS, bool> legState;
    public float StepInterval => stepInterval;
    
    private void Start()
    {
        instance = this;
        legState = new Dictionary<LEGS, bool> { 
            { LEGS.leftFront, false },
            { LEGS.rightFront, false },
            { LEGS.leftBack, false },
            { LEGS.rightBack, false }
        };
    }

    public void UpdateLEGS(LEGS leg,bool value)
    {
        legState[leg] = value;
    }


}
