using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "MapType", menuName = "Add MapType")]
public class DungeonMapType : ScriptableObject
{
    [Header("¸Ê ÇÁ¸®ÆÕ")]
    public GameObject[] mapPrefab;

    [Header("¹æ °¹¼ö")]
    public int[] mapCount;
}
