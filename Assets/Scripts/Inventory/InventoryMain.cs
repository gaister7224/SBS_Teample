using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public enum UIType
{
    None,
    Inventory,
    Chest,
    SkillWindow,
    Dialogue,
    Store,
    VillageStore,
    LevelReward,
    DungeonEntry,
    OptionWindow
}

/// <summary>
/// 여러 아이템을 담을 가장 기본적인 인벤토리
/// </summary>
public class InventoryMain : InventoryBase
{
    public InputActionAsset uiInputAction;
    public InputActionMap uiActionMap;

    public static bool IsInventoryActive = false;
    public bool slotClick = false;

    public GameObject player;
    public PlayerAttack playerAttack;
    private ItemRaycast itemRaycast;

    public GameObject playerProfile;
    public InventorySlot hpPotionSlot;
    public InventorySlot mpPotionSlot;

    public TextMeshProUGUI goldText;

    [SerializeField] private GameObject levelRewardWindow;

    public UIType currentUI = UIType.None;

    new void Awake()
    {
        base.Awake();

        uiActionMap = uiInputAction.FindActionMap("Option");
    }

    private void OnEnable()
    {
        uiActionMap.Enable();
        uiActionMap.FindAction("OpenInventory").performed += OnOpenInventory;
        uiActionMap.FindAction("ESC").performed += OnESC;
        
    }

    private void OnDisable()
    {
        if (uiActionMap != null)
        {
            uiActionMap.FindAction("OpenInventory").performed -= OnOpenInventory;
            uiActionMap.FindAction("ESC").performed -= OnESC;
            uiActionMap.Disable();
        }
    }
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerAttack = player.GetComponent<PlayerAttack>();
        itemRaycast = player.GetComponent<ItemRaycast>();
    }
    private void OnOpenInventory(InputAction.CallbackContext value)
    {
        if (GameManager.instance.storageTutorial)
        {
            if (!IsInventoryActive && currentUI == UIType.None /*&& GameManager.instance.mapState == MapState.Village*/)
            {
                OpenInventory();
            }
            else if (IsInventoryActive && currentUI == UIType.Inventory)
            {
                CloseInventory();

                if (!GameManager.instance.inventoryTutorial)
                {
                    if (!DialogueManager.instance.start)
                    {
                        DialogueManager.instance.OnDialogue(UIManager.Instance.statusExplainDialogue);
                        GameManager.instance.inventoryTutorial = true;
                    }
                }
            }
        }
    }

    private void OnESC(InputAction.CallbackContext value)
    {
        if (currentUI == UIType.LevelReward)
        {
            levelRewardWindow.SetActive(false);
        }
    }

    public void LevelRewardWindowAppear()
    {
        levelRewardWindow.SetActive(true);
    }

    public void LevelRewardDisWindowAppear()
    {
        levelRewardWindow.SetActive(false);
    }

    private void OpenInventory()
    {
        if (inventoryBase != null)
        {
            inventoryBase.SetActive(true);
            goldText.text = "Gold : " + GameManager.instance.gold.ToString();
            playerProfile.SetActive(false);
            IsInventoryActive = true;
            playerAttack.uiClicking = true;
            Time.timeScale = 0;
            currentUI = UIType.Inventory;

            Cursor.visible = true;
        }
    }

    private void CloseInventory()
    {
        if (inventoryBase != null)
        {
            inventoryBase.SetActive(false);
            playerProfile.SetActive(true);
            IsInventoryActive = false;
            playerAttack.uiClicking = false;
            Time.timeScale = 1;
            currentUI = UIType.None;

            //Cursor.visible = false;
        }
    }

    /// <summary>
    /// 특정 아이템 슬롯에 아이템을 등록시킨다
    /// </summary>
    /// <param name="item">어떤 아이템?</param>
    /// <param name="targetSlot">어느 슬롯에?</param>
    /// <param name="count">개수는?></param>

    public void AcquireItem(Item item, InventorySlot targetSlot, int count = 1)
    {
        //중첩 가능하면
        if (item.CanOverlap)
        {
            if (targetSlot.Item != null && targetSlot.IsMask(item))
            {
                if (targetSlot.Item.ItemID == item.ItemID)
                {
                    targetSlot.UpdateSlotCount(count);
                }
            }
        }
        else
        {
            targetSlot.AddItem(item, count);
        }
    }

    public void AcquireItem(Item item, int count = 1)
    {
        //중첩 가능하면
        if (item.CanOverlap)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].Item != null && slots[i].IsMask(item))
                {
                    if (slots[i].Item.ItemID == item.ItemID)
                    {
                        slots[i].UpdateSlotCount(count);
                        return;
                    }
                }
            }
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].Item == null && slots[i].IsMask(item))
            {
                slots[i].AddItem(item, count);
                return;
            }
        }
    }

    public InventorySlot[] GetAllItems()
    {
        return slots;
    }
}
