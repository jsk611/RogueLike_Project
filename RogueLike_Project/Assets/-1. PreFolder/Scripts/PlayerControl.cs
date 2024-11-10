using InfimaGames.LowPolyShooterPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
//using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor.Rendering;
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
    [Range(0,100)] public float Stamina = 100;

    float dashCool;
    public bool isGrounded = true;

    bool dashOver = true;
    bool crawlOver = true;


    Animator playerAnimator;
  
    Rigidbody playerRigidbody;
    CharacterController character;
    CharacterBehaviour playerCharacter;

    CameraControl cameraController;
    Rigidbody rigidBody;
    //ShootingController shootingController;

    PlayerStatus characterStatus;

    RaycastHit hitInfo;

    public Vector3 Movement = Vector3.zero;
    public Vector3 Vertical = Vector3.zero;

    Transform originalParent;
    Vector3 initialWorldScale;
    [SerializeField] PlayerPositionData positionData;
    // Start is called before the first frame update
    void Start()
    {
        playerCharacter = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        playerAnimator = playerCharacter.GetPlayerAnimator();
        playerRigidbody = GetComponent<Rigidbody>();
        character = GetComponent<CharacterController>();
       
        rigidBody = GetComponent<Rigidbody>();

        cameraController = GameObject.Find("ViewCamera").GetComponent<CameraControl>();
        //upperBodyMovement = GameObject.Find("PBRCharacter").GetComponent<UpperBodyMovement>();
        //shootingController = GameObject.Find("PBRCharacter").GetComponent<ShootingController>();
        characterStatus = GetComponent<PlayerStatus>();

        moveSpeed = characterStatus.GetMovementSpeed();
        moveSpeed_origin = moveSpeed;

        originalParent = transform.parent;
        initialWorldScale = transform.lossyScale;


    }

    // Update is called once per frame
    void Update()
    {

        
        MoveMent();
        StaminaRegeneration();
        CheckGrounded();
        Jumping();

    }

    void LateUpdate()
    {
        if(transform.parent != null)
        {
            Transform parentTransform = transform.parent.transform;
            // 부모의 스케일을 추적합니다.
            Vector3 parentScale = parentTransform.localScale;

            // 부모의 스케일 변화에 따라 자식의 로컬 스케일을 조정합니다.
            Vector3 currentWorldScale = transform.lossyScale;
            Vector3 scaleRatio = new Vector3(
                initialWorldScale.x / currentWorldScale.x,
                initialWorldScale.y / currentWorldScale.y,
                initialWorldScale.z / currentWorldScale.z
            );

            transform.localScale = new Vector3(
                transform.localScale.x * scaleRatio.x,
                transform.localScale.y * scaleRatio.y,
                transform.localScale.z * scaleRatio.z
            );
        }
        
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
        character.height = 1.8f;
        character.center = new Vector3(0, 0, 0);

        Movement = Movement.normalized * moveSpeed;
        character.Move (Movement * Time.deltaTime);
        // character.Move(Vector3.down * 0.8f * Time.deltaTime);

     
        // character.Move(Vertical * Time.deltaTime
        return;
    }
    public bool CheckGrounded()
    {
        isGrounded = Physics.SphereCast(character.transform.position, character.radius - 0.05f, Vector3.down,out hitInfo ,1.03f,LayerMask.GetMask("Wall"));
        
        if (!isGrounded) rigidBody.isKinematic = false;//Vertical.y += Physics.gravity.y * Time.deltaTime;
        if (isGrounded)
        {
            rigidBody.isKinematic = true;//Vertical.y = -0.8f;
            character.Move(Vector3.down * 2f * Time.deltaTime);
        }
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
                playerCharacter.AnimationCancelReload();
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
                rigidBody.isKinematic = false;
                rigidBody.AddForce(Vector3.up*10, ForceMode.Impulse);
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

                // transform.Translate(new Vector3(transform.position.x, transform.position.y + 10, transform.position.z), Space.World);
        
    }

    // 발판과 충돌할 때
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Debug.Log(hit.gameObject.name);
        if (hit.gameObject.CompareTag("Wall") && hit.transform != transform.parent)
        {
            transform.SetParent(hit.transform); // 플레이어를 발판의 자식으로 설정
            string[] tilePos = hit.gameObject.name.Split(',');
            if(tilePos.Length == 2) positionData.playerTilePosition = new Vector2Int(int.Parse(tilePos[0]), int.Parse(tilePos[1]));
        }
        else if(!hit.gameObject.CompareTag("Wall"))
        {
            transform.SetParent(originalParent); // 원래 부모로 복원
        }
    }

}
