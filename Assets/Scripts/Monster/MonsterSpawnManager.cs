using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MonsterStatData
{
    public float MaxHP;
    public float CurHP;
    public float MoveSpeed;

    [Space(10f)]
    public float AttackDamage;
    public float AttackDelay;

    [Space(10f)]
    public float AttackRange;
}

public class MonsterSpawnManager : MonoBehaviour
{
    public static MonsterSpawnManager instance;

    public List<GameObject> CurrentAliveMonsters = new List<GameObject>();

    public StageManager stageManager;

    public bool isMonsterSpawn;

    [SerializeField] private GameObject[] dropItems;

    [HideInInspector]
    public Vector3 spawnPos;

    private SealedStoneRoom currentSealedStoneRoom;
    private bool sealedStoneWaveStarted = false;

    private void Awake()
    {
        instance = this;
        stageManager = StageManager.instance;
        if (stageManager == null)
            stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
    }

    private void Update()
    {
        if (stageManager == null)
            return;

        if (stageManager.curStageCleared &&
            stageManager.curStageType != StageType.Trap &&
            stageManager.curStageType != StageType.Treasure)
        {
            stageManager.activePortal = true;
            return;
        }

        switch (stageManager.curStageType)
        {
            case StageType.Normal:
                HandleCombatStage(false);
                break;

            case StageType.Boss:
                HandleCombatStage(true);
                break;

            case StageType.Bonfire:
                HandleBonfireStage();
                break;

            case StageType.Trap:
                HandleTrapStage();
                break;

            case StageType.Treasure:
                HandleTreasureStage();
                break;

            case StageType.RandomPortal:
            case StageType.ReturnPortal:
            case StageType.BuffStone:
            case StageType.BuffStatue:
            case StageType.Store:
            case StageType.Shop:
            case StageType.None:
                HandleFreeClearStage();
                break;

            case StageType.SealedStone:
                HandleSealedStoneStage();
                break;
        }
    }

    void HandleCombatStage(bool isBoss)
    {
        if (isMonsterSpawn)
        {
            bool spawned = SpawnGrid();

            if (!spawned)
            {
                stageManager.curStageCleared = true;
                stageManager.activePortal = true;
                isMonsterSpawn = false;
                return;
            }
        }

        PruneDeadMonsters();

        if (CurrentAliveMonsters.Count > 0)
            stageManager.activePortal = false;
        else
        {
            stageManager.activePortal = true;
            stageManager.curStageCleared = true;
        }
    }

    void HandleBonfireStage()
    {
        stageManager.curStageCleared = true;
        stageManager.activePortal = true;

        if (isMonsterSpawn)
        {
            spawnPos.y = 1f;

            if (TrySpawnAt(spawnPos, 0))
                Debug.Log("Bonfire Spawn");

            isMonsterSpawn = false;
        }

        GameManager.instance.OnShelterEnter?.Invoke();

        PlayerProfile playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>();

        if (playerProfile != null)
        {
            playerProfile.MPBuff(4);

            if (!GameManager.instance.shelterHpBan)
                playerProfile.HPBuff(0.5f);

            if (!GameManager.instance.shelterActCountBan)
                playerProfile.ActCountPlus(3, GameManager.instance.recoveryMultiplier);
        }
    }

    void HandleTrapStage()
    {
        stageManager.curStageCleared = true;
        stageManager.activePortal = true;

        if (isMonsterSpawn)
        {
            spawnPos.y = 0.5f;
            TrySpawnRandom(spawnPos);
            isMonsterSpawn = false;
        }
    }

    void HandleTreasureStage()
    {
        stageManager.curStageCleared = true;
        stageManager.activePortal = true;

        if (isMonsterSpawn)
        {
            spawnPos.y = 2f;
            TrySpawnRandom(spawnPos);
            isMonsterSpawn = false;
        }
    }

    void HandleFreeClearStage()
    {
        stageManager.curStageCleared = true;
        stageManager.activePortal = true;
        isMonsterSpawn = false;
    }

