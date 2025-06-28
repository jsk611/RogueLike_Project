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

    public List<FootIK> Foots = new List<FootIK>();

    private void Start()
    {
        Foots.Add(frontLeft);
        Foots.Add(frontRight);
        Foots.Add(backLeft);
        Foots.Add(backRight);
        Foots.Add(midLeft);
        Foots.Add(midRight);
    }
    public void LegStop()
    {
        foreach (FootIK foot in Foots)
        {
            foot.LegControl(true);
        }
    }
    public void LegMove()
    {
        foreach(FootIK foot in Foots)
        {
            foot.LegControl(false);
        }
    }
    public void LegReset()
    {
        foreach(FootIK foot in Foots)
        {
            foot.LegReset();
        }
    }
}
