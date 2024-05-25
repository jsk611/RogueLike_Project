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
    [SerializeField] float jumpPower = 10f;
    [SerializeField] float horizonRotateSpeed = 50f;
    [SerializeField] float verticalRotateSpeed = 30f;
    [SerializeField] int HP = 100;
    [SerializeField] [Range(0,100)]float Stamina = 100;
    float time;

    Animator playerAnimator;
    GameObject Player;
    Rigidbody playerRigidbody;

    float yRotation;
    float xRotation;

    Vector3 Movement = Vector3.zero;

    bool isJumping = false;
    // Start is called before the first frame update
    void Start()
    {
        Player = GetComponent<GameObject>();
        playerAnimator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
        moveSpeed_origin = moveSpeed;
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
        isCrawling();
        IsRunning();
        WtoMoveForward();
        DtoMoveRight();
        StoMoveBackward();
        AtoMoveLeft();
        cameraRotation();
        transform.Translate(Movement, Space.Self);
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
            if (playerAnimator.GetBool("isRunning")) moveSpeed /= 2f;
            if (time  > 1f) Stamina+=10;
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
            Movement += Vector3.left * Time.deltaTime * moveSpeed;
        }
    }

    private void StoMoveBackward()
    {
        if (Input.GetKey(KeyCode.S))
        {
            playerAnimator.SetBool("isWalking", true);
            playerAnimator.SetBool("walkingBackward", true);
            Movement += Vector3.back * Time.deltaTime * moveSpeed;
        }
    }

    private void DtoMoveRight()
    {
        if (Input.GetKey(KeyCode.D))
        {
            playerAnimator.SetBool("isWalking", true);
            playerAnimator.SetBool("walkingRight", true);
            Movement += Vector3.right * Time.deltaTime * moveSpeed;
            
        }
    }

    private void WtoMoveForward()
    {
        if (Input.GetKey(KeyCode.W))
        {
            playerAnimator.SetBool("isWalking", true);
            playerAnimator.SetBool("walkingForward", true);
            Movement += Vector3.forward * Time.deltaTime * moveSpeed;
        }
    }

    void Jump()
    {
        if (!playerAnimator.GetBool("isJumping") && Input.GetKey(KeyCode.Space) && Stamina >0)
        {
            playerRigidbody.AddForce(new Vector3(0, jumpPower, 0), ForceMode.Impulse);
            playerAnimator.SetBool("isJumping", true);
            Stamina -= 20;
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
    private void cameraRotation()
    {
        float xRotate = Input.GetAxis("Mouse Y") * horizonRotateSpeed * Time.deltaTime;
        float yRotate = Input.GetAxis("Mouse X") * verticalRotateSpeed * Time.deltaTime;
        xRotation +=  -xRotate;
        yRotation +=  yRotate;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
      
    }
}
