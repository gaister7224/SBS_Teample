using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// MainScene MiniMapCanvas: 레거시 RenderTexture 미니맵 제거 후 우측 상단 그리드 미니맵을 표시합니다.
/// </summary>
[DefaultExecutionOrder(-10)]
public class CornerMinimapInstaller : MonoBehaviour
{
    const string CornerMinimapName = "DungeonCornerMinimap";
    const string CornerMinimapBackgroundName = "Background";

    [SerializeField] RectTransform legacyMinimapImage;
    [SerializeField] bool replaceLegacyMinimap = true;

    const string MarkingToolbarName = "MarkingToolbar";

    RectTransform cornerRect;
    DungeonMapGridBuilder gridBuilder;

    void Awake()
    {
        MinimapHudLayout.EnsureMiniMapCanvas();
        CacheLegacyLayoutReference();
        EnsureCornerMinimap();
        BringCornerMinimapToFront();

        if (replaceLegacyMinimap)
            RemoveLegacyMinimapObjects();
    }

    void Start()
    {
        StartCoroutine(RefreshGridWhenStageReady());
    }

    IEnumerator RefreshGridWhenStageReady()
    {
        const int maxFrames = 90;

        for (var frame = 0; frame < maxFrames; frame++)
        {
            if (gridBuilder == null)
                EnsureCornerMinimap();

            if (gridBuilder != null && !gridBuilder.HasCells)
            {
                if (StageManager.instance != null)
                {
                    StageManager.instance.EnsureStagePositions();
                    if (StageManager.instance.StagePositions.Count > 0)
                        StageManager.instance.SyncDungeonMapAfterLayout();
                    else
                        gridBuilder.ForceRebuild();
                }
                else
                {
                    gridBuilder.ForceRebuild();
                }
            }

            if (gridBuilder != null && gridBuilder.HasCells)
                yield break;

            yield return null;
        }
    }

    void CacheLegacyLayoutReference()
    {
        if (legacyMinimapImage != null)
            return;

        var legacyTransform = transform.Find("MinimapImage");
        if (legacyTransform != null)
            legacyMinimapImage = legacyTransform as RectTransform;
    }

    void RemoveLegacyMinimapObjects()
    {
        if (ShouldKeepLegacyMinimap())
            return;

        CacheLegacyLayoutReference();

        if (legacyMinimapImage != null)
            Destroy(legacyMinimapImage.gameObject);

        legacyMinimapImage = null;

        var legacyDisplay = transform.Find("LargeMinimapDisplay");
        if (legacyDisplay != null)
            Destroy(legacyDisplay.gameObject);

        var minimapCamera = GameObject.Find("MinimapCamera");
        if (minimapCamera != null)
            minimapCamera.SetActive(false);
    }

    static bool ShouldKeepLegacyMinimap()
    {
        if (GameManager.instance == null)
            return true;

        return GameManager.instance.mapState == MapState.Village;
    }

    void BringCornerMinimapToFront()
    {
        if (cornerRect == null)
            return;

        cornerRect.SetAsLastSibling();
    }

    void EnsureCornerMinimap()
    {
        if (ShouldKeepLegacyMinimap())
            return;

        var existing = transform.Find(CornerMinimapName);
        if (existing != null)
        {
            cornerRect = existing as RectTransform;
            EnsureCornerBackground(existing);
            var gridTransform = existing.Find("Grid");
            gridBuilder = gridTransform != null
                ? gridTransform.GetComponent<DungeonMapGridBuilder>()
                : existing.GetComponent<DungeonMapGridBuilder>();

            var existingCornerView = existing.GetComponent<CornerMinimapView>();
            if (existingCornerView != null && gridBuilder != null)
                existingCornerView.gridBuilder = gridBuilder;

            if (gridTransform is RectTransform existingGridRect)
                ApplyGridLayout(existingGridRect);

            if (gridBuilder != null)
            {
                gridBuilder.SetCellPrefab(ResolveMapStagePrefab());
                gridBuilder.SetMarkSpriteSet(ResolveMarkSpriteSet());
                gridBuilder.ConfigureForCornerMinimap();
                gridBuilder.ForceRebuild();
            }

            RemoveMarkingToolbarIfDisabled(existing);
            if (CornerMinimapSettings.ShowMarkingToolbar)
                EnsureMarkingToolbar(existing, ResolveMarkSpriteSet());

            ApplyCornerLayout();
            ApplyCornerPanelSize();
            BringCornerMinimapToFront();
            return;
        }

        var cornerRoot = new GameObject(CornerMinimapName, typeof(RectTransform));
        cornerRoot.transform.SetParent(transform, false);
        cornerRect = cornerRoot.GetComponent<RectTransform>();

        ApplyCornerLayout();
        ApplyCornerPanelSize();

        EnsureCornerBackground(cornerRoot.transform);
        cornerRoot.AddComponent<RectMask2D>();

        var gridObject = new GameObject("Grid", typeof(RectTransform));
        gridObject.transform.SetParent(cornerRoot.transform, false);
        var gridRect = gridObject.GetComponent<RectTransform>();
        ApplyGridLayout(gridRect);

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
            DungeonMapLayoutResolver.SyncAfterLayoutChange();

        BringCornerMinimapToFront();
    }

