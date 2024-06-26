using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] AnimationClip weaponAnimation;
    // Start is called before the first frame update
    public AnimationClip GetAnimation()
    {
        return weaponAnimation;
    }
    void PlayAnimation(Animator playerAnimator )
    {
        playerAnimator.Play(weaponAnimation.name);
    }

}
