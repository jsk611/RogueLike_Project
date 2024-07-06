using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;




/* WASD
 * SHIFT ??????
 * CTRL ????????
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

    void Start()
    {
        Player = GetComponent<GameObject>();
        playerAnimator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        MoveMent();
        Jump();
    }

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
