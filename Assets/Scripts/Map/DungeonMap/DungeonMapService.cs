using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMapService : MonoBehaviour
{
    public static DungeonMapService Instance { get; private set; }

    [SerializeField] float saveDebounceSeconds = 0.3f;

    public DungeonMapData Current { get; private set; } = new();
    public Vector2Int? SelectedCell { get; private set; }
    public bool IsReadOnly { get; set; }

    public event Action<Vector2Int?> OnSelectionChanged;
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

    public void LoadForCurrentDungeon()
    {
        var dungeonId = GetCurrentDungeonId();
        LoadDungeon(dungeonId);
    }

    public void EnsureLoadedForCurrentDungeon()
    {
        var dungeonId = GetCurrentDungeonId();
        if (loadedDungeonId != dungeonId || Current.DungeonId != dungeonId)
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
        OnSelectionChanged?.Invoke(null);
        OnDungeonLoaded?.Invoke();
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
        if (IsReadOnly || !Current.IsRevealed(cell))
            return false;

        Current.ApplyMark(cell, markType);
        ScheduleSave();
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
