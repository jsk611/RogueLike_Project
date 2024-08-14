using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkilltreeDisplay : MonoBehaviour
{
    public SkilltreeManager sm;

    public int startslevel, startalevel, startelevel;

    public Button[] surviveButtons, attackButtons, etcButtons;
    Button cursurviveButton, curattackButton, curetcButton;

    private void OnEnable()
    {
        ResetSkillTree();
    }

    void ResetSkillTree()
    {
        if(sm.slevel >= startslevel && sm.slevel - startslevel < surviveButtons.Length)
        {
            cursurviveButton = surviveButtons[sm.slevel - startslevel];
            for (int i = 1; i <= sm.slevel - startslevel; i++)
            {
                AlreadyActivated("Survive", i);
            }
            cursurviveButton.interactable = true;
            cursurviveButton.onClick.RemoveListener(OnClickSurvive);
            cursurviveButton.onClick.AddListener(OnClickSurvive);
        }
        if (sm.alevel >= startalevel && sm.alevel - startalevel < attackButtons.Length)
        {
            curattackButton = attackButtons[sm.alevel - startalevel];
            for (int i = 1; i <= sm.alevel - startalevel; i++)
            {
                AlreadyActivated("Attack", i);
            }
            curattackButton.interactable = true;
            curattackButton.onClick.RemoveListener(OnClickAttack);
            curattackButton.onClick.AddListener(OnClickAttack);
        }
        if (sm.elevel >= startelevel && sm.elevel - startelevel < etcButtons.Length)
        {
            curetcButton = etcButtons[sm.elevel - startelevel];
            for (int i = 1; i <= sm.elevel - startelevel; i++)
            {
                AlreadyActivated("Etc", i);
            }
            curetcButton.interactable = true;
            curetcButton.onClick.RemoveListener(OnClickEtc);
            curetcButton.onClick.AddListener(OnClickEtc);
        }
    }

    void AlreadyActivated(string type, int i)
    {
        switch (type)
        {
            case "Survive":
                ColorBlock scolorBlock = surviveButtons[i - 1].colors;
                scolorBlock.disabledColor = Color.yellow;
                surviveButtons[i - 1].colors = scolorBlock;
                break;

            case "Attack":
                ColorBlock acolorBlock = attackButtons[i - 1].colors;
                acolorBlock.disabledColor = Color.yellow;
                attackButtons[i - 1].colors = acolorBlock;
                break;

            case "Etc":
                ColorBlock ecolorBlock = etcButtons[i - 1].colors;
                ecolorBlock.disabledColor = Color.yellow;
                etcButtons[i - 1].colors = ecolorBlock;
                break;
        }
    }

    void OnClickSurvive()
    {
        sm.slevel++;
        sm.SurviveLevelUp(sm.slevel);
        AlreadyActivated("Survive", sm.slevel - startslevel);

        cursurviveButton.interactable = false;
        cursurviveButton.onClick.RemoveListener(OnClickSurvive);

        if(sm.slevel - startslevel < surviveButtons.Length)
        {
            cursurviveButton = surviveButtons[sm.slevel - startslevel];
            cursurviveButton.interactable = true;
            cursurviveButton.onClick.AddListener(OnClickSurvive);
        }
    }

    void OnClickAttack()
    {
        sm.alevel++;
        sm.AttackLevelUp(sm.alevel);
        AlreadyActivated("Attack", sm.alevel - startalevel);

        curattackButton.interactable = false;
        curattackButton.onClick.RemoveListener(OnClickAttack);

        if (sm.alevel - startalevel < attackButtons.Length)
        {
            curattackButton = attackButtons[sm.alevel - startalevel];
            curattackButton.interactable = true;
            curattackButton.onClick.AddListener(OnClickAttack);
        }

        Debug.Log(sm.attack);
    }

    void OnClickEtc()
    {
        sm.elevel++;
        sm.EtcLevelUp(sm.elevel);
        AlreadyActivated("Etc", sm.elevel - startelevel);

        curetcButton.interactable = false;
        curetcButton.onClick.RemoveListener(OnClickEtc);

        if (sm.elevel - startelevel < etcButtons.Length)
        {
            curetcButton = etcButtons[sm.elevel - startelevel];
            curetcButton.interactable = true;
            curetcButton.onClick.AddListener(OnClickEtc);
        }
    }
}
