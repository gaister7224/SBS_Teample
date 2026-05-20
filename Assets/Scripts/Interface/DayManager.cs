using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public enum Day
{
    day,
    night
}

public class DayManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dayText;
    public Button dayEndButton;
    public GameObject sunLight;
    public Vector3 daySunRotation;
    public Vector3 nightSunRotation;

    private StorageToInventory storageToInventory;
    private StoreManager storeManager;
    private PlayerProfile playerProfile;

    public Day curDay = Day.day;

    [SerializeField] private GameObject dayIcon;
    [SerializeField] private GameObject nightIcon;

    public static DayManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        storageToInventory = GameObject.Find("InventorySystem").GetComponent<StorageToInventory>();
        storeManager = GameObject.Find("InventorySystem").GetComponent<StoreManager>();
        playerProfile = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerProfile>();
        if (dayEndButton != null)
        {
            dayEndButton.interactable = false;
        }
        daySunRotation = new Vector3(50, -30, 0);
        nightSunRotation = new Vector3(-49, -195, -11);
    }

    public void DayEnd()
    {
        if (UIManager.Instance.inventory.currentUI == UIType.None && GameManager.instance.itemGetAll)
        {
            dayEndButton.interactable = false;
            sunLight.transform.rotation = Quaternion.Euler(daySunRotation);
            curDay = Day.day;
            DayIconAppear();
            GameManager.instance.dayCount++;
            if(GameManager.instance.installImpossibleStart)
            {
                GameManager.instance.artifactInstallImpossibleDay++;

                if(GameManager.instance.artifactInstallImpossibleDay >= 3)
                {
                    GameManager.instance.artifactInstallImpossibleDay = 0;
                    GameManager.instance.installImpossibleStart = false;
                }
            }
            dayText.text = GameManager.instance.dayCount.ToString();
            GameManager.instance.dayEnd = true;
            storeManager.VillageStoreReset();
            playerProfile.ActCountReset();
            GameManager.instance.OnDayChange?.Invoke();
            StoreManager.Instance.ReFreshShop();

            if (!GameManager.instance.dayTutorial)
            {
                if (!DialogueManager.instance.start)
                {
                    DialogueManager.instance.OnDialogue(UIManager.Instance.endExplainDialogue);
                    DialogueManager.instance.OnDialogueComplete += PortalZoom;
                    GameManager.instance.dayTutorial = true;
                }
            }
        }
    }

    private void PortalZoom()
    {
        Time.timeScale = 0;
        UIManager.Instance.portalDirector.Play();
        StartCoroutine(PlayAndCheck());
    }

    IEnumerator PlayAndCheck()
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime((float)UIManager.Instance.storageDirector.duration);
        Time.timeScale = 1;
        DialogueManager.instance.OnDialogueComplete -= PortalZoom;
        UIManager.Instance.storageDirector.gameObject.SetActive(false);
    }

    public void DayIconAppear()
    {
        dayIcon.SetActive(true);
        nightIcon.SetActive(false);
    }
    public void NightIconAppear()
    {
        dayIcon.SetActive(false);
        nightIcon.SetActive(true);
    }
}
