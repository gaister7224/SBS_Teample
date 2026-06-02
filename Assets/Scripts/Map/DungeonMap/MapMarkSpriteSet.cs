using UnityEngine;

[CreateAssetMenu(fileName = "MapMarkSpriteSet", menuName = "Map/Map Mark Sprite Set")]
public class MapMarkSpriteSet : ScriptableObject
{
    [SerializeField] Sprite trap;
    [SerializeField] Sprite treasure;
    [SerializeField] Sprite sealedStone;
    [SerializeField] Sprite shop;
    [SerializeField] Sprite bonfire;
    [SerializeField] Sprite buffStatue;
    [SerializeField] Sprite backPortal;
    [SerializeField] Sprite randomPortal;

    public Sprite GetSprite(DungeonMapMarkType markType)
    {
        //return markType switch
        //{
        //    DungeonMapMarkType.Trap => trap,
        //    DungeonMapMarkType.Treasure => treasure,
        //    DungeonMapMarkType.SealedStone => sealedStone,
        //    DungeonMapMarkType.Shop => shop,
        //    DungeonMapMarkType.Bonfire => bonfire,
        //    DungeonMapMarkType.BuffStatue => buffStatue,
        //    DungeonMapMarkType.ReturnPortal => backPortal,
        //    DungeonMapMarkType.RandomPortal => randomPortal != null ? randomPortal : backPortal,
        //    _ => null
        //};

        switch (markType)
        {
            case DungeonMapMarkType.Trap: return trap;
            case DungeonMapMarkType.Treasure: return treasure;
            case DungeonMapMarkType.SealedStone: return sealedStone;
            case DungeonMapMarkType.Shop: return shop;
            case DungeonMapMarkType.Bonfire: return bonfire;
            case DungeonMapMarkType.BuffStatue: return buffStatue;
            case DungeonMapMarkType.ReturnPortal: return backPortal;
            case DungeonMapMarkType.RandomPortal: return randomPortal;
            default: return null;
        }
    }

    public static MapMarkSpriteSet LoadFromResources()
    {
        var asset = Resources.Load<MapMarkSpriteSet>("MapMarkSpriteSet");
        if (asset != null && asset.HasAnySprite())
            return asset;

        MapMarkSpriteCache.EnsureLoaded();

        var set = CreateInstance<MapMarkSpriteSet>();
        set.trap = MapMarkSpriteCache.Get("mark_trap");
        set.treasure = MapMarkSpriteCache.Get("mark_treasure");
        set.sealedStone = MapMarkSpriteCache.Get("mark_sealed_stone");
        set.shop = MapMarkSpriteCache.Get("mark_shop");
        set.bonfire = MapMarkSpriteCache.Get("mark_bonfire");
        set.buffStatue = MapMarkSpriteCache.Get("mark_buff_statue");
        set.backPortal = MapMarkSpriteCache.Get("mark_portal");
        set.randomPortal = MapMarkSpriteCache.Get("mark_portal");
        return set;
    }

    bool HasAnySprite() =>
        trap != null || treasure != null || sealedStone != null || shop != null
        || bonfire != null || buffStatue != null || backPortal != null || randomPortal != null;
}
