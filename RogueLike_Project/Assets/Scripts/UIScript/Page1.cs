using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Page1 : MonoBehaviour
{
    public Button[] healthButton;
    Button curhealthButton;
    int hlevel = 2;

    public Button[] attackButton;
    Button curattackButton;
    int alevel = 2;

    private void Start()
    {
        ResetSkillTree();
    }

    void ResetSkillTree()
    {
        curhealthButton = healthButton[hlevel];
        for(int i = 0; i < hlevel; i++)
        {
            ButtonActivated("Health", i);
        }
        curhealthButton.interactable = true;
        curhealthButton.onClick.AddListener(OnClickHealth);

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
        if (type == "Health")
        {
            ColorBlock colorBlock = healthButton[i].colors;
            colorBlock.disabledColor = Color.yellow;
            healthButton[i].colors = colorBlock;
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
        curhealthButton.interactable = false;
        ButtonActivated("Health", hlevel);
        curhealthButton.onClick.RemoveListener(OnClickHealth);

        hlevel++;
        if (hlevel < healthButton.Length)
        {
            curhealthButton = healthButton[hlevel];
            curhealthButton.interactable = true;
            curhealthButton.onClick.AddListener(OnClickHealth);
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
    }

}