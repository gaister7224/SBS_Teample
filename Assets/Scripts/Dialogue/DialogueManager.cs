using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    private InventoryMain inventory;

    private float typingSpeed = 0.1f;

    private Queue<DialogueData> datas;

    public string curSentence; //현재 문장
    private string curName;

    public bool quit;
    public bool start;
    public bool noFast;
    private bool isTyping;

    [Header("UI Reference")]
    [SerializeField] GameObject dialogueBox;
    [SerializeField] GameObject nameBox;
    [SerializeField] GameObject textBox;
    //public GameObject skipButton;

    //[Header("Other UI")]
    //public GameObject profile;
    //public GameObject slot;

    public TextMeshProUGUI sentenceText;
    [SerializeField] TextMeshProUGUI nameText;

    [Header("UI Image Component")]
    [SerializeField] private Image displayFaceImage;

    public InputActionReference spaceAction;

    //대화 끝나고 나올 이벤트
    public Action OnDialogueComplete;

    private void OnEnable() => spaceAction.action.Enable();
    private void OnDisable() => spaceAction.action.Disable();

    static public DialogueManager instance;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        typingSpeed = 0.1f;
        quit = false;
        noFast = false;
    }

    public void OnDialogue(DialogueGroup group) //대화 시작
    {
        datas = new Queue<DialogueData>();
        inventory = GameObject.Find("InventorySystem").GetComponent<InventoryMain>();
        inventory.playerAttack.uiClicking = true;
        inventory.playerProfile.SetActive(false);
        Time.timeScale = 0f;

        inventory.currentUI = UIType.Dialogue;

        quit = false;
        start = true;
        typingSpeed = 0.1f;

        sentenceText.text = string.Empty;
        datas.Clear();

        foreach (DialogueData data in group.dialogues)
        {
            datas.Enqueue(data);
        }

        //profile.SetActive(false);
        //slot.SetActive(false);

        dialogueBox.SetActive(true);
        nameBox.SetActive(true);
        textBox.SetActive(true);
        //skipButton.SetActive(true);

        NextSentence();
    }

    private void Update()
    {
        if (!isTyping && start) //대사 하나가 끝나면
        {

            if (spaceAction.action.WasPressedThisFrame())
            {
                NextSentence(); //다음 대사 나옴
            }
        }

        if (quit == true)
        {
            EndDialogue();
        }

        TextSpeed();
    }

    private void EndDialogue()
    {
        start = false;
        quit = false;
        dialogueBox.SetActive(false);
        nameBox.SetActive(false);
        textBox.SetActive(false);
        //skipButton.SetActive(false);

        //profile.SetActive(true);
        //slot.SetActive(true);
        inventory.currentUI = UIType.None;
        inventory.playerAttack.uiClicking = false;
        inventory.playerProfile.SetActive(true);
        Time.timeScale = 1f;
    }

    public void NextSentence() //문자열 출력
    {
        if (datas.Count != 0) //대사가 있음
        {
            DialogueData data = datas.Dequeue();

            curSentence = data.sentence;
            if (string.IsNullOrEmpty(data.name))
                curName = GameManager.instance.nickName;
            else
                curName = data.name;
            Sprite curFaceImage = data.faceImage;

            if (curFaceImage != null)
            {
                displayFaceImage.sprite = curFaceImage;
                displayFaceImage.gameObject.SetActive(true);
            }
            else
            {
                displayFaceImage.gameObject.SetActive(false);
            }

            StopAllCoroutines(); // 혹시 실행 중인 타이핑 멈춤
            StartCoroutine(Typing(curSentence, curName));
        }
        else //대사 없음 즉 대사 끝남
        {
            quit = true;
            start = false;

            OnDialogueComplete?.Invoke();
        }
    }

    IEnumerator Typing(string line, string name) //글자 하나하나씩 나오게함
    {
        isTyping = true;
        sentenceText.text = string.Empty; //글자 초기화
        nameText.text = name;

        foreach (char letter in line.ToCharArray()) //글자를 한글자씩 뽑음
        {
            sentenceText.text += letter;

            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    void TextSpeed()
    {
        if (spaceAction.action.IsPressed() && quit == false && !noFast)
        {
            typingSpeed = 0.01f;
        }

        if (spaceAction.action.WasReleasedThisFrame() && quit == false)
        {
            typingSpeed = 0.1f;
        }
    }
}
