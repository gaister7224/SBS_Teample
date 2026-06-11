using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DungeonCacheEntry
{
    public int dungeonId;
    public DungeonMapData data;
}

public class DungeonMapService : MonoBehaviour
{
    public static DungeonMapService Instance { get; private set; }

    [SerializeField] float saveDebounceSeconds = 0.3f;

    public DungeonMapData Current { get; set; } = new();
    public Vector2Int? SelectedCell { get; private set; }
    public DungeonMapMarkType? PendingMarkType { get; private set; }
    public bool IsReadOnly { get; set; }

    public event Action<Vector2Int?> OnSelectionChanged;
    public event Action<DungeonMapMarkType?> OnPendingMarkChanged;
    public event Action OnDungeonLoaded;

    float saveTimer = -1f;
    public int LoadedDungeonId = -1;
    bool dirty;

    public HashSet<Vector2Int> StagePositions { get; set; }

    // 2. 인스펙터에서 볼 수 있는 List
    [SerializeField] private List<DungeonCacheEntry> dungeonList = new();
    // 3. 실제 로직용 Dictionary
    private Dictionary<int, DungeonMapData> _dungeonCache = new();
    private DungeonMapData _activeData;
    public DungeonMapData ActiveData
    {
        get => _activeData;
        set
        {
            _activeData = value;
            if (_activeData != null) OnMapLoaded?.Invoke(_activeData);
        }
    }

