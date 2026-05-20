using UnityEngine;
using UnityEngine.InputSystem;

public class BuffStoneManager : MonoBehaviour
{
    private InventoryMain inventory;
    private bool isbuffActive;

    [SerializeField] private bool getBuff;
    private GameObject player;
    private GameObject fKey;
    private void Start()
    {
        inventory = UIManager.Instance.inventory;
        fKey = transform.GetChild(1).gameObject;

        if (inventory == null)
        {
            GameObject obj = GameObject.Find("InventorySystem");
            if (obj != null) inventory = obj.GetComponent<InventoryMain>();
        }

        if (inventory != null && inventory.uiInputAction != null)
        {
            inventory.uiActionMap = inventory.uiInputAction.FindActionMap("Option");
            inventory.uiActionMap.Enable();
            inventory.uiActionMap.FindAction("OpenStorage").performed += OnOpenStorage;
        }
    }

    private void OnOpenStorage(InputAction.CallbackContext context)
    {
        if (isbuffActive && !getBuff)
        {
            PlayerProfile playerProfile = player.GetComponent<PlayerProfile>();
            playerProfile.BuffStoneRelease();
            playerProfile.GetBuffStone();
            getBuff = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            fKey.SetActive(true);
            isbuffActive = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isbuffActive = false;
            fKey.SetActive(false);
        }
    }
}
