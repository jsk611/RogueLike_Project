using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.UIElements;




/* WASD
 * SHIFT 달리기
 * CTRL 웅크리기
 * RIFLE, PISTOL, origin weapons...
 * 
 */



public class PlayerControl : MonoBehaviour
{
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float jumpPower = 1f;

    Animator playerAnimator;
    GameObject Player;
    Rigidbody playerRigidbody;

    Vector3 Movement = Vector3.zero;

    bool isJumping = false;
    // Start is called before the first frame update
    void Start()
    {
        Player = GetComponent<GameObject>();
        playerAnimator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveMent();
        Jump();
    }

    // 이런 ㅆㅂ 그냥 개꿀 아니 ㅆㅂ아 진짜 패 죽여버리고 싶네
    // 개같은 유니티 개같은 애니메이션 개같은 코딩

    private void MoveMent()
    {
        Movement = Vector3.zero;
        ResetAnimationState();
        if (Input.GetKey(KeyCode.LeftControl))
        {

        }
        WtoMoveForward();
        DtoMoveRight();
        StoMoveBackward();
        AtoMoveLeft();
        transform.Translate(Movement, Space.World);
        return;
    }

    private void AtoMoveLeft()
    {
        if (Input.GetKey(KeyCode.A))
        {
            playerAnimator.SetBool("isWalking", true);
            if (Input.GetKey(KeyCode.LeftControl))
            {
                playerAnimator.SetBool("crawling",true);
                Movement += Vector3.left * Time.deltaTime * moveSpeed * 0.6f;
            }
            else if (Input.GetKey(KeyCode.LeftShift) && !isJumping)
            {
                playerAnimator.SetBool("runningLeft", true);
                Movement += Vector3.left * Time.deltaTime * moveSpeed * 2;
            }
            else
            {
                playerAnimator.SetBool("walkingLeft", true);
                Movement += Vector3.left * Time.deltaTime * moveSpeed;
            }
        }
    }

    private void StoMoveBackward()
    {
        if (Input.GetKey(KeyCode.S))
        {
            playerAnimator.SetBool("isWalking", true);
            if (Input.GetKey(KeyCode.LeftControl))
            {
                playerAnimator.SetBool("crawling", true);
                Movement += Vector3.back * Time.deltaTime * moveSpeed * 0.6f;
            }
            else if (Input.GetKey(KeyCode.LeftShift) && !isJumping)
            {
                playerAnimator.SetBool("runningBackward", true);
                Movement += Vector3.back * Time.deltaTime * moveSpeed * 2;
            }
            else
            {
                playerAnimator.SetBool("walkingBackward", true);
                Movement += Vector3.back * Time.deltaTime * moveSpeed;
            }
        }
    }

    private void DtoMoveRight()
    {
        if (Input.GetKey(KeyCode.D))
        {
            playerAnimator.SetBool("isWalking", true);
            if (Input.GetKey(KeyCode.LeftControl))
            {
                playerAnimator.SetBool("crawling", true);
                Movement += Vector3.right * Time.deltaTime * moveSpeed * 0.6f;
            }
            else if (Input.GetKey(KeyCode.LeftShift) && !isJumping)
            {
                playerAnimator.SetBool("runningRight", true);
                Movement += Vector3.right * Time.deltaTime * moveSpeed * 2;
            }
            else
            {
                playerAnimator.SetBool("walkingRight", true);
                Movement += Vector3.right * Time.deltaTime * moveSpeed;
            }
        }
    }

    private void WtoMoveForward()
    {
        if (Input.GetKey(KeyCode.W))
        {
            playerAnimator.SetBool("isWalking", true);
            if (Input.GetKey(KeyCode.LeftControl))
            {
                playerAnimator.SetBool("crawling", true);
                Movement += Vector3.forward * Time.deltaTime * moveSpeed * 0.6f;
            }
            else if (Input.GetKey(KeyCode.LeftShift) && !isJumping)
            {
                playerAnimator.SetBool("runningForward", true);
                Movement += Vector3.forward * Time.deltaTime * moveSpeed * 2;
            }
            else
            {
                playerAnimator.SetBool("walkingForward", true);
                Movement += Vector3.forward * Time.deltaTime * moveSpeed;
            }
        }
    }

    void Jump()
    {
        if (!isJumping && Input.GetKey(KeyCode.Space))
        {
            playerRigidbody.AddForce(new Vector3(0, jumpPower, 0), ForceMode.Impulse);
            playerAnimator.SetBool("isJumping", true);
            Transform arm = transform.Find("Arm1");
            arm.transform.GetComponent<SkinnedMeshRenderer>().enabled = false;
            transform.Find("Leg1").GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
    }
    
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            Transform arm = transform.Find("Arm1");
            playerAnimator.SetBool("isJumping", false);
            arm.transform.GetComponent<SkinnedMeshRenderer>().enabled = true;
            transform.Find("Leg1").GetComponent<SkinnedMeshRenderer>().enabled = true;
        }
        }
        private void ResetAnimationState()
    {
        playerAnimator.SetBool("isWalking", false);
        playerAnimator.SetBool("walkingRight", false);
        playerAnimator.SetBool("walkingLeft", false);
        playerAnimator.SetBool("walkingForward", false);
        playerAnimator.SetBool("walkingBackward", false);
        playerAnimator.SetBool("runningForward", false);
        playerAnimator.SetBool("runningBackward", false);
        playerAnimator.SetBool("runningLeft", false);
        playerAnimator.SetBool("runningRight", false);
        playerAnimator.SetBool("crawling", false);
    }
}
