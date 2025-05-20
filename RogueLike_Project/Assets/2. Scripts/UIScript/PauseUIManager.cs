using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.WSA;

public class PauseUIManager : MonoBehaviour
{
    public static PauseUIManager instance;
    [SerializeField] GameObject Main;
    [SerializeField] GameObject GamePlayOption;
    [SerializeField] GameObject DisplayOption;
    [SerializeField] GameObject SoundOption;
    [SerializeField] GameObject LanguageOption;

    CharacterBehaviour character;

    public enum Options{
        Main,
        GamePlayOption,
        DisplayOption,
        SoundOption,
        LanguageOption
    }

    private Dictionary<Options, GameObject> option;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        ResetDisplay();
        gameObject.SetActive(false);

        character = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
    }

    // Update is called once per frame
 
    public void ResetDisplay()
    {
        GamePlayOption.SetActive(false);
        DisplayOption.SetActive(false);
        SoundOption.SetActive(false);
        LanguageOption.SetActive(false);
        Main.SetActive(true);
    }
    public void OptionDisplay(int selectedOption)
    {
        Debug.Log((Options)selectedOption + "selected");
        option.TryGetValue((Options)selectedOption, out GameObject display);
        ResetDisplay();
        Main.SetActive(false);
        display.SetActive(true);
    }
    public void UpdateDisplay()
    {
        ResetDisplay();
        Time.timeScale = 0f;
        gameObject.SetActive(true);
    }
    public void CancelDisplay()
    {
        ResetDisplay();
        Resume();
    }
    public void Resume()
    {
        character.SetCursorState(true);
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
    public void Exit()
    {
        PermanentUpgradeManager.instance.SaveData();
        SceneManager.LoadScene(0);
    }
}
