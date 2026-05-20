using UnityEngine;

/// <summary>
/// 던전 씬 진입 시 코너 미니맵 + M키 지도 게시판 UI를 구성합니다.
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
        if (markSpriteSet == null)
            markSpriteSet = MapMarkSpriteSet.LoadFromResources();

        EnsureMapStagePrefab();
        EnsureService();
        BuildUi();
        DungeonMapUiInstaller.EnsureMapBoardUi();
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

        if (MinimapManager.instance != null)
            mapStagePrefab = MinimapManager.instance.MapStagePrefab;
    }

    void BuildUi()
    {
        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        EnsureCornerMinimapOnCanvas(canvas.transform);

        if (Object.FindAnyObjectByType<MapBoardPanelView>() != null
            && Object.FindAnyObjectByType<DungeonMapUI>() != null)
            return;

        var mapBoard = MapBoardPanelFactory.Create(
            canvas.transform,
            mapStagePrefab,
            markSpriteSet,
            includeMarkingToolbar: true,
            panelName: "DungeonMapBoardPanel");

        var mapUiObject = new GameObject("DungeonMapUI");
        mapUiObject.transform.SetParent(canvas.transform, false);
        var mapUi = mapUiObject.AddComponent<DungeonMapUI>();
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
}
