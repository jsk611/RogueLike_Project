using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class UpperBodyMovement : MonoBehaviour
{
    [SerializeField] Transform target;
    Vector3 relativeVec;

    Animator anim;
    Transform spine;
    Vector3 aim;

    float xRotation;
    float yRotation;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        spine = anim.GetBoneTransform(HumanBodyBones.Spine);
        if (spine != null) Debug.Log("It is Spine");
    }
    private void Update()
    {
        float xRotate = Input.GetAxis("Mouse Y") * 150 * Time.deltaTime;
        float yRotate = Input.GetAxis("Mouse X") * 120 * Time.deltaTime;

        xRotation -= xRotate;
        yRotation += yRotate;
        xRotation = Mathf.Clamp(xRotation, -120f, 60f);
    }
    // Update is called once per frame
    void LateUpdate()
    {
        
        //spine.localEulerAngles =  Vector3.up*90 + Vector3.forward * xRotation;
        //spine.localEulerAngles = Vector3.forward * xRotation;
        //spine.Rotate(0, 180, 0);
        //spine.rotation = Quaternion.LookRotation(aim,Vector3.up);
        //spine.LookAt(aim,Vector3.up);
        //chest.LookAt(target.position,Vector3.left);

    }


}
