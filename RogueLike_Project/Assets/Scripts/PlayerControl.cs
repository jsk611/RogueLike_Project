using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
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
    [SerializeField] float moveSpeed = 8f;
    float moveSpeed_origin;
    

    [SerializeField] int HP = 100;
    [SerializeField] [Range(0,100)] public float Stamina = 100;
    float time;

    Animator playerAnimator;
    
    Rigidbody playerRigidbody;

    [SerializeField] GameObject MainCharacter;


    Vector3 Movement = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = MainCharacter.GetComponent<Animator>();
        playerRigidbody = MainCharacter.GetComponent<Rigidbody>();
        moveSpeed_origin = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (HP > 0)
        {
            MoveMent();
 
            shooting();
            
            if (Input.GetKey(KeyCode.L)) HP -= 1;
        }
        Die();
    }

    private void Die()
    {
        if (HP <= 0)
        {
            playerAnimator.Play("Die");
        }
    }

    // 이런 ㅆㅂ 그냥 개꿀 아니 ㅆㅂ아 진짜 패 죽여버리고 싶네
    // 개같은 유니티 개같은 애니메이션 개같은 코딩

    private void MoveMent()
    {
        Movement = Vector3.zero;
        ResetAnimationState();

        isCrawling();
        IsRunning();
        WtoMoveForward();
        DtoMoveRight();
        StoMoveBackward();
        AtoMoveLeft();
        Movement.Normalize();
        transform.Translate(Movement*Time.deltaTime*moveSpeed, Space.Self);
        return;
    }

    private void IsRunning()
    {
        time += Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftShift) && Stamina >0 && !playerAnimator.GetBool("isJumping") && !playerAnimator.GetBool("crawling"))
        {
            if (!playerAnimator.GetBool("isRunning")) moveSpeed *= 2f;
            playerAnimator.SetBool("isRunning", true);
            Stamina -= 10 * Time.deltaTime;
            time = 0;
        }
        else
        {
            if (playerAnimator.GetBool("isRunning")) moveSpeed = moveSpeed_origin;
            if (time  > 1f && Stamina <100 && !playerAnimator.GetBool("isJumping")) Stamina+=2;

            if (playerAnimator.GetBool("isRunning")) moveSpeed /= 2f;
            if (time  > 1f && Stamina <100) Stamina+=10;
            playerAnimator.SetBool("isRunning", false);
        }
    }
    private void isCrawling()
    {
        if (Input.GetKey(KeyCode.LeftControl) && !playerAnimator.GetBool("isJumping") && !playerAnimator.GetBool("isRunning"))
        {
            if (!playerAnimator.GetBool("crawling")) moveSpeed *= 0.6f;
            playerAnimator.SetBool("crawling", true);
           // transform.Translate(new Vector3(transform.position.x, transform.position.y - 10, transform.position.z),Space.World);
        }
        else
        {
            if (playerAnimator.GetBool("crawling")) moveSpeed = moveSpeed_origin;
            playerAnimator.SetBool("crawling", false);
           // transform.Translate(new Vector3(transform.position.x, transform.position.y + 10, transform.position.z), Space.World);

        }
    }
    private void AtoMoveLeft()
    {
        if (Input.GetKey(KeyCode.A))
        {
            playerAnimator.SetBool("isWalking", true);
            playerAnimator.SetBool("walkingLeft", true);
            Movement += Vector3.left;
        }
    }

    private void StoMoveBackward()
    {
        if (Input.GetKey(KeyCode.S))
        {
            playerAnimator.SetBool("isWalking", true);
            playerAnimator.SetBool("walkingBackward", true);
            Movement += Vector3.back;
        }
    }

    private void DtoMoveRight()
    {
        if (Input.GetKey(KeyCode.D))
        {
            playerAnimator.SetBool("isWalking", true);
            playerAnimator.SetBool("walkingRight", true);
            Movement += Vector3.right;
            
        }
    }

    private void WtoMoveForward()
    {
        if (Input.GetKey(KeyCode.W))
        {
            playerAnimator.SetBool("isWalking", true);
            playerAnimator.SetBool("walkingForward", true);
            Movement += Vector3.forward;
        }
    }


    
    

        private void ResetAnimationState()
    {
        playerAnimator.SetBool("isWalking", false);
        playerAnimator.SetBool("walkingRight", false);
        playerAnimator.SetBool("walkingLeft", false);
        playerAnimator.SetBool("walkingForward", false);
        playerAnimator.SetBool("walkingBackward", false);
    }
    
    private void shooting()
    {
        if (Input.GetMouseButton(0))
        {
            Debug.Log("shooting");
            playerAnimator.Play("Shoot_SingleShot_AR");
        }   
        if (Input.GetMouseButton(1)) Debug.Log("targeting");
        if (Input.GetKey(KeyCode.R))
        {
            Debug.Log("Reroad");
            playerAnimator.Play("Reload");
        }

    }

}
