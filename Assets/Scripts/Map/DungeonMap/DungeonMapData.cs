using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DungeonMapMarkEntry
{
    public int x;
    public int y;
    public DungeonMapMarkType markType;
}

[Serializable]
public class DungeonMapCellEntry
{
    public int x;
    public int y;
}

[Serializable]
public class DungeonMapSaveDto
{
    public int dungeonId;
    public List<DungeonMapCellEntry> revealed = new();
    public List<DungeonMapMarkEntry> marks = new();
    public bool hasPlayerPosition;
    public int playerX;
    public int playerY;
}

public class DungeonMapData
{
    public int DungeonId { get; private set; }
    public HashSet<Vector2Int> Revealed { get; } = new();
    public Dictionary<Vector2Int, DungeonMapMarkType> Marks { get; } = new();
    public Vector2Int? PlayerPosition { get; private set; }

    public event Action OnChanged;
    public HashSet<Vector2Int> ValidCells { get; private set; } = new();

    public bool IsVisited { get; set; } = false;

    public void SetValidCells(HashSet<Vector2Int> cells)
    {
        ValidCells = new HashSet<Vector2Int>(cells);
        NotifyChanged();
    }
    public void SetDungeonId(int dungeonId)
    {
        DungeonId = dungeonId;
        IsVisited = true; // ID가 설정되면 방문한 것으로 간주
        NotifyChanged();
    }

    public void Reveal(Vector2Int cell)
    {
        if (Revealed.Add(cell))
            NotifyChanged();
    }

    public void RevealMany(IEnumerable<Vector2Int> cells)
    {
        var changed = false;
        foreach (var cell in cells)
        {
            if (Revealed.Add(cell))
                changed = true;
        }

        if (changed)
            NotifyChanged();
    }

    public bool IsRevealed(Vector2Int cell) => Revealed.Contains(cell);

    public void SetPlayerPosition(Vector2Int cell)
    {
        if (PlayerPosition.HasValue && PlayerPosition.Value == cell)
            return;

        PlayerPosition = cell;
        NotifyChanged();
    }

    public void ApplyMark(Vector2Int cell, DungeonMapMarkType markType)
    {
        Marks[cell] = markType;
        NotifyChanged();
    }

    public void ClearMark(Vector2Int cell)
    {
        if (Marks.Remove(cell))
            NotifyChanged();
    }

    public bool TryGetMark(Vector2Int cell, out DungeonMapMarkType markType) => Marks.TryGetValue(cell, out markType);

    public void Clear()
    {
        Revealed.Clear();
        Marks.Clear();
        PlayerPosition = null;
        IsVisited = false;
        NotifyChanged();
    }

    public DungeonMapSaveDto ToDto()
    {
        var dto = new DungeonMapSaveDto
        {
            dungeonId = DungeonId,
            hasPlayerPosition = PlayerPosition.HasValue
        };

        if (PlayerPosition.HasValue)
        {
            dto.playerX = PlayerPosition.Value.x;
            dto.playerY = PlayerPosition.Value.y;
        }

        // Revealed·마크는 이번 플레이 세션만 (PlayerPrefs에 저장하지 않음).

        return dto;
    }

    public void LoadFromDto(DungeonMapSaveDto dto)
    {
        if (dto == null)
            return;

        DungeonId = dto.dungeonId;
        Revealed.Clear();
        Marks.Clear();

        // Revealed·마크는 복원하지 않음 (이전 저장의 marks 필드는 무시).

        PlayerPosition = dto.hasPlayerPosition
            ? new Vector2Int(dto.playerX, dto.playerY)
            : (Vector2Int?)null;

        NotifyChanged();
    }

    void NotifyChanged() => OnChanged?.Invoke();
}
