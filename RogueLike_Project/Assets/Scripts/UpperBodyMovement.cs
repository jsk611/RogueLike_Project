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
        
    }

    // Update is called once per frame
    void Update()
    {
        spine.transform.position += new Vector3(0, 1, 0);
        relativeVec = target.position;
        spine.LookAt(target.position);
        spine.rotation *= Quaternion.Euler(relativeVec);
    }
}
