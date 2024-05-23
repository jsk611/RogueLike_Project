using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [SerializeField] float jumpPower = 10f;
    [SerializeField] float horizonRotateSpeed = 5f;
    [SerializeField] float verticalRotateSpeed = 3f;
    [SerializeField] int HP = 100;
    [SerializeField] [Range(0,100)]int Stamina = 100;

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
        shooting();
        //cameraRotation();
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
        IsRunning();
        WtoMoveForward();
        DtoMoveRight();
        StoMoveBackward();
        AtoMoveLeft();
        transform.Translate(Movement, Space.World);
        return;
    }

    private void IsRunning()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Stamina >0 && !isJumping)
        {
            if (!playerAnimator.GetBool("isRunning")) moveSpeed *= 2f;
            playerAnimator.SetBool("isRunning", true);
            Stamina--;
        }
        else
        {
            if (playerAnimator.GetBool("isRunning")) moveSpeed /= 2f;
            Stamina++;
            playerAnimator.SetBool("isRunning", false);
        }
    }
    private void AtoMoveLeft()
    {
        if (Input.GetKey(KeyCode.A))
        {
            playerAnimator.SetBool("isWalking", true);
            playerAnimator.SetBool("walkingLeft", true);
            if (Input.GetKey(KeyCode.LeftControl))
            {
                playerAnimator.SetBool("crawling",true);
                 moveSpeed *= 0.6f;
            }
                Movement += Vector3.left * Time.deltaTime * moveSpeed;
        }
    }

    private void StoMoveBackward()
    {
        if (Input.GetKey(KeyCode.S))
        {
            playerAnimator.SetBool("isWalking", true);
            playerAnimator.SetBool("walkingBackward", true);
            if (Input.GetKey(KeyCode.LeftControl))
            {
                playerAnimator.SetBool("crawling", true);
                moveSpeed *= 0.6f;
            }
            Movement += Vector3.back * Time.deltaTime * moveSpeed;
        }
    }

    private void DtoMoveRight()
    {
        if (Input.GetKey(KeyCode.D))
        {
            playerAnimator.SetBool("isWalking", true);
            playerAnimator.SetBool("walkingRight", true);
            if (Input.GetKey(KeyCode.LeftControl))
            {
                playerAnimator.SetBool("crawling", true);
                Movement += Vector3.right * Time.deltaTime * moveSpeed * 0.6f;
            }
            Movement += Vector3.right * Time.deltaTime * moveSpeed;
            
        }
    }

    private void WtoMoveForward()
    {
        if (Input.GetKey(KeyCode.W))
        {
            playerAnimator.SetBool("isWalking", true);
            playerAnimator.SetBool("walkingForward", true);
            if (Input.GetKey(KeyCode.LeftControl))
            {
                playerAnimator.SetBool("crawling", true);
                moveSpeed *= 0.6f;
            }
            Movement += Vector3.forward * Time.deltaTime * moveSpeed;
        }
    }

    void Jump()
    {
        if (!playerAnimator.GetBool("isJumping") && Input.GetKey(KeyCode.Space))
        {
            playerRigidbody.AddForce(new Vector3(0, jumpPower, 0), ForceMode.Impulse);
            playerAnimator.SetBool("isJumping", true);
    
        }
    }
    
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            Transform arm = transform.Find("Arm1");
            playerAnimator.SetBool("isJumping", false);
        }
    }
        private void ResetAnimationState()
    {
        playerAnimator.SetBool("isWalking", false);
        playerAnimator.SetBool("walkingRight", false);
        playerAnimator.SetBool("walkingLeft", false);
        playerAnimator.SetBool("walkingForward", false);
        playerAnimator.SetBool("walkingBackward", false);
        playerAnimator.SetBool("crawling", false);
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
    //private void cameraRotation()
    //{
    //    float xRotate = Input.GetAxis("mouseX") * horizonRotateSpeed * Time.deltaTime;
    //    float yRotate = Input.GetAxis("mouseY") * verticalRotateSpeed * Time.deltaTime;
    //    float mouseX = xRotate; 
    //    float mouseY = 
    //    transform.rotation = Quaternion.EulerRotation(mouseX, mouseY, 0) ;
        
       
    //}
}
