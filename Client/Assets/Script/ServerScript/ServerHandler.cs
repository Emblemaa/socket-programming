using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public enum SendType
{
    TCP, UDP
}

public class Packet
{
    public SendType type;
    public string packetType;
    public string content;
    public Packet(SendType type, string packetType, string content)
    {
        this.type = type;
        this.packetType = packetType;
        this.content = content;
    }

    public void Handle(Action<IEnumerator> StartCoroutine)
    {
        Debug.Log(packetType + " " + content);
        switch (packetType)
        {
            case "JOIN_ROOM":
            case "CREATE_ROOM":
                StartCoroutine(ChangeLobby(content));
                break;
            case "UDP":
                RoomHandler.Instance.OWNER_ID = content.Split('|')[1];
                ServerHandler.Instance.OnConnectionEstablish();
                break;
            case "LEAVE_ROOM":
                SceneManager.LoadScene("MenuScene");
                break;
            case "UPDATE_PROFILE":
                SceneManager.LoadScene("MenuScene");
                break;
            case "LIST_PLAYER":
                List<Player> playerList = new List<Player>();
                string[] player = content.Split('|');
                int playerCount = player.Length / 3;
                if (playerCount > 0)
                {
                    for (int i = 0; i < playerCount; i++)
                    {
                        int index = i * 3;
                        Player p = new Player
                        {
                            name = player[index + 1],
                            characterIcon = CharacterLibrary.Instance.GetCharacter(int.Parse(player[index + 2])),
                            ID = player[index]
                        };
                        playerList.Add(p);
                    }
                }
                EventManager.Instance.OnPlayerListUpdate?.Invoke(playerList);
                break;
            case "GAME_START":
                SceneManager.LoadScene("GameScene");
                break;
            case "WIN":
                EventManager.Instance.onWinReturn?.Invoke(content);
                break;
            case "RESULT":
                EventManager.Instance.onResultReturn?.Invoke(int.Parse(content));
                break;
            case "UPDATE_INFO":
                EventManager.Instance.OnEntityUpdate?.Invoke(content);
                break;
            case "QUESTION":
                EventManager.Instance.OnQuestionUpdate?.Invoke(content);
                break;
            case "ERROR":
                HandleError(content);
                break;
        }
    }
    private void HandleError(string error)
    {
        string[] list = error.Split('|');
        if (list.Length < 2) return;
        ServerHandler.Instance.ShowPopup("Error", list[1]);
        switch (list[0])
        {
            case "PROFILE_REQUIRED":
                SceneManager.LoadScene("SelectCharacterScene");
                break;
            default:
                Debug.Log(error);
                break;
        }
    }

    IEnumerator ChangeLobby(string content)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LobbyScene");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        EventManager.Instance.OnJoinRoom?.Invoke(content);
    }
}

public class ServerHandler : MonoBehaviour
{
    public static ServerHandler Instance;

    [Header("UI")]
    public TMP_Text status;
    public Image statusIcon;

    public static string ENDPOINT = "127.0.0.1";
    public static int TCP_PORT = 8080;
    public static int UDP_PORT = 8081;

    private Thread listeningThreadUDP;

    private bool _IsOpen = false;
    private bool _IsClosed = false;
    private Action OnClose;
    private Action<String> OnSendTCP;
    private Action<String> OnSendUDP;

    private Queue pQueue = Queue.Synchronized(new Queue());

    Thread tryThread;
    private void TryConnect()
    {
        if (_IsClosed) return;
        _IsOpen = false;
        tryThread = new Thread(new ThreadStart(() =>
        {
            try
            {
                TcpClient _client;
                _client = new TcpClient();
                _client.Connect(ENDPOINT, TCP_PORT);
                _IsOpen = true;

                Stream stream = _client.GetStream();
                Thread listeningThread = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        var reader = new StreamReader(stream);
                        while (_IsOpen)
                        {
                            string message = reader.ReadLine();
                            ReceivePackage(message, SendType.TCP);
                        }
                        reader.Close();
                    }
                    catch (Exception e)
                    {
                        TryConnect();
                    }
                }));
                listeningThread.Start();

                var writer = new StreamWriter(stream);
                writer.AutoFlush = true;
                OnSendTCP = (message) =>
                {
                    writer.Write(message);
                };

                OnClose = () =>
                {
                    listeningThreadUDP?.Abort();
                    listeningThread.Abort();
                    writer.Close();
                    stream.Close();
                    _client.Close();
                    Debug.Log("Disconnecting");
                };
            }
            catch (Exception e)
            {
                Thread.Sleep(2000);
                TryConnect();
            }
        }));
        tryThread.Start();
    }

    public void OnConnectionEstablish()
    {
        string playerName = PlayerPrefs.GetString("PLAYER_NAME");
        int charID = PlayerPrefs.GetInt("PLAYER_CHAR_ID");
        SendPackage($"UPDATE_PROFILE#{playerName}|{charID}", SendType.TCP);
    }

    public void ReceivePackage(string message, SendType type)
    {
        if (message == null) return;
        string[] list = message.Split('#');
        string packetType = list[0];
        string content = list.Length > 1 ? list[1] : null;
        if (packetType == "UDP")
        {
            //UDP
            int port = int.Parse(content.Split('|')[0]);
            UdpClient udpClient = new UdpClient(port);
            OnSendUDP = (message) =>
            {
                byte[] sendbuf = Encoding.ASCII.GetBytes(message);
                udpClient.Send(sendbuf, sendbuf.Length, ENDPOINT, UDP_PORT);
            };
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);
            Thread listeningThreadUDP = new Thread(new ThreadStart(() =>
            {
                while (_IsOpen)
                {
                    byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.ASCII.GetString(receiveBytes);
                    ReceivePackage(returnData, SendType.UDP);
                }
            }));
            listeningThreadUDP.Start();
        }
        pQueue.Enqueue(new Packet(type, packetType, content));
    }

    public void SendPackage(string message, SendType type)
    {
        if (!_IsOpen) return;
        if (type == SendType.TCP)
        {
            OnSendTCP(message);
            return;
        }
        OnSendUDP?.Invoke(message);
    }

    void OnApplicationQuit()
    {
        _IsClosed = true;
        tryThread?.Abort();
        OnClose?.Invoke();
    }

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        try
        {
            StreamReader reader = new StreamReader(Application.streamingAssetsPath + "/config.txt");
            string content = reader.ReadLine();
            ENDPOINT = content;
            reader.Close();
        }
        catch (Exception _) { }
        TryConnect();
    }

    public GameObject popUpPrefab;
    public void ShowPopup(string title, string content)
    {
        GameObject obj = Instantiate(popUpPrefab);
        obj.transform.Find("Popup").Find("Title").GetComponent<TMP_Text>().text = title;
        obj.transform.Find("Popup").Find("Content").GetComponent<TMP_Text>().text = content;
    }

    private void FixedUpdate()
    {
        if (_IsOpen)
        {
            statusIcon.color = Color.green;
            status.text = "Online";
            status.color = Color.green;
        }
        else
        {
            statusIcon.color = Color.red;
            status.text = $"Connecting to {ENDPOINT}";
            status.color = Color.red;
        }
        lock (pQueue.SyncRoot)
        {
            if (pQueue.Count > 0)
            {
                Packet p = (Packet)pQueue.Dequeue();
                p.Handle((IEnumerator function) =>
                {
                    StartCoroutine(function);
                });
            }
        }
    }
}
