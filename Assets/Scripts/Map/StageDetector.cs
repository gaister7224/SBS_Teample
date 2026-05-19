using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class StageDetector : MonoBehaviour
{
    [SerializeField] PortalManager portalManager;
    [SerializeField] StageManager stageManager;

    private void Awake()
    {
        portalManager = GetComponentInParent<PortalManager>();
        stageManager = StageManager.instance;
        if (stageManager == null)
        {
            Debug.Log("awake : stageManager ����");
        }
    }

    void Start()
    {
        
        

        //StartCoroutine(StageChangeCoroutine());
    }

    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log(stageManager);
            //StartCoroutine(StageChangeCoroutine());
            if (portalManager == null)
            {
                Debug.Log("potalManager ����");
            }
            if(portalManager.CinemachineCamera == null)
            {
                Debug.Log("potalManager.CinemachineCamera ����");
            }
            if (portalManager.CinemachineCamera != null)
            {
                var confinder = portalManager.CinemachineCamera.GetComponent<CinemachineConfiner3D>();
                if (confinder != null)
                    confinder.BoundingVolume = gameObject.GetComponent<Collider>();
            }

            stageManager.curStagePos = new Vector2Int((int)(transform.position.x / stageManager.spacing), (int)(transform.position.z / stageManager.spacing));
            stageManager.curStageType = portalManager.stageType;
            if (stageManager.monsterSpawnManager != null)
                stageManager.monsterSpawnManager.isMonsterSpawn = true;
            stageManager.activePortal = false;
            stageManager.curStageCleared = portalManager.isCleared;
            stageManager.curStageSpawnPrefabs = portalManager.SpawnPrefabs;
            stageManager.surroundStagePositions.Clear();
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

            other.GetComponent<PlayerProfile>()?.ResetLocomotion();
            NotifyDungeonMap(stageManager);
        }
    }

    static void NotifyDungeonMap(StageManager stageManager)
    {
        if (DungeonMapService.Instance == null)
        {
            var serviceObject = new GameObject("DungeonMapService");
            serviceObject.AddComponent<DungeonMapService>();
        }

        DungeonMapService.Instance.EnsureLoadedForCurrentDungeon();

        DungeonMapService.Instance.RevealAround(
            stageManager.curStagePos,
            1,
            stageManager.StagePositions);
        DungeonMapService.Instance.SetPlayerPosition(stageManager.curStagePos);
    }

    IEnumerator StageChangeCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        portalManager.PortalEffectImage.gameObject.SetActive(true);
        //yield return new WaitForSeconds(0.5f);
        Color c = Color.black;
        //Time.timeScale = 0;
        for (float i = 1; i > 0; i -= Time.unscaledDeltaTime)
        {
            c.a = i;
            portalManager.PortalEffectImage.color = c;
            yield return null;
        }
        //Time.timeScale = 1;
        portalManager.PortalEffectImage.gameObject.SetActive(false);
    }
}
