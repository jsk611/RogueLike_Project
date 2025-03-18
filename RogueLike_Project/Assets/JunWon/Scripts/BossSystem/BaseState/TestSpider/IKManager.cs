using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKManager : MonoBehaviour
{
    RobotJoint[] Joints;
    public Vector3 ForwardKinematics(float[] angles)
    {
        Vector3 prevPoint = Joints[0].transform.position;
        Quaternion rotation = Quaternion.identity;
        for (int i = 1; i < Joints.Length; i++)
        {
            // Rotates around a new axis
            rotation *= Quaternion.AngleAxis(angles[i - 1], Joints[i - 1].Axis);
            Vector3 nextPoint = prevPoint + rotation * Joints[i].StartOffset;

            prevPoint = nextPoint;
        }
        return prevPoint;
    }
    
    public float DistanceFromTarget(Vector3 target, float[] angles)
    {
        Vector3 point = ForwardKinematics(angles);
        return Vector3.Distance(point, target);
    }
    public float PartialGradient(Vector3 target, float[] angles, int i)
    {
        float SamplingDistance = 1;
        float angle = angles[i];
        float f_x = DistanceFromTarget(target, angles);
        angles[i] += SamplingDistance;
        float f_x_plus_d = DistanceFromTarget(target, angles);
        float gradient = (f_x_plus_d - f_x) / SamplingDistance;
        angles[i] = angle;
        return gradient;
    }
    public void InverseKinematics(Vector3 target, float[] angles)
    {
        float LearningRate = 0.3f;
        for (int i = 0; i < Joints.Length;i++)
        {
            float gradient = PartialGradient(target, angles, i);
            angles[i] -= LearningRate * gradient;
        }
    }
}
