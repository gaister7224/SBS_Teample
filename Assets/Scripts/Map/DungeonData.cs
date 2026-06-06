using UnityEngine;

[CreateAssetMenu]
public class DungeonData : ScriptableObject
{
    public int dungeonNumber;
    public string dungeonName;
    public GameObject mapPrefab;
    public int floor;
}
