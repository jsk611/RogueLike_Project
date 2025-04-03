using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LegIKManager : MonoBehaviour
{
    [SerializeField] float stepInterval = 3f;
    public float StepInterval => stepInterval;


    public FootIK frontLeft;
    public FootIK frontRight;
    public FootIK midLeft;
    public FootIK midRight;
    public FootIK backLeft;
    public FootIK backRight;
    


}
