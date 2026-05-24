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
        if (mapBoardPanel != null)
            return;

        if (largeMapPanel == null)
            return;

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

    GameObject ResolveMapStagePrefab()
    {
        if (mapStagePrefab != null)
            return mapStagePrefab;
        if (MinimapManager.instance != null)
            return MinimapManager.instance.MapStagePrefab;
        return null;
    }

    MapMarkSpriteSet ResolveMarkSpriteSet()
    {
        if (markSpriteSet != null)
            return markSpriteSet;
        return MapMarkSpriteSet.LoadFromResources();
    }

    public void ToggleLargeMap()
    {
        EnsureMapBoardPanel();
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
