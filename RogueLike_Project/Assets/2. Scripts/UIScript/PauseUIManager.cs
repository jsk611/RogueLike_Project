using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
    }
    public void OptionDisplay(int selectedOption)
    {
        Debug.Log((Options)selectedOption + "selected");
        option.TryGetValue((Options)selectedOption, out GameObject display);
        ResetDisplay();
        Main.SetActive(false);
        display.SetActive(true);
    }
    public void UpdateDisplay(bool cursorState)
    {
        ResetDisplay();
        gameObject.SetActive(cursorState);
    }
    public void Resume()
    {
        bool cursorState = !character.GetCursorState();
        character.SetCursorState(cursorState);
        gameObject.SetActive(!cursorState);
    }
}
