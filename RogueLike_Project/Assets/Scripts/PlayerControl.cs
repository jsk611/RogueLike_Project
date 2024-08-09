using System;
using System.Collections;
using System.Collections.Generic;
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
    public float moveSpeed = 12f;
    float moveSpeed_origin;
    public float jumpPower = 20f;

    public int HP = 100;
    [Range(0,100)] public float Stamina = 100;

    float dashCool;
    bool isGrounded = true;
    float gravity = 9.8f;


    [SerializeField] GameObject MainCharacter;
    Animator playerAnimator;
    Rigidbody playerRigidbody;
    CharacterController character;

    CameraControl cameraController;
    UpperBodyMovement upperBodyMovement;
    ShootingController shootingController;

    StatusBehaviour characterStatus;

    RaycastHit hitInfo;

    Vector3 Movement = Vector3.zero;
    Vector3 Vertical = Vector3.zero;
    
    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = MainCharacter.GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
        character = GetComponent<CharacterController>();

        moveSpeed_origin = moveSpeed;
        

        cameraController = GameObject.Find("ViewCamera").GetComponent<CameraControl>();
        //upperBodyMovement = GameObject.Find("PBRCharacter").GetComponent<UpperBodyMovement>();
        //shootingController = GameObject.Find("PBRCharacter").GetComponent<ShootingController>();
        characterStatus = GetComponent<Status>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (HP > 0)
        {
           
            MoveMent();
            StaminaRegeneration();
            //shooting();
            if (Input.GetKey(KeyCode.L)) HP -= 1;
        }
        else Die();
    }

    private void Die()
    {
        cameraController.enabled = false;
        upperBodyMovement.enabled = false;
        shootingController.enabled = false;
        if (playerAnimator.GetBool("isAlive"))
        {
            playerAnimator.SetTrigger("dead");
            playerAnimator.SetBool("isAlive", false);
        }
          
        
    }

  

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
    private void CheckGrounded()
    {
        isGrounded = false;

        isGrounded = Physics.SphereCast(character.transform.position, character.radius - 0.1f, Vector3.down,out hitInfo ,1.1f);
       
    }
    private void Dash()
    {
        if (dashCool > 0.15f) moveSpeed = moveSpeed_origin;
        if (Input.GetKey(KeyCode.LeftShift) && Stamina > 0 && Movement.magnitude > Mathf.Epsilon)
        {
            if (dashCool > 1f)
            {
                Stamina -= 15f;
                moveSpeed = moveSpeed_origin * 4f;
                dashCool = 0;
            }
        }
    }

    private void Jumping()
    {
        
        if (isGrounded)
        {
           // Debug.Log("On ground");
            Movement.y = -0.8f;
            if (Input.GetKey(KeyCode.Space))
            {
               // Debug.Log("Jump");
                Vertical.y = jumpPower;
                ////playerRigidbody.AddForce(new Vector3(0, jumpPower, 0), ForceMode.Impulse);
                
            }
        }
        else Vertical.y += Physics.gravity.y * Time.deltaTime;
    }

    private void StaminaRegeneration()
    {
        dashCool += Time.deltaTime;
        
        if (dashCool > 1.5f) Stamina += 40 * Time.deltaTime;
        Stamina = Mathf.Clamp(Stamina, 0f, 100f);
    }
    
    //private void isSprinting()
    //{
    //    time += Time.deltaTime;
    //    if (Input.GetKey(KeyCode.LeftShift) && Stamina >0 && playerAnimator.GetBool("isWalking") && !playerAnimator.GetBool("isJumping") && !playerAnimator.GetBool("crawling"))
    //    {
    //        if (!playerAnimator.GetBool("isRunning")) moveSpeed = moveSpeed_origin * 2f;
    //        playerAnimator.SetBool("isRunning", true);
    //        Stamina -= 10 * Time.deltaTime;
    //        time = 0;
    //    }
    //    else
    //    {
    //        if (playerAnimator.GetBool("isRunning")) moveSpeed = moveSpeed_origin;
    //        if (time  > 1f && Stamina <100 && !playerAnimator.GetBool("isJumping")) Stamina+=2;


    //        playerAnimator.SetBool("isRunning", false);
    //    }
    //}
    private void isCrawling()
    {
        if (Input.GetKey(KeyCode.LeftControl) && isGrounded)
        {
            if (!playerAnimator.GetBool("Crouch")) playerAnimator.SetBool("Crouch", true);

                moveSpeed = moveSpeed_origin * 0.5f;
                character.height = 1.6f;
                character.center = new Vector3(0, -0.09f, character.center.z);
                // transform.Translate(new Vector3(transform.position.x, transform.position.y - 10, transform.position.z),Space.World);
            
            
        }
        else
        {
            if (playerAnimator.GetBool("Crouch"))  moveSpeed = moveSpeed_origin;
                playerAnimator.SetBool("Crouch", false);
               
                character.height = 1.8f;
                character.center = new Vector3(0, 0, 0);
                // transform.Translate(new Vector3(transform.position.x, transform.position.y + 10, transform.position.z), Space.World);
            

        }
    }
    //private void AtoMoveLeft()
    //{
    //    if (Input.GetKey(KeyCode.A))
    //    {
    //        playerAnimator.SetBool("isWalking", true);
    //        Movement += transform.rotation * Vector3.left;
    //    }
    //}
    //private void StoMoveBackward()
    //{
    //    if (Input.GetKey(KeyCode.S))
    //    {
    //        playerAnimator.SetBool("isWalking", true);
    //        Movement += transform.rotation * Vector3.back;
    //    }
    //}
    //private void DtoMoveRight()
    //{
    //    if (Input.GetKey(KeyCode.D))
    //    {
    //        playerAnimator.SetBool("isWalking", true);
    //        Movement += transform.rotation * Vector3.right;

    //    }
    //}
    //private void WtoMoveForward()
    //{
    //    if (Input.GetKey(KeyCode.W))
    //    {
    //        playerAnimator.SetBool("isWalking", true);
    //        Movement += transform.rotation * Vector3.forward;
    //    }

    //}


  
    



    private void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.layer == 3)
        //{
        //    isGrounded = true;
        //}
    }










    private void shooting()
    {
        if (Input.GetMouseButton(0))
        {
            playerAnimator.SetTrigger("shooting");
        }
        if (Input.GetMouseButton(1)) 
        if (Input.GetKey(KeyCode.R))
        {
            
            playerAnimator.SetTrigger("reloading");
        }

    }
}
