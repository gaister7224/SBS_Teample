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

    public List<GameObject> CurrentAliveMonsters;

    public StageManager stageManager;

    public bool isMonsterSpawn;

    [SerializeField] private GameObject[] dropItems;

    [HideInInspector]
    public Vector3 spawnPos;

    void Awake()
    {
        instance = this;
        stageManager = GetComponentInParent<StageManager>();
    }

    void Update()
    {
        if (stageManager == null)
            return;

        if (!stageManager.curStageCleared)
        {
            if (stageManager.curStageType == StageType.Normal)
            {
                if (isMonsterSpawn)
                    SpawnGrid();

                PruneDeadMonsters();

                //if (CurrentAliveMonsters.Count > 0)
                //    stageManager.activePortal = false;
                //else
                //    stageManager.activePortal = true;
                //stageManager.curStageCleared = true;

                if (CurrentAliveMonsters.Count > 0)
                {
                    for (int i = 0; i < CurrentAliveMonsters.Count; i++)
                    {
                        if (CurrentAliveMonsters[i] == null)
                        {
                            CurrentAliveMonsters.RemoveAt(i);
                        }
                    }

                    stageManager.activePortal = false;
                }
                else if (CurrentAliveMonsters.Count == 0)
                {
                    stageManager.activePortal = true;
                    stageManager.curStageCleared = true;
                }
            }
            else if (stageManager.curStageType == StageType.Bonfire)
            {
                stageManager.curStageCleared = true;
                stageManager.activePortal = true;

                if (isMonsterSpawn && stageManager.curStageSpawnPrefabs[0] != null)
                {
                    Vector3 spawnPos = new Vector3(stageManager.curStagePos.x * stageManager.spacing, 2f, stageManager.curStagePos.y * stageManager.spacing);
                    Instantiate(stageManager.curStageSpawnPrefabs[0], spawnPos, Quaternion.identity, stageManager.transform);

                    isMonsterSpawn = false;
                }

                Debug.Log("�÷��̾� ȸ��");

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
            else if (stageManager.curStageType == StageType.Trap)
            {
                stageManager.curStageCleared = true;
                stageManager.activePortal = true;

                if (isMonsterSpawn)
                {
                    Vector3 spawnPos = new Vector3(stageManager.curStagePos.x * stageManager.spacing, 0f, stageManager.curStagePos.y * stageManager.spacing);
                    Instantiate(stageManager.curStageSpawnPrefabs[Random.Range(0, stageManager.curStageSpawnPrefabs.Count)], spawnPos, Quaternion.identity, stageManager.transform);

                    isMonsterSpawn = false;
                }
            }
            else if (stageManager.curStageType == StageType.RandomPortal)
            {
                stageManager.curStageCleared = true;
                stageManager.activePortal = true;

                Vector3 spawnPos = new Vector3(stageManager.curStagePos.x * stageManager.spacing, 2f, stageManager.curStagePos.y * stageManager.spacing);
                //Instantiate(stageManager.randomPortalPrefab, spawnPos, Quaternion.identity);
            }
            else if (stageManager.curStageType == StageType.Treasure)
            {
                stageManager.curStageCleared = true;
                stageManager.activePortal = true;

                if (isMonsterSpawn)
                {
                    Vector3 spawnPos = new Vector3(stageManager.curStagePos.x * stageManager.spacing, 2f, stageManager.curStagePos.y * stageManager.spacing);
                    Instantiate(stageManager.curStageSpawnPrefabs[Random.Range(0, stageManager.curStageSpawnPrefabs.Count)], spawnPos, Quaternion.identity, stageManager.transform);

                    isMonsterSpawn = false;
                }
            }
            else if (stageManager.curStageType == StageType.Boss)
            {
                if (isMonsterSpawn)
                {
                    SpawnGrid();
                } //���� ���� ����

                if (CurrentAliveMonsters.Count > 0)
                {
                    for (int i = 0; i < CurrentAliveMonsters.Count; i++)
                    {
                        if (CurrentAliveMonsters[i] == null)
                        {
                            CurrentAliveMonsters.RemoveAt(i);
                        }
                    }

                    stageManager.activePortal = false;
                } //���� ���� ���� Ȯ��
                else if (CurrentAliveMonsters.Count == 0)
                {
                    stageManager.activePortal = true;
                    stageManager.curStageCleared = true;
                }
            }
            else if (stageManager.curStageType == StageType.None)
            {
                stageManager.curStageCleared = true;
                stageManager.activePortal = true;
            }
            else if (stageManager.curStageType == StageType.BuffStone)
            {
                stageManager.curStageCleared = true;
                stageManager.activePortal = true;
            }
        }
        else
        {
            stageManager.activePortal = true;
        }
    }

    void HandleSpecialStageEntry()
    {
        var spawnPos = new Vector3(
            stageManager.curStagePos.x * stageManager.spacing,
            2f,
            stageManager.curStagePos.y * stageManager.spacing);

        switch (stageManager.curStageType)
        {
            case StageType.Bonfire:
                stageManager.curStageCleared = true;
                stageManager.activePortal = true;
                TrySpawnAt(spawnPos, 0);
                ApplyShelterRecovery();
                break;

            case StageType.Trap:
                stageManager.curStageCleared = true;
                stageManager.activePortal = true;
                spawnPos.y = 0f;
                TrySpawnRandom(spawnPos);
                break;

            case StageType.Treasure:
                stageManager.curStageCleared = true;
                stageManager.activePortal = true;
                TrySpawnRandom(spawnPos);
                break;

            case StageType.RandomPortal:
                stageManager.curStageCleared = true;
                break;

            case StageType.None:
                stageManager.curStageCleared = true;
                break;

            default:
                stageManager.activePortal = true;
                break;
        }

        isMonsterSpawn = false;
        EnsurePlayerCanMove();
    }

    void ApplyShelterRecovery()
    {
        Debug.Log("??????? ???");

        if (GameManager.instance != null)
            GameManager.instance.OnShelterEnter?.Invoke();

        var playerProfile = PlayerLocomotion.GetProfile(PlayerLocomotion.ResolvePlayerObject());
        if (playerProfile == null)
            return;

        playerProfile.MPBuff(4);

        if (GameManager.instance != null && !GameManager.instance.shelterHpBan)
            playerProfile.HPBuff(0.5f);

        if (GameManager.instance != null && !GameManager.instance.shelterActCountBan)
            playerProfile.ActCountPlus(3, GameManager.instance.recoveryMultiplier);
    }

    static void EnsurePlayerCanMove()
    {
        GameplayInputUtility.ReleaseUiFocus();

        var player = PlayerLocomotion.ResolvePlayerObject();
        var profile = PlayerLocomotion.GetProfile(player);
        profile?.ResetLocomotion();

        var body = player != null ? player.GetComponent<Rigidbody>() : null;
        body?.WakeUp();
    }

    bool TrySpawnAt(Vector3 spawnPos, int index)
    {
        var prefab = GetSpawnPrefab(index);
        if (prefab == null)
            return false;

        Instantiate(prefab, spawnPos, Quaternion.identity, stageManager.transform);
        return true;
    }

    bool TrySpawnRandom(Vector3 spawnPos)
    {
        var prefab = GetSpawnPrefab(-1);
        if (prefab == null)
            return false;

        Instantiate(prefab, spawnPos, Quaternion.identity, stageManager.transform);
        return true;
    }

    GameObject GetSpawnPrefab(int index)
    {
        var list = stageManager.curStageSpawnPrefabs;
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning(
                $"[{stageManager.curStageType}] ???? ?????? ????? ??? ??????. " +
                $"?? ??? {stageManager.curStagePos} ? PortalManager.SpawnPrefabs?? Inspector???? ????????.");
            return null;
        }

        GameObject prefab;
        if (index >= 0 && index < list.Count)
            prefab = list[index];
        else
            prefab = list[Random.Range(0, list.Count)];

        if (prefab != null)
            return prefab;

        Debug.LogWarning(
            $"[{stageManager.curStageType}] SpawnPrefabs ?????? ??? ??????. " +
            $"?? ??? {stageManager.curStagePos} ? Inspector???? ???????? ????????.");
        return null;
    }

    void SpawnGrid()
    {
        if (stageManager.curStageSpawnPrefabs == null || stageManager.curStageSpawnPrefabs.Count == 0)
        {
            Debug.LogWarning("??? ?? ???? ???? ???????? ???????. PortalManager.SpawnPrefabs?? ????????.");
            isMonsterSpawn = false;
            EnsurePlayerCanMove();
            return;
        }

        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(stageManager.curStageSpawnPrefabs.Count));
        float spacing = 2f;
        int count = 0;

        isMonsterSpawn = false;

        for (int i = 0; i < stageManager.curStageSpawnPrefabs.Count; i++)
        {
            var prefab = stageManager.curStageSpawnPrefabs[i];
            if (prefab == null)
            {
                Debug.LogWarning($"???? ???? ???? [{i}]?? ??? ??????.");
                continue;
            }

            for (int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    if (count >= stageManager.curStageSpawnPrefabs.Count)
                        return;

                    //var spawnPos = new Vector3(
                    //    stageManager.curStagePos.x * stageManager.spacing,
                    //    2f,
                    //    stageManager.curStagePos.y * stageManager.spacing);

                    Debug.Log(spawnPos);
                    var monster = Instantiate(prefab, spawnPos, Quaternion.identity);
                    CurrentAliveMonsters.Add(monster);

                    var offset = new Vector3(x * spacing, 0f, z * spacing);
                    monster.transform.position += offset;

                    count++;
                }
            }
        }

        EnsurePlayerCanMove();
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

        var map = GameObject.FindGameObjectWithTag("Map");
        if (map == null)
            return;

        Instantiate(dropItems[0], enemy.transform.position, Quaternion.identity, map.transform);
    }
}
