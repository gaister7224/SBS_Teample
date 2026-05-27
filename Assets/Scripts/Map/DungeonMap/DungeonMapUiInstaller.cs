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
            EnsureDungeonMapBoardLayout(mapBoardPanel);
            EnsureDungeonMapUiRoot(dungeonMapUi, MinimapHudLayout.FindMainOverlayCanvas());
            return;
        }

        dungeonMapUi = DungeonMapUI.Instance;
        mapBoardPanel = ResolveDungeonMapBoardPanel();
        if (dungeonMapUi != null && mapBoardPanel != null)
        {
            dungeonMapUi.SetMapBoardPanel(mapBoardPanel);
            EnsureDungeonMapBoardLayout(mapBoardPanel);
            EnsureDungeonMapUiRoot(dungeonMapUi, MinimapHudLayout.FindMainOverlayCanvas());
            return;
        }

        if (markSpriteSet == null)
            markSpriteSet = MapMarkSpriteSet.LoadFromResources();

        var canvas = MinimapHudLayout.FindMainOverlayCanvas();
        if (canvas == null)
            return;

        mapBoardPanel = MapBoardPanelFactory.Create(
            canvas.transform,
            mapStagePrefab,
            markSpriteSet,
            includeMarkingToolbar: true,
            panelName: "DungeonMapBoardPanel");

        MinimapHudLayout.ApplyFullscreenPanel(mapBoardPanel.panelRoot.GetComponent<RectTransform>());
        mapBoardPanel.transform.SetAsLastSibling();

        dungeonMapUi = DungeonMapUI.EnsureSingleInstance(null);
        EnsureDungeonMapUiRoot(dungeonMapUi, canvas);
        dungeonMapUi.SetMapBoardPanel(mapBoardPanel);

        if (DungeonMapService.Instance != null)
            DungeonMapService.Instance.EnsureLoadedForCurrentDungeon();
    }

    static void EnsureDungeonMapBoardLayout(MapBoardPanelView panel)
    {
        if (panel?.panelRoot == null)
            return;

        MinimapHudLayout.ApplyFullscreenPanel(panel.panelRoot.GetComponent<RectTransform>());
    }

    static void EnsureDungeonMapUiRoot(DungeonMapUI mapUi, Canvas rootCanvas)
    {
        if (mapUi == null)
            return;

        if (mapUi.transform.parent != null)
            mapUi.transform.SetParent(null, false);
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

            if (!panel.gameObject.name.Contains("DungeonMapBoard"))
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
}
