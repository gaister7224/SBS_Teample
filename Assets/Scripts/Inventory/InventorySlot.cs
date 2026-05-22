using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 슬롯 하나를 담당
/// </summary>

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Item item;
    public Item Item
    {
        get
        {
            return item;
        }
    }

    [Header("해당 슬롯에 어떤 타입이 들어올 수 있는가")]
    [SerializeField] private ItemType slotMask;

    public int itemCount; //획득한 아이템 개수

    [Header("아이템 슬롯에 있는 UI 오브젝트")]
    [SerializeField] private Image itemImage;
    [SerializeField] private GameObject explanToolTip;
    [SerializeField] private TextMeshProUGUI textCount;
    [SerializeField] private TextMeshProUGUI explanText;
    [SerializeField] private TextMeshProUGUI nameText;

    [SerializeField] private Animator ani;

    private InventoryMain inventory;
    private StorageToInventory storageToInventory;

    private void OnEnable()
    {
        inventory = GameObject.Find("InventorySystem").GetComponent<InventoryMain>();
        storageToInventory = GameObject.Find("InventorySystem").GetComponent<StorageToInventory>();

        if (explanToolTip != null)
        {
            explanToolTip.SetActive(false);
            inventory.slotClick = false;
        }

        SlotCount();
    }

    // 아이템 이미지의 투명도 조절
    private void SetColor(float _alpha)
    {
        Color color = itemImage.color;
        color.a = _alpha;
        itemImage.color = color;
    }

    /// <summary>
    /// slotMask에서 설정된 값에 따라 비트연산을한다.
    /// 현재 마스크값이 비트연산으로 0이 나온다면 현재 슬롯에 마스크가 일치하지 않는다는 뜻.
    /// 0이 아닌 수는 현재 비트위치(10진수로 1, 2, 4, 8)로 값이 나온다.
    /// </summary>
    public bool IsMask(Item item)
    {
        return ((int)item.Type & (int)slotMask) == 0 ? false : true;
    }

    //인벤토리에 새로운 아이템 슬롯 추가
    public void AddItem(Item nItem, int count = 1)
    {
        item = nItem;
        itemCount = count;
        itemImage.sprite = item.Image;
        if (explanText != null)
        {
            explanText.text = item.Explanation;
            nameText.text = item.ItemName;
        }

        if (item.IsEquipment || item.IsAccessory)
        {
            textCount.text = "";
        }
        else
        {
            textCount.text = itemCount.ToString();
        }

        SetColor(1);
    }

    //해당 슬롯의 아이템 개수 업데이트
    public void UpdateSlotCount(int count)
    {
        itemCount += count;
        textCount.text = itemCount.ToString();

        if (itemCount == 0)
        {
            ClearSlot();
        }
    }

    private void SlotCount()
    {
        textCount.text = itemCount.ToString();

        if (itemCount <= 0)
        {
            ClearSlot();
        }
    }

    //해당 슬롯 하나 삭제
    public void ClearSlot()
    {
        item = null;
        itemCount = 0;
        itemImage.sprite = null;
        SetColor(0);

        textCount.text = "";
    }

    /// <summary>
    /// 슬롯 드래그하면 설명창이 나옴
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (item != null && item.Type != ItemType.ArtiFact && eventData.clickCount == 2)
        {
            storageToInventory.Install(this);
        }
        else if (item != null && item.Type == ItemType.ArtiFact && eventData.clickCount == 2)
        {
            if (DayManager.instance.curDay == Day.day)
            {
                storageToInventory.ArtiFactInstall(this);
            }
            else if(DayManager.instance.curDay == Day.night)
            {
                ani.SetTrigger("OnClick");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null && explanToolTip != null)
        {
            explanToolTip.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (explanToolTip != null)
        {
            explanToolTip.SetActive(false);
        }
    }

    public void InstallButton()
    {
        if (item != null)
        {
            if (item.Type == ItemType.HealPotion_Small || item.Type == ItemType.HealPotion_Middle
                || item.Type == ItemType.HealPotion_Big)
                inventory.hpPotionSlot = this;
            else if (item.Type == ItemType.MPPotion_Small || item.Type == ItemType.MPPotion_Middle
                || item.Type == ItemType.MPPotion_Big)
                inventory.mpPotionSlot = this;
            else if (item.Type == ItemType.GoldBox)
            {
                int random = Random.Range(item.MinGold, item.MaxGold);
                int bounusGold = Mathf.RoundToInt(random * GameManager.instance.goldMultiplier);
                GameManager.instance.gold += bounusGold;
                inventory.goldText.text = "Gold : " + GameManager.instance.gold.ToString();
                itemCount--;
                if (itemCount <= 0)
                {
                    ClearSlot();
                }
            }
        }
    }
}
