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
    [SerializeField] float moveSpeed = 16f;
    float moveSpeed_origin;
    float jumpPower = 5f;

    [SerializeField] int HP = 100;
    [SerializeField] [Range(0,100)] public float Stamina = 100;

    float time;
    bool canDash =false ;

    Animator playerAnimator;
    
    Rigidbody playerRigidbody;

    CharacterController character;

    [SerializeField] GameObject MainCharacter;


    Vector3 Movement = Vector3.zero;

    
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
        var h = Input.GetAxisRaw("Horizontal") * transform.right;
        var v = Input.GetAxisRaw("Vertical") * transform.forward;
        Movement = h + v;
        ResetAnimationState();

        if (Movement != Vector3.zero) playerAnimator.SetBool("isWalking", true); 
        //WtoMoveForward();
        //DtoMoveRight();
        //StoMoveBackward();
        //AtoMoveLeft();

        Jumping();
       
        isCrawling();
            
        Movement.Normalize();
        
        //transform.TransformDirection(Movement);
        Dash();
        character.Move (Movement * Time.deltaTime * moveSpeed);
        //transform.Translate(Movement * Time.deltaTime * moveSpeed, Space.Self);
        
        return;
    }

    private void Dash()
    {
        
        
        if (Input.GetKey(KeyCode.LeftShift) && Stamina > 0 && playerAnimator.GetBool("isWalking") && !playerAnimator.GetBool("crawling"))
        {
            if (time > 0.5f)
            {
                Stamina -= 20;
                playerRigidbody.MovePosition(transform.position + Movement *  moveSpeed * 50);
                time = 0;
            }
        }
    }

    private void Jumping()
    {
        if (!playerAnimator.GetBool("isJumping") && Input.GetKey(KeyCode.Space) && Stamina > 0)
        {
            playerRigidbody.AddForce(new Vector3(0, jumpPower, 0), ForceMode.Impulse);
            playerAnimator.SetBool("isJumping", true);
            time = 0;
            Stamina -= 20;
        }
    }
    private void StaminaRegeneration()
    {
        time += Time.deltaTime;
        if (time > 1.5f) Stamina += 20 * Time.deltaTime;
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
        if (Input.GetKey(KeyCode.LeftControl) && !playerAnimator.GetBool("isJumping") && !playerAnimator.GetBool("isRunning"))
        {
            if (!playerAnimator.GetBool("crawling"))
            {
                moveSpeed = moveSpeed_origin * 0.6f;
                Debug.Log("crawling");
            }
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
    }
    

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            playerAnimator.SetBool("isJumping", false);
        }
        else playerAnimator.SetBool("isJumping", true);
    }














    private void shooting()
    {
        if (Input.GetMouseButton(0))
        {
            playerAnimator.SetTrigger("shooting");
        }
        if (Input.GetMouseButton(1)) Debug.Log("targeting");
        if (Input.GetKey(KeyCode.R))
        {
            Debug.Log("Reroad");
            playerAnimator.SetTrigger("reloading");
        }

    }
}
