using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MainScene M키 지도 게시판 UI (단일 인스턴스).
/// </summary>
public class DungeonMapUiInstaller : MonoBehaviour
{
    public static DungeonMapUiInstaller Instance { get; private set; }

    [SerializeField] GameObject mapStagePrefab;
    [SerializeField] MapMarkSpriteSet markSpriteSet;

    MapBoardPanelView mapBoardPanel;
    DungeonMapUI dungeonMapUi;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureMapBoardUiInternal();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void EnsureMapBoardUi()
    {
        if (SceneManager.GetActiveScene().name != "MainScene")
            return;

        CleanupDuplicateMapUi();

        if (Instance != null)
        {
            Instance.EnsureMapBoardUiInternal();
            return;
        }

        var installerObject = new GameObject("DungeonMapUiInstaller");
        installerObject.AddComponent<DungeonMapUiInstaller>();
    }

    void EnsureMapBoardUiInternal()
    {
        CleanupDuplicateMapUi();

        if (dungeonMapUi != null && mapBoardPanel != null)
        {
            dungeonMapUi.SetMapBoardPanel(mapBoardPanel);
            return;
        }

        dungeonMapUi = DungeonMapUI.Instance;
        mapBoardPanel = ResolveDungeonMapBoardPanel();
        if (dungeonMapUi != null && mapBoardPanel != null)
        {
            dungeonMapUi.SetMapBoardPanel(mapBoardPanel);
            return;
        }

        if (markSpriteSet == null)
            markSpriteSet = MapMarkSpriteSet.LoadFromResources();

        if (mapStagePrefab == null)
            mapStagePrefab = MinimapManager.ResolveMapStagePrefab();

        var canvas = FindMapBoardCanvas();
        if (canvas == null)
            return;

        mapBoardPanel = MapBoardPanelFactory.Create(
            canvas.transform,
            mapStagePrefab,
            markSpriteSet,
            includeMarkingToolbar: true,
            panelName: "DungeonMapBoardPanel");

        mapBoardPanel.transform.SetAsLastSibling();

        dungeonMapUi = DungeonMapUI.EnsureSingleInstance(canvas.transform);
        dungeonMapUi.SetMapBoardPanel(mapBoardPanel);

        if (DungeonMapService.Instance != null)
            DungeonMapService.Instance.EnsureLoadedForCurrentDungeon();
    }

    static void CleanupDuplicateMapUi()
    {
        MapBoardPanelView keepPanel = null;
        foreach (var panel in Object.FindObjectsByType<MapBoardPanelView>(FindObjectsSortMode.None))
        {
            if (panel == null)
                continue;

            if (panel.gameObject.name.Contains("DungeonMapBoard"))
            {
                keepPanel = panel;
                break;
            }
        }

        foreach (var panel in Object.FindObjectsByType<MapBoardPanelView>(FindObjectsSortMode.None))
        {
            if (panel == null || panel == keepPanel)
                continue;

            var root = panel.panelRoot != null ? panel.panelRoot : panel.gameObject;
            if (root != null)
                Destroy(root);
        }

        DungeonMapUI keepUi = null;
        foreach (var ui in Object.FindObjectsByType<DungeonMapUI>(FindObjectsSortMode.None))
        {
            if (ui == null)
                continue;

            if (keepUi == null)
                keepUi = ui;
            else
                Destroy(ui.gameObject);
        }
    }

    static MapBoardPanelView ResolveDungeonMapBoardPanel()
    {
        foreach (var panel in Object.FindObjectsByType<MapBoardPanelView>(FindObjectsSortMode.None))
        {
            if (panel != null && panel.gameObject.name.Contains("DungeonMapBoard"))
                return panel;
        }

        return null;
    }

    static Canvas FindMapBoardCanvas()
    {
        var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Canvas best = null;

        foreach (var canvas in allCanvases)
        {
            if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                continue;

            if (canvas.gameObject.name.Contains("MiniMap"))
                continue;

            if (best == null || canvas.sortingOrder >= best.sortingOrder)
                best = canvas;
        }

        if (best != null)
            return best;

        foreach (var canvas in allCanvases)
        {
            if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                continue;

            if (best == null || canvas.sortingOrder >= best.sortingOrder)
                best = canvas;
        }

        return best ?? Object.FindAnyObjectByType<Canvas>();
    }
}
