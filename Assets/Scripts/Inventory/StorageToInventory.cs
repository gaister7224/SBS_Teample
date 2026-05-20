using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 창고에 있는 아이템을 인벤토리로 이동
/// </summary>

public class StorageToInventory : MonoBehaviour
{
    [Header("인벤토리")]
    [SerializeField] private GameObject e_InventorySlotsParent;
    [SerializeField] private GameObject a_InventorySlotsParent;
    [SerializeField] private GameObject p_InventorySlotsParent;
    [SerializeField] private GameObject artFactInventorySlotsParent;
    private InventorySlot[] e_InventorySlots;
    private InventorySlot[] a_InventorySlots;
    private InventorySlot[] p_InventorySlots;
    private InventorySlot[] artFactInventorySlots;

    [SerializeField] private GameObject equipmentSlotsParent;
    private EquipmentItemSlot[] equipmentSlots;

    [SerializeField] private GameObject artiFactSlotsParent;
    private EquipmentItemSlot[] artiFactSlots;

    [SerializeField] private InventoryMain inventory;
    private SkillPlay skillPlay;
    private PlayerProfile playerProfile;

    private void Awake()
    {
        e_InventorySlots = e_InventorySlotsParent.GetComponentsInChildren<InventorySlot>();
        a_InventorySlots = a_InventorySlotsParent.GetComponentsInChildren<InventorySlot>();
        p_InventorySlots = p_InventorySlotsParent.GetComponentsInChildren<InventorySlot>();
        artFactInventorySlots = artFactInventorySlotsParent.GetComponentsInChildren<InventorySlot>();

        equipmentSlots = equipmentSlotsParent.GetComponentsInChildren<EquipmentItemSlot>();
        artiFactSlots = artiFactSlotsParent.GetComponentsInChildren<EquipmentItemSlot>();
    }

