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
    Return
}

public enum StageType
{
    Normal,
    Difficult,
    Trap,
    SealedStone,
    Treasure,
    Boss,
    Bonfire,
    Shop,
    BuffStatue,
    RandomPortal,
    BackPortal,
    None
}

public class StageManager : MonoBehaviour
{
    public static StageManager instance;
    public MonsterSpawnManager monsterSpawnManager;

    public bool Tutorial;


    public List<GameObject> stage = new List<GameObject>();
    public GameObject BossStage;
    public float spacing = 40f;

    public int StageCount = 7;

    public HashSet<Vector2Int> StagePositions = new HashSet<Vector2Int>();
    public List<Vector2Int> surroundStagePositions = new List<Vector2Int>();
    [Space(10f)]
    public Vector2Int curStagePos;
    public StageType curStageType;
    public bool curStageCleared;
    public List<GameObject> curStageSpawnPrefabs = new List<GameObject>();

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
        if (!Tutorial)
        {
            StartCoroutine(StageCreate());
        }
    }

    void Update()
    {
        //StartCoroutine(SurroundStage());
    }

    IEnumerator SurroundStage()
    {
        for (int i = 0; i < 9; i++)
        {
            int x = curStagePos.x + (i % 3 - 1);
            int z = curStagePos.y + (i / 3 - 1);
            Vector2Int pos = new Vector2Int(x, z);
            if (StagePositions.Contains(pos))
            {
                surroundStagePositions.Add(pos);
            }
        }

        yield return null;
    }

    IEnumerator StageCreate()
    {
        int countHalf = (StageCount % 2 == 1) ? StageCount / 2 + 1 : StageCount / 2;

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
                else
                {
                    Vector3 spawnPos = new Vector3
                    (
                        x * spacing,
                        0f,
                        z * spacing
                    );
                    StagePositions.Add(new Vector2Int(x, z));
                    Instantiate(stage[Random.Range(0, stage.Count)], spawnPos, Quaternion.identity, transform);
                }

            }
        }

        yield return null;
    }
}