    public static event System.Action OnMapDataChanged;
    public static event System.Action<DungeonMapData> OnMapLoaded;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            _dungeonCache = new Dictionary<int, DungeonMapData>();
        }
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

    // 데이터를 넣을 때 List도 같이 업데이트 (인스펙터 확인용)
    private void SyncInspector()
    {
        dungeonList.Clear();
        foreach (var kvp in _dungeonCache)
            dungeonList.Add(new DungeonCacheEntry { dungeonId = kvp.Key, data = kvp.Value });
    }

    public int GetCurrentDungeonId()
    {
        if (GameManager.instance != null)
            return Mathf.Max(1, GameManager.instance.curDungeonNumber);

        return 1;
    }

    public static int GetDungeonMapId(int dungeonNumber, int floor)
    {
        // 일반 던전 공식
        return (dungeonNumber * 100) + floor;
    }

    public void LoadDungeonData(int dungeonId, System.Action onComplete)
    {
        if (Current == null)
        {
            Current = new DungeonMapData();
        }

        Current.SetDungeonId(dungeonId);

        // 3. 데이터 확실하게 초기화
        Current.Revealed.Clear();
        Current.Marks.Clear(); // 맵 전환 시 마크도 같이 지워주는 게 안전합니다.

        // 4. 로드 로직
        Debug.Log($"[DungeonMapService] ID {dungeonId} 로드 완료 - 데이터 교체 성공");

        // 5. 콜백 호출
        onComplete?.Invoke();
    }

    public void LoadForCurrentDungeon() => EnsureLoaded(GetCurrentDungeonId());

    public void EnsureLoadedForCurrentDungeon() => EnsureLoaded(GetCurrentDungeonId());

    /// <summary>
    /// 같은 던전이 이미 메모리에 있으면 밝힘·마크를 유지합니다. UI 토글용.
    /// </summary>
    public void EnsureLoaded(int dungeonId)
    {
        // 1. 만약 보관함에 이 던전 ID의 데이터가 없다면 새로 만들어서 넣어줍니다.
        if (!_dungeonCache.TryGetValue(dungeonId, out var data))
        {
            data = new DungeonMapData();
            data.SetDungeonId(dungeonId);
            _dungeonCache[dungeonId] = data;
            Debug.Log($"[DungeonMapService] 새로운 던전 ID {dungeonId} 캐시 생성.");
        }

        Current = data;
        ActiveData = data;

        // 이후 이벤트를 쏩니다.
        OnMapLoaded?.Invoke(ActiveData);
        Debug.Log($"[DungeonMapService] 메모리 캐시에서 던전 ID {dungeonId} 데이터를 로드했습니다.");
    }

    public void ReloadForBoard(int dungeonId)
    {
        // 1. 기존 캐시가 있다면 제거하여 새로 로드되게 함
        if (_dungeonCache.ContainsKey(dungeonId))
        {
            _dungeonCache.Remove(dungeonId);
        }

        // 2. 새로 생성하거나 파일에서 로드
        var newData = new DungeonMapData();
        newData.SetDungeonId(dungeonId);

        // 파일에 저장된 최신 데이터가 있다면 덮어쓰기
        if (DungeonMapSaveStore.TryLoad(dungeonId, newData))
        {
            Debug.Log($"[DungeonMapService] 파일에서 ID {dungeonId} 데이터를 새로 로드했습니다.");
        }
        else
        {
            Debug.Log($"[DungeonMapService] ID {dungeonId}의 저장된 데이터가 없어 초기화합니다.");
        }

        _dungeonCache[dungeonId] = newData;
        Current = _dungeonCache[dungeonId];
    }
    public DungeonMapData GetDungeonData(int dungeonId)
    {
        // 1. 이미 데이터가 있으면 가져오고
        //if (_dungeonCache.ContainsKey(dungeonId))
        //{
        //    return _dungeonCache[dungeonId];
        //}

        //// 2. 없으면 새로 만들어서 캐시에 저장합니다 (처음 방문한 던전)

        //SyncInspector();
        if (_dungeonCache.TryGetValue(dungeonId, out var data))
        {
            return data;
        }

        var newData = new DungeonMapData();
        newData.SetDungeonId(dungeonId);
        _dungeonCache[dungeonId] = newData;
        return newData;

        // 캐시에 없다면 방문한 적이 없는 던전입니다.
        // 여기서 null을 리턴하거나, 완전히 초기화된 빈 데이터를 리턴하세요.
        //return null;
    }

    //public void NotifyAllMapGridsRefresh()
    //{
    //    // 씬에 있는 모든 빌더를 찾습니다.
    //    var builders = FindObjectsByType<DungeonMapGridBuilder>(FindObjectsSortMode.None);
    //    foreach (var grid in builders)
    //    {
    //        // 맵이 데이터가 있다면 강제로 다시 빌드합니다.
    //        if (ActiveData != null)
    //        {
    //            grid.BuildGrid(ActiveData);
    //        }
    //    }
    //}

    public void InitializeMap(int dungeonId, HashSet<Vector2Int> allCells)
    {
        var data = GetDungeonData(dungeonId);

        data.SetValidCells(allCells);

        this.ActiveData = data;
        Debug.Log($"[Service] {dungeonId} 던전 초기화 완료. 셀 개수: {allCells.Count}");
    }
    // [핵심] 이제 서비스는 '현재' 던전을 이렇게 가져옵니다.
    public DungeonMapData GetCurrent() => _currentData;
    private DungeonMapData _currentData;

    public void LoadDungeon(int dungeonId)
    {
        if (this == null) return;

        var data = GetDungeonData(dungeonId);
        //Debug.Log($"[디버그] 캐시에서 가져온 데이터 ID: {data.DungeonId}, 전체 셀 개수: {data.Revealed?.Count}");
        //if (data == null)
        //{
        //    data = new DungeonMapData();
        //    data.SetDungeonId(dungeonId);
        //    _dungeonCache[dungeonId] = data;
        //}
        this.Current = data;
        this.ActiveData = data;

        LoadedDungeonId = dungeonId;
        Debug.Log($"[Service] {dungeonId} 던전으로 완벽 전환 완료.");
        OnMapLoaded?.Invoke(ActiveData);
    }

    public void SetPendingMark(DungeonMapMarkType? markType)
    {
        if (PendingMarkType == markType)
            return;

        PendingMarkType = markType;
        OnPendingMarkChanged?.Invoke(markType);
        NotifyAllMapGridsRefresh();
    }

    public void NotifyAllMapGridsRefresh()
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
        if (Current.PlayerPosition.HasValue && Current.PlayerPosition.Value == cell)
            return;

        Current.SetPlayerPosition(cell);
        ScheduleSave();
        NotifyAllMapGridsRefresh();
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
        var player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && stage != null && stage.spacing > 0.0001f)
        {
            var fromWorld = stage.WorldToGrid(player.transform.position);
            if (ContainsCell(validCells, fromWorld))
            {
                cell = fromWorld;
                stage.curStagePos = fromWorld;
                return true;
            }
        }

        if (stage != null && ContainsCell(validCells, stage.curStagePos))
        {
            cell = stage.curStagePos;
            return true;
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
        //NotifyAllMapGridsRefresh();
        OnMapDataChanged?.Invoke();
        return true;
    }

    public bool ClearMark(Vector2Int cell)
    {
        if (IsReadOnly || !Current.TryGetMark(cell, out _))
            return false;

        Current.ClearMark(cell);
        //NotifyAllMapGridsRefresh();
        OnMapDataChanged?.Invoke();
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

        if (LoadedDungeonId < 0)
            LoadedDungeonId = Current.DungeonId;

        Debug.Log("FlushSave");
        DungeonMapSaveStore.Save(Current);
    }
}
