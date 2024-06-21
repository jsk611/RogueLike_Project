using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchController : MonoBehaviour
{
    [SerializeField] Animator playerAnimator;
    BoxCollider HitBox;
    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        HitBox = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if(playerAnimator.GetBool("crawling"))
        {
            HitBox.transform.localScale = new Vector3(1,0.5f,1);
            Debug.Log("crawling");
        }
        else HitBox.transform.localScale = new Vector3(1,1,1);
    }
}
