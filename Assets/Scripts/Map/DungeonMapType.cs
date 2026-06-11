using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "MapType", menuName = "Add MapType")]
public class DungeonMapType : ScriptableObject
{
    [Header("½ºÅ×ÀÌÁö Å©±â")]
    public int stageSize;

    [Header("¸Ê ÇÁ¸®ÆÕ")]
    public GameObject[] mapPrefab;

    [Header("º¸½º ¸Ê ÇÁ¸®ÆÕ")]
    public GameObject bossMapPrefab;

    [Header("¹æ °¹¼ö")]
    public int[] mapCount;
}
