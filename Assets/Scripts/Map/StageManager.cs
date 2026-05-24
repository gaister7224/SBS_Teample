using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PortalDirection
{
    Front,
    Back,
    Left,
    Right,
    Clear,
    Random,
    Return,
    toBoss
}

public enum StageType
{
    Normal,
    Difficult,
    Trap,
    SealedStone,
    BuffStone,
    Treasure,
    Boss,
    Bonfire,
    Shop,
    BuffStatue,
    RandomPortal,
    ReturnPortal,
    Store,
    None
}

public class StageManager : MonoBehaviour
{
    public static StageManager instance;
    [Space(10f)]
    public MonsterSpawnManager monsterSpawnManager;

    public bool Tutorial;
    [Space(10f)]


    public List<GameObject> stage = new List<GameObject>();
    public GameObject BossStage;
    public float spacing = 40f;
    public List<GameObject> TutorialStage = new List<GameObject>();
    [Space(10f)]

    public int StageCount = 7;
    public int MaxSealedStoneCount = 3;

    public HashSet<Vector2Int> StagePositions = new HashSet<Vector2Int>();
    public List<Vector2Int> surroundStagePositions = new List<Vector2Int>();
    [Space(10f)]
    public Vector2Int curStagePos;
    public StageType curStageType;
    public bool curStageCleared;
    public List<GameObject> curStageSpawnPrefabs = new List<GameObject>();
    [Space(10f)]
    public bool curFloorCleared = true;
    public int LeftFloorCount = 1;
    public int curFloor;

    [Space(10f)]
    public int SealedStoneLeft;

    public GameObject Player;

    public bool activePortal;


    private void Awake()
    {
        monsterSpawnManager = GetComponentInChildren<MonsterSpawnManager>();
        Player = GameObject.FindGameObjectWithTag("Player");
        instance = this;
    }

    void Start()
    {
        curFloorCleared = false;
        if (Tutorial && HasTutorialStageInHierarchy())
            curFloor = Mathf.Min(1, TutorialStage.Count);

        StartCoroutine(InitializeDungeonMapWhenReady());
    }

    IEnumerator InitializeDungeonMapWhenReady()
    {
        const int maxFrames = 60;

        for (var frame = 0; frame < maxFrames; frame++)
        {
            RebuildStagePositions();
            if (StagePositions.Count > 0)
                break;

            if (Player == null)
                Player = GameObject.FindGameObjectWithTag("Player");

            yield return null;
        }

        SyncDungeonMapAfterLayout();
    }

    bool HasTutorialStageInHierarchy()
    {
        foreach (Transform child in transform)
        {
            if (child.name.Contains("TutorialStages"))
                return true;
        }

        return false;
    }

    /// <summary>???? ?? ??? ???? TutorialStage ???? ?????.</summary>
    public void AdvanceTutorialFloor()
    {
        StartCoroutine(AdvanceTutorialFloorRoutine());
    }

    public IEnumerator AdvanceTutorialFloorRoutine()
    {
        if (!Tutorial || TutorialStage.Count == 0 || curFloor >= TutorialStage.Count)
            yield break;

        DestroyTutorialStageRoots();
        yield return WaitUntilTutorialRootsDestroyed();

        var prefab = TutorialStage[curFloor];
        if (prefab != null)
        {
            var stageInstance = Instantiate(prefab, transform.position, Quaternion.identity, transform);
            stageInstance.name = prefab.name;
        }

        curFloor++;
        LeftFloorCount = Mathf.Max(0, TutorialStage.Count - curFloor);
        curFloorCleared = false;
        surroundStagePositions.Clear();
        curStagePos = Vector2Int.zero;

        yield return WaitForStageLayoutReady();

        ResetMapVisibilityForLayoutChange();
        SyncDungeonMapAfterLayout();
    }

    IEnumerator WaitUntilTutorialRootsDestroyed()
    {
        const int maxFrames = 15;

        for (var frame = 0; frame < maxFrames; frame++)
        {
            if (!HasTutorialStageInHierarchy())
                yield break;

            yield return null;
        }
    }

    IEnumerator WaitForStageLayoutReady()
    {
        const int maxFrames = 30;

        for (var frame = 0; frame < maxFrames; frame++)
        {
            RebuildStagePositions();
            if (StagePositions.Count > 0)
                yield break;

            yield return null;
        }
    }

