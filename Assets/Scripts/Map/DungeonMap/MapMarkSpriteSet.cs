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
        return markType switch
        {
            DungeonMapMarkType.Trap => trap,
            DungeonMapMarkType.Treasure => treasure,
            DungeonMapMarkType.SealedStone => sealedStone,
            DungeonMapMarkType.Shop => shop,
            DungeonMapMarkType.Bonfire => bonfire,
            DungeonMapMarkType.BuffStatue => buffStatue,
            DungeonMapMarkType.BackPortal => backPortal,
            DungeonMapMarkType.RandomPortal => randomPortal != null ? randomPortal : backPortal,
            _ => null
        };
    }

    public static MapMarkSpriteSet LoadFromResources()
    {
        var set = CreateInstance<MapMarkSpriteSet>();
        set.trap = Resources.Load<Sprite>("MapMarks/mark_trap");
        set.treasure = Resources.Load<Sprite>("MapMarks/mark_treasure");
        set.sealedStone = Resources.Load<Sprite>("MapMarks/mark_sealed_stone");
        set.shop = Resources.Load<Sprite>("MapMarks/mark_shop");
        set.bonfire = Resources.Load<Sprite>("MapMarks/mark_bonfire");
        set.buffStatue = Resources.Load<Sprite>("MapMarks/mark_buff_statue");
        set.backPortal = Resources.Load<Sprite>("MapMarks/mark_portal");
        set.randomPortal = Resources.Load<Sprite>("MapMarks/mark_portal");
        return set;
    }
}
