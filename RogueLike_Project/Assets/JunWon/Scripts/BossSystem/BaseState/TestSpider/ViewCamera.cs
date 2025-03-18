using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public class ViewCamera : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] float sensitiviy = 1;
    [SerializeField] Transform bodyTransform;
    float x = 0;
    float y = 0;
    Vector2 mouseInput;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        x += mouseInput.x*sensitiviy; y += -mouseInput.y*sensitiviy;
        y = Mathf.Clamp(y, -70, 70);
        transform.localRotation  = Quaternion.Euler(y,0,0);
        bodyTransform.localRotation = Quaternion.Euler(0, x, 0);
        
    }
    public void OnLook(InputAction.CallbackContext context) {
        mouseInput = context.ReadValue<Vector2>();
    }
}
