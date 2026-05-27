using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// MainScene MiniMapCanvas의 RenderTexture 미니맵을 그리드 지도 미니맵으로 교체합니다.
/// </summary>
[DefaultExecutionOrder(-10)]
public class CornerMinimapInstaller : MonoBehaviour
{
    const string CornerMinimapName = "DungeonCornerMinimap";

    [SerializeField] RectTransform legacyMinimapImage;
    [SerializeField] bool replaceLegacyMinimap = true;

    const string MarkingToolbarName = "MarkingToolbar";

    RectTransform cornerRect;
    DungeonMapGridBuilder gridBuilder;

    void Awake()
    {
        if (replaceLegacyMinimap)
            DisableLegacyMinimap();

        EnsureCornerMinimap();
    }

    void DisableLegacyMinimap()
    {
        if (legacyMinimapImage == null)
        {
            var legacyTransform = transform.Find("MinimapImage");
            if (legacyTransform != null)
                legacyMinimapImage = legacyTransform as RectTransform;
        }

        if (legacyMinimapImage != null)
            legacyMinimapImage.gameObject.SetActive(false);

        var minimapCamera = GameObject.Find("MinimapCamera");
        if (minimapCamera != null)
            minimapCamera.SetActive(false);
    }

    void EnsureCornerMinimap()
    {
        var existing = transform.Find(CornerMinimapName);
        if (existing != null)
        {
            cornerRect = existing as RectTransform;
            var gridTransform = existing.Find("Grid");
            gridBuilder = gridTransform != null
                ? gridTransform.GetComponent<DungeonMapGridBuilder>()
                : existing.GetComponent<DungeonMapGridBuilder>();

            var existingCornerView = existing.GetComponent<CornerMinimapView>();
            if (existingCornerView != null && gridBuilder != null)
                existingCornerView.gridBuilder = gridBuilder;

            if (gridBuilder != null)
            {
                gridBuilder.ConfigureForCornerMinimap();
                gridBuilder.ForceRebuild();
            }

            RemoveMarkingToolbarIfDisabled(existing);
            if (CornerMinimapSettings.ShowMarkingToolbar)
                EnsureMarkingToolbar(existing, ResolveMarkSpriteSet());

            ApplyCornerPanelSize();
            return;
        }

        var cornerRoot = new GameObject(CornerMinimapName, typeof(RectTransform));
        cornerRoot.transform.SetParent(transform, false);
        cornerRect = cornerRoot.GetComponent<RectTransform>();

        ApplyLayoutFromLegacyOrDefault();
        ApplyCornerPanelSize();

        cornerRoot.AddComponent<RectMask2D>();

        var gridObject = new GameObject("Grid", typeof(RectTransform));
        gridObject.transform.SetParent(cornerRoot.transform, false);
        var gridRect = gridObject.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0.5f, 1f);
        gridRect.anchorMax = new Vector2(0.5f, 1f);
        gridRect.pivot = new Vector2(0.5f, 1f);
        gridRect.anchoredPosition = Vector2.zero;
        gridRect.sizeDelta = new Vector2(CornerMinimapSettings.PanelSize, CornerMinimapSettings.PanelSize);
        gridRect.localScale = Vector3.one;

        gridBuilder = gridObject.AddComponent<DungeonMapGridBuilder>();
        gridBuilder.ConfigureForCornerMinimap();

        var mapStagePrefab = ResolveMapStagePrefab();
        var markSprites = ResolveMarkSpriteSet();
        gridBuilder.SetCellPrefab(mapStagePrefab);
        gridBuilder.SetMarkSpriteSet(markSprites);

        var cornerView = cornerRoot.AddComponent<CornerMinimapView>();
        cornerView.gridBuilder = gridBuilder;

        RemoveMarkingToolbarIfDisabled(cornerRoot.transform);
        if (CornerMinimapSettings.ShowMarkingToolbar)
            EnsureMarkingToolbar(cornerRoot.transform, markSprites);

        if (DungeonMapService.Instance == null)
        {
            var serviceObject = new GameObject("DungeonMapService");
            serviceObject.AddComponent<DungeonMapService>();
        }

        DungeonMapService.Instance?.EnsureLoadedForCurrentDungeon();

        if (StageManager.instance != null)
            StageManager.instance.SyncDungeonMapAfterLayout();
        else
            gridBuilder.BuildGrid();
    }

    void ApplyLayoutFromLegacyOrDefault()
    {
        if (legacyMinimapImage != null)
        {
            cornerRect.anchorMin = legacyMinimapImage.anchorMin;
            cornerRect.anchorMax = legacyMinimapImage.anchorMax;
            cornerRect.pivot = legacyMinimapImage.pivot;
            cornerRect.anchoredPosition = legacyMinimapImage.anchoredPosition;
            return;
        }

        cornerRect.anchorMin = new Vector2(1f, 1f);
        cornerRect.anchorMax = new Vector2(1f, 1f);
        cornerRect.pivot = new Vector2(1f, 1f);
        cornerRect.anchoredPosition = new Vector2(-20f, -20f);
    }

    void ApplyCornerPanelSize()
    {
        if (cornerRect == null)
            return;

        var height = CornerMinimapSettings.PanelSize;
        if (CornerMinimapSettings.ShowMarkingToolbar)
            height += CornerMinimapSettings.ToolbarHeight;

        cornerRect.sizeDelta = new Vector2(CornerMinimapSettings.PanelSize, height);
    }

    static void RemoveMarkingToolbarIfDisabled(Transform cornerRoot)
    {
        if (CornerMinimapSettings.ShowMarkingToolbar)
            return;

        var existing = cornerRoot.Find(MarkingToolbarName);
        if (existing != null)
            Destroy(existing.gameObject);
    }

    void EnsureMarkingToolbar(Transform cornerRoot, MapMarkSpriteSet markSprites)
    {
        var existing = cornerRoot.Find(MarkingToolbarName);
        if (existing != null)
            return;

        MapBoardPanelFactory.CreateMarkingToolbar(cornerRoot, markSprites, MarkToolbarLayout.CornerMinimap);
    }

    static GameObject ResolveMapStagePrefab()
    {
        // 레거시 MinimapManager 의존 제거: 없으면 GridBuilder가 런타임 셀을 생성합니다.
        return null;
    }

    static MapMarkSpriteSet ResolveMarkSpriteSet()
    {
        return MapMarkSpriteSet.LoadFromResources();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InstallOnMainSceneCanvas()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.name.Contains("Main"))
            return;

        var canvas = GameObject.Find("MiniMapCanvas");
        if (canvas == null || canvas.GetComponent<CornerMinimapInstaller>() != null)
            return;

        canvas.AddComponent<CornerMinimapInstaller>();
    }
}
