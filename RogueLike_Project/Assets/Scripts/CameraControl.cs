using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraControl : MonoBehaviour
{
    [SerializeField] float horizonRotateSpeed = 50f;
    [SerializeField] float verticalRotateSpeed = 30f;

    float yRotation;
    float xRotation;

    [SerializeField] Transform playerRotation;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        cameraRotation();
    }
    private void cameraRotation()
    {
        float xRotate = Input.GetAxis("Mouse Y") * horizonRotateSpeed * Time.deltaTime;
        float yRotate = Input.GetAxis("Mouse X") * verticalRotateSpeed * Time.deltaTime;
 
        xRotation += -xRotate;
        yRotation += yRotate;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        playerRotation.rotation  = Quaternion.Euler(0,yRotation,0f);
        transform.localEulerAngles = Vector3.right * xRotation;
    }
}
