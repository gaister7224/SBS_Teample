using System.Collections.Generic;
using UnityEngine;

public class DungeonMapGridBuilder : MonoBehaviour
{
    [SerializeField] GameObject cellPrefab;
    [SerializeField] MapMarkSpriteSet markSpriteSet;
    [SerializeField] float cellSize = 25f;
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

    void Start()
    {
        if (buildOnStart)
            BuildGrid();

        if (DungeonMapService.Instance != null)
        {
            DungeonMapService.Instance.Current.OnChanged += RefreshAll;
            DungeonMapService.Instance.OnSelectionChanged += OnSelectionChanged;
            DungeonMapService.Instance.OnDungeonLoaded += RefreshAll;
        }

        RefreshAll();
    }

    void OnDestroy()
    {
        if (DungeonMapService.Instance == null)
            return;

        DungeonMapService.Instance.Current.OnChanged -= RefreshAll;
        DungeonMapService.Instance.OnSelectionChanged -= OnSelectionChanged;
        DungeonMapService.Instance.OnDungeonLoaded -= RefreshAll;
    }

    public void SetCellPrefab(GameObject prefab) => cellPrefab = prefab;

    public void SetMarkSpriteSet(MapMarkSpriteSet spriteSet) => markSpriteSet = spriteSet;

    public void ConfigureForCornerMinimap()
    {
        cellSize = CornerMinimapSettings.CellSize;
        showPlayerPin = CornerMinimapSettings.ShowPlayerPin;
        allowCellInteraction = CornerMinimapSettings.AllowCellInteraction;
        centerOnPlayer = true;
    }

    public void ConfigureForMapBoard(bool allowMarking)
    {
        cellSize = MapBoardPanelSettings.CellSize;
        showPlayerPin = MapBoardPanelSettings.ShowPlayerPin;
        allowCellInteraction = allowMarking;
        centerOnPlayer = false;
    }

    public void BuildGrid()
    {
        if (built)
            return;

        if (cellPrefab == null)
            cellPrefab = CreateRuntimeCellPrefab();

        ClearGrid();

        var positions = GetStagePositions();
        if (positions.Count == 0)
            return;

        foreach (var pos in positions)
        {
            var cellObject = Instantiate(cellPrefab, transform);
            cellObject.name = $"MapCell_{pos.x}_{pos.y}";

            var cellRect = cellObject.GetComponent<RectTransform>();
            if (cellRect != null)
                cellRect.anchoredPosition = new Vector2(pos.x * cellSize, pos.y * cellSize);

            var legacyStage = cellObject.GetComponent<MinimapStage>();
            if (legacyStage != null)
                legacyStage.enabled = false;

            var cellView = cellObject.GetComponent<DungeonMapCellView>();
            if (cellView == null)
                cellView = cellObject.AddComponent<DungeonMapCellView>();

            cellView.Initialize(pos, markSpriteSet);
            cells[pos] = cellView;
        }

        built = true;
        RefreshAll();
    }

    HashSet<Vector2Int> GetStagePositions()
    {
        if (StageManager.instance != null && StageManager.instance.StagePositions.Count > 0)
            return StageManager.instance.StagePositions;

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
        rectTransform.anchoredPosition = new Vector2(-playerPos.x * cellSize, -playerPos.y * cellSize);
    }

    public DungeonMapCellView GetCell(Vector2Int index)
    {
        cells.TryGetValue(index, out var cell);
        return cell;
    }

    static GameObject CreateRuntimeCellPrefab()
    {
        var go = new GameObject("MapCellRuntime", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button), typeof(DungeonMapCellView));
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(20f, 20f);
        return go;
    }
}
