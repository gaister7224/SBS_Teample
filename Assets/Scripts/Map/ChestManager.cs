using Unity.AppUI.Redux.DevTools;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChestManager : MonoBehaviour
{
    public ChestData[] chestData;

    private InventoryMain inventory;
    private bool isChestActive;

    [SerializeField] private GameObject[] itemObjects;
    private bool itemSpawn;
    private GameObject fKey;

    [SerializeField] private Animator ani;
    private void Start()
    {
        inventory = UIManager.Instance.inventory;
        fKey = transform.GetChild(0).gameObject;

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
        if(isChestActive && !itemSpawn)
        {
            for(int i = 0; i < 4; i++)
            {
                itemObjects[i] = RollByChance();
                GameObject newItem = Instantiate(itemObjects[i], transform.position, Quaternion.identity, transform);
                Rigidbody rb = newItem.GetComponent<Rigidbody>();

                if(rb != null)
                {
                    Vector2 randomCircle = Random.insideUnitCircle.normalized;

                    float spread = 1.5f;

                    Vector3 jumpDir = new Vector3(randomCircle.x * spread, 1.5f, randomCircle.y * spread).normalized;
                    rb.AddForce(jumpDir * 5f, ForceMode.Impulse);
                }

                //쉴터 대용 임시
                //GameManager.instance.OnShelterEnter?.Invoke();
            }
            ani.SetBool("Open", true);
            itemSpawn = true;
        }
    }

    private GameObject RollByChance()
    {
        float roll = Random.Range(0f, 100f);
        float cumulativeChance = 0f;

        //어떤 등급이 당첨되었는지
        foreach (var pool in chestData[GameManager.instance.spawnedDungeon.data.dungeonNumber].itemPools)
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

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isChestActive = true;
            fKey.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isChestActive = false;
            fKey.SetActive(false);
        }
    }
}
