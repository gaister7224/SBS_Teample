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

    public void SetDungeonId(int dungeonId) => DungeonId = dungeonId;

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

        foreach (var cell in Revealed)
            dto.revealed.Add(new DungeonMapCellEntry { x = cell.x, y = cell.y });

        foreach (var pair in Marks)
        {
            dto.marks.Add(new DungeonMapMarkEntry
            {
                x = pair.Key.x,
                y = pair.Key.y,
                markType = pair.Value
            });
        }

        return dto;
    }

    public void LoadFromDto(DungeonMapSaveDto dto)
    {
        if (dto == null)
            return;

        DungeonId = dto.dungeonId;
        Revealed.Clear();
        Marks.Clear();

        if (dto.revealed != null)
        {
            foreach (var cell in dto.revealed)
                Revealed.Add(new Vector2Int(cell.x, cell.y));
        }

        if (dto.marks != null)
        {
            foreach (var mark in dto.marks)
                Marks[new Vector2Int(mark.x, mark.y)] = mark.markType;
        }

        PlayerPosition = dto.hasPlayerPosition
            ? new Vector2Int(dto.playerX, dto.playerY)
            : (Vector2Int?)null;

        NotifyChanged();
    }

    void NotifyChanged() => OnChanged?.Invoke();
}
