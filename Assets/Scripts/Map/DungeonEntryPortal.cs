using UnityEngine;

public class DungeonEntryPortal : MonoBehaviour
{
    [SerializeField] GameObject ui;

    void Awake() => EnsureUiResolved();

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (DayManager.instance != null && DayManager.instance.curDay != Day.day)
            return;

        if (!EnsureUiResolved())
        {
            Debug.LogWarning(
                "DungeonEntryPortal: DungeonPortal UI를 찾을 수 없습니다. Canvas가 씬에 있는지 확인하세요.",
                this);
            return;
        }

        if (!VillageDungeonEntryUi.ShowDungeonEntryMenu())
            return;

        Time.timeScale = 0f;

        if (TryGetInventory(out var inventory))
        {
            inventory.currentUI = UIType.DungeonEntry;

            if (inventory.playerAttack != null)
                inventory.playerAttack.uiClicking = true;
        }
    }

    public void Back()
    {
        Time.timeScale = 1f;
        VillageDungeonEntryUi.HideDungeonEntryMenu();

        if (TryGetInventory(out var inventory))
        {
            inventory.currentUI = UIType.None;

            if (inventory.playerAttack != null)
                inventory.playerAttack.uiClicking = false;
        }
    }

    public bool EnsureUiResolved()
    {
        if (ui != null)
            return true;

        ui = FindDungeonPortalPanel();
        return ui != null;
    }

    static GameObject FindDungeonPortalPanel()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (transform.name == "DungeonPortal")
                    return transform.gameObject;
            }
        }

        return null;
    }

    static bool TryGetInventory(out InventoryMain inventory)
    {
        inventory = null;
        if (UIManager.Instance == null || UIManager.Instance.inventory == null)
            return false;

        inventory = UIManager.Instance.inventory;
        return true;
    }
}
