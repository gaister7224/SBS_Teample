using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 마을 지도 게시판(F) / 던전 지도 제작 UI(M) 공통 대형 지도 패널.
/// </summary>
public class MapBoardPanelView : MonoBehaviour
{
    public static MapBoardPanelView ActiveMarkingPanel { get; private set; }

    [SerializeField] public GameObject panelRoot;
    [SerializeField] public GameObject mapContentRoot;
    [SerializeField] public GameObject markingToolbar;
    [SerializeField] public DungeonMapGridBuilder gridBuilder;
    [SerializeField] public GameObject mapStagePrefab;
    [SerializeField] public MapMarkSpriteSet markSpriteSet;
    [SerializeField] int dungeonIdOverride;

    InventoryMain inventory;

    bool built;
    bool isOpen;

    public bool IsOpen => isOpen;

    public void Configure(GameObject panel, GameObject content, GameObject toolbar,
        DungeonMapGridBuilder grid, GameObject stagePrefab, MapMarkSpriteSet sprites)
    {
        panelRoot = panel;
        mapContentRoot = content;
        markingToolbar = toolbar;
        gridBuilder = grid;
        mapStagePrefab = stagePrefab;
        markSpriteSet = sprites;
    }

    public void Toggle(bool readOnly)
    {
        if (isOpen)
            Close();
        else
            Open(readOnly);
    }

    public void Open(bool readOnly)
    {
        inventory = GameObject.Find("InventorySystem").GetComponent<InventoryMain>();
        inventory.playerProfile.SetActive(false);
        inventory.playerAttack.uiClicking = true;

        EnsureService();
        DungeonMapService.Instance.IsReadOnly = readOnly;

        var dungeonId = dungeonIdOverride > 0
            ? dungeonIdOverride
            : DungeonMapService.Instance.GetCurrentDungeonId();
        DungeonMapService.Instance.EnsureLoaded(dungeonId);

        if (readOnly)
            DungeonMapService.Instance.SetPendingMark(null);

        EnsurePanelLayout();

        if (panelRoot != null)
        {
            MapBoardPanelFactory.ApplyPanelBackground(panelRoot);
            panelRoot.SetActive(true);
            panelRoot.transform.SetAsLastSibling();
        }

        EnsureMarkingToolbar(!readOnly);

        EnsureGrid(readOnly);
        if (!readOnly)
            ActiveMarkingPanel = this;

        if (gridBuilder != null)
        {
            gridBuilder.BindServiceEventsForPanel();
            gridBuilder.ForceRebuild();
            built = gridBuilder.HasCells;
            EnsurePanelLayout();
            gridBuilder.RefreshAll();
            EnsureMarkingInput();
        }

        isOpen = true;
    }

    public void Close()
    {
        isOpen = false;
        inventory = GameObject.Find("InventorySystem").GetComponent<InventoryMain>();
        inventory.playerProfile.SetActive(true);
        inventory.playerAttack.uiClicking = false;

        if (ActiveMarkingPanel == this)
            ActiveMarkingPanel = null;

        DungeonMapService.Instance?.SetPendingMark(null);
        GameplayInputUtility.ReleaseUiFocus();

        if (panelRoot != null)
            panelRoot.SetActive(false);

        EnsureMarkingToolbar(false);

        DungeonMapService.Instance?.FlushSave();
    }

    void EnsureService()
    {
        if (DungeonMapService.Instance != null)
            return;

        var serviceObject = new GameObject("DungeonMapService");
        serviceObject.AddComponent<DungeonMapService>();
    }

    void EnsureGrid(bool allowMarking)
    {
        if (mapContentRoot != null)
        {
            var gridTransform = mapContentRoot.transform.Find("Grid");
            if (gridTransform != null)
                gridBuilder = gridTransform.GetComponent<DungeonMapGridBuilder>();

            // Legacy panels had GridBuilder directly on MapContent; migrate to child Grid so Background survives rebuild.
            var legacyBuilder = mapContentRoot.GetComponent<DungeonMapGridBuilder>();
            if (legacyBuilder != null)
            {
                if (gridBuilder == null)
                {
                    var gridObject = new GameObject("Grid", typeof(RectTransform));
                    gridObject.transform.SetParent(mapContentRoot.transform, false);
                    var gridRect = gridObject.GetComponent<RectTransform>();
                    gridRect.anchorMin = Vector2.zero;
                    gridRect.anchorMax = Vector2.one;
                    gridRect.offsetMin = Vector2.zero;
                    gridRect.offsetMax = Vector2.zero;
                    gridRect.pivot = new Vector2(0.5f, 0.5f);
                    gridRect.anchoredPosition = Vector2.zero;
                    gridObject.AddComponent<RectMask2D>();
                    gridBuilder = gridObject.AddComponent<DungeonMapGridBuilder>();
                }

                Destroy(legacyBuilder);
            }
        }

        if (gridBuilder == null && mapContentRoot != null)
        {
            var gridObject = new GameObject("Grid", typeof(RectTransform));
            gridObject.transform.SetParent(mapContentRoot.transform, false);
            var gridRect = gridObject.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.5f, 0.5f);
            gridRect.anchorMax = new Vector2(0.5f, 0.5f);
            gridRect.pivot = new Vector2(0.5f, 0.5f);
            gridRect.sizeDelta = new Vector2(
                MapBoardPanelSettings.MapAreaWidth,
                MapBoardPanelSettings.PanelSize - MapBoardPanelSettings.MapAreaTopInset - MapBoardPanelSettings.MapAreaBottomInset);
            gridRect.anchoredPosition = new Vector2(
                0f,
                (MapBoardPanelSettings.MapAreaBottomInset - MapBoardPanelSettings.MapAreaTopInset) * 0.5f);
            gridObject.AddComponent<RectMask2D>();
            gridBuilder = gridObject.AddComponent<DungeonMapGridBuilder>();
        }

