using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MainScene에서 StageManager 자식·마을 입구 스폰 던전 등 활성 맵 루트를 찾아 미니맵 그리드를 맞춥니다.
/// </summary>
public static class DungeonMapLayoutResolver
{
    const float DefaultSpacing = 40f;

    public static Transform ResolveActiveMapRoot()
    {
        var spawned = GameManager.instance?.spawnedDungeon?.spawnedDungeonInstance;
        if (spawned != null && spawned.activeInHierarchy)
            return spawned.transform;

        if (StageManager.instance == null)
            return null;

        Transform activeRoot = null;
        foreach (Transform child in StageManager.instance.transform)
        {
            if (!child.name.Contains("TutorialStages"))
                continue;

            if (!child.gameObject.activeInHierarchy)
                continue;

            activeRoot = child;
        }

        return activeRoot;
    }

    public static float ResolveSpacing()
    {
        if (StageManager.instance != null && StageManager.instance.spacing > 0.0001f)
            return StageManager.instance.spacing;

        return DefaultSpacing;
    }

    public static HashSet<Vector2Int> CollectStagePositions()
    {
        var positions = new HashSet<Vector2Int>();
        var spacing = ResolveSpacing();
        if (spacing <= 0.0001f)
            return positions;

        var mapRoot = ResolveActiveMapRoot();
        if (mapRoot != null)
        {
            CollectFromRoot(mapRoot, spacing, positions);
            return positions;
        }

        if (StageManager.instance == null)
            return positions;

        var portals = Object.FindObjectsByType<PortalManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var portal in portals)
        {
            if (!BelongsToManagedDungeon(portal))
                continue;

            TryAddPortalPosition(portal, spacing, positions);
        }

        return positions;
    }

    public static void SyncAfterLayoutChange(bool clearVisibility = false)
    {
        if (clearVisibility && DungeonMapService.Instance != null)
        {
            var dungeonId = DungeonMapService.Instance.GetCurrentDungeonId();
            DungeonMapService.Instance.Current.Clear();
            DungeonMapService.Instance.Current.SetDungeonId(dungeonId);
        }

        if (StageManager.instance != null)
            StageManager.instance.RebuildStagePositions();

        foreach (var grid in Object.FindObjectsByType<DungeonMapGridBuilder>(FindObjectsSortMode.None))
            grid.ForceRebuild();

        SyncPlayerPosition();
    }

    static void CollectFromRoot(Transform mapRoot, float spacing, HashSet<Vector2Int> positions)
    {
        foreach (var portal in mapRoot.GetComponentsInChildren<PortalManager>(true))
            TryAddPortalPosition(portal, spacing, positions);
    }

    static void TryAddPortalPosition(PortalManager portal, float spacing, HashSet<Vector2Int> positions)
    {
        if (portal == null || !portal.isActiveAndEnabled)
            return;

        var root = portal.ThisStage != null ? portal.ThisStage.transform : portal.transform;
        positions.Add(WorldToGrid(root.position, spacing));
    }

    static Vector2Int WorldToGrid(Vector3 worldPosition, float spacing)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / spacing),
            Mathf.RoundToInt(worldPosition.z / spacing));
    }

    static bool BelongsToManagedDungeon(PortalManager portal)
    {
        if (StageManager.instance == null)
            return false;

        return portal.transform.IsChildOf(StageManager.instance.transform);
    }

    static void SyncPlayerPosition()
    {
        var positions = CollectStagePositions();
        if (positions.Count == 0)
            return;

        if (DungeonMapService.Instance == null)
        {
            var serviceObject = new GameObject("DungeonMapService");
            serviceObject.AddComponent<DungeonMapService>();
        }

        DungeonMapService.Instance.EnsureLoadedForCurrentDungeon();

        if (StageManager.instance != null)
        {
            StageManager.instance.SyncPlayerToMinimap();
            return;
        }

        if (!DungeonMapService.TryResolvePlayerGridCell(positions, out var cell))
            return;

        DungeonMapService.Instance.SetPlayerPosition(cell);
        DungeonMapService.Instance.RevealAround(cell, 1, positions);
    }
}
