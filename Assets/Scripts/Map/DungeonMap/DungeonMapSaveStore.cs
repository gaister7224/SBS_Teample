using UnityEngine;

public static class DungeonMapSaveStore
{
    const string KeyPrefix = "DungeonMap_";

    public static void Save(DungeonMapData data)
    {
        if (data == null)
            return;

        var json = JsonUtility.ToJson(data.ToDto());
        PlayerPrefs.SetString(KeyPrefix + data.DungeonId, json);
        PlayerPrefs.Save();
    }

    public static bool TryLoad(int dungeonId, DungeonMapData data)
    {
        if (data == null)
            return false;

        var key = KeyPrefix + dungeonId;
        if (!PlayerPrefs.HasKey(key))
            return false;

        var json = PlayerPrefs.GetString(key);
        if (string.IsNullOrEmpty(json))
            return false;

        var dto = JsonUtility.FromJson<DungeonMapSaveDto>(json);
        if (dto == null)
            return false;

        data.LoadFromDto(dto);
        return true;
    }

    public static void Delete(int dungeonId)
    {
        PlayerPrefs.DeleteKey(KeyPrefix + dungeonId);
        PlayerPrefs.Save();
    }
}