    /// <summary>
    /// 상자에 있는 아이템 인벤토리 창으로 일괄수령
    /// </summary>
    public void GetAll()
    {
        InventorySlot[] allSlots = inventory.GetAllItems();
        int eCount = 0;
        int aCount = 0;
        int pCount = 0;
        int artCount = 0;
        for (int i = 0; i < allSlots.Length; i++)
        {
            if (allSlots[i].Item != null)
            {
                //중첩 가능하면
                if (allSlots[i].Item.CanOverlap)
                {
                    if (allSlots[i].Item.Type == ItemType.Gold)
                    {
                        int itemCount = allSlots[i].itemCount;
                        for (int g = 0; g <= itemCount; g++)
                        {
                            int random = Random.Range(allSlots[i].Item.MinGold, allSlots[i].Item.MaxGold);
                            int bounusGold = Mathf.RoundToInt(random * GameManager.instance.goldMultiplier);
                            GameManager.instance.gold += bounusGold;
                            inventory.goldText.text = "Gold : " + GameManager.instance.gold.ToString();
                            allSlots[i].itemCount--;
                            if (allSlots[i].itemCount <= 0)
                            {
                                allSlots[i].ClearSlot();
                                break;
                            }
                        }
                    }
                    else if(allSlots[i].Item.Type == ItemType.ArtiFactSlot)
                    {
                        artiFactSlots[2].installImpossible = false;
                        allSlots[i].ClearSlot();
                    }
                    else
                    {
                        for (int j = 0; j < e_InventorySlots.Length; j++)
                        {
                            if (p_InventorySlots[pCount].Item == null && p_InventorySlots[pCount].IsMask(allSlots[i].Item))
                            {
                                p_InventorySlots[pCount].AddItem(allSlots[i].Item, allSlots[i].itemCount);
                                allSlots[i].ClearSlot();
                                pCount++;
                                break;
                            }
                            else if (p_InventorySlots[pCount].Item != null && p_InventorySlots[pCount].IsMask(allSlots[i].Item))
                            {
                                if (p_InventorySlots[pCount].Item.ItemID == allSlots[i].Item.ItemID)
                                {
                                    p_InventorySlots[pCount].UpdateSlotCount(allSlots[i].itemCount);
                                    allSlots[i].ClearSlot();
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (!allSlots[i].Item.CanOverlap)
                {
                    for (int j = 0; j < e_InventorySlots.Length; j++)
                    {
                        if (e_InventorySlots[eCount].Item == null && e_InventorySlots[eCount].IsMask(allSlots[i].Item))
                        {
                            e_InventorySlots[eCount].AddItem(allSlots[i].Item, 1);
                            allSlots[i].ClearSlot();
                            eCount++;
                            break;
                        }
                        else if (a_InventorySlots[aCount].Item == null && a_InventorySlots[aCount].IsMask(allSlots[i].Item))
                        {
                            a_InventorySlots[aCount].AddItem(allSlots[i].Item, 1);
                            allSlots[i].ClearSlot();
                            aCount++;
                            break;
                        }
                        else if (artFactInventorySlots[artCount].Item == null && artFactInventorySlots[artCount].IsMask(allSlots[i].Item))
                        {
                            artFactInventorySlots[artCount].AddItem(allSlots[i].Item, 1);
                            allSlots[i].ClearSlot();
                            artCount++;
                            break;
                        }

                        if (e_InventorySlots[eCount].Item != null)
                        {
                            eCount++;
                        }
                        if (a_InventorySlots[aCount].Item != null)
                        {
                            aCount++;
                        }
                        if (artFactInventorySlots[artCount].Item != null)
                        {
                            artCount++;
                        }
                    }
                }
                GameManager.instance.level += GameManager.instance.curLevel;
                GameManager.instance.curLevel = 0;
                UIManager.Instance.profileLevelText.text = "LV." + GameManager.instance.level.ToString();
                GameManager.instance.itemGetAll = true;
            }
        }
        
    }

    /// <summary>
    /// 인벤토리에 있는 장비 장비창으로 창작 InventorySlot에서 기능
    /// </summary>
    /// <param name="inventorySlot"></param>
    public void Install(InventorySlot inventorySlot)
    {
        Item newItem = null;
        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            if (inventorySlot.Item != null)
            {
                if (equipmentSlots[i].Item != null) //장비 교체
                {
                    if (equipmentSlots[i].IsMask(inventorySlot.Item))
                    {
                        newItem = equipmentSlots[i].Item;
                        equipmentSlots[i].AddItem(inventorySlot.Item);
                        inventorySlot.ClearSlot();
                        inventorySlot.AddItem(newItem);
                        BuffGet(equipmentSlots[i]);
                        return;
                    }
                }
                else if (equipmentSlots[i].Item == null) //그대로 장비 착용
                {
                    if (equipmentSlots[i].IsMask(inventorySlot.Item))
                    {
                        equipmentSlots[i].AddItem(inventorySlot.Item);
                        inventorySlot.ClearSlot();
                        BuffGet(equipmentSlots[i]);
                        return;
                    }
                }
            }
        }
    }

    public void ArtiFactInstall(InventorySlot inventorySlot)
    {
        for (int i = 0; i < artiFactSlots.Length; i++)
        {
            if (inventorySlot.Item != null)
            {
                if (artiFactSlots[i].Item == null && !artiFactSlots[i].installImpossible) //그대로 장비 착용
                {
                    if (artiFactSlots[i].IsMask(inventorySlot.Item))
                    {
                        artiFactSlots[i].AddItem(inventorySlot.Item);
                        ArtiFactManager.instance.EquipArtifact(inventorySlot.Item.ItemID);
                        inventorySlot.ClearSlot();
                        return;
                    }
                }
                else if(artiFactSlots[i].installImpossible)
                {
                    return;
                }
            }
        }
    }

    public void ReleaseOfArtFact(EquipmentItemSlot slot)
    {
        int Count = 0;
        for (int i = 0; i < artFactInventorySlots.Length; i++)
        {
            if (artFactInventorySlots[Count].Item == null && artFactInventorySlots[Count].IsMask(slot.Item))
            {
                if (GameManager.instance.installImpossibleStart)
                {
                    if (slot.Item.ItemID == 707)
                    {
                        break;
                    }
                }
                else
                {
                    artFactInventorySlots[Count].AddItem(slot.Item, 1);
                    ArtiFactManager.instance.UnequipArtifact(slot.Item.ItemID);
                    slot.ClearSlot();
                    Count++;
                    break;
                }
            }

            if (artFactInventorySlots[Count].Item != null)
            {
                Count++;
            }
        }
    }


    public void ReleaseOfEquipment(EquipmentItemSlot slot)
    {
        int eCount = 0;
        int aCount = 0;
        for (int i = 0; i < e_InventorySlots.Length; i++)
        {
            if (e_InventorySlots[eCount].Item == null && e_InventorySlots[eCount].IsMask(slot.Item))
            {
                e_InventorySlots[eCount].AddItem(slot.Item, 1);
                ReleaseBuff(slot.Item, slot);
                slot.ClearSlot();
                
                eCount++;
                break;
            }
            else if (a_InventorySlots[aCount].Item == null && a_InventorySlots[aCount].IsMask(slot.Item))
            {
                a_InventorySlots[aCount].AddItem(slot.Item, 1);
                ReleaseBuff(slot.Item, slot);
                slot.ClearSlot();
                
                aCount++;
                break;
            }

            if (e_InventorySlots[eCount].Item != null)
            {
                eCount++;
            }
            if (a_InventorySlots[aCount].Item != null)
            {
                aCount++;
            }
        }
    }

    public void BuyProduct(StoreSlot slot)
    {
        if (slot.Item != null)
        {
            InventorySlot[] allitems = inventory.GetAllItems();

            int count = 0;
            for (; count < allitems.Length; ++count)
            {
                //현재 아이템 칸이 null 이면 주울 수 있음
                if (allitems[count].Item == null)
                {
                    allitems[count].AddItem(slot.Item, 1);
                    break;
                }

                //현제 아이템이 null이 아니지만 중첩 가능하면 주울 수 있음
                if (allitems[count].Item.ItemID == slot.Item.ItemID && allitems[count].Item.CanOverlap)
                {
                    allitems[count].UpdateSlotCount(1);
                    break;
                }
            }

            //다 차고 중첩 아니면 못 주움
            if (count == allitems.Length)
            {
                return;
            }

        }
    }

    private void ReleaseBuff(Item item, EquipmentItemSlot slot)
    {
        skillPlay = inventory.player.GetComponent<SkillPlay>();
        playerProfile = inventory.player.GetComponent<PlayerProfile>();

        if (item.IsEquipment)
        {
            GameManager.instance.e_atk -= item.AtkBuff;
            GameManager.instance.e_critical -= item.CriticalBuff;
            GameManager.instance.e_def -= item.DefBuff;
            GameManager.instance.e_hp -= item.HpBuff;

            SetBuff();
        }
        if (item.IsAccessory)
        {
            if (slot.onAtkBuff)
            {
                GameManager.instance.a_atk -= item.AtkBuff;
                slot.onAtkBuff = false;
            }
            if (slot.onHpBuff)
            {
                GameManager.instance.a_hp -= item.HpBuff;
                slot.onHpBuff = false;
            }
            if (slot.onMPBuff)
            {
                playerProfile.SetMaxMp(-GameManager.instance.a_mp);
                GameManager.instance.a_mp -= item.MPBuff;
                slot.onMPBuff = false;
            }
            if (slot.onCriticalBuff)
            {
                GameManager.instance.a_critical -= item.CriticalBuff;
                slot.onCriticalBuff = false;
            }
            if (slot.onSkillCoolTimeBuff)
            {
                GameManager.instance.a_skillCoolTime = 0;
                slot.onSkillCoolTimeBuff = false;
            }

            SetBuff();
        }
    }

    private void BuffGet(EquipmentItemSlot slot)
    {
        skillPlay = inventory.player.GetComponent<SkillPlay>();
        playerProfile = inventory.player.GetComponent<PlayerProfile>();

        if (slot.Item.IsEquipment)
        {
            GameManager.instance.e_atk = slot.Item.AtkBuff;
            GameManager.instance.e_critical = slot.Item.CriticalBuff;
            GameManager.instance.e_def = slot.Item.DefBuff;
            GameManager.instance.e_hp = slot.Item.HpBuff;

            SetBuff();
        }
        if (slot.Item.IsAccessory)
        {
            int random1 = 0;
            int random2 = 0;
            if (slot.Item.Tier == 1)
            {
                random1 = Random.Range(1, 6);
            }
            else if(slot.Item.Tier == 2)
            {
                random1 = Random.Range(1, 6);
                do
                {
                    random2 = Random.Range(1, 6);
                } while (random1 == random2);
                
            }
            if (random1 == 1 || random2 == 1)
            {
                GameManager.instance.a_atk = slot.Item.AtkBuff;
                slot.onAtkBuff = true;
            }
            if (random1 == 2 || random2 == 2)
            {
                GameManager.instance.a_hp = slot.Item.HpBuff;
                slot.onHpBuff = true;
            }
            if (random1 == 3 || random2 == 3)
            {
                GameManager.instance.a_mp = slot.Item.MPBuff;
                playerProfile.SetMaxMp(GameManager.instance.a_mp);
                slot.onMPBuff = true;
            }
            if (random1 == 4 || random2 == 4)
            {
                GameManager.instance.a_critical = slot.Item.CriticalBuff;
                slot.onCriticalBuff = true;
            }
            if (random1 == 5 || random2 == 5)
            {
                GameManager.instance.a_skillCoolTime = slot.Item.SkillCoolTimeBuff;
                slot.onSkillCoolTimeBuff = true;
            }

            SetBuff();
        }
    }

    private void SetBuff()
    {
        playerProfile.SetMaxHp(GameManager.instance.hpPoint, GameManager.instance.a_hp, GameManager.instance.e_hp);
        playerProfile.SetMaxATK(GameManager.instance.atkPoint, GameManager.instance.a_atk, GameManager.instance.e_atk);
        playerProfile.SetMaxDEF(GameManager.instance.defPoint, GameManager.instance.a_def, GameManager.instance.e_def);
        playerProfile.SetCritical(GameManager.instance.criticalPoint, GameManager.instance.a_critical, GameManager.instance.e_critical);
        skillPlay.SetSkillCoolTimeBuff(GameManager.instance.a_skillCoolTime);
    }
}