    static void EnsureCornerBackground(Transform cornerRoot)
    {
        var background = cornerRoot.Find(CornerMinimapBackgroundName);
        RectTransform backgroundRect;
        Image backgroundImage;

        if (background == null)
        {
            var backgroundObject = new GameObject(
                CornerMinimapBackgroundName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            backgroundObject.transform.SetParent(cornerRoot, false);
            backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundImage = backgroundObject.GetComponent<Image>();
        }
        else
        {
            backgroundRect = background as RectTransform;
            backgroundImage = background.GetComponent<Image>();
            if (backgroundImage == null)
                backgroundImage = background.gameObject.AddComponent<Image>();
        }

        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        backgroundRect.anchoredPosition = Vector2.zero;
        backgroundRect.SetAsFirstSibling();

        backgroundImage.sprite = MapUiSpriteUtil.White;
        backgroundImage.type = Image.Type.Sliced;
        backgroundImage.color = CornerMinimapSettings.BackgroundColor;
        backgroundImage.raycastTarget = false;
    }

    static void ApplyGridLayout(RectTransform gridRect)
    {
        if (gridRect == null)
            return;

        gridRect.anchorMin = new Vector2(0.5f, 1f);
        gridRect.anchorMax = new Vector2(0.5f, 1f);
        gridRect.pivot = new Vector2(0.5f, 1f);
        gridRect.anchoredPosition = new Vector2(0f, -CornerMinimapSettings.BackgroundPadding);
        gridRect.sizeDelta = new Vector2(CornerMinimapSettings.PanelSize, CornerMinimapSettings.PanelSize);
        gridRect.localScale = Vector3.one;
    }

    void ApplyCornerLayout()
    {
        if (cornerRect == null)
            return;

        var height = CornerMinimapSettings.TotalPanelSize;
        if (CornerMinimapSettings.ShowMarkingToolbar)
            height += CornerMinimapSettings.ToolbarHeight;

        MinimapHudLayout.ApplyTopRight(cornerRect, CornerMinimapSettings.TotalPanelSize, height);
    }

    void ApplyCornerPanelSize() => ApplyCornerLayout();

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
        if (SceneManager.GetActiveScene().name != "MainScene")
            return;

        MinimapManager.EnsureRuntimeInstance();

        var canvas = GameObject.Find("MiniMapCanvas");
        if (canvas == null || canvas.GetComponent<CornerMinimapInstaller>() != null)
            return;

        canvas.AddComponent<CornerMinimapInstaller>();
    }

    public static void RefreshForCurrentMapState()
    {
        var installer = Object.FindAnyObjectByType<CornerMinimapInstaller>();
        if (installer == null)
            return;

        if (ShouldKeepLegacyMinimap())
            installer.RestoreLegacyMinimap();
        else
            installer.EnsureGridMinimapForDungeon();
    }

    void RestoreLegacyMinimap()
    {
        var cornerRoot = transform.Find(CornerMinimapName);
        if (cornerRoot != null)
            Destroy(cornerRoot.gameObject);

        cornerRect = null;
        gridBuilder = null;

        CacheLegacyLayoutReference();
        if (legacyMinimapImage != null)
        {
            legacyMinimapImage.gameObject.SetActive(true);
            MinimapHudLayout.ApplyTopRight(
                legacyMinimapImage,
                MinimapHudLayout.VillageMinimapSize,
                MinimapHudLayout.VillageMinimapSize);
        }
    }

    void EnsureGridMinimapForDungeon()
    {
        RemoveLegacyMinimapObjects();
        EnsureCornerMinimap();
        BringCornerMinimapToFront();
    }
}
