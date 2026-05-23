using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonMapGridBuilder : MonoBehaviour
{
    public bool HasCells => cells.Count > 0;
    [SerializeField] GameObject cellPrefab;
    [SerializeField] MapMarkSpriteSet markSpriteSet;
    [SerializeField] float cellSpacing = 25f;
    [SerializeField] float cellVisualSize = 20f;
    [SerializeField] bool buildOnStart = false;
    [SerializeField] bool showPlayerPin = true;
    [SerializeField] bool allowCellInteraction = true;
    [SerializeField] bool centerOnPlayer = false;

    readonly Dictionary<Vector2Int, DungeonMapCellView> cells = new();

    RectTransform rectTransform;
    bool built;

    void Awake()
    {
        rectTransform = transform as RectTransform;
    }

    void OnEnable() => BindServiceEvents();

    void Start()
    {
        if (buildOnStart)
            BuildGrid();

        BindServiceEvents();
        RefreshAll();
    }

    void OnDestroy() => UnbindServiceEvents();

    public void BindServiceEventsForPanel() => BindServiceEvents();

    void BindServiceEvents()
    {
        if (DungeonMapService.Instance == null)
            return;

        var service = DungeonMapService.Instance;
        service.Current.OnChanged -= RefreshAll;
        service.OnSelectionChanged -= OnSelectionChanged;
        service.OnDungeonLoaded -= RefreshAll;
        service.OnPendingMarkChanged -= OnPendingMarkChanged;

        service.Current.OnChanged += RefreshAll;
        service.OnSelectionChanged += OnSelectionChanged;
        service.OnDungeonLoaded += RefreshAll;
        service.OnPendingMarkChanged += OnPendingMarkChanged;
    }

    void UnbindServiceEvents()
    {
        if (DungeonMapService.Instance == null)
            return;

        var service = DungeonMapService.Instance;
        service.Current.OnChanged -= RefreshAll;
        service.OnSelectionChanged -= OnSelectionChanged;
        service.OnDungeonLoaded -= RefreshAll;
        service.OnPendingMarkChanged -= OnPendingMarkChanged;
    }

    void OnPendingMarkChanged(DungeonMapMarkType? _) => RefreshAll();

    public void ForceRebuild()
    {
        built = false;
        BuildGrid();
    }

    public void SetCellPrefab(GameObject prefab) => cellPrefab = prefab;

    public void SetMarkSpriteSet(MapMarkSpriteSet spriteSet) => markSpriteSet = spriteSet;

    public void ConfigureForCornerMinimap()
    {
        cellSpacing = CornerMinimapSettings.CellSize;
        cellVisualSize = CornerMinimapSettings.CellVisualSize;
        showPlayerPin = CornerMinimapSettings.ShowPlayerPin;
        allowCellInteraction = CornerMinimapSettings.AllowCellInteraction;
        centerOnPlayer = true;
        if (built)
            ApplyCellLayout();
    }

    public void ConfigureForMapBoard(bool allowMarking)
    {
        cellSpacing = MapBoardPanelSettings.CellSize;
        cellVisualSize = MapBoardPanelSettings.CellVisualSize;
        showPlayerPin = MapBoardPanelSettings.ShowPlayerPin;
        allowCellInteraction = allowMarking;
        centerOnPlayer = false;
        if (built)
            ApplyCellLayout();
    }

    public void BuildGrid()
    {
        if (cellPrefab == null)
            cellPrefab = CreateRuntimeCellPrefab();

        var positions = GetStagePositions();
        if (positions.Count == 0)
            return;

        if (built && cells.Count > 0 && SameCellLayout(cells.Keys, positions))
            return;

        ClearGrid();
        built = false;

        foreach (var pos in positions)
        {
            var cellObject = Instantiate(cellPrefab, transform);
            cellObject.name = $"MapCell_{pos.x}_{pos.y}";

            var cellRect = cellObject.GetComponent<RectTransform>();
            if (cellRect != null)
                cellRect.anchoredPosition = new Vector2(pos.x * cellSpacing, pos.y * cellSpacing);

            SanitizeCellHierarchy(cellObject);

            var legacyStage = cellObject.GetComponent<MinimapStage>();
            if (legacyStage != null)
            {
                legacyStage.enabled = false;
                legacyStage.index = pos;
            }

            var cellView = cellObject.GetComponent<DungeonMapCellView>();
            if (cellView == null)
                cellView = cellObject.AddComponent<DungeonMapCellView>();

            cellView.Initialize(pos, markSpriteSet);
            cells[pos] = cellView;
        }

        built = cells.Count > 0;
        ApplyCellLayout();
        RefreshAll();
    }

    static void SanitizeCellHierarchy(GameObject cellObject)
    {
        var rootButton = cellObject.GetComponent<Button>();
        if (rootButton != null)
            UnityEngine.Object.Destroy(rootButton);

        foreach (var childButton in cellObject.GetComponentsInChildren<Button>(true))
        {
            if (childButton.gameObject == cellObject)
                continue;

            childButton.gameObject.SetActive(false);
        }

        foreach (var graphic in cellObject.GetComponentsInChildren<Graphic>(true))
        {
            if (graphic.gameObject == cellObject)
                continue;

            graphic.raycastTarget = false;
        }
    }

    void ApplyCellLayout()
    {
        foreach (var pair in cells)
        {
            var cellRect = pair.Value.transform as RectTransform;
            if (cellRect == null)
                continue;

            cellRect.sizeDelta = new Vector2(cellVisualSize, cellVisualSize);
            cellRect.anchoredPosition = new Vector2(pair.Key.x * cellSpacing, pair.Key.y * cellSpacing);
        }
    }

    HashSet<Vector2Int> GetStagePositions()
    {
        if (StageManager.instance != null)
        {
            StageManager.instance.EnsureStagePositions();
            if (StageManager.instance.StagePositions.Count > 0)
                return new HashSet<Vector2Int>(StageManager.instance.StagePositions);
        }

        var fallback = new HashSet<Vector2Int>();
        var stageCount = StageManager.instance != null ? StageManager.instance.StageCount : 7;
        var countHalf = (stageCount % 2 == 1) ? stageCount / 2 + 1 : stageCount / 2;

        for (int x = -countHalf; x < stageCount - countHalf + 2; x++)
        {
            for (int y = -countHalf; y < stageCount - countHalf + 2; y++)
            {
                if (x >= -1 && x <= 1 && y >= -1 && y <= 1)
                    continue;

                fallback.Add(new Vector2Int(x, y));
            }
        }

        return fallback;
    }

    static bool SameCellLayout(IEnumerable<Vector2Int> current, HashSet<Vector2Int> next)
    {
        var count = 0;
        foreach (var pos in current)
        {
            count++;
            if (!next.Contains(pos))
                return false;
        }

        return count == next.Count;
    }

    void ClearGrid()
    {
        cells.Clear();
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    void OnSelectionChanged(Vector2Int? _) => RefreshAll();

    public void RefreshAll()
    {
        if (DungeonMapService.Instance == null)
            return;

        if (!built)
            BuildGrid();

        if (cells.Count > 0)
            DungeonMapService.Instance.TrySyncPlayerPositionFromWorld(cells.Keys);

        var data = DungeonMapService.Instance.Current;
        var selected = DungeonMapService.Instance.SelectedCell;

        foreach (var pair in cells)
            pair.Value.Refresh(data, selected, markSpriteSet, showPlayerPin, allowCellInteraction);

        if (centerOnPlayer)
            CenterOnPlayer();
    }

    public void CenterOnPlayer()
    {
        if (rectTransform == null || DungeonMapService.Instance == null)
            return;

        if (!DungeonMapService.Instance.Current.PlayerPosition.HasValue)
            return;

        var playerPos = DungeonMapService.Instance.Current.PlayerPosition.Value;
        rectTransform.anchoredPosition = new Vector2(-playerPos.x * cellSpacing, -playerPos.y * cellSpacing);
    }

    public DungeonMapCellView GetCell(Vector2Int index)
    {
        cells.TryGetValue(index, out var cell);
        return cell;
    }

    public DungeonMapCellView PickCellAtScreen(Vector2 screenPosition, Camera eventCamera)
    {
        if (rectTransform == null || cells.Count == 0)
            return null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, screenPosition, eventCamera, out var localPoint))
            return null;

        var halfExtent = Mathf.Max(cellVisualSize * 0.5f, 1f);
        DungeonMapCellView bestCell = null;
        var bestDistance = float.MaxValue;

        foreach (var pair in cells)
        {
            var cellCenter = new Vector2(pair.Key.x * cellSpacing, pair.Key.y * cellSpacing);
            var delta = localPoint - cellCenter;
            if (Mathf.Abs(delta.x) > halfExtent || Mathf.Abs(delta.y) > halfExtent)
                continue;

            var distance = delta.sqrMagnitude;
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestCell = pair.Value;
        }

        return bestCell;
    }

    static GameObject CreateRuntimeCellPrefab()
    {
        var go = new GameObject("MapCellRuntime", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(DungeonMapCellView));
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(20f, 20f);
        var image = go.GetComponent<Image>();
        image.sprite = MapUiSpriteUtil.White;
        image.color = new Color(0.92f, 0.9f, 0.85f, 1f);
        image.raycastTarget = false;
        return go;
    }
}
