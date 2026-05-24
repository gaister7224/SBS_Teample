using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 우측 상단 코너 미니맵은 항상 표시, MapBoard F키로 대형 지도 게시판을 토글합니다.
/// </summary>
public class VillageMinimapUI : MonoBehaviour
{
    public static VillageMinimapUI Instance { get; private set; }

    [SerializeField] private GameObject largeMapPanel;
    [SerializeField] private MapBoardPanelView mapBoardPanel;
    [SerializeField] private GameObject mapStagePrefab;
    [SerializeField] private MapMarkSpriteSet markSpriteSet;

    private void Awake()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainScene"
            && (GameManager.instance == null || GameManager.instance.mapState != MapState.Village))
        {
            enabled = false;
            return;
        }

        if (GameManager.instance == null)
            DemoSceneBootstrap.EnsureGameManager();

        Instance = this;

        if (largeMapPanel != null)
            largeMapPanel.SetActive(false);

        DisableLegacyLargeMapDisplay();
        EnsureMapBoardPanel();
    }

    private void Update()
    {
        if (mapBoardPanel == null || !mapBoardPanel.IsOpen)
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseLargeMap();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void PrepareForVillageMode()
    {
        Instance = this;
        enabled = true;

        ResolveLargeMapPanelReference();
        if (largeMapPanel == null)
            largeMapPanel = CreateLargeMapPanel();

        mapBoardPanel = null;

        if (largeMapPanel != null)
            largeMapPanel.SetActive(false);

        DisableLegacyLargeMapDisplay();
        EnsureMapBoardPanel();
    }

    void ResolveLargeMapPanelReference()
    {
        if (largeMapPanel != null)
            return;

        var miniMapCanvas = GameObject.Find("MiniMapCanvas");
        if (miniMapCanvas != null)
        {
            var found = miniMapCanvas.transform.Find("LargeMapPanel");
            if (found != null)
                largeMapPanel = found.gameObject;
        }

        if (largeMapPanel == null)
            largeMapPanel = GameObject.Find("LargeMapPanel");
    }

    GameObject CreateLargeMapPanel()
    {
        var panelObject = new GameObject("LargeMapPanel", typeof(RectTransform));
        var miniMapCanvas = GameObject.Find("MiniMapCanvas");
        if (miniMapCanvas != null)
            panelObject.transform.SetParent(miniMapCanvas.transform, false);

        return panelObject;
    }

    void DisableLegacyLargeMapDisplay()
    {
        if (largeMapPanel == null)
            return;

        var legacyDisplay = largeMapPanel.transform.Find("LargeMinimapDisplay");
        if (legacyDisplay != null)
            legacyDisplay.gameObject.SetActive(false);
    }

    void EnsureMapBoardPanel()
    {
        ResolveLargeMapPanelReference();
        if (largeMapPanel == null)
            largeMapPanel = CreateLargeMapPanel();

        if (mapBoardPanel != null && mapBoardPanel.panelRoot == largeMapPanel)
        {
            EnsureLargeMapPanelLayout();
            return;
        }

        if (largeMapPanel == null)
            return;

        EnsureLargeMapPanelLayout();

        mapBoardPanel = largeMapPanel.GetComponent<MapBoardPanelView>();

        var content = largeMapPanel.transform.Find("MapContent");
        GameObject toolbar = null;

        if (content == null)
        {
            var contentObject = new GameObject("MapContent", typeof(RectTransform));
            contentObject.transform.SetParent(largeMapPanel.transform, false);
            content = contentObject.transform;
        }

        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(MapBoardPanelSettings.PanelSize, MapBoardPanelSettings.PanelSize);

        var grid = content.GetComponent<DungeonMapGridBuilder>();
        if (grid == null)
            grid = content.gameObject.AddComponent<DungeonMapGridBuilder>();

        grid.ConfigureForMapBoard(allowMarking: false);
        grid.SetCellPrefab(ResolveMapStagePrefab());
        grid.SetMarkSpriteSet(ResolveMarkSpriteSet());

        if (mapBoardPanel == null)
        {
            mapBoardPanel = largeMapPanel.GetComponent<MapBoardPanelView>();
            if (mapBoardPanel == null)
                mapBoardPanel = largeMapPanel.AddComponent<MapBoardPanelView>();
        }

        mapBoardPanel.Configure(largeMapPanel, content.gameObject, null, grid,
            ResolveMapStagePrefab(), ResolveMarkSpriteSet());
    }

    void EnsureLargeMapPanelLayout()
    {
        if (largeMapPanel == null)
            return;

        var panelRect = largeMapPanel.GetComponent<RectTransform>();
        if (panelRect == null)
            return;

        MinimapHudLayout.ApplyFullscreenPanel(panelRect);
    }

    GameObject ResolveMapStagePrefab()
    {
        if (mapStagePrefab != null)
            return mapStagePrefab;
        return MinimapManager.ResolveMapStagePrefab();
    }

    MapMarkSpriteSet ResolveMarkSpriteSet()
    {
        if (markSpriteSet != null)
            return markSpriteSet;
        return MapMarkSpriteSet.LoadFromResources();
    }

    public void ToggleLargeMap()
    {
        PrepareForVillageMode();
        EnsureLargeMapPanelLayout();
        EnsureDungeonMapService();

        if (mapBoardPanel == null)
            return;

        mapBoardPanel.Toggle(readOnly: true);
    }

    void EnsureDungeonMapService()
    {
        if (DungeonMapService.Instance == null)
        {
            var serviceObject = new GameObject("DungeonMapService");
            serviceObject.AddComponent<DungeonMapService>();
        }

        DungeonMapService.Instance.EnsureLoadedForCurrentDungeon();
    }

    public void CloseLargeMap()
    {
        mapBoardPanel?.Close();
    }
}
