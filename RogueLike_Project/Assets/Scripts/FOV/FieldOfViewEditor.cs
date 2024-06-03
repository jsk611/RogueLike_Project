using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    float radius = 0.0f;
    float angle = 0.0f;
    void OnSceneGUI()
    {
        FieldOfView fow = (FieldOfView)target;
        radius = fow.GetRadius();
        angle = fow.GetAngle();
        Handles.color = Color.black;
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, angle);
        Vector3 viewAngleA = fow.DirFromAngle(-angle / 2, false);
        Vector3 viewAngleB = fow.DirFromAngle(angle / 2, false);

        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleA * radius);
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleB * radius);

        Handles.color = Color.red;
        foreach (Transform visible in fow.visibleTargets)
        {
            Handles.DrawLine(fow.transform.position, visible.transform.position);
        }
    }
}