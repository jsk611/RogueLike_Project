using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    [SerializeField] float horizonRotateSpeed = 500f;
    [SerializeField] float verticalRotateSpeed = 300f;
    float yRotation;
    float xRotation;
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
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);

    }
}
