using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
    // Start is called before the first frame update
    Animator playerAnimator;
    Rigidbody playerRigidbody;

    float Stamina;
    [SerializeField] float jumpPower = 10f;
    void Start()
    {
        Stamina = GetComponentInChildren<PlayerControl>().Stamina;
        playerAnimator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Jumping();
    }
    void Jumping()
    {
        if (!playerAnimator.GetBool("isJumping") && Input.GetKey(KeyCode.Space) && Stamina > 0)
        {
            playerRigidbody.AddForce(new Vector3(0, jumpPower, 0), ForceMode.Impulse);
            playerAnimator.SetBool("isJumping", true);
            Stamina -= 20;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
          
            playerAnimator.SetBool("isJumping", false);
        }
    }
}
