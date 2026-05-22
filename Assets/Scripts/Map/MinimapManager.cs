using UnityEngine;

/// <summary>
/// 레거시 미니맵 매니저. 그리드 셀 프리팹 참조와 던전 지도 부트스트랩을 담당합니다.
/// </summary>
public class MinimapManager : MonoBehaviour
{
    public static MinimapManager instance;

    [SerializeField] GameObject MapStageImage;
    [SerializeField] MapMarkSpriteSet markSpriteSet;

    public GameObject MapStagePrefab => MapStageImage;
    public MapMarkSpriteSet MarkSpriteSet => markSpriteSet;

    void Awake()
    {
        instance = this;

        if (markSpriteSet == null)
            markSpriteSet = MapMarkSpriteSet.LoadFromResources();

        if (markSpriteSet == null)
            markSpriteSet = Resources.Load<MapMarkSpriteSet>("MapMarkSpriteSet");

        EnsureDungeonMapSystems();
        EnsureMainSceneCornerMinimap();
    }

    void EnsureMainSceneCornerMinimap()
    {
        var miniMapCanvas = GameObject.Find("MiniMapCanvas");
        if (miniMapCanvas == null || miniMapCanvas.GetComponent<CornerMinimapInstaller>() != null)
            return;

        miniMapCanvas.AddComponent<CornerMinimapInstaller>();
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    void EnsureDungeonMapSystems()
    {
        if (Object.FindAnyObjectByType<DungeonMapService>() == null)
        {
            var serviceObject = new GameObject("DungeonMapService");
            serviceObject.AddComponent<DungeonMapService>();
        }

        if (Object.FindAnyObjectByType<DungeonMapBootstrap>() == null)
        {
            var bootstrapObject = new GameObject("DungeonMapBootstrap");
            var bootstrap = bootstrapObject.AddComponent<DungeonMapBootstrap>();
            bootstrap.Initialize(MapStageImage, markSpriteSet);
        }

        DungeonMapUiInstaller.EnsureMapBoardUi();
    }
}
