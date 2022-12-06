using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button submitButton;
    [SerializeField] private Image characterIcon;
    [SerializeField] private TMP_InputField playerName;

    private int currentCharacterID = 0;

    void Start()
    {
        playerName.text = PlayerPrefs.GetString("PLAYER_NAME");
        currentCharacterID = PlayerPrefs.GetInt("PLAYER_CHAR_ID");
        UpdateCharacter();

        prevButton.onClick.AddListener(() =>
        {
            AddIndex(-1);
        });

        nextButton.onClick.AddListener(() =>
        {
            AddIndex(1);
        });

        submitButton.onClick.AddListener(() =>
        {
            bool check = Regex.IsMatch(playerName.text, @"^[_a-zA-Z0-9]+$") && playerName.text.Length > 2 && playerName.text.Length < 15;
            if (!check)
            {
                ServerHandler.Instance.ShowPopup("Warning", "Your name should not have any special character or blank space and the length should be between 2-15 characters");
                return;
            }
            PlayerPrefs.SetString("PLAYER_NAME", playerName.text);
            PlayerPrefs.SetInt("PLAYER_CHAR_ID", currentCharacterID);

            ServerHandler.Instance.SendPackage($"UPDATE_PROFILE#{playerName.text}|{currentCharacterID}", SendType.TCP);
        });
    }

    private void AddIndex(int k)
    {
        currentCharacterID+=k;
        if (currentCharacterID >= CharacterLibrary.Instance.GetCount()) currentCharacterID = 0;
        if (currentCharacterID < 0) currentCharacterID = CharacterLibrary.Instance.GetCount() - 1;
        UpdateCharacter();
    }

    private void UpdateCharacter()
    {
        characterIcon.sprite = CharacterLibrary.Instance.GetCharacter(currentCharacterID);
    }
}
