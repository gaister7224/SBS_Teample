using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MainScene 등에서 M키 지도 게시판 UI를 보장합니다.
/// </summary>
public class DungeonMapUiInstaller : MonoBehaviour
{
    public static DungeonMapUiInstaller Instance { get; private set; }

    [SerializeField] GameObject mapStagePrefab;
    [SerializeField] MapMarkSpriteSet markSpriteSet;

    MapBoardPanelView mapBoardPanel;
    DungeonMapUI dungeonMapUi;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoInstallOnMainScene()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.name.Contains("Main"))
            return;

        if (Object.FindAnyObjectByType<DungeonMapUiInstaller>() != null)
            return;

        var installerObject = new GameObject("DungeonMapUiInstaller");
        installerObject.AddComponent<DungeonMapUiInstaller>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureMapBoardUi();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void EnsureMapBoardUi()
    {
        if (Instance != null)
        {
            Instance.EnsureMapBoardUiInternal();
            return;
        }

        var scene = SceneManager.GetActiveScene();
        if (scene.name.Contains("Main"))
        {
            var installerObject = new GameObject("DungeonMapUiInstaller");
            installerObject.AddComponent<DungeonMapUiInstaller>();
        }
    }

    void EnsureMapBoardUiInternal()
    {
        if (dungeonMapUi != null && mapBoardPanel != null)
            return;

        dungeonMapUi = Object.FindAnyObjectByType<DungeonMapUI>();
        mapBoardPanel = ResolveMapBoardPanel();
        if (dungeonMapUi != null && mapBoardPanel != null)
        {
            dungeonMapUi.SetMapBoardPanel(mapBoardPanel);
            return;
        }

        if (markSpriteSet == null)
            markSpriteSet = MapMarkSpriteSet.LoadFromResources();

        var canvas = FindTargetCanvas();
        if (canvas == null)
            return;

        mapBoardPanel = MapBoardPanelFactory.Create(
            canvas.transform,
            mapStagePrefab,
            markSpriteSet,
            includeMarkingToolbar: true,
            panelName: "DungeonMapBoardPanel");

        mapBoardPanel.transform.SetAsLastSibling();

        var mapUiObject = new GameObject("DungeonMapUI");
        mapUiObject.transform.SetParent(canvas.transform, false);
        dungeonMapUi = mapUiObject.AddComponent<DungeonMapUI>();
        dungeonMapUi.SetMapBoardPanel(mapBoardPanel);

        if (DungeonMapService.Instance != null)
            DungeonMapService.Instance.EnsureLoadedForCurrentDungeon();
    }

    static Canvas FindTargetCanvas()
    {
        var miniMapCanvas = GameObject.Find("MiniMapCanvas");
        if (miniMapCanvas != null)
        {
            var canvas = miniMapCanvas.GetComponent<Canvas>();
            if (canvas != null)
                return canvas;

            canvas = miniMapCanvas.GetComponentInParent<Canvas>();
            if (canvas != null)
                return canvas;
        }

        var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Canvas best = null;
        foreach (var canvas in allCanvases)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                continue;

            if (best == null || canvas.sortingOrder >= best.sortingOrder)
                best = canvas;
        }

        return best ?? Object.FindAnyObjectByType<Canvas>();
    }

    static MapBoardPanelView ResolveMapBoardPanel()
    {
        var panels = Object.FindObjectsByType<MapBoardPanelView>(FindObjectsSortMode.None);
        MapBoardPanelView fallback = null;

        foreach (var panel in panels)
        {
            fallback ??= panel;

            if (panel.gameObject.name.Contains("DungeonMapBoard"))
                return panel;

            if (panel.markingToolbar != null
                || panel.transform.Find("MarkingToolbar") != null)
                return panel;
        }

        return fallback;
    }
}
