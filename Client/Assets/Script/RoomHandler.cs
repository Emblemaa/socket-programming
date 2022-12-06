using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string ID;
    public string name;
    public Sprite characterIcon;
}

public class RoomHandler : MonoBehaviour
{
    public static RoomHandler Instance;

    public string ID { private set; get; } = "";
    public string OWNER_ID = "OWNER";
    public bool isOwner { private set; get; } = false;

    public List<Player> playerList;

    private void OnPlayerListUpdate(List<Player> playerList)
    {
        this.playerList = playerList;
    }

    public Player GetPlayer(string ID)
    {
        foreach (Player p in playerList) if (p.ID.Equals(ID)) return p;
        return null;
    }

    private void OnJoinRoom(string para)
    {
        string[] list = para.Split('|');
        ID = list[0];
        isOwner = bool.Parse(list[1]);
        OWNER_ID = list[2];
    }

    void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        EventManager.Instance.OnJoinRoom += OnJoinRoom;
        EventManager.Instance.OnPlayerListUpdate += OnPlayerListUpdate;
    }

    void OnDisable()
    {
        EventManager.Instance.OnJoinRoom -= OnJoinRoom;
        EventManager.Instance.OnPlayerListUpdate -= OnPlayerListUpdate;
    }
}
