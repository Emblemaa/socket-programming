using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreateRoomUI : MonoBehaviour
{
    [SerializeField]
    private Button createButton;
    [SerializeField]
    private Button joinButton;
    [SerializeField]
    private Button customizeButton;
    void Start()
    {
        createButton.onClick.AddListener(() =>
        {
            ServerHandler.Instance.SendPackage("CREATE_ROOM", SendType.TCP);
        });
        joinButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("JoinRoomScene");
        });

        customizeButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("SelectCharacterScene");
        });
    }
}
