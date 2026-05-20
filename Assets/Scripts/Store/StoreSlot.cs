using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 슬롯 하나를 담당
/// </summary>

public class StoreSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
    public int canBuyCount; //구매 가능 횟수
    public int maxCanBuyCount; //구매 가능 횟수
    public int slotNumber; //슬롯 번호
    public bool resetPossible; //다시 구매 가능한지

    [SerializeField] private bool thisVillageStore; //이 슬롯이 마을 상점 슬롯인지 확인

    [Header("아이템 슬롯에 있는 UI 오브젝트")]
    [SerializeField] private Image itemImage;
    [SerializeField] private GameObject explanToolTip;
    [SerializeField] private TextMeshProUGUI textCount;
    [SerializeField] private TextMeshProUGUI explanText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Button buyButton;

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

    public void SetUpSlot(Item r_item, int count)
    {
        AddItem(r_item, count);
        canBuyCount = maxCanBuyCount;
        buyButton.interactable = true;
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
    public void AddItem(Item nItem, int count = 0)
    {
        item = nItem;
        itemCount = count;
        itemImage.sprite = item.Image;
        if (item.Type == ItemType.GoldBox)
        {
            priceText.text = item.Price.ToString();
        }
        else
        {
            int randomPrice = Random.Range(item.MinGold, item.MaxGold + 1);
            priceText.text = randomPrice.ToString();
        }

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

    public void BuyClick()
    {
        if (item != null && GameManager.instance.gold >= item.Price)
        {
            storageToInventory.BuyProduct(this);
            int newGold = GameManager.instance.gold -= item.Price;
            goldText.text = newGold.ToString();
            if (!thisVillageStore)
                buyButton.interactable = false;
            else
            {
                canBuyCount--;
                GameManager.instance.canBuyCount[slotNumber] = canBuyCount;
                if (canBuyCount <= 0)
                {
                    buyButton.interactable = false;
                }
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
}
