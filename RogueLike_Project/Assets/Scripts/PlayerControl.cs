using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


// 게임 내 time 관리를 플레이어 쪽에서 하는 게 맞나? 게임 매니저를 만들어야 하는가?

/* WASD
 * SHIFT 달리기
 * CTRL 웅크리기
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
    //ShootingController shootingController;

    StatusBehaviour characterStatus;

    RaycastHit hitInfo;

    public Vector3 Movement = Vector3.zero;
    public Vector3 Vertical = Vector3.zero;
    
    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = MainCharacter.GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
        character = GetComponent<CharacterController>();

        cameraController = GameObject.Find("ViewCamera").GetComponent<CameraControl>();
        //upperBodyMovement = GameObject.Find("PBRCharacter").GetComponent<UpperBodyMovement>();
        //shootingController = GameObject.Find("PBRCharacter").GetComponent<ShootingController>();
        characterStatus = GetComponent<Status>();

        moveSpeed = characterStatus.GetMovementSpeed();
        moveSpeed_origin = moveSpeed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

            MoveMent();
            StaminaRegeneration();
            //shooting();
            if (Input.GetKey(KeyCode.L)) HP -= 1;
        

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

        CheckGrounded();
        Jumping();

        character.Move(Vertical * Time.deltaTime);

        return;
    }
    public bool CheckGrounded()
    {
        isGrounded = Physics.SphereCast(character.transform.position, character.radius - 0.05f, Vector3.down,out hitInfo ,1.1f);
        if (!isGrounded) Vertical.y += Physics.gravity.y * Time.deltaTime;
        if (isGrounded) Vertical.y = -0.8f;
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












}
