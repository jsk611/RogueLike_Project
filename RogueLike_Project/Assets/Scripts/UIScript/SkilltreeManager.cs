using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkilltreeManager : MonoBehaviour
{
    public int slevel, alevel, elevel;

    public int attack = 0;
    public int attackspeed = 0;
    public int reloadspeed = 0;

    public void AttackLevelUp(int i)
    {
        switch (i)
        {
            case 1: case 4:
                attack += 3;
                break;

            case 2: case 5:
                attackspeed += 2;
                break;

            case 3: case 6:
                reloadspeed += 2;
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

    /*public Button[] surviveButton;
    Button cursurviveButton;
    int slevel = 2;

    public Button[] attackButton;
    Button curattackButton;
    int alevel = 2;

    private void Start()
    {
        ResetSkillTree();
    }

    void ResetSkillTree()
    {
        cursurviveButton = surviveButton[slevel];
        for(int i = 0; i < slevel; i++)
        {
            ButtonActivated("Survive", i);
        }
        cursurviveButton.interactable = true;
        cursurviveButton.onClick.AddListener(OnClickHealth);

        curattackButton = attackButton[alevel];
        for (int i = 0; i < alevel; i++)
        {
            ButtonActivated("Attack", i);
        }
        curattackButton.interactable = true;
        curattackButton.onClick.AddListener(OnClickAttack);

    }

    void ButtonActivated(string type, int i)
    {
        if (type == "Survive")
        {
            ColorBlock colorBlock = surviveButton[i].colors;
            colorBlock.disabledColor = Color.yellow;
            surviveButton[i].colors = colorBlock;
        }
        else if (type == "Attack")
        {
            ColorBlock colorBlock = attackButton[i].colors;
            colorBlock.disabledColor = Color.yellow;
            attackButton[i].colors = colorBlock;
        }
    }

    void OnClickHealth()
    {
        cursurviveButton.interactable = false;
        ButtonActivated("Survive", slevel);
        cursurviveButton.onClick.RemoveListener(OnClickHealth);
        slevel++;

        if (slevel < surviveButton.Length)
        {
            cursurviveButton = surviveButton[slevel];
            cursurviveButton.interactable = true;
            cursurviveButton.onClick.AddListener(OnClickHealth);
        }
    }
    void OnClickAttack()
    {
        curattackButton.interactable = false;
        ButtonActivated("Attack", alevel);
        curattackButton.onClick.RemoveListener(OnClickAttack);

        alevel++;
        if (alevel < attackButton.Length)
        {
            curattackButton = attackButton[alevel];
            curattackButton.interactable = true;
            curattackButton.onClick.AddListener(OnClickAttack);
        }
    }*/
}