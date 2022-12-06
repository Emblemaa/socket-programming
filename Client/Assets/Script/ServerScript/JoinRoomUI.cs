using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class JoinRoomUI : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField roomIDInput;
    [SerializeField]
    private Button joinButton;
    [SerializeField]
    private Button backButton;
    void Start()
    {
        joinButton.onClick.AddListener(() =>
        {
            ServerHandler.Instance.SendPackage("JOIN_ROOM#" + roomIDInput.text, SendType.TCP);
        });
        backButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MenuScene");
        });
    }
}
