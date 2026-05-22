using UnityEngine;

public enum DungeonMapMarkType
{
    Trap,
    Treasure,
    SealedStone,
    Shop,
    Bonfire,
    BuffStatue,
    BackPortal,
    RandomPortal
}

public static class DungeonMapTypeMapping
{
    public static bool TryFromStageType(StageType stageType, out DungeonMapMarkType markType)
    {
        switch (stageType)
        {
            case StageType.Trap:
                markType = DungeonMapMarkType.Trap;
                return true;
            case StageType.Treasure:
                markType = DungeonMapMarkType.Treasure;
                return true;
            case StageType.SealedStone:
                markType = DungeonMapMarkType.SealedStone;
                return true;
            case StageType.Shop:
                markType = DungeonMapMarkType.Shop;
                return true;
            case StageType.Bonfire:
                markType = DungeonMapMarkType.Bonfire;
                return true;
            case StageType.BuffStatue:
                markType = DungeonMapMarkType.BuffStatue;
                return true;
            case StageType.BackPortal:
                markType = DungeonMapMarkType.BackPortal;
                return true;
            case StageType.RandomPortal:
                markType = DungeonMapMarkType.RandomPortal;
                return true;
            default:
                markType = default;
                return false;
        }
    }

    public static bool IsMarkable(StageType stageType) => TryFromStageType(stageType, out _);
}