    void DestroyTutorialStageRoots()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name.Contains("TutorialStages"))
                Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// ???? ????? ??(PortalManager) ???????? ????? ????? ?????. ???????? ??? ??????.
    /// </summary>
    public void EnsureStagePositions()
    {
        if (Tutorial || StagePositions.Count == 0)
            RebuildStagePositions();
    }

    public void RebuildStagePositions()
    {
        StagePositions.Clear();

        foreach (var position in DungeonMapLayoutResolver.CollectStagePositions())
            StagePositions.Add(position);
    }

    void ResetMapVisibilityForLayoutChange()
    {
        if (DungeonMapService.Instance == null)
            return;

        var service = DungeonMapService.Instance;
        var dungeonId = service.GetCurrentDungeonId();
        service.Current.Clear();
        service.Current.SetDungeonId(dungeonId);
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / spacing),
            Mathf.RoundToInt(worldPosition.z / spacing));
    }

    public void SyncDungeonMapAfterLayout()
    {
        foreach (var grid in Object.FindObjectsByType<DungeonMapGridBuilder>(FindObjectsSortMode.None))
            grid.ForceRebuild();

        SyncPlayerToMinimap();
    }

    /// <summary>?? ???????? ?? ???? ??????? ??????? ????.</summary>
    public void SyncPlayerToMinimap(Vector2Int? cellOverride = null)
    {
        EnsureStagePositions();

        if (DungeonMapService.Instance == null)
        {
            var serviceObject = new GameObject("DungeonMapService");
            serviceObject.AddComponent<DungeonMapService>();
        }

        DungeonMapService.Instance.EnsureLoadedForCurrentDungeon();

        if (StagePositions.Count == 0)
            return;

        var cell = cellOverride ?? ResolvePlayerMapCell();
        if (!StagePositions.Contains(cell) && !TryResolveInitialMapCell(out cell))
            return;

        curStagePos = cell;
        DungeonMapService.Instance.SetPlayerPosition(cell);
        DungeonMapService.Instance.RevealAround(cell, 1, StagePositions);
    }

    Vector2Int ResolvePlayerMapCell()
    {
        if (Player == null)
            Player = GameObject.FindGameObjectWithTag("Player");

        if (Player != null)
        {
            var fromPlayer = WorldToGrid(Player.transform.position);
            if (StagePositions.Contains(fromPlayer))
                return fromPlayer;
        }

        if (StagePositions.Contains(curStagePos))
            return curStagePos;

        foreach (var pos in StagePositions)
            return pos;

        return curStagePos;
    }

    bool TryResolveInitialMapCell(out Vector2Int cell)
    {
        if (StagePositions.Contains(curStagePos))
        {
            cell = curStagePos;
            return true;
        }

        if (Player != null)
        {
            var fromPlayer = WorldToGrid(Player.transform.position);
            if (StagePositions.Contains(fromPlayer))
            {
                cell = fromPlayer;
                return true;
            }
        }

        foreach (var pos in StagePositions)
        {
            cell = pos;
            return true;
        }

        cell = default;
        return false;
    }

    void Update()
    {
        //StartCoroutine(SurroundStage());
        //if (curFloorCleared && !Tutorial)
        //{
        //    curFloorCleared = false;
        //    StagePositions.Clear();

        //    for (int i = transform.childCount - 1; i >= 0; i--)
        //    {
        //        if (transform.GetChild(i).name.Contains("Stage"))
        //        {
        //            Destroy(transform.GetChild(i).gameObject);
        //        }
        //    }

        //    if (LeftFloorCount > 0)
        //    {
        //        curFloor++;
        //        LeftFloorCount--;
        //        StartCoroutine(StageCreate());
        //    }
        //    else
        //    {
        //        //???? ?????
        //    }
        //}
        //else if (curFloorCleared && Tutorial && TutorialStage.Count > 0)
        //{
        //    curFloorCleared = false; 
            
        //    for (int i = transform.childCount - 1; i >= 0; i--)
        //    {
        //        if (transform.GetChild(i).name.Contains("Stage"))
        //        {
        //            Destroy(transform.GetChild(i).gameObject);
        //        }
        //    }

        //    if (LeftFloorCount == 2f)
        //    {
        //        curFloor++;
        //        LeftFloorCount--;
        //        Instantiate(TutorialStage[0], transform.position, Quaternion.identity, transform);
        //    }
        //    else if (LeftFloorCount == 1f)
        //    {
        //        curFloor++;
        //        LeftFloorCount--;
        //        Instantiate(TutorialStage[1], transform.position, Quaternion.identity, transform);
        //    }
        //    else if (LeftFloorCount == 0f)
        //    {
        //        //????? ?????
        //    }
        //}

        //IEnumerator SurroundStage()
        //{
        //    for (int i = 0; i < 9; i++)
        //    {
        //        int x = curStagePos.x + (i % 3 - 1);
        //        int z = curStagePos.y + (i / 3 - 1);
        //        Vector2Int pos = new Vector2Int(x, z);
        //        if (StagePositions.Contains(pos))
        //        {
        //            surroundStagePositions.Add(pos);
        //        }
        //    }

        //    yield return null;
        //}

        IEnumerator StageCreate()
        {
            int countHalf = (StageCount % 2 == 1) ? StageCount / 2 + 1 : StageCount / 2;
            HashSet<Vector2Int> sealedStonePositions = new HashSet<Vector2Int>();
            for (int i = 0; i < MaxSealedStoneCount; i++)
            {
                int x, z;
                do
                {
                    x = Random.Range(-countHalf, StageCount - countHalf + 2);
                    z = Random.Range(-countHalf, StageCount - countHalf + 2);
                } while ((x >= -1 && x <= 1 && z >= -1 && z <= 1) || sealedStonePositions.Contains(new Vector2Int(x, z)));
                sealedStonePositions.Add(new Vector2Int(x, z));
            }

            HashSet<Vector2Int> trapStagePositions = new HashSet<Vector2Int>();
            for (int i = 0; i < Mathf.FloorToInt((Mathf.Pow(StageCount + 2, 2) - 9) / 10f); i++)
            {
                int x, z;
                do
                {
                    x = Random.Range(-countHalf, StageCount - countHalf + 2);
                    z = Random.Range(-countHalf, StageCount - countHalf + 2);
                } while ((x >= -1 && x <= 1 && z >= -1 && z <= 1) || trapStagePositions.Contains(new Vector2Int(x, z)));
                trapStagePositions.Add(new Vector2Int(x, z));
            }

            for (int x = -countHalf; x < StageCount - countHalf + 2; x++)
            {
                for (int z = -countHalf; z < StageCount - countHalf + 2; z++)
                {
                    if (x >= -1 && x <= 1 && z >= -1 && z <= 1)
                    {
                        if (x == 0 && z == 0)
                        {
                            Vector3 spawnPos = new Vector3
                            (
                                x * spacing,
                                0f,
                                z * spacing
                            );

                            Instantiate(BossStage, transform.localPosition, Quaternion.identity, transform);
                            StagePositions.Add(new Vector2Int(x, z));
                        }
                    }
                    else if (x == -countHalf && z == -countHalf)
                    {
                        Vector3 spawnPos = new Vector3
                        (
                            x * spacing,
                            0f,
                            z * spacing
                        );

                        Instantiate(stage[0], spawnPos, Quaternion.identity, transform);
                        StagePositions.Add(new Vector2Int(x, z));
                    }
                    else if (sealedStonePositions.Contains(new Vector2Int(x, z)))
                    {
                        Vector3 spawnPos = new Vector3
                        (
                            x * spacing,
                            0f,
                            z * spacing
                        );
                        StagePositions.Add(new Vector2Int(x, z));
                        Instantiate(stage[1], spawnPos, Quaternion.identity, transform);
                    }
                    else if (trapStagePositions.Contains(new Vector2Int(x, z)))
                    {
                        Vector3 spawnPos = new Vector3
                        (
                            x * spacing,
                            0f,
                            z * spacing
                        );
                        StagePositions.Add(new Vector2Int(x, z));
                        Instantiate(stage[2], spawnPos, Quaternion.identity, transform);
                    }
                    else
                    {
                        Vector3 spawnPos = new Vector3
                        (
                            x * spacing,
                            0f,
                            z * spacing
                        );
                        StagePositions.Add(new Vector2Int(x, z));

                        Instantiate(stage[Random.Range(3, stage.Count)], spawnPos, Quaternion.identity, transform);
                    }
                }
            }

            yield return null;
        }
    }
}