        if (gridBuilder == null)
            return;

        // 안전장치: MapContent 하위에 GridBuilder가 중복으로 남아있으면(레거시/RequireComponent 영향)
        // 아이콘/그리드가 배경 밖에 같이 그려질 수 있어 모두 정리한다.
        var allBuilders = mapContentRoot.GetComponentsInChildren<DungeonMapGridBuilder>(true);
        foreach (var b in allBuilders)
        {
            if (b != null && b != gridBuilder)
                Destroy(b);
        }

        gridBuilder.ConfigureForMapBoard(allowMarking);

        if (mapStagePrefab == null)
            mapStagePrefab = MinimapManager.ResolveMapStagePrefab();

        gridBuilder.SetCellPrefab(mapStagePrefab);

        if (markSpriteSet == null)
            markSpriteSet = MapMarkSpriteSet.LoadFromResources();

        gridBuilder.SetMarkSpriteSet(markSpriteSet);
    }

    void EnsurePanelLayout()
    {
        if (panelRoot == null)
            return;

        var panelRect = panelRoot.GetComponent<RectTransform>();
        if (panelRect != null)
            MinimapHudLayout.ApplyFullscreenPanel(panelRect);

        MapBoardPanelFactory.ApplyPanelBackground(panelRoot, warnOnFailure: false);

        if (mapContentRoot == null)
            return;

        var contentRect = mapContentRoot.GetComponent<RectTransform>();
        
        if (contentRect != null)
        {
            MinimapHudLayout.ApplyCenteredMapContent(
                contentRect,
                MapBoardPanelSettings.PanelSize);

            if (mapContentRoot.GetComponent<RectMask2D>() == null)
                mapContentRoot.AddComponent<RectMask2D>();

            // ───────────────── [강력한 치트키 코드 추가] ─────────────────
            // 다른 맵으로 이동할 때 불필요하게 부활한 검은 배경(dim)이나 
            // 중복 생성된 배경 오브젝트가 있다면 이름으로 찾아서 강제로 파괴합니다.

            // 1. 만약 panelRoot 자체에 Image 컴포넌트(검은판)가 다시 붙었다면 제거
            var panelImage = mapContentRoot.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(1f, 1f, 1f, 0.004f);
                panelImage.raycastTarget = true;
            }
        }
        float targetX = -contentRect.sizeDelta.x * 0.5f + MapBoardPanelSettings.MapAreaInsetX;
        float targetY = contentRect.sizeDelta.y * 0.5f - MapBoardPanelSettings.MapAreaTopInset;

        if(StageManager.instance.Tutorial)
            contentRect.anchoredPosition = new Vector2(targetX + 180, targetY - 60);
        if (!StageManager.instance.Tutorial)
            contentRect.anchoredPosition = new Vector2(targetX + 393, targetY - 267);

    }

    void EnsureMarkingInput()
    {
        if (mapContentRoot == null || gridBuilder == null)
            return;

        var input = mapContentRoot.GetComponent<MapBoardMarkingInput>();
        if (input == null)
            input = mapContentRoot.AddComponent<MapBoardMarkingInput>();

        input.BindGrid(gridBuilder);
    }

    void EnsureMarkingToolbar(bool visible)
    {
        if (panelRoot == null)
            return;

        if (mapContentRoot != null)
        {
            var misplaced = mapContentRoot.transform.Find("MarkingToolbar");
            if (misplaced != null)
                Destroy(misplaced.gameObject);
        }

        var existingToolbar = panelRoot.transform.Find("MarkingToolbar");
        if (existingToolbar != null)
        {
            if (!visible)
            {
                markingToolbar = existingToolbar.gameObject;
                markingToolbar.SetActive(false);
                return;
            }

            Destroy(existingToolbar.gameObject);
            markingToolbar = null;
        }

        if (visible)
        {
            var sprites = markSpriteSet != null
                ? markSpriteSet
                : MapMarkSpriteSet.LoadFromResources();
            markingToolbar = MapBoardPanelFactory.CreateMarkingToolbar(
                panelRoot.transform, sprites, MarkToolbarLayout.MapBoard);
        }

        if (markingToolbar == null)
            return;

        markingToolbar.transform.SetAsLastSibling();
        markingToolbar.SetActive(visible);

        if (visible)
        {
            var toolbarRect = markingToolbar.GetComponent<RectTransform>();
            if (toolbarRect != null)
            {
                toolbarRect.anchoredPosition += new Vector2(0f, 180f);
            }
        }

        if (visible)
            RefreshToolbarSprites();
    }

    void RefreshToolbarSprites()
    {
        if (markingToolbar == null)
            return;

        var sprites = markSpriteSet != null
            ? markSpriteSet
            : MapMarkSpriteSet.LoadFromResources();

        foreach (var markButton in markingToolbar.GetComponentsInChildren<MapMarkButton>(true))
            markButton.Configure(markButton.MarkType, sprites.GetSprite(markButton.MarkType));
    }
}
