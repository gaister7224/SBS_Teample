using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// VillageDemo 등 단독 실행 씬에서 GameManager·Canvas·던전 입장 UI를 보장합니다.
/// </summary>
[DefaultExecutionOrder(-100)]
public class DemoSceneBootstrap : MonoBehaviour
{
    const string CanvasPrefabPath = "Assets/Prefabs/Player/Canvas.prefab";
    const string InventorySystemPrefabPath = "Assets/Prefabs/Player/UI/InventorySystem.prefab";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureOnSceneLoad()
    {
        if (!IsVillageDemoScene())
            return;

        EnsureGameManager();
        EnsureCanvasAndDungeonUi();
    }

    void Awake()
    {
        if (!IsVillageDemoScene())
            return;

        EnsureGameManager();
        EnsureCanvasAndDungeonUi();
    }

    static bool IsVillageDemoScene()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        return sceneName.Contains("Village") || sceneName.Contains("Vilage");
    }

    public static void EnsureGameManager()
    {
        if (GameManager.instance != null)
        {
            ApplyDemoDungeonEntryDefaults(GameManager.instance);
            return;
        }

        var go = new GameObject("GameManager");
        var gm = go.AddComponent<GameManager>();
        gm.nickName = "Player";
        gm.level = 1;
        gm.curLevel = 1;
        gm.mapState = MapState.Village;
        gm.maxActCount = 10;
        gm.curActCount = 10;
        gm.possibleDungeon = new bool[11];
        ApplyDemoDungeonEntryDefaults(gm);
    }

    static void ApplyDemoDungeonEntryDefaults(GameManager gm)
    {
        gm.dayEnd = true;
        gm.itemGetAll = false;
        gm.mapState = MapState.Village;

        if (gm.possibleDungeon == null || gm.possibleDungeon.Length == 0)
            gm.possibleDungeon = new bool[11];

        for (var i = 0; i < gm.possibleDungeon.Length; i++)
            gm.possibleDungeon[i] = true;
    }

    public static void EnsureCanvasAndDungeonUi()
    {
        if (Object.FindAnyObjectByType<Canvas>() == null
            && Object.FindAnyObjectByType<UIManager>() == null)
        {
            var canvasPrefab = LoadCanvasPrefab();
            if (canvasPrefab == null)
            {
                Debug.LogError(
                    "VillageDemo: Canvas 프리팹을 로드하지 못했습니다. " +
                    "Assets/Prefabs/Player/Canvas.prefab 을 씬에 배치하세요.");
                return;
            }

            Object.Instantiate(canvasPrefab);
        }

        EnsureInventorySystem();
        EnsureUIManager();
        MinimapManager.EnsureVillageCornerMinimap();
        VillageDungeonEntryUi.ApplyVillageExplorationHud();
        RefreshDungeonEntryPortals();
    }

    static void EnsureInventorySystem()
    {
        if (Object.FindAnyObjectByType<InventoryMain>() != null)
            return;

        var prefab = LoadInventorySystemPrefab();
        if (prefab != null)
            Object.Instantiate(prefab);
    }

    static void EnsureUIManager()
    {
        var inventory = Object.FindAnyObjectByType<InventoryMain>();
        if (inventory == null)
            return;

        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.inventory == null)
                UIManager.Instance.inventory = inventory;
            return;
        }

        var uiManagerObject = new GameObject("UIManager");
        var uiManager = uiManagerObject.AddComponent<UIManager>();
        uiManager.inventory = inventory;

        var fadeObject = GameObject.Find("fade");
        if (fadeObject != null)
            uiManager.fade = fadeObject;
    }

    static GameObject LoadCanvasPrefab()
    {
#if UNITY_EDITOR
        var editorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CanvasPrefabPath);
        if (editorPrefab != null)
            return editorPrefab;
#endif

        return Resources.Load<GameObject>("Canvas");
    }

    static GameObject LoadInventorySystemPrefab()
    {
#if UNITY_EDITOR
        var editorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(InventorySystemPrefabPath);
        if (editorPrefab != null)
            return editorPrefab;
#endif

        return null;
    }

    static void RefreshDungeonEntryPortals()
    {
        foreach (var portal in Object.FindObjectsByType<DungeonEntryPortal>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
            portal.EnsureUiResolved();
    }
}
