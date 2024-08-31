using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


// ???? ?? time ?????? ???????? ?????? ???? ?? ????? ???? ???????? ???????? ???????

/* WASD
 * SHIFT ??????
 * CTRL ????????
 * RIFLE, PISTOL, origin weapons...
 * 
 */



public class PlayerControl : MonoBehaviour
{
    private float moveSpeed;
    private float moveSpeed_origin;
    private float jumpPower;

    private int HP = 100;
    [Range(0,100)] public float Stamina = 100;

    float dashCool;
    public bool isGrounded = true;

    bool dashOver = true;
    bool crawlOver = true;


    [SerializeField] GameObject MainCharacter;
    Animator playerAnimator;
    Rigidbody playerRigidbody;
    CharacterController character;

    CameraControl cameraController;
    Rigidbody rigidBody;
    //ShootingController shootingController;

    PlayerStatus characterStatus;

    RaycastHit hitInfo;

    public Vector3 Movement = Vector3.zero;
    public Vector3 Vertical = Vector3.zero;

    Transform originalParent;
    Vector3 initialWorldScale;
    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = MainCharacter.GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
        character = GetComponent<CharacterController>();
        rigidBody = GetComponent<Rigidbody>();

        cameraController = GameObject.Find("ViewCamera").GetComponent<CameraControl>();
        //upperBodyMovement = GameObject.Find("PBRCharacter").GetComponent<UpperBodyMovement>();
        //shootingController = GameObject.Find("PBRCharacter").GetComponent<ShootingController>();
        characterStatus = GetComponent<PlayerStatus>();

        moveSpeed = characterStatus.GetMovementSpeed();
        moveSpeed_origin = moveSpeed;

        originalParent = transform.parent;
        initialWorldScale = transform.lossyScale;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
            MoveMent();
            StaminaRegeneration();
            //shooting();
            if (Input.GetKey(KeyCode.L)) HP -= 1;
    }

    void LateUpdate()
    {
        if(transform.parent != null)
        {
            Transform parentTransform = transform.parent.transform;
            // ?????? ???????? ??????????.
            Vector3 parentScale = parentTransform.localScale;

            // ?????? ?????? ?????? ???? ?????? ???? ???????? ??????????.
            Vector3 currentWorldScale = transform.lossyScale;
            Vector3 scaleRatio = new Vector3(
                initialWorldScale.x / currentWorldScale.x,
                initialWorldScale.y / currentWorldScale.y,
                initialWorldScale.z / currentWorldScale.z
            );

            transform.localScale = new Vector3(
                transform.localScale.x * scaleRatio.x,
                transform.localScale.y * scaleRatio.y,
                transform.localScale.z * scaleRatio.z
            );
        }
        
    }
    //private void Die()
    //{
    //    cameraController.enabled = false;
    //    //shootingController.enabled = false;
    //    if (playerAnimator.GetBool("isAlive"))
    //    {
    //        playerAnimator.SetTrigger("dead");
    //        playerAnimator.SetBool("isAlive", false);
    //    }


    //}

    private void MoveMent()
    {
        moveSpeed = characterStatus.GetMovementSpeed();
        var h = Input.GetAxisRaw("Horizontal") * transform.right;
        var v = Input.GetAxisRaw("Vertical") * transform.forward;
        Movement = h + v;
        
        Dash();
        isCrawling();
        Movement = Movement.normalized * moveSpeed;
        character.Move (Movement * Time.deltaTime);
       // character.Move(Vector3.down * 0.8f * Time.deltaTime);

        CheckGrounded();
        Jumping();

       // character.Move(Vertical * Time.deltaTime);

        return;
    }

    public bool CheckGrounded()
    {
        isGrounded = Physics.SphereCast(character.transform.position, character.radius - 0.05f, Vector3.down,out hitInfo ,1.1f,LayerMask.GetMask("Wall"));
        Debug.Log(isGrounded);
        if (!isGrounded) rigidBody.isKinematic = false;//Vertical.y += Physics.gravity.y * Time.deltaTime;
        if (isGrounded)
        {
            rigidBody.isKinematic = true;//Vertical.y = -0.8f;
            character.Move(Vector3.down * 2f * Time.deltaTime);
        }
        return isGrounded;
    }

    private void Dash()
    {
        if (dashCool > 0.15f && !dashOver)
        {
            characterStatus.SetMovementSpeed(moveSpeed_origin);
            dashOver = true;
        }
        if (Input.GetKey(KeyCode.LeftShift) && Stamina >= 100f && Movement.magnitude > Mathf.Epsilon && crawlOver)
        {
            if (dashCool > 1f)
            {
                moveSpeed_origin = characterStatus.GetMovementSpeed();
                Stamina = 0f;
                characterStatus.SetMovementSpeed(moveSpeed * 4f);
                dashCool = 0;
                dashOver = false;
            }
        }
    }

    private void Jumping()
    {

        if (Input.GetKey(KeyCode.Space))
        {
            if (isGrounded) 
            {
                rigidBody.isKinematic = false;
                rigidBody.AddForce(Vector3.up*10, ForceMode.Impulse);
                jumpPower = 4.9f;//characterStatus.GetJumpPower();
                // Debug.Log("Jump");
                Vertical.y = jumpPower;
                ////playerRigidbody.AddForce(new Vector3(0, jumpPower, 0), ForceMode.Impulse);
            }
        }
     
    }

    private void StaminaRegeneration()
    {
        dashCool += Time.deltaTime;

        if (dashCool > 1.5f)
        {
            if (Stamina < 100) Stamina += characterStatus.GetStaminaRegen() * Time.deltaTime;
            if (Stamina > 100) Stamina = 100f;
        }
    }

    private void isCrawling()
    {
        if (Input.GetKey(KeyCode.LeftControl) && isGrounded && dashOver)
        {
            if (!playerAnimator.GetBool("Crouch")) playerAnimator.SetBool("Crouch", true);
            {
                moveSpeed_origin = moveSpeed;
                moveSpeed = moveSpeed_origin * 0.5f;
            }
                character.height = 1.6f;
                character.center = new Vector3(0, -0.09f, character.center.z);
            crawlOver = false;
                // transform.Translate(new Vector3(transform.position.x, transform.position.y - 10, transform.position.z),Space.World);
        }
        else
        {
            if (playerAnimator.GetBool("Crouch"))  moveSpeed = moveSpeed_origin;
                playerAnimator.SetBool("Crouch", false);
               
                character.height = 1.8f;
                character.center = new Vector3(0, 0, 0);
            crawlOver = true;
                // transform.Translate(new Vector3(transform.position.x, transform.position.y + 10, transform.position.z), Space.World);
        }
    }

    // ?????? ?????? ??
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Debug.Log(hit.gameObject.name);
        if (hit.gameObject.CompareTag("Wall") && hit.transform != transform.parent)
        {
            transform.SetParent(hit.transform); // ?????????? ?????? ???????? ????
        }
        else if(!hit.gameObject.CompareTag("Wall"))
        {
            transform.SetParent(originalParent); // ???? ?????? ????
        }
    }













}