    void HandleSealedStoneStage()
    {
        if (isMonsterSpawn)
        {
            currentSealedStoneRoom = FindCurrentSealedStoneRoom();
            sealedStoneWaveStarted = false;

            if (currentSealedStoneRoom != null && currentSealedStoneRoom.isFake)
            {
                WaveMonsterSpawn wave = currentSealedStoneRoom.waveMonsterSpawn;

                if (wave != null)
                {
                    wave.spawnPos = spawnPos;
                    wave.StartWaves();
                    sealedStoneWaveStarted = true;
                }
                else
                {
                    Debug.LogWarning("[MonsterSpawnManager] 가짜 봉인석 방에 WaveMonsterSpawn이 없습니다.");
                    stageManager.curStageCleared = true;
                    stageManager.activePortal = true;
                }

                isMonsterSpawn = false;
                return;
            }

            bool spawned = SpawnGrid();

            if (!spawned)
            {
                stageManager.curStageCleared = false;
                stageManager.activePortal = false;
                isMonsterSpawn = false;
                return;
            }

            isMonsterSpawn = false;
        }

        if (currentSealedStoneRoom != null && currentSealedStoneRoom.isFake)
            return;

        PruneDeadMonsters();

        // 몬스터가 남아있으면 포탈 닫기
        if (CurrentAliveMonsters.Count > 0)
        {
            stageManager.activePortal = false;
            stageManager.curStageCleared = false;
            return;
        }

        // 봉인석이 아직 하나라도 남아있으면 포탈 닫기
        if (stageManager.SealedStoneLeft > 0)
        {
            stageManager.activePortal = false;
            stageManager.curStageCleared = false;
            return;
        }

        // 몬스터와 봉인석이 모두 없어졌을 때만 포탈 생성
        stageManager.activePortal = true;
        stageManager.curStageCleared = true;
    }


    SealedStoneRoom FindCurrentSealedStoneRoom()
    {
        Vector3 stageWorldPos = stageManager.GridToWorld(stageManager.curStagePos);

        Collider[] hits = Physics.OverlapSphere(stageWorldPos, stageManager.spacing * 0.4f);
        foreach (var hit in hits)
        {
            SealedStoneRoom room = hit.GetComponentInParent<SealedStoneRoom>();
            if (room != null)
                return room;
        }

        return null;
    }

    bool TrySpawnAt(Vector3 pos, int index)
    {
        if (stageManager.curStageSpawnPrefabs == null)
            return false;

        if (stageManager.curStageSpawnPrefabs.Count == 0)
            return false;

        if (index < 0 || index >= stageManager.curStageSpawnPrefabs.Count)
            return false;

        GameObject prefab = stageManager.curStageSpawnPrefabs[index];

        if (prefab == null)
            return false;

        Instantiate(prefab, pos, Quaternion.identity, stageManager.transform);
        return true;
    }

    bool TrySpawnRandom(Vector3 pos)
    {
        if (stageManager.curStageSpawnPrefabs == null)
            return false;

        if (stageManager.curStageSpawnPrefabs.Count == 0)
            return false;

        int randomIndex = Random.Range(0, stageManager.curStageSpawnPrefabs.Count);
        GameObject prefab = stageManager.curStageSpawnPrefabs[randomIndex];

        if (prefab == null)
            return false;

        Instantiate(prefab, pos, Quaternion.identity, stageManager.transform);
        return true;
    }

    bool SpawnGrid()
    {
        Debug.Log("SpawnGrid");

        if (stageManager.curStageSpawnPrefabs == null)
        {
            Debug.LogWarning("SpawnPrefabs NULL");
            isMonsterSpawn = false;
            return false;
        }

        if (stageManager.curStageSpawnPrefabs.Count == 0)
        {
            Debug.LogWarning("SpawnPrefabs Count 0");
            isMonsterSpawn = false;
            return false;
        }

        List<GameObject> validPrefabs = new List<GameObject>();
        for (int i = 0; i < stageManager.curStageSpawnPrefabs.Count; i++)
        {
            if (stageManager.curStageSpawnPrefabs[i] != null)
                validPrefabs.Add(stageManager.curStageSpawnPrefabs[i]);
        }

        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning("유효한 몬스터 프리팹 없음");
            isMonsterSpawn = false;
            return false;
        }

        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(validPrefabs.Count));
        float gridSpacing = 2f;
        int count = 0;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                if (count >= validPrefabs.Count)
                {
                    isMonsterSpawn = false;
                    return true;
                }

                GameObject prefab = validPrefabs[count];
                Vector3 offset = new Vector3(x * gridSpacing, 0f, z * gridSpacing);
                GameObject monster = Instantiate(prefab, spawnPos + offset, Quaternion.identity, transform);
                CurrentAliveMonsters.Add(monster);
                count++;
            }
        }

        isMonsterSpawn = false;
        return true;
    }

    void PruneDeadMonsters()
    {
        if (CurrentAliveMonsters == null)
            return;

        for (int i = CurrentAliveMonsters.Count - 1; i >= 0; i--)
        {
            if (CurrentAliveMonsters[i] == null)
                CurrentAliveMonsters.RemoveAt(i);
        }
    }

    public void MonsterDead(GameObject enemy)
    {
        if (dropItems == null || dropItems.Length == 0 || dropItems[0] == null)
            return;

        GameObject map = GameObject.FindGameObjectWithTag("Map");

        if (map == null)
            return;

        Instantiate(dropItems[0], enemy.transform.position, Quaternion.identity, map.transform);
    }
}