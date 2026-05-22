using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreatingCharacter : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject nameZone;
    [SerializeField] private TMP_InputField nameInput;

    private Image playerDisplayImage;
    private RectTransform pos;
    [SerializeField] private Sprite[] playerImage;
    [SerializeField] private GameObject playZone;
    [SerializeField] private TextMeshProUGUI[] infoTexts;
    [SerializeField] private Sprite[] swordIdleSprites;
    private float frameRate = 0.05f; // 프레임 속도

    private int currentFrame = 0;
    private float timer = 0f;

    private bool characterClick = false;
    private bool create = false;
    private string curName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerDisplayImage = GetComponent<Image>();
        pos = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        //게임오브젝트에서 직업 뭐 선택했는지 가져와서 해야함.

        //이미지 움직임
        if (create)
        {
            timer += Time.deltaTime;
            if (timer >= frameRate)
            {
                // 프레임 교체
                currentFrame = (currentFrame + 1) % swordIdleSprites.Length;
                playerDisplayImage.sprite = swordIdleSprites[currentFrame];
                timer = 0f;
            }
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!characterClick)
        {
            nameZone.SetActive(true);
            characterClick = true;
        }
        else if (characterClick)
        {
            if (!playZone.activeSelf)
            {
                playZone.SetActive(true);
                Info();
            }
            else
            {
                playZone.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 닉네임 생성
    /// </summary>
    public void NickNameCreate()
    {
        curName = nameInput.text;
        //GameManager.instance.nickName = curName;
        if (curName.Length > 0)
        {
            UpdatePlayerPreview(GameManager.instance.profileIndex);
            pos.position = new Vector3(pos.position.x, pos.position.y + 130, pos.position.z);
            pos.localScale = new Vector3(2.5f, 4.5f, 4);
            GameManager.instance.name = curName;
            create = true;
            nameZone.SetActive(false);
        }
    }

    /// <summary>
    /// 플레이어 생성 시 이미지 변환
    /// </summary>
    /// <param name="jobIndex"></param>
    private void UpdatePlayerPreview(int jobIndex)
    {
        if (jobIndex >= 0 && jobIndex < playerImage.Length)
        {
            playerDisplayImage.sprite = playerImage[jobIndex];
        }
    }

    /// <summary>
    /// 스테이터스 설명 작성
    /// </summary>
    private void Info()
    {
        infoTexts[0].text = "이름 : " + GameManager.instance.name;
        infoTexts[1].text = "직업 : " + GameManager.instance.job;
        infoTexts[2].text = "레벨 : " + GameManager.instance.level;
    }

    public void GamePlay()
    {
        SceneManager.LoadScene("MainScene");
        GameManager.instance.character1Spawn = true;
    }

    public void Back()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
