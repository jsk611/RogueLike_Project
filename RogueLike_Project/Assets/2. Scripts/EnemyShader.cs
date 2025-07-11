using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShader : MonoBehaviour {

    static public EnemyShader instance = new EnemyShader();

    public Material monsterNormal;
    public Material monsterBlazed;
    public Material monsterShocked;
    public Material monsterFrozen;

    private void Start()
    {
        instance = this;
    }
}
