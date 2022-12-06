using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    public Action<string> OnEntityDie;
    public Action<string> OnEntityAttack;
    public Action<string> OnAddPoint;
    public Action<string> OnEntityUpdate;
    public Action<List<Player>> OnPlayerListUpdate;
    public Action<int> onResultReturn;
    public Action<string> onWinReturn;
    public Action<string> OnJoinRoom;
    public Action<string> OnQuestionUpdate;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
