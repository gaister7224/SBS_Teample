using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class StageDetector : MonoBehaviour
{
    [SerializeField] PortalManager portalManager;
    [SerializeField] StageManager stageManager;

    private MonsterSpawnManager monsterSpawnManager;

    private void Awake()
    {
        // GameObject.Find("StageManager") 는 Instantiate 직후 Awake가 실행될 때
        // 씬에서 오브젝트를 못 찾으면 NullReferenceException 이 납니다.
        // StageManager.instance 는 StageManager.Awake() 에서 이미 할당되어 있으므로
        // 이쪽을 사용하는 것이 안전합니다.
        stageManager = StageManager.instance;

        if (stageManager == null)
        {
            Debug.LogError("[StageDetector] StageManager.instance 가 null입니다. " +
                           "StageManager가 StageDetector보다 먼저 Awake되는지 확인하세요.");
            return;
        }

        portalManager = GetComponentInParent<PortalManager>();
        monsterSpawnManager = stageManager.GetComponentInChildren<MonsterSpawnManager>();
    }

    void Start()
    {
        //StartCoroutine(StageChangeCoroutine());
    }

    void Update()
    {
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        StartCoroutine(StageChangeCoroutine());
    //        var confinder = portalManager.CinemachineCamera.GetComponent<CinemachineConfiner3D>();

    //        confinder.BoundingVolume = gameObject.GetComponent<Collider>();

    //        stageManager.curStagePos = new Vector2Int((int)(transform.position.x / stageManager.spacing), (int)(transform.position.z / stageManager.spacing));
    //        stageManager.curStageType = portalManager.stageType;
    //        if (stageManager.monsterSpawnManager != null)
    //            stageManager.monsterSpawnManager.isMonsterSpawn = true;
    //        //stageManager.activePortal = false;
    //        stageManager.curStageCleared = portalManager.isCleared;
    //        stageManager.curStageSpawnPrefabs = portalManager.SpawnPrefabs != null
    //            ? new List<GameObject>(portalManager.SpawnPrefabs)
    //            : new List<GameObject>();
    //        stageManager.surroundStagePositions.Clear();
    //        for (int i = 0; i < 9; i++)
    //        {
    //            int x = stageManager.curStagePos.x + (i % 3 - 1);
    //            int z = stageManager.curStagePos.y + (i / 3 - 1);
    //            Vector2Int pos = new Vector2Int(x, z);
    //            if (stageManager.StagePositions.Contains(pos))
    //            {
    //                stageManager.surroundStagePositions.Add(pos);
    //            }
    //        }

    //        PlayerLocomotion.GetProfile(other)?.ResetLocomotion();
    //        NotifyDungeonMap(stageManager);
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Awake 에서 stageManager 가 null 이었을 경우 재시도
        if (stageManager == null)
        {
            stageManager = StageManager.instance;
            if (stageManager == null)
            {
                Debug.LogError("[StageDetector] OnTriggerEnter: StageManager를 찾을 수 없습니다.");
                return;
            }
            monsterSpawnManager = stageManager.GetComponentInChildren<MonsterSpawnManager>();
        }

        if (portalManager == null)
        {
            Debug.LogError("[StageDetector] OnTriggerEnter: PortalManager가 null입니다.");
            return;
        }

        if (monsterSpawnManager != null)
        {
            monsterSpawnManager.spawnPos = new Vector3(transform.position.x, 2f, transform.position.z);
        }
        else
        {
            Debug.LogError("MonsterSpawnManager null");
        }

        StartCoroutine(StageChangeCoroutine());

        var confinder = portalManager.CinemachineCamera.GetComponent<CinemachineConfiner3D>();
        confinder.BoundingVolume = gameObject.GetComponent<Collider>();

        stageManager.EnsureStagePositions();
        var stageRoot = portalManager.ThisStage != null
            ? portalManager.ThisStage.transform
            : portalManager.transform;
        stageManager.curStagePos = stageManager.WorldToGrid(stageRoot.position);
        stageManager.curStageType = portalManager.stageType;
        if (stageManager.monsterSpawnManager != null)
            stageManager.monsterSpawnManager.isMonsterSpawn = true;
        stageManager.activePortal = false;
        stageManager.curStageCleared = portalManager.isCleared;
        stageManager.curStageSpawnPrefabs = portalManager.SpawnPrefabs;
        stageManager.surroundStagePositions.Clear();
        stageManager.curStageType = portalManager.stageType;

        for (int i = 0; i < 9; i++)
        {
            int x = stageManager.curStagePos.x + (i % 3 - 1);
            int z = stageManager.curStagePos.y + (i / 3 - 1);
            Vector2Int pos = new Vector2Int(x, z);
            if (stageManager.StagePositions.Contains(pos))
            {
                stageManager.surroundStagePositions.Add(pos);
            }
        }

        PlayerLocomotion.GetProfile(other)?.ResetLocomotion();
        NotifyDungeonMap(stageManager);
    }

    static void NotifyDungeonMap(StageManager stageManager)
    {
        stageManager.SyncPlayerToMinimap(stageManager.curStagePos);
    }

    IEnumerator StageChangeCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        portalManager.PortalEffectImage.gameObject.SetActive(true);
        Color c = Color.black;
        for (float i = 1; i > 0; i -= Time.unscaledDeltaTime)
        {
            c.a = i;
            portalManager.PortalEffectImage.color = c;
            yield return null;
        }
        portalManager.PortalEffectImage.gameObject.SetActive(false);
    }
}