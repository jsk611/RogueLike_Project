using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpperBodyMovement : MonoBehaviour
{
    [SerializeField] Transform target;
    Vector3 relativeVec;

    Animator anim;
    Transform chest;
    Transform eye;

   

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        chest = anim.GetBoneTransform(HumanBodyBones.Chest);
        eye = GetComponent<Transform>();
        if (chest != null) Debug.Log("It is Spine");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        eye.rotation = Quaternion.Euler(target.position - chest.position );
        chest.rotation = eye.rotation;
       // chest.LookAt(target.position);
    }


}
