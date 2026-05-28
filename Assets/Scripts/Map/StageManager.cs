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
    toBoss,
    Villiage
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
    public List<int> StageTypeCount = new List<int>();
    // stage 리스트 인덱스에 해당하는 방이 StageTypeCount 개수만큼 생성
    // 0(시작방), 1(함정방), stage.Count - 1(일반방)은 StageTypeCount에 포함X

    public GameObject BossStage;
    public float spacing = 40f;
    public List<GameObject> TutorialStage = new List<GameObject>();
    [Space(10f)]

    public int StageCount = 7;

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

    public GameObject StageParent;

    public DungeonMapType[] mapType;
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
        {
            curFloor = Mathf.Min(1, TutorialStage.Count);
            StartCoroutine(InitializeDungeonMapWhenReady());
        }
        else if(!Tutorial)
        {
            for (int i = 0; i < mapType[GameManager.instance.spawnedDungeon.data.dungeonNumber - 2].mapPrefab.Length; i++)
            {
                stage.Add(mapType[GameManager.instance.spawnedDungeon.data.dungeonNumber - 2].mapPrefab[i]);
                StageTypeCount.Add(mapType[GameManager.instance.spawnedDungeon.data.dungeonNumber - 2].mapCount[i]);
            }

            BossStage = mapType[GameManager.instance.spawnedDungeon.data.dungeonNumber - 2].bossMapPrefab;
            StartCoroutine(StartDungeon());
        }
    }

    IEnumerator StartDungeon()
    {
        yield return StartCoroutine(CreateStage());

        if (Player == null)
            Player = GameObject.FindGameObjectWithTag("Player");

        SyncPlayerToMinimap();
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
        var origin = GetGridOrigin();
        return new Vector2Int(
            Mathf.RoundToInt((worldPosition.x - origin.x) / spacing),
            Mathf.RoundToInt((worldPosition.z - origin.z) / spacing));
    }

    public Vector3 GridToWorld(Vector2Int gridPosition, float y = 0f)
    {
        var origin = GetGridOrigin();
        return new Vector3(
            origin.x + gridPosition.x * spacing,
            y,
            origin.z + gridPosition.y * spacing);
    }

    Vector3 GetGridOrigin()
    {
        if (StageParent != null)
            return new Vector3(StageParent.transform.position.x, 0f, StageParent.transform.position.z);

        var activeMapRoot = DungeonMapLayoutResolver.ResolveActiveMapRoot();
        if (activeMapRoot != null)
            return new Vector3(activeMapRoot.position.x, 0f, activeMapRoot.position.z);

        return new Vector3(transform.position.x, 0f, transform.position.z);
    }

    public void SyncDungeonMapAfterLayout()
    {
        foreach (var grid in Object.FindObjectsByType<DungeonMapGridBuilder>(FindObjectsSortMode.None))
            grid.ForceRebuild();

        SyncPlayerToMinimap();
    }

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

    // ─────────────────────────────────────────────────────────────────
    // CreateStage
    //
    // 중앙 (0,0)
    //
    // stage[] 인덱스 구조
    //   stage[0]       → 코너 시작방 (1개 고정)
    //   stage[1]       → Trap 방 (10% 비율)
    //   stage[2+]      → StageTypeCount[i] 개수만큼 생성
    //   stage[마지막]  → 일반방 (나머지 빈 칸 전부 채움)
    // ─────────────────────────────────────────────────────────────────
    public IEnumerator CreateStage()
    {
        Transform stageRoot = (StageParent != null) ? StageParent.transform : transform;

        int half = StageCount / 2;

        int xMin = -half;
        int xMax = xMin + StageCount - 1;

        int zMin = -half;
        int zMax = zMin + StageCount - 1;

        List<Vector2Int> availableCells = new List<Vector2Int>();


        for (int x = xMin; x <= xMax; x++)
        {
            for (int z = zMin; z <= zMax; z++)
            {
                // 중앙 보스 3x3 제외
                if (x >= -1 && x <= 1 &&
                    z >= -1 && z <= 1)
                {
                    continue;
                }

                // 시작방 제외
                if (x == xMin && z == zMin)
                {
                    continue;
                }

                availableCells.Add(new Vector2Int(x, z));
            }
        }

        // ───────────────── 랜덤 로직 ─────────────────

        for (int i = 0; i < availableCells.Count; i++)
        {
            int rand = Random.Range(i, availableCells.Count);

            Vector2Int temp = availableCells[i];
            availableCells[i] = availableCells[rand];
            availableCells[rand] = temp;
        }

        //while (StageTypeCount.Count < stage.Count)
        //{
        //    StageTypeCount.Add(0);
        //}

        int cellIndex = 0;

        // ───────────────── Trap 위치 ─────────────────

        HashSet<Vector2Int> trapPositions
            = new HashSet<Vector2Int>();

        for (int i = 0;
             i < StageTypeCount[1] &&
             cellIndex < availableCells.Count;
             i++, cellIndex++)
        {
            trapPositions.Add(availableCells[cellIndex]);
        }

        // ───────────────── 특수방 위치 ─────────────────

        Dictionary<int, HashSet<Vector2Int>> typePositions
            = new Dictionary<int, HashSet<Vector2Int>>();

        for (int si = 2; si < stage.Count - 1; si++)
        {
            int count = StageTypeCount[si];

            HashSet<Vector2Int> positions
                = new HashSet<Vector2Int>();

            for (int i = 0;
                 i < count &&
                 cellIndex < availableCells.Count;
                 i++, cellIndex++)
            {
                positions.Add(availableCells[cellIndex]);
            }

            typePositions[si] = positions;
        }

        // ───────────────── 보스방 생성 ─────────────────

        GameObject bossRoom = Instantiate(BossStage, stageRoot);

        bossRoom.transform.localPosition = Vector3.zero;

        StagePositions.Add(Vector2Int.zero);

        // ───────────────── 시작방 생성 ─────────────────

        GameObject startRoom = Instantiate(stage[0], stageRoot);

        startRoom.transform.localPosition = new Vector3(
            xMin * spacing,
            0f,
            zMin * spacing);

        StagePositions.Add(new Vector2Int(xMin, zMin));

        // ───────────────── 나머지 방 생성 ─────────────────

        for (int i = 0; i < availableCells.Count; i++)
        {
            Vector2Int gridPos = availableCells[i];

            Vector3 localPos = new Vector3(
                gridPos.x * spacing,
                0f,
                gridPos.y * spacing);

            GameObject spawnedStage = null;

            // ───── Trap 방 ─────

            if (trapPositions.Contains(gridPos))
            {
                spawnedStage = Instantiate(stage[1], stageRoot);
            }
            else
            {
                bool placed = false;

                // ───── 특수방 ─────

                for (int si = 2; si < stage.Count - 1; si++)
                {
                    if (typePositions.ContainsKey(si) &&
                        typePositions[si].Contains(gridPos))
                    {
                        spawnedStage = Instantiate(stage[si], stageRoot);

                        placed = true;

                        break;
                    }
                    
                }
                if (!placed)
                {
                    spawnedStage = Instantiate(
                        stage[stage.Count - 1],
                        stageRoot);
                }
            }

            if (spawnedStage != null)
            {
                spawnedStage.transform.localPosition = localPos;
            }

            StagePositions.Add(gridPos);
        }

        yield return null;

        // ───────────────── 플레이어 위치 ─────────────────

        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }

        if (Player != null)
        {
            Player.transform.position = stageRoot.TransformPoint(
                new Vector3(
                    xMin * spacing,
                    1.9f,
                    zMin * spacing));
        }

        RebuildStagePositions();

        SyncDungeonMapAfterLayout();
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
        //        //클리어 처리
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
        //        //튜토리얼 클리어
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

        //IEnumerator StageCreate()
        //{
        //    int countHalf = (StageCount % 2 == 1) ? StageCount / 2 + 1 : StageCount / 2;
        //    HashSet<Vector2Int> sealedStonePositions = new HashSet<Vector2Int>();
        //    for (int i = 0; i < MaxSealedStoneCount; i++)
        //    {
        //        int x, z;
        //        do
        //        {
        //            x = Random.Range(-countHalf, StageCount - countHalf + 2);
        //            z = Random.Range(-countHalf, StageCount - countHalf + 2);
        //        } while ((x >= -1 && x <= 1 && z >= -1 && z <= 1) || sealedStonePositions.Contains(new Vector2Int(x, z)));
        //        sealedStonePositions.Add(new Vector2Int(x, z));
        //    }

        //    HashSet<Vector2Int> trapStagePositions = new HashSet<Vector2Int>();
        //    for (int i = 0; i < Mathf.FloorToInt((Mathf.Pow(StageCount + 2, 2) - 9) / 10f); i++)
        //    {
        //        int x, z;
        //        do
        //        {
        //            x = Random.Range(-countHalf, StageCount - countHalf + 2);
        //            z = Random.Range(-countHalf, StageCount - countHalf + 2);
        //        } while ((x >= -1 && x <= 1 && z >= -1 && z <= 1) || trapStagePositions.Contains(new Vector2Int(x, z)));
        //        trapStagePositions.Add(new Vector2Int(x, z));
        //    }

        //    for (int x = -countHalf; x < StageCount - countHalf + 2; x++)
        //    {
        //        for (int z = -countHalf; z < StageCount - countHalf + 2; z++)
        //        {
        //            if (x >= -1 && x <= 1 && z >= -1 && z <= 1)
        //            {
        //                if (x == 0 && z == 0)
        //                {
        //                    Instantiate(BossStage, transform.localPosition, Quaternion.identity, StageParent.transform);
        //                    StagePositions.Add(new Vector2Int(x, z));
        //                }
        //            }
        //            else if (x == -countHalf && z == -countHalf)
        //            {
        //                Instantiate(stage[0], spawnPos, Quaternion.identity, StageParent.transform);
        //                StagePositions.Add(new Vector2Int(x, z));
        //            }
        //            else if (sealedStonePositions.Contains(new Vector2Int(x, z)))
        //            {
        //                StagePositions.Add(new Vector2Int(x, z));
        //                Instantiate(stage[1], spawnPos, Quaternion.identity, StageParent.transform);
        //            }
        //            else if (trapStagePositions.Contains(new Vector2Int(x, z)))
        //            {
        //                StagePositions.Add(new Vector2Int(x, z));
        //                Instantiate(stage[2], spawnPos, Quaternion.identity, transform);
        //            }
        //            else
        //            {
        //                StagePositions.Add(new Vector2Int(x, z));
        //                Instantiate(stage[Random.Range(3, stage.Count)], spawnPos, Quaternion.identity, transform);
        //            }
        //        }
        //    }

        //    yield return null;
        //}
    }
}