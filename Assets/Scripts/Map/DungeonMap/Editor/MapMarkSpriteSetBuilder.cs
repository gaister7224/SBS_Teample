#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class MapMarkSpriteSetBuilder
{
    const string AssetPath = "Assets/Resources/MapMarkSpriteSet.asset";

    [MenuItem("Tools/Map/Build MapMarkSpriteSet Asset")]
    public static void BuildAsset()
    {
        var set = AssetDatabase.LoadAssetAtPath<MapMarkSpriteSet>(AssetPath);
        if (set == null)
        {
            set = ScriptableObject.CreateInstance<MapMarkSpriteSet>();
            AssetDatabase.CreateAsset(set, AssetPath);
        }

        var so = new SerializedObject(set);
        Assign(so, "trap", "mark_trap");
        Assign(so, "treasure", "mark_treasure");
        Assign(so, "sealedStone", "mark_sealed_stone");
        Assign(so, "shop", "mark_shop");
        Assign(so, "bonfire", "mark_bonfire");
        Assign(so, "buffStatue", "mark_buff_statue");
        Assign(so, "backPortal", "mark_portal");
        Assign(so, "randomPortal", "mark_portal");
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(set);
        AssetDatabase.SaveAssets();
        Debug.Log($"MapMarkSpriteSet saved: {AssetPath}");
    }

    static void Assign(SerializedObject so, string property, string fileName)
    {
        var sprite = LoadSprite(fileName);
        so.FindProperty(property).objectReferenceValue = sprite;
    }

    static Sprite LoadSprite(string fileName)
    {
        var fromResources = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/MapMarks/{fileName}.png");
        if (fromResources != null)
            return fromResources;

        return AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/UI/MapMarks/{fileName}.png");
    }
}
#endif
