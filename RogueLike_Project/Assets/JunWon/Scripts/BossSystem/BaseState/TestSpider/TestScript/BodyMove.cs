using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;


public class BodyMove : MonoBehaviour
{
    [SerializeField] float speed = 0.7f;

    [Header("Legs")]
    [SerializeField] List<Transform> legList;
    [SerializeField] Vector3 bodyYOffset;
    Vector3 moveDirection;

    Vector3 origin;
    public float SPEED => speed;
    public Vector3 MoveDirection => moveDirection;

    #region bossProperties
    SpiderPrime spiderPrime;
    #endregion
    // Update is called once per frame
    private void Start()
    {
        origin = transform.position;
        moveDirection = Vector3.zero;
        spiderPrime = GetComponent<SpiderPrime>();
     //   nmAgent.baseOffset = -bodyYOffset.y;
    }
    void Update()
    {
    //    var h = Input.GetAxisRaw("Horizontal") * transform.right;
   //     var v = Input.GetAxisRaw("Vertical") * transform.forward;
      //  moveDirection = h + v;
     //   moveDirection.Normalize();
        spiderPrime.NmAgent.SetDestination(spiderPrime.Player.position);
        //     moveDirection = nmAgent.
        //    transform.position += moveDirection * speed * Time.deltaTime ;
        moveDirection = (spiderPrime.Player.position - transform.position).normalized;
        float totalLegY = 0;
        foreach (var leg in legList)
        {
            totalLegY += leg.position.y;
        }
        transform.position = new Vector3(transform.position.x, totalLegY / legList.Count+bodyYOffset.y, transform.position.z);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        switch (context) {
            case { phase: InputActionPhase.Performed }:
            //    moveDirection = context.ReadValue<Vector2>();

            //    moveDirection = h + v;
                break;
            case { phase: InputActionPhase.Canceled }:
           //     moveDirection =Vector3.zero;
                break;
            default:
                break;
        }
        Debug.Log(moveDirection);
     //   moveDirection.z = moveDirection.y;
       // moveDirection.y = 0;
       // moveDirection = transform.InverseTransformDirection(moveDirection);
       // moveDirection.Normalize();



       
        moveDirection.Normalize();
    }
}
