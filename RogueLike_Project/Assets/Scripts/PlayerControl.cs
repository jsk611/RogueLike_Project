using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] float jumpPower = 10f;

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
        if (Input.GetKey(KeyCode.W))
        {
            playerAnimator.SetBool("isWalking", true);
            if (Input.GetKey(KeyCode.LeftShift) && !isJumping)
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
        if (Input.GetKey(KeyCode.D))
        {
            playerAnimator.SetBool("isWalking", true);
            if (Input.GetKey(KeyCode.LeftShift) && !isJumping)
            {
                playerAnimator.SetBool("runningRight",true);
                Movement += Vector3.right * Time.deltaTime * moveSpeed * 2;
            }
            else
            {
                playerAnimator.SetBool("walkingRight", true);
                Movement += Vector3.right * Time.deltaTime * moveSpeed;
            }
        }
        if (Input.GetKey(KeyCode.S))
        {
            playerAnimator.SetBool("isWalking", true);
            if (Input.GetKey(KeyCode.LeftShift) && !isJumping)
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
        if (Input.GetKey(KeyCode.A))
        {
            playerAnimator.SetBool("isWalking", true);
            if (Input.GetKey(KeyCode.LeftShift) && !isJumping)
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
        transform.Translate(Movement,Space.World);
    }
    void Jump()
    {
        if (!isJumping && Input.GetKey(KeyCode.Space))
        {
            playerRigidbody.AddForce(new Vector3(0, jumpPower, 0), ForceMode.Impulse);
            playerAnimator.SetBool("isJumping", true);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor") playerAnimator.SetBool("isJumping", false);
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
    }
}
