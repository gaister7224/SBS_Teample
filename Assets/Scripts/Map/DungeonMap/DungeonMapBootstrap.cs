using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 던전 전용 씬(DungeonSystem 등) 진입 시 지도 UI를 구성합니다. MainScene은 MinimapManager가 담당합니다.
/// </summary>
public class DungeonMapBootstrap : MonoBehaviour
{
    [SerializeField] GameObject mapStagePrefab;
    [SerializeField] MapMarkSpriteSet markSpriteSet;

    public void Initialize(GameObject stagePrefab, MapMarkSpriteSet sprites)
    {
        if (stagePrefab != null)
            mapStagePrefab = stagePrefab;
        if (sprites != null)
            markSpriteSet = sprites;
    }

    void Awake()
    {
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            Destroy(gameObject);
            return;
        }

        if (markSpriteSet == null)
            markSpriteSet = MapMarkSpriteSet.LoadFromResources();

        EnsureMapStagePrefab();
        EnsureService();
        BuildUi();
        DungeonMapService.Instance.LoadForCurrentDungeon();
    }

    void EnsureService()
    {
        if (DungeonMapService.Instance != null)
            return;

        var serviceObject = new GameObject("DungeonMapService");
        serviceObject.AddComponent<DungeonMapService>();
    }

    void EnsureMapStagePrefab()
    {
        if (mapStagePrefab != null)
            return;

        mapStagePrefab = MinimapManager.ResolveMapStagePrefab();
    }

    void BuildUi()
    {
        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        EnsureCornerMinimapOnCanvas(canvas.transform);

        if (DungeonMapUI.Instance != null && ResolveMapBoardPanel() != null)
            return;

        var mapBoard = MapBoardPanelFactory.Create(
            canvas.transform,
            mapStagePrefab,
            markSpriteSet,
            includeMarkingToolbar: true,
            panelName: "DungeonMapBoardPanel");

        var mapUi = DungeonMapUI.EnsureSingleInstance(canvas.transform);
        mapUi.SetMapBoardPanel(mapBoard);
    }

    void EnsureCornerMinimapOnCanvas(Transform canvasTransform)
    {
        var miniMapCanvas = GameObject.Find("MiniMapCanvas");
        if (miniMapCanvas != null)
        {
            if (miniMapCanvas.GetComponent<CornerMinimapInstaller>() == null)
                miniMapCanvas.AddComponent<CornerMinimapInstaller>();
            return;
        }

        if (Object.FindAnyObjectByType<CornerMinimapInstaller>() != null)
            return;

        var installerObject = new GameObject("CornerMinimapInstaller");
        installerObject.transform.SetParent(canvasTransform, false);
        installerObject.AddComponent<CornerMinimapInstaller>();
    }

    static MapBoardPanelView ResolveMapBoardPanel()
    {
        foreach (var panel in Object.FindObjectsByType<MapBoardPanelView>(FindObjectsSortMode.None))
        {
            if (panel != null && panel.gameObject.name.Contains("DungeonMapBoard"))
                return panel;
        }

        return null;
    }
}
