using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{
    public InputField input;
    public Button button;
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            ServerHandler.Instance.SendPackage(input.text, SendType.TCP);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
