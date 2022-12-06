using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    public GameObject PlayerPrefab;

    private void UpdateRoomInfo()
    {
         
    }
    private void Start()
    {
        UpdateRoomInfo();
    }
    private void OnAddPoint(string content)
    {
        try
        {
            string[] list = content.Split('|');
            Transform target = transform.Find(list[1]);
            target.GetChild(0).GetComponent<Text>().text = "" + (int.Parse(target.GetChild(0).GetComponent<Text>().text) + int.Parse(list[2]));
        }
        catch(Exception e)
        {

        }
    }
    void Awake()
    {
        EventManager.Instance.OnAddPoint += OnAddPoint;
    }

    void OnDisable()
    {
        EventManager.Instance.OnAddPoint -= OnAddPoint;
    }
}
