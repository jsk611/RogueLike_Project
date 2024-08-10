using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkilltreeManager : MonoBehaviour
{
    public Button[] attackButton;
    Button curattackButton;
    int alevel = 2;

    private void Start()
    {
        curattackButton = attackButton[alevel];
        for(int i = 0; i < alevel; i++)
        {
            ButtonActivated(i);
        }
        curattackButton.interactable = true;
        curattackButton.onClick.AddListener(OnClickStart);
    }

    void OnClickStart()
    {
        curattackButton.interactable = false;
        ButtonActivated(alevel);
        curattackButton.onClick.RemoveListener(OnClickStart);

        alevel++;
        if (alevel < attackButton.Length)
        {
            curattackButton = attackButton[alevel];
            curattackButton.interactable = true;
            curattackButton.onClick.AddListener(OnClickStart);
        }
    }

    void ButtonActivated(int i)
    {
        ColorBlock colorBlock = attackButton[i].colors;
        colorBlock.disabledColor = Color.yellow;
        attackButton[i].colors = colorBlock;
    }
}