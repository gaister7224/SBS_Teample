using UnityEngine;

public class UIBackButton : MonoBehaviour
{
    private InventoryMain inventory;
    private ItemRaycast itemRaycast;
    private SkillUIManager skillUIManager;

    private void Start()
    {
        inventory = GameObject.Find("InventorySystem").GetComponent<InventoryMain>();
        itemRaycast = GameObject.FindWithTag("Player").GetComponent<ItemRaycast>();
        skillUIManager = GameObject.Find("InventorySystem").GetComponent<SkillUIManager>();
    }

    public void InventoryUIBack()
    {
        inventory.CloseInventory();
    }

    public void StorageUIBack()
    {
        itemRaycast.StorageClose();
    }

    public void StoreUIBack()
    {
        itemRaycast.StoreClose();
    }

    public void VillageStoreUIBack()
    {
        itemRaycast.VillageStoreClose();
    }

    public void InfoUIBack()
    {
        skillUIManager.InfoUIClose();
    }
}
