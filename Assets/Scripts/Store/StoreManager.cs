using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    public StoreData storeData;
    public List<StoreSlot> storeSlots;
    [SerializeField] private List<StoreSlot> villageStoreSlots;
    [SerializeField] private Item[] villageStoreItems;

    [SerializeField] private TextMeshProUGUI storeGoldText;
    [SerializeField] private TextMeshProUGUI villagestoreGoldText;

    private List<Item> currentPickedItems = new List<Item>(); //현재 상점에 진열된 아이템을 기억하는 리스트

    public static StoreManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        VillageStoreSlot();
    }

    private void Update()
    {
        storeGoldText.text = GameManager.instance.gold.ToString();
        villagestoreGoldText.text = GameManager.instance.gold.ToString();
    }

    private void VillageStoreSlot()
    {
        int count = 0;
        foreach (var slot in villageStoreSlots)
        {
            slot.SetUpSlot(villageStoreItems[count], 1);
            slot.slotNumber = count;
            GameManager.instance.canBuyCount.Add(slot.canBuyCount);
            count++;
        }
    }

    [ContextMenu("Refresh VillageStore")]
    public void VillageStoreReset()
    {
        GameManager.instance.canBuyCount.Clear();
        int count = 0;
        foreach (var slot in villageStoreSlots)
        {
            if (slot.resetPossible)
            {
                slot.SetUpSlot(villageStoreItems[count], 1);
                slot.slotNumber = count;
                GameManager.instance.canBuyCount.Add(slot.maxCanBuyCount);
                count++;
            }
        }
    }

    [ContextMenu("Refresh Store")]
    public void ReFreshShop()
    {
        currentPickedItems.Clear();

        foreach (var slot in storeSlots)
        {
            Item selectedItem = GetRandomItem();

            if (selectedItem != null)
            {
                currentPickedItems.Add(selectedItem);
                slot.SetUpSlot(selectedItem, 1);
            }
        }

        Debug.Log("상점 아이템이 섞였습니다!");
    }

    private Item GetRandomItem()
    {
        int safetyNet = 0; //무한 루프 방지

        while (safetyNet < 50)
        {
            Item candidate = RollByChance();

            if (candidate != null && !currentPickedItems.Contains(candidate)) //안겹치게 함
            {
                return candidate;
            }
            safetyNet++;
        }

        return null;
    }

    private Item RollByChance()
    {
        float roll = Random.Range(0f, 100f);
        float cumulativeChance = 0f;

        //어떤 등급이 당첨되었는지
        foreach (var pool in storeData.itemPools)
        {
            cumulativeChance += pool.dropChance;
            if (roll <= cumulativeChance)
            {
                //당첨된 등급의 아이템 리스트 중 하나를 무작위로 반환
                if (pool.items.Count > 0)
                {
                    int randomIndex = Random.Range(0, pool.items.Count);
                    return pool.items[randomIndex];
                }
            }
        }
        return null;
    }
}
