using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkilltreeManager : MonoBehaviour
{
    public int slevel = 0, alevel = 0, elevel = 0;

    public int health = 100;
    public int staminaspeed = 10;
    public int resilience = 10;

    public int attack = 100;
    public int attackspeed = 10;
    public int reloadspeed = 10;

    public float DNAdrop = 1f;
    public float extraUpgrade = 0f;

    public void LevelUp(string type)
    {
        switch (type)
        {
            case "Survive":
                slevel++;
                break;

            case "Attack":
                alevel++;
                break;

            case "Etc":
                elevel++;
                break;
        }
    }

    public void SurviveLevelUp(int i)
    {
        switch (i)
        {
            case 1:
            case 4:
                health += 3;
                break;

            case 2:
            case 5:
                staminaspeed += 1;
                break;

            case 3:
            case 6:
                resilience += 1;
                break;
        }
    }

    public void AttackLevelUp(int i)
    {
        switch (i)
        {
            case 1:
            case 4:
                attack += 3;
                break;

            case 2:
            case 5:
                attackspeed += 1;
                break;

            case 3:
            case 6:
                reloadspeed += 1;
                break;
        }
    }

    public void EtcLevelUp(int i)
    {
        switch (i)
        {
            case 1:
            case 3:
                DNAdrop += 0.1f;
                break;

            case 2:
            case 4:
                extraUpgrade += 1f;
                break;
        }
    }

    private void Start()
    {
        for(int i = 0; i < alevel; i++)
        {
            AttackLevelUp(i);
        }
    }
}