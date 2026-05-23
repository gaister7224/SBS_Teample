using UnityEngine;

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
        EnsureService();
        DungeonMapService.Instance.IsReadOnly = readOnly;

        var dungeonId = dungeonIdOverride > 0
            ? dungeonIdOverride
            : DungeonMapService.Instance.GetCurrentDungeonId();
        DungeonMapService.Instance.EnsureLoaded(dungeonId);

        if (readOnly)
            DungeonMapService.Instance.SetPendingMark(null);

        if (panelRoot != null)
            panelRoot.SetActive(true);

        EnsureMarkingToolbar(!readOnly);

        EnsureGrid(readOnly);
        if (!readOnly)
            ActiveMarkingPanel = this;

        if (gridBuilder != null)
        {
            gridBuilder.BindServiceEventsForPanel();
            gridBuilder.ForceRebuild();
            built = gridBuilder.HasCells;
            gridBuilder.RefreshAll();
            EnsureMarkingInput();
        }

        isOpen = true;
    }

    public void Close()
    {
        isOpen = false;

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
        if (gridBuilder == null && mapContentRoot != null)
            gridBuilder = mapContentRoot.GetComponent<DungeonMapGridBuilder>();

        if (gridBuilder == null && mapContentRoot != null)
            gridBuilder = mapContentRoot.AddComponent<DungeonMapGridBuilder>();

        if (gridBuilder == null)
            return;

        gridBuilder.ConfigureForMapBoard(allowMarking);
        gridBuilder.SetCellPrefab(null);

        if (markSpriteSet == null)
            markSpriteSet = MapMarkSpriteSet.LoadFromResources();

        gridBuilder.SetMarkSpriteSet(markSpriteSet);
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
