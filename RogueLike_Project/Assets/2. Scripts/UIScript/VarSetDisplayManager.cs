using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VarSetDisplayManager : MonoBehaviour
{

    // Start is called before the first frame update
    [SerializeField] GameObject ATKvar_set; //type 0
    [SerializeField] GameObject UTLvar_set; //type 1
    [SerializeField] GameObject Coinvar_set; //type 2

    private GameObject selectedUpgradeType;

    private UpgradeManager_New upgradeManager;
    void Start()
    {
        upgradeManager = ServiceLocator.Current.Get<IGameModeService>().GetUpgradeManager();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ShowVarSet(int type)
    {
        ResetVarSet();
        switch(type)
        {
            case 0:
                selectedUpgradeType = ATKvar_set;
                ATKvar_set.SetActive(true);
                break;
            case 1:
                selectedUpgradeType = UTLvar_set;
                UTLvar_set.SetActive(true); 
                break;
            case 2:
                selectedUpgradeType = Coinvar_set;
                Coinvar_set.SetActive(true);
                break;
        }
        TypingButtons();
    }
    public void ResetVarSet()
    {
        if (selectedUpgradeType != null)
        {
            CompleteUpgrade[] deActiveButtons = selectedUpgradeType.GetComponentsInChildren<CompleteUpgrade>();
            foreach (CompleteUpgrade de in deActiveButtons)
            {
                de.gameObject.SetActive(false);
            }
        }
    }
    private void TypingButtons()
    {
        CompleteUpgrade[] childrens = selectedUpgradeType.GetComponentsInChildren<CompleteUpgrade>(true);
        foreach (CompleteUpgrade child in childrens)
        {
            child.gameObject.SetActive(true);
            StartCoroutine(Typing(child.gameObject));
        }
    }

    IEnumerator Typing(GameObject curButton)
    {
        TMP_Text tx = curButton.transform.GetChild(1).GetComponent<TMP_Text>();
        string temptx = curButton.GetComponent<CompleteUpgrade>().baseText;
        tx.text = "";

        for (int i = 0; i <= temptx.Length; i++)
        {
            tx.text = temptx.Substring(0, i);
            yield return new WaitForSeconds(0.06f);
        }
    }
}
