using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListUI : MonoBehaviour
{
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private TMP_Text RoomInfo;
    [SerializeField] private GameObject MasterPanel;

    public void BeginGame()
    {
        ServerHandler.Instance.SendPackage("START_GAME", SendType.TCP);
    }
    public void LeaveRoom()
    {
        ServerHandler.Instance.SendPackage("LEAVE_ROOM", SendType.TCP);
    }

    private void OnPlayerListUpdate(List<Player> playerList)
    {
        if (playerList == null) return;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject obj = transform.GetChild(i).gameObject;
            if (obj.activeSelf) Destroy(obj);
        }
        
        for (int i = 0; i < playerList.Count; i++)
        {
            Player player = playerList[i];
            GameObject prefab = Instantiate(PlayerPrefab, transform);
            prefab.name = player.name;
            prefab.transform.Find("Name").GetComponent<TMP_Text>().text = player.name;
            prefab.transform.Find("Character").GetComponent<Image>().sprite = player.characterIcon;
            prefab.transform.GetChild(0).gameObject.SetActive(RoomHandler.Instance.OWNER_ID.Equals(player.ID));
            prefab.SetActive(true);
        }
        RoomInfo.text = "Room ID: " + RoomHandler.Instance.ID;
        MasterPanel.SetActive(RoomHandler.Instance.isOwner);
    }

    private void OnJoinRoom(string para = "")
    {
        RoomInfo.text = "Room ID: " + RoomHandler.Instance.ID;
        MasterPanel.SetActive(RoomHandler.Instance.isOwner);
    }

    void Awake()
    {
        EventManager.Instance.OnJoinRoom += OnJoinRoom;
        EventManager.Instance.OnPlayerListUpdate += OnPlayerListUpdate;
    }

    private void Start()
    {
        OnPlayerListUpdate(RoomHandler.Instance.playerList);
        OnJoinRoom();
    }

    void OnDisable()
    {
        EventManager.Instance.OnJoinRoom -= OnJoinRoom;
        EventManager.Instance.OnPlayerListUpdate -= OnPlayerListUpdate;
    }
}
