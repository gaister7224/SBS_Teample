using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProfilePotion : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemCountText;

    public InventorySlot slot;
    public float coolTime;

    [SerializeField] private ItemType myType;
    private ItemType type;

    private InventoryMain inventory;

    private bool install = false;

    [SerializeField] private bool hpSlot;
    [SerializeField] private bool mpSlot;

    public bool add = false;

    private void Start()
    {

    }

    private void OnEnable()
    {
        inventory = GameObject.Find("InventorySystem").GetComponent<InventoryMain>();
        if (hpSlot)
            slot = inventory.hpPotionSlot;
        if (mpSlot)
            slot = inventory.mpPotionSlot;
        if (slot != null)
        {
            GetPotionItem();
        }

    }

    private bool IsMask(Item item)
    {
        return ((int)item.Type & (int)myType) == 0 ? false : true;
    }

    public void GetPotionItem()
    {
        if (slot.Item != null)
        {
            type = slot.Item.Type;
            if (IsMask(slot.Item))
            {
                itemImage.sprite = slot.Item.Image;
                itemCountText.text = slot.itemCount.ToString();
                coolTime = slot.Item.CoolTime;
                add = true;

                SetColor(1);
            }
        }
    }

    private void SetColor(float _alpha)
    {
        Color color = itemImage.color;
        color.a = _alpha;
        itemImage.color = color;
    }

    public void ClearSlot()
    {
        slot = null;
        itemCountText.text = "";
        itemImage.sprite = null;
        if (hpSlot)
        {
            inventory.hpPotionSlot = null;
        }
        if(mpSlot)
        {
            inventory.mpPotionSlot = null;
        }
        type = ItemType.NONE;
        add = false;
        SetColor(0);
    }
    public void Use()
    {
        slot.itemCount--;
        itemCountText.text = slot.itemCount.ToString();

        if (slot.itemCount <= 0)
        {
            ClearSlot();
        }
    }
}
