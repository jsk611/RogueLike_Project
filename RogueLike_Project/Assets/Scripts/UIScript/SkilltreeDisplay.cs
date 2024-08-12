using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkilltreeDisplay : MonoBehaviour
{
    SkilltreeManager sm = new SkilltreeManager();

    public int startslevel, startalevel, startelevel;

    public Button[] surviveButtons, attackButtons, etcButtons;
    Button cursurviveButton, curattackButton, curetcButton;

    private void OnEnable()
    {
        ResetSkillTree();
    }

    void ResetSkillTree()
    {
        curattackButton = attackButtons[sm.alevel - startalevel];
        for(int i = 1; i <= sm.alevel - startalevel; i++)
        {
            AlreadyActivated("Attack", i);
        }
        curattackButton.interactable = true;
        curattackButton.onClick.AddListener(OnClickAttack);
    }

    void AlreadyActivated(string type, int i)
    {
        switch (type)
        {
            case "Attack":
                ColorBlock colorBlock = attackButtons[i-1].colors;
                colorBlock.disabledColor = Color.yellow;
                attackButtons[i-1].colors = colorBlock;
                break;
        }
    }

    void OnClickAttack()
    {
        sm.alevel++;
        sm.AttackLevelUp(sm.alevel);
        AlreadyActivated("Attack", sm.alevel);

        curattackButton.interactable = false;
        curattackButton.onClick.RemoveListener(OnClickAttack);
        curattackButton = attackButtons[sm.alevel - startalevel];
        curattackButton.interactable = true;
        curattackButton.onClick.AddListener(OnClickAttack);

        Debug.Log(sm.attack);
    }
}
