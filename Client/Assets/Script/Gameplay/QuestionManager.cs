using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuestionManager : MonoBehaviour
{
    [SerializeField] private GameObject passPrefab;
    [SerializeField] private GameObject eliminatedPrefab;
    [SerializeField] private GameObject winPrefab;

    [SerializeField] private Button skipButton;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text targetPlayerName;
    [SerializeField] private List<GameObject> buttons;

    [SerializeField] private string targetQuestion;

    private float time = 0;
    private float maxTime = 20;
    private string targetID;

    private void OnQuestionUpdate(string para)
    {
        string[] list = para.Split('|');
        targetID = list[0];
        targetQuestion = list[1];
        Player player = RoomHandler.Instance.GetPlayer(targetID);
        if (player != null)
        {
            characterImage.sprite = player.characterIcon;
            targetPlayerName.text = player.name;
        }
        StartCoroutine(TypeQuestion(targetQuestion));
        bool flag = targetID == RoomHandler.Instance.OWNER_ID;
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].transform.Find("Content").GetComponent<TMP_Text>().text = list.Length <= 2 + i ? "" : list[2 + i];
            buttons[i].transform.GetComponent<Button>().interactable = flag;
        }
        skipButton.gameObject.SetActive(flag);
        time = maxTime;
    }

    IEnumerator TypeQuestion(string text)
    {
        questionText.text = "";
        for(int i = 0; i < text.Length; i++)
        {
            questionText.text += text[i];
            yield return new WaitForSeconds(0.02f);
        }
    }
    void Awake()
    {
        skipButton.onClick.AddListener(() =>
        {
            ServerHandler.Instance.SendPackage($"ANSWER#{-1}", SendType.TCP);
            skipButton.gameObject.SetActive(false);
        });
        EventManager.Instance.OnQuestionUpdate += OnQuestionUpdate;
        EventManager.Instance.onResultReturn += OnResultReturn;
        EventManager.Instance.onWinReturn += OnWinReturn;
        for (int i = 0; i < buttons.Count; i++)
        {
            RegisterButton(i);
        }
    }

    private void RegisterButton(int k)
    {
        buttons[k].GetComponent<Button>().onClick.AddListener(() =>
        {
            if (targetID != RoomHandler.Instance.OWNER_ID) return;
            ServerHandler.Instance.SendPackage($"ANSWER#{k}", SendType.TCP);
        });
    }

    private void OnResultReturn(int k)
    {
        Player player = RoomHandler.Instance.GetPlayer(targetID);
        if (player == null) return;
        if (k > 0)
        {
            GameObject obj = Instantiate(passPrefab);
            obj.transform.Find("Popup").Find("Text").GetComponent<TMP_Text>().text = player.name + " passed";
        }
        else if(k <= 0)
        {
            GameObject obj = Instantiate(eliminatedPrefab);
            obj.transform.Find("Popup").Find("Text").GetComponent<TMP_Text>().text = player.name + (k<0?" eliminated" : " timed out");
        }
    }

    private void OnWinReturn(string id)
    {
        StartCoroutine(NextScene());
        Player player = RoomHandler.Instance.GetPlayer(id);
        if (player == null) return;
        GameObject obj = Instantiate(winPrefab);
        obj.transform.Find("Name").GetComponent<TMP_Text>().text = player.name;
    }

    IEnumerator NextScene()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("LobbyScene");
    }

    void OnDisable()
    {
        EventManager.Instance.OnQuestionUpdate -= OnQuestionUpdate;
        EventManager.Instance.onResultReturn -= OnResultReturn;
        EventManager.Instance.onWinReturn -= OnWinReturn;
    }

    private void Update()
    {
        time -= Time.deltaTime;
        time = Mathf.Max(0, time);

        timerText.text = ((int)time) + "s";
        timerText.color = time < 5 ? Color.red : Color.white;
    }
}
