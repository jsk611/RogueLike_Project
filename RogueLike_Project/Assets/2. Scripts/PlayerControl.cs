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
//using UnityEngine.WSA;


// ???? ?? time ?????? ???????? ?????? ???? ?? ????? ???? ???????? ???????? ???????

/* WASD
 * SHIFT ??????
 * CTRL ????????
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
    public bool isGrounded = false;

    public bool dashOver = false;


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

    TileManager tileManager;
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

        tileManager = FindObjectOfType<TileManager>();  
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
            // ?????? ???????? ??????????.
            Vector3 parentScale = parentTransform.localScale;

            // ?????? ?????? ?????? ???? ?????? ???? ???????? ??????????.
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
        
    //    Dash();
        character.height = 1.8f;
        character.center = new Vector3(0, 0, 0);

        Movement = Movement.normalized * moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) && Stamina >= 100f && Movement.magnitude > Mathf.Epsilon) StartCoroutine( Dashdd(Movement));

        if (playerCharacter.GetCursorState())character.Move (Movement * Time.deltaTime);
        // character.Move(Vector3.down * 0.8f * Time.deltaTime);

     
        // character.Move(Vertical * Time.deltaTime
        return;
    }

    IEnumerator Dashdd(Vector3 Movement)
    {
        if (dashCool > 1f && !dashOver)
        {
            Stamina = 0;
            dashCool = 0;
            float t = 0;
            playerCharacter.AnimationCancelReload();
            while (t <= 0.15f)
            {
                character.Move(Movement * 4 * Time.deltaTime);
                t += Time.deltaTime;
                yield return null;
            }
        }
    }
    public bool CheckGrounded()
    {
        isGrounded = Physics.SphereCast(character.transform.position, character.radius, Vector3.down, out hitInfo, 0.83f, LayerMask.GetMask("Wall"));

        if (!isGrounded)
        {
            rigidBody.isKinematic = false;//Vertical.y += Physics.gravity.y * Time.deltaTime;
            transform.SetParent(tileManager.gameObject.transform);


        }
        if (isGrounded)
        {
            rigidBody.isKinematic = true;//Vertical.y = -0.8f;
            character.Move(Vector3.down * 4f * Time.deltaTime);
        }
        if (transform.position.y < -5f)
        {
            characterStatus.DecreaseHealth(60 * Time.deltaTime);
        }
        return isGrounded;
    }
    public IEnumerator AirBorne(Vector3 enemyDirection)
    {
        int temp = 0;
        while (temp < 3f)
        {
            rigidBody.isKinematic = false;
            rigidBody.AddForce(Vector3.up * 10+enemyDirection*2, ForceMode.Impulse);
            jumpPower = 4.9f;//characterStatus.GetJumpPower();
                             // Debug.Log("Jump");
            Vertical.y = jumpPower;
            temp++;
            yield return null;
        }
        ////playerRigidbody.AddForce(new Vector3(0, jumpPower, 0), ForceMode.Impulse);
        //float velocity = 1f;
        //while(velocity>0)
        //{
        //    character.Move(Vector3.up * velocity+enemyDirection);

        //    velocity -= 2 * Time.deltaTime;
        //    yield return null;
        //}
    }

    private void Jumping()
    {

        if (Input.GetKey(KeyCode.Space))
        {
            if (isGrounded) 
            {
                isGrounded = false;
                rigidBody.isKinematic = false;
                rigidBody.AddForce(Vector3.up*11, ForceMode.Impulse);
                jumpPower = 1f;//characterStatus.GetJumpPower();
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

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Debug.Log(hit.gameObject.name);
        if (hit.gameObject.CompareTag("Floor") && hit.transform != transform.parent)
        {
            transform.SetParent(hit.transform); 
            string[] tilePos = hit.gameObject.name.Split(',');
            if(tilePos.Length == 2) positionData.playerTilePosition = new Vector2Int(int.Parse(tilePos[0]), int.Parse(tilePos[1]));
        }
        
    }
}
