#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

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
        Assign(so, "randomPortal", "mark_random_portal");
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(set);
        AssetDatabase.SaveAssets();
        Debug.Log($"MapMarkSpriteSet saved: {AssetPath}");
    }

    static void Assign(SerializedObject so, string property, string fileName)
    {
        var sprite = LoadSprite(fileName);

        if (sprite != null)
        {
            Debug.Log($"<color=green>[성공]</color> 속성 [{property}] 에 스프라이트 [{sprite.name}] 등록 완료!");
        }
        else
        {
            Debug.LogError($"<color=red>[실패]</color> 경로에서 스프라이트를 찾지 못했습니다: {fileName}");
        }

        so.FindProperty(property).objectReferenceValue = sprite;
    }

    static Sprite LoadSprite(string spriteName)
    {
        //var fromResources = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/MapMarks/{fileName}.png");
        //if (fromResources != null)
        //    return fromResources;

        //return AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/UI/MapMarks/{fileName}.png");

        string[] guids = AssetDatabase.FindAssets("MarkingIcon t:Texture2D");

        if (guids.Length == 0) return null;

        // 첫 번째로 찾아낸 MarkingIcon의 정확한 내부 경로를 알아냅니다.
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);

        // 그 경로 안에 들어있는 Multiple 조각 스프라이트들을 전부 긁어옵니다.
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var asset in assets)
        {
            if (asset is Sprite && asset.name == spriteName)
            {
                return (Sprite)asset;
            }
        }
        return null;
    }
}
#endif
