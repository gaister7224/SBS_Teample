using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 그리드 미니맵 셀 프리팹 참조와 MainScene 던전 지도 UI 부트스트랩.
/// </summary>
public class MinimapManager : MonoBehaviour
{
    public static MinimapManager instance;

    const string MapStageResourcePath = "MapStage";

    [SerializeField] GameObject MapStageImage;
    [SerializeField] MapMarkSpriteSet markSpriteSet;

    public GameObject MapStagePrefab => MapStageImage;
    public MapMarkSpriteSet MarkSpriteSet => markSpriteSet;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void BootstrapForMainScene()
    {
        if (SceneManager.GetActiveScene().name != "MainScene")
            return;

        EnsureRuntimeInstance();
        instance?.BootstrapMainScene();
    }

    public static void EnsureRuntimeInstance()
    {
        if (instance != null)
            return;

        var existing = Object.FindAnyObjectByType<MinimapManager>();
        if (existing != null)
            return;

        var managerObject = new GameObject("MinimapManager");
        Object.DontDestroyOnLoad(managerObject);
        managerObject.AddComponent<MinimapManager>();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (MapStageImage == null)
            MapStageImage = Resources.Load<GameObject>(MapStageResourcePath);

        if (markSpriteSet == null)
            markSpriteSet = MapMarkSpriteSet.LoadFromResources();

        if (markSpriteSet == null)
            markSpriteSet = Resources.Load<MapMarkSpriteSet>("MapMarkSpriteSet");

        if (SceneManager.GetActiveScene().name == "MainScene")
            BootstrapMainScene();
    }

    void BootstrapMainScene()
    {
        EnsureDungeonMapService();
        ApplyMainSceneMinimapMode();
        DungeonMapUiInstaller.EnsureMapBoardUi();
    }

    public static void ApplyMainSceneMinimapMode()
    {
        if (SceneManager.GetActiveScene().name != "MainScene")
            return;

        MinimapHudLayout.EnsureMiniMapCanvas();

        if (IsVillageMapState())
        {
            EnableMainSceneVillageMapUi();
            EnsureVillageCornerMinimap();
            CornerMinimapInstaller.RefreshForCurrentMapState();
            return;
        }

        DisableMainSceneVillageMapUi();
        EnsureMainSceneCornerMinimap();
        CornerMinimapInstaller.RefreshForCurrentMapState();
    }

    static bool IsVillageMapState()
    {
        if (GameManager.instance == null)
            return true;

        return GameManager.instance.mapState == MapState.Village;
    }

    public static void EnsureVillageCornerMinimap()
    {
        MinimapHudLayout.EnsureMiniMapCanvas();

        var canvas = GameObject.Find("MiniMapCanvas");
        if (canvas == null)
            return;

        EnsureVillageMinimapCamera();
        EnsureMinimapImageDisplay(canvas.transform);
    }

    static void EnableMainSceneVillageMapUi()
    {
        foreach (var villageUi in Object.FindObjectsByType<VillageMinimapUI>(FindObjectsSortMode.None))
            villageUi.enabled = true;
    }

    static void DisableMainSceneVillageMapUi()
    {
        foreach (var villageUi in Object.FindObjectsByType<VillageMinimapUI>(FindObjectsSortMode.None))
            villageUi.enabled = false;

        var largeMapPanel = GameObject.Find("LargeMapPanel");
        if (largeMapPanel != null)
            Destroy(largeMapPanel);
    }

    static void EnsureVillageMinimapCamera()
    {
        var minimapCamera = GameObject.Find("MinimapCamera");
        if (minimapCamera == null)
            return;

        minimapCamera.SetActive(true);

        if (minimapCamera.GetComponent<VillageMinimapSync>() == null)
            minimapCamera.AddComponent<VillageMinimapSync>();
    }

    static void EnsureMinimapImageDisplay(Transform canvasTransform)
    {
        var existing = canvasTransform.Find("MinimapImage");
        if (existing != null)
        {
            existing.gameObject.SetActive(true);
            MinimapHudLayout.ApplyTopRight(
                existing as RectTransform,
                MinimapHudLayout.VillageMinimapSize,
                MinimapHudLayout.VillageMinimapSize);
            return;
        }

        var minimapCamera = GameObject.Find("MinimapCamera")?.GetComponent<Camera>();
        if (minimapCamera == null || minimapCamera.targetTexture == null)
            return;

        var imageObject = new GameObject("MinimapImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        imageObject.transform.SetParent(canvasTransform, false);

        var rect = imageObject.GetComponent<RectTransform>();
        MinimapHudLayout.ApplyTopRight(rect, MinimapHudLayout.VillageMinimapSize, MinimapHudLayout.VillageMinimapSize);

        var rawImage = imageObject.GetComponent<RawImage>();
        rawImage.texture = minimapCamera.targetTexture;
        rawImage.raycastTarget = false;

        imageObject.transform.SetAsLastSibling();
    }

    static void EnsureMainSceneCornerMinimap()
    {
        var miniMapCanvas = GameObject.Find("MiniMapCanvas");
        if (miniMapCanvas == null || miniMapCanvas.GetComponent<CornerMinimapInstaller>() != null)
            return;

        miniMapCanvas.AddComponent<CornerMinimapInstaller>();
    }

    static void EnsureDungeonMapService()
    {
        if (Object.FindAnyObjectByType<DungeonMapService>() != null)
            return;

        var serviceObject = new GameObject("DungeonMapService");
        serviceObject.AddComponent<DungeonMapService>();
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public static GameObject ResolveMapStagePrefab()
    {
        if (instance != null && instance.MapStagePrefab != null)
            return instance.MapStagePrefab;

        return Resources.Load<GameObject>(MapStageResourcePath);
    }
}
