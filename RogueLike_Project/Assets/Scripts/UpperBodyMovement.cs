using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpperBodyMovement : MonoBehaviour
{
    [SerializeField] Transform target;
    Vector3 relativeVec;

    Animator anim;
    Transform spine;


   

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        spine = anim.GetBoneTransform(HumanBodyBones.Spine);
        if (spine != null) Debug.Log("It is Spine");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        
        spine.LookAt(target.position);
    }


}
