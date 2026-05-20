using JetBrains.Annotations;
using UnityEngine;

[System.Flags]
public enum ItemType
{
    //NONE Type는 인벤토리에 들어오지 않는다
    NONE = 0, //0
    SKILL = 1 << 0, //1

    //장비 아이템
    Equipment_HEAD = 1 << 1, //2
    Equipment_ARMOR = 1 << 2, //4
    Equipment_BOTTOMS = 1 << 3, //8
    Equipment_SHOES = 1 << 4, //16
    Equipment_WEAPON = 1 << 5,

    //장신구 아이템
    Equipment_RING = 1 << 6,
    Equipment_PENDANT = 1 << 7,
    Equipment_Glove = 1 << 8,
    Equipment_AMULET = 1 << 9,

    //물약 아이템
    HealPotion_Small = 1 << 10,
    HealPotion_Middle = 1 << 11,
    HealPotion_Big = 1 << 12,

    MPPotion_Small = 1 << 13,
    MPPotion_Middle = 1 << 14,
    MPPotion_Big = 1 << 15,

    //골드 아이템
    GoldBox = 1 << 16,
    Gold = 1 << 17,

    //유물
    ArtiFact = 1 << 18,

    //유물 슬롯
    ArtiFactSlot = 1 << 19
}

[CreateAssetMenu(fileName = "Item", menuName = "Add Item/Item")]
public class Item : ScriptableObject
{
    //장비 그룹
    public bool IsEquipment = (ItemType.Equipment_HEAD | ItemType.Equipment_ARMOR | ItemType.Equipment_BOTTOMS
        | ItemType.Equipment_SHOES | ItemType.Equipment_WEAPON) != 0;
    //장신구 그룹
    public bool IsAccessory = (ItemType.Equipment_RING | ItemType.Equipment_PENDANT | ItemType.Equipment_Glove
        | ItemType.Equipment_AMULET) != 0;

    [Header("고유한 아이템의 ID(중복불가)")]
    [SerializeField] private int itemID;
    /// <summary>
    /// 아이템의 고유 번호
    /// </summary>
    /// <value></value>
    public int ItemID
    {
        get
        {
            return itemID;
        }
    }

    [Header("아이템 이름")]
    [SerializeField] private string itemName;

    public string ItemName
    {
        get
        {
            return itemName;
        }
    }

    [Header("아이템의 중첩 가능한가?")]
    [SerializeField] private bool canOverlap;
    /// <summary>
    /// 아이템이 중첩 가능한가?
    /// </summary>
    /// <value></value>
    public bool CanOverlap
    {
        get
        {
            return canOverlap;
        }
    }

    [Header("사용(상호작용)이 가능한 아이템인가?")]
    [SerializeField] private bool isInteractivity;
    /// <summary>
    /// 사용(상호작용)이 가능한 아이템인가?
    /// </summary>
    /// <value></value>
    public bool IsInteractivity
    {
        get
        {
            return isInteractivity;
        }
    }

    [Header("아이템을 사용하면 사라지는가?")]
    [SerializeField] private bool isConsumable;
    /// <summary>
    /// 아이템을 사용하면 한개씩 사라지는가?
    /// </summary>
    /// <value></value>
    public bool IsConsumable
    {
        get
        {
            return isConsumable;
        }
    }

    [Header("아이템의 타입")]
    [SerializeField] private ItemType itemType;
    /// <summary>
    /// 아이템의 유형
    /// </summary>
    /// <value></value>
    public ItemType Type
    {
        get
        {
            return itemType;
        }
    }

    [Header("아이템 티어")]
    [SerializeField] private int tier;

    public int Tier
    {
        get
        {
            return tier;
        }
    }

    [Header("인벤토리에서 보여질 아이템의 이미지")]
    [SerializeField] private Sprite itemImage;
    public Sprite Image
    {
        get
        {
            return itemImage;
        }
    }

    [Header("가격")]
    [SerializeField] private int price;
    public int Price
    {
        get
        {
            return price;
        }
    }

    [Header("쿨타임")]
    [SerializeField] float coolTime;
    public float CoolTime
    {
        get
        {
            return coolTime;
        }
    }

    [Header("아이템 설명"), TextArea]
    [SerializeField] private string explanation;

    public string Explanation
    {
        get
        {
            return explanation;
        }
    }

    [Header("물약 효과")]
    [SerializeField] float buff;

    public float Buff
    {
        get
        {
            return buff;
        }
    }

    [Header("공격력 효과")]
    [SerializeField] float atkBuff;

    public float AtkBuff
    {
        get
        {
            return atkBuff;
        }
    }

    [Header("크리티컬 확률 효과")]
    [SerializeField] float criticalBuff;

    public float CriticalBuff
    {
        get
        {
            return criticalBuff;
        }
    }

    [Header("방어력 효과")]
    [SerializeField] float defBuff;

    public float DefBuff
    {
        get
        {
            return defBuff;
        }
    }

    [Header("hp효과")]
    [SerializeField] float hpBuff;

    public float HpBuff
    {
        get
        {
            return hpBuff;
        }
    }

    [Header("mp효과")]
    [SerializeField] int mpBuff;

    public int MPBuff
    {
        get
        {
            return mpBuff;
        }
    }

    [Header("스킬 쿨타임 감소 효과")]
    [SerializeField] float skillCoolTimeBuff;

    public float SkillCoolTimeBuff
    {
        get
        {
            return skillCoolTimeBuff;
        }
    }

    [Header("랜덤 골드 (최소 ~ 최대)")]
    [SerializeField] private int minGold;
    [SerializeField] private int maxGold;
    public int MinGold
    {
        get
        {
            return minGold;
        }
    }
    public int MaxGold
    {
        get
        {
            return maxGold;
        }
    }
}
