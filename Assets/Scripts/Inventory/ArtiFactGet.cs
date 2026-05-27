using UnityEngine;

public class ArtiFactGet : MonoBehaviour
{
    private InventoryMain inventory;

    [SerializeField] private Item[] artifactItems = new Item[12];
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (UIManager.Instance != null)
            inventory = UIManager.Instance.inventory;

        if (inventory == null)
        {
            GameObject obj = GameObject.Find("InventorySystem");
            if (obj != null) inventory = obj.GetComponent<InventoryMain>();
        }
    }

    [ContextMenu("Refresh ArtiFactRandomGet")]
    public void ArtiFactRandomGet()
    {
        InventorySlot[] allitems = inventory.GetAllItems();
        Item artifact = null;

        int random = Random.Range(0, artifactItems.Length);
        Debug.Log(random);

        for(int i = 0; i  < artifactItems.Length; i++)
        {
            if (!artifactItems[random].ArtifactGet)
            {
                artifact = artifactItems[random];
                int count = 0;
                for (; count < allitems.Length; ++count)
                {
                    if (allitems[count].Item == null) 
                    {
                        inventory.AcquireItem(artifact);
                        break; 
                    }
                }
                artifactItems[random].ArtifactGet = true;
                break;
            }
            else
            {
                random = Random.Range(0, artifactItems.Length);
                Debug.Log(random);
            }
        }
    }
}
