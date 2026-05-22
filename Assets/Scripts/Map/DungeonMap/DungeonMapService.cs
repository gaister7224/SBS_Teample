using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMapService : MonoBehaviour
{
    public static DungeonMapService Instance { get; private set; }

    [SerializeField] float saveDebounceSeconds = 0.3f;

    public DungeonMapData Current { get; private set; } = new();
    public Vector2Int? SelectedCell { get; private set; }
    public DungeonMapMarkType? PendingMarkType { get; private set; }
    public bool IsReadOnly { get; set; }

    public event Action<Vector2Int?> OnSelectionChanged;
    public event Action<DungeonMapMarkType?> OnPendingMarkChanged;
    public event Action OnDungeonLoaded;

    float saveTimer = -1f;
    int loadedDungeonId = -1;
    bool dirty;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (!dirty || saveTimer < 0f)
            return;

        saveTimer -= Time.unscaledDeltaTime;
        if (saveTimer <= 0f)
            FlushSave();
    }

    void OnApplicationQuit() => FlushSave();

    public int GetCurrentDungeonId()
    {
        if (GameManager.instance != null)
            return Mathf.Max(1, GameManager.instance.curDungeonNumber);

        return 1;
    }

    public void LoadForCurrentDungeon() => EnsureLoaded(GetCurrentDungeonId());

    public void EnsureLoadedForCurrentDungeon() => EnsureLoaded(GetCurrentDungeonId());

    /// <summary>
    /// 같은 던전이 이미 메모리에 있으면 밝힘·마크를 유지합니다. UI 토글용.
    /// </summary>
    public void EnsureLoaded(int dungeonId)
    {
        if (loadedDungeonId == dungeonId && Current.DungeonId == dungeonId)
            return;

        LoadDungeon(dungeonId);
    }

    public void LoadDungeon(int dungeonId)
    {
        FlushSave();

        loadedDungeonId = dungeonId;
        Current.SetDungeonId(dungeonId);
        Current.Clear();

        if (!DungeonMapSaveStore.TryLoad(dungeonId, Current))
            Current.SetDungeonId(dungeonId);

        SelectedCell = null;
        SetPendingMark(null);
        OnSelectionChanged?.Invoke(null);
        OnDungeonLoaded?.Invoke();
    }

    public void SetPendingMark(DungeonMapMarkType? markType)
    {
        if (PendingMarkType == markType)
            return;

        PendingMarkType = markType;
        OnPendingMarkChanged?.Invoke(markType);
        NotifyAllMapGridsRefresh();
    }

    static void NotifyAllMapGridsRefresh()
    {
        foreach (var grid in UnityEngine.Object.FindObjectsByType<DungeonMapGridBuilder>(FindObjectsSortMode.None))
            grid.RefreshAll();
    }

    public void RevealAround(Vector2Int center, int radius, HashSet<Vector2Int> validCells)
    {
        if (validCells == null || validCells.Count == 0)
            return;

        var toReveal = new List<Vector2Int>();
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                var pos = new Vector2Int(center.x + dx, center.y + dy);
                if (validCells.Contains(pos))
                    toReveal.Add(pos);
            }
        }

        Current.RevealMany(toReveal);
        ScheduleSave();
    }

    public void SetPlayerPosition(Vector2Int cell)
    {
        Current.SetPlayerPosition(cell);
        ScheduleSave();
    }

    /// <summary>
    /// StageManager·플레이어 월드 좌표로 그리드 칸을 맞춥니다. 지도 갱신 시 호출.
    /// </summary>
    public bool TrySyncPlayerPositionFromWorld(IReadOnlyCollection<Vector2Int> validCells)
    {
        if (validCells == null || validCells.Count == 0)
            return false;

        if (!TryResolvePlayerGridCell(validCells, out var cell))
            return false;

        SetPlayerPosition(cell);
        return true;
    }

    public static bool TryResolvePlayerGridCell(IReadOnlyCollection<Vector2Int> validCells, out Vector2Int cell)
    {
        cell = default;
        if (validCells == null || validCells.Count == 0)
            return false;

        var stage = StageManager.instance;
        if (stage != null && ContainsCell(validCells, stage.curStagePos))
        {
            cell = stage.curStagePos;
            return true;
        }

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && stage != null)
        {
            var spacing = stage.spacing;
            if (spacing > 0.0001f)
            {
                var fromWorld = new Vector2Int(
                    Mathf.RoundToInt(player.transform.position.x / spacing),
                    Mathf.RoundToInt(player.transform.position.z / spacing));

                if (ContainsCell(validCells, fromWorld))
                {
                    cell = fromWorld;
                    return true;
                }
            }
        }

        if (Instance != null && Instance.Current.PlayerPosition.HasValue)
        {
            var saved = Instance.Current.PlayerPosition.Value;
            if (ContainsCell(validCells, saved))
            {
                cell = saved;
                return true;
            }
        }

        return false;
    }

    static bool ContainsCell(IReadOnlyCollection<Vector2Int> validCells, Vector2Int pos)
    {
        foreach (var c in validCells)
        {
            if (c.x == pos.x && c.y == pos.y)
                return true;
        }

        return false;
    }

    public void SelectCell(Vector2Int cell)
    {
        if (IsReadOnly || !Current.IsRevealed(cell))
            return;

        SelectedCell = SelectedCell.HasValue && SelectedCell.Value == cell ? cell : cell;
        OnSelectionChanged?.Invoke(SelectedCell);
    }

    public bool ApplyMarkToSelected(DungeonMapMarkType markType)
    {
        if (IsReadOnly || !SelectedCell.HasValue)
            return false;

        return ApplyMark(SelectedCell.Value, markType);
    }

    public bool ApplyMark(Vector2Int cell, DungeonMapMarkType markType)
    {
        if (IsReadOnly)
            return false;

        if (Current.TryGetMark(cell, out var existing) && existing == markType)
            return ClearMark(cell);

        Current.ApplyMark(cell, markType);
        NotifyAllMapGridsRefresh();
        return true;
    }

    public bool ClearMark(Vector2Int cell)
    {
        if (IsReadOnly || !Current.TryGetMark(cell, out _))
            return false;

        Current.ClearMark(cell);
        NotifyAllMapGridsRefresh();
        return true;
    }

    public void ScheduleSave()
    {
        dirty = true;
        saveTimer = saveDebounceSeconds;
    }

    public void FlushSave()
    {
        if (!dirty && saveTimer < 0f)
            return;

        dirty = false;
        saveTimer = -1f;

        if (loadedDungeonId < 0)
            loadedDungeonId = Current.DungeonId;

        DungeonMapSaveStore.Save(Current);
    }
}
