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
    [SerializeField] float moveSpeed = 12f;
    float moveSpeed_origin;
    [SerializeField] float jumpPower = 20f;

    [SerializeField] int HP = 100;
    [SerializeField] [Range(0,100)] public float Stamina = 100;

    float dashCool;
    bool isGrounded = true;
    float gravity = 9.8f;

    Animator playerAnimator;
    
    Rigidbody playerRigidbody;

    CharacterController character;

    [SerializeField] GameObject MainCharacter;


    Vector3 Movement = Vector3.zero;
    Vector3 Vertical = Vector3.zero;
    
    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = MainCharacter.GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
        character = GetComponent<CharacterController>();

        moveSpeed_origin = moveSpeed;
        
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
        Die();
    }

    private void Die()
    {
        if (HP <= 0)
        {
            playerAnimator.SetBool("isAlive", false);
            playerAnimator.Play("Die");
        }
    }

  

    private void MoveMent()
    {
        ResetAnimationState();
        var h = Input.GetAxisRaw("Horizontal") * transform.right;
        var v = Input.GetAxisRaw("Vertical") * transform.forward;
        Movement = h + v;
        
        if (Movement != Vector3.zero) playerAnimator.SetBool("isWalking", true);
        
        
        Dash();
        isCrawling();
        Movement = Movement.normalized * moveSpeed;
        character.Move (Movement * Time.deltaTime);
        
        CheckGrounded();
        Jumping();
       
        character.Move(Vertical * Time.deltaTime);

        //transform.TransformDirection(Movement);

        //transform.Translate(Movement * Time.deltaTime * moveSpeed, Space.Self);
       // Debug.Log(Vertical.y);
        return;
    }
    private void CheckGrounded()
    {
        isGrounded = false;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, 3);
        if (isGrounded) Debug.Log("grounded");
    }
    private void Dash()
    {
        if (dashCool > 0.15f) moveSpeed = moveSpeed_origin;
        if (Input.GetKey(KeyCode.LeftShift) && Stamina > 0 && playerAnimator.GetBool("isWalking") && !playerAnimator.GetBool("crawling"))
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

                moveSpeed = moveSpeed_origin * 0.5f;
                

            playerAnimator.SetBool("crawling", true);
            character.height = 0.6f;
            character.center = new Vector3(0, -0.18f, character.center.z);
                // transform.Translate(new Vector3(transform.position.x, transform.position.y - 10, transform.position.z),Space.World);
            
        }
        else
        {
            if (playerAnimator.GetBool("crawling")) moveSpeed = moveSpeed_origin;
            playerAnimator.SetBool("crawling", false);
            character.center = new Vector3(0, 0, 0);
            character.height =1f;
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


    private void ResetAnimationState()
    {
        playerAnimator.SetBool("isWalking", false);
        playerAnimator.SetBool("reloading", false);
        playerAnimator.SetBool("isJumping", false);
    }
    

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            playerAnimator.SetBool("isJumping", false);
        }
        else playerAnimator.SetBool("isJumping", true);
    }

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
