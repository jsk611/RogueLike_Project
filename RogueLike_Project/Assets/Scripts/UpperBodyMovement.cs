using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpperBodyMovement : MonoBehaviour
{
    [SerializeField] Transform target;
    Vector3 relativeVec;

    Animator anim;
    Transform chest;
    Vector3 aim;

   

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        chest = anim.GetBoneTransform(HumanBodyBones.Chest);
        if (chest != null) Debug.Log("It is Spine");
    }

    // Update is called once per frame
    void LateUpdate()
    {
       aim = (target.position - chest.position );
       chest.rotation = Quaternion.Euler(aim);
       //chest.LookAt(target.position,Vector3.left);
    }


}
