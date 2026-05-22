using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject jobChoiceUI;
    public JobChoice jobChoice;
    public GameObject storageInventroy;
    public InventoryMain inventory;
    public GameObject player;

    [Header("HP관련 오브젝트")]
    public Image hpBackground;
    public Image hpMask;
    public TextMeshProUGUI hpText;
    public GameObject barrierHpText;
    [Header("MP관련 오브젝트")]
    public Image mpBackground;
    public Image mpMask;
    public TextMeshProUGUI mpText;
    [Header("행동력 관련 오브젝트")]
    public Slider acSlider;
    public TextMeshProUGUI acText;
    [Header("스테이터스 표시")]
    public TextMeshProUGUI hpStatusText;
    public TextMeshProUGUI mpStatusText;
    public TextMeshProUGUI atkStatusText;
    public TextMeshProUGUI basicAtkStatusText;
    public TextMeshProUGUI defStatusText;
    public TextMeshProUGUI moveSpeedStatusText;
    public TextMeshProUGUI criticalStatusText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI jobText;
    [Header("프로필 스킬 슬롯 부모")]
    public GameObject slotParent;
    [Header("쿨타임")]
    public Slider[] coolTimeSlider;
    [Header("상점 관련 창들")]
    public GameObject storeWindow;
    public GameObject villageStoreWindow;
    [Header("시네머신 카메라")]
    public CinemachineCamera virtualCamera;
    [Header("프로필 관련 오브젝트")]
    public TextMeshProUGUI profileNameText;
    public TextMeshProUGUI profileLevelText;
    [Header("마을장소 위치")]
    public Transform villagePos;
    public BoxCollider villageCollider;
    [Header("직업 선택 버튼")]
    public Button swordJobButton;
    public Button bowJobButton;
    public Button stampJobButton;
    [Header("대사")]
    public DialogueGroup storeExplainDialogue;
    public DialogueGroup inventoryExplainDialogue;
    public DialogueGroup statusExplainDialogue;
    public DialogueGroup endExplainDialogue;
    [Header("연출 타임라인")]
    public PlayableDirector storageDirector;
    public PlayableDirector portalDirector;
    [Header("페이트 이미지")]
    public GameObject fade;

    private void Awake()
    {
        Instance = this;
    }
}
