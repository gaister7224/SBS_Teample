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
    RandomPortal,
    BackPortal,
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

    public GameObject StageParent;


    private void Awake()
    {
        monsterSpawnManager = GetComponentInChildren<MonsterSpawnManager>();
        Player = GameObject.FindGameObjectWithTag("Player");
        instance = this;
    }

    void Start()
    {
        curFloorCleared = true;
    }

    void Update()
    {
        //StartCoroutine(SurroundStage());
        if (curFloorCleared && !Tutorial)
        {
            curFloorCleared = false;
            StagePositions.Clear();

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).name.Contains("Stage"))
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }

            if (LeftFloorCount > 0)
            {
                curFloor++;
                LeftFloorCount--;
                StartCoroutine(StageCreate());
            }
            else
            {
                //던전 클리어
            }
        }
        else if (curFloorCleared && Tutorial && TutorialStage.Count > 0)
        {
            curFloorCleared = false; 
            
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).name.Contains("Stage"))
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }

            if (LeftFloorCount == 2f)
            {
                curFloor++;
                LeftFloorCount--;
                Instantiate(TutorialStage[0], transform.position, Quaternion.identity, transform);
            }
            else if (LeftFloorCount == 1f)
            {
                curFloor++;
                LeftFloorCount--;
                Instantiate(TutorialStage[1], transform.position, Quaternion.identity, transform);
            }
            else if (LeftFloorCount == 0f)
            {
                //튜토리얼 클리어
            }
        }

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

                            Instantiate(BossStage, transform.localPosition, Quaternion.identity, StageParent.transform);
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

                        Instantiate(stage[0], spawnPos, Quaternion.identity, StageParent.transform);
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
                        Instantiate(stage[1], spawnPos, Quaternion.identity, StageParent.transform);
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

                        Instantiate(stage[Random.Range(2, stage.Count)], spawnPos, Quaternion.identity, StageParent.transform);
                    }
                }
            }

            yield return null;
        }
    }
}
