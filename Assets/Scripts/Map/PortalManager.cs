using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public enum WallDirection
{
    North,
    South,
    East,
    West
}

public class PortalManager : MonoBehaviour
{
    public StageType stageType;
    public List<GameObject> SpawnPrefabs = new List<GameObject>();
    public bool isCleared;
    [Space(10f), HideInInspector]
    public bool isPortalActive;
    [Space(10f)]
    public List<GameObject> PortalObject = new List<GameObject>();
    [Space(10f)]
    public Transform PlayerTpSpotTransform;
    public Transform MainCameraTpSpotTransform;
    [Space(10f)]
    public Dictionary<WallDirection, GameObject> NearStage = new Dictionary<WallDirection, GameObject>();

    StageManager stageManager;
    [HideInInspector] public GameObject ThisStage;
    [HideInInspector] public GameObject PlayerObject;
    [HideInInspector] public GameObject MainCameraObject;
    [HideInInspector] public Image PortalEffectImage;
    [HideInInspector] public CinemachineCamera CinemachineCamera;
    [HideInInspector] public Transform PlayerTransform;

    void Awake()
    {
        ThisStage = transform.parent != null ? transform.parent.gameObject : gameObject;

        stageManager = StageManager.instance;

        if (stageManager == null)
        {
            stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        }

        PlayerObject = GameObject.FindGameObjectWithTag("Player");
        MainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");

        var playerCameraObj = GameObject.Find("PlayerCamera");
        if (playerCameraObj != null)
            CinemachineCamera = playerCameraObj.GetComponent<CinemachineCamera>();
    }

    void Start()
    {
        if (PlayerObject != null)
            PlayerTransform = PlayerObject.GetComponent<Transform>();
        else
            PlayerObject = GameObject.FindGameObjectWithTag("Player");

        PortalEffectImage = UIManager.Instance.fade.GetComponent<Image>();

        if (MainCameraObject == null)
            MainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");

        if (stageType == StageType.Normal || stageType == StageType.Boss || stageType == StageType.SealedStone)
        {
            isPortalActive = false;
            isCleared = false;
        }
        else
        {
            isPortalActive = true;
        }
    }

    void Update()
    {

        PortalActivation();

        isPortalActive = stageManager.activePortal;

        if (!stageManager.Tutorial)
        {
            SurroundStageCheck();
            BossPortalCheck();
        }

        for (int i = 0; i < PortalObject.Count; i++)
        {
            if (PortalObject[i] == null)
                continue;

            var portalSystem = PortalObject[i].GetComponent<PortalSystem>();
            if (portalSystem == null)
                continue;

            if (portalSystem.toBoss)
            {
                PortalObject[i].SetActive(stageManager.SealedStoneLeft == 0 && stageManager.curStageCleared);
            }
        }
    }

    void BossPortalCheck()
    {
        if (PortalObject[0] != null && stageManager.curStagePos.y == -2 && (stageManager.curStagePos.x >= -1 && stageManager.curStagePos.x <= 1))
            PortalObject[0].GetComponent<PortalSystem>().toBoss = true;

        if (PortalObject[1] != null && stageManager.curStagePos.y == 2 && (stageManager.curStagePos.x >= -1 && stageManager.curStagePos.x <= 1))
            PortalObject[1].GetComponent<PortalSystem>().toBoss = true;

        if (PortalObject[2] != null && stageManager.curStagePos.x == 2 && (stageManager.curStagePos.y >= -1 && stageManager.curStagePos.y <= 1))
            PortalObject[2].GetComponent<PortalSystem>().toBoss = true;

        if (PortalObject[3] != null && stageManager.curStagePos.x == -2 && (stageManager.curStagePos.y >= -1 && stageManager.curStagePos.y <= 1))
            PortalObject[3].GetComponent<PortalSystem>().toBoss = true;
    }

    void PortalActivation()
    {
        Vector2Int pos = stageManager.WorldToGrid(ThisStage.transform.position);

        bool north = stageManager.StagePositions.Contains(new Vector2Int(pos.x, pos.y + 1));
        bool south = stageManager.StagePositions.Contains(new Vector2Int(pos.x, pos.y - 1));
        bool west = stageManager.StagePositions.Contains(new Vector2Int(pos.x - 1, pos.y));
        bool east = stageManager.StagePositions.Contains(new Vector2Int(pos.x + 1, pos.y));

        if (PortalObject[0] != null && !PortalObject[0].GetComponent<PortalSystem>().toBoss)
        {
            PortalObject[0].SetActive(north && isPortalActive);
            PortalObject[0].SetActive(isPortalActive);
        }
        if (PortalObject[1] != null && !PortalObject[1].GetComponent<PortalSystem>().toBoss)
        {
            PortalObject[1].SetActive(south && isPortalActive);
            PortalObject[1].SetActive(isPortalActive);
        }
        if (PortalObject[2] != null && !PortalObject[2].GetComponent<PortalSystem>().toBoss)
        {
            PortalObject[2].SetActive(west && isPortalActive);
            PortalObject[2].SetActive(isPortalActive);
        }
        if (PortalObject[3] != null && !PortalObject[3].GetComponent<PortalSystem>().toBoss)
        {
            PortalObject[3].SetActive(east && isPortalActive);
            PortalObject[3].SetActive(isPortalActive);
        }
    }

    void SurroundStageCheck()
    {
        Vector2Int stageV2I = stageManager.WorldToGrid(ThisStage.transform.position);
        Vector2Int frontStageV2I = new Vector2Int(stageV2I.x, stageV2I.y + 1);
        Vector2Int backStageV2I = new Vector2Int(stageV2I.x, stageV2I.y - 1);
        Vector2Int leftStageV2I = new Vector2Int(stageV2I.x - 1, stageV2I.y);
        Vector2Int rightStageV2I = new Vector2Int(stageV2I.x + 1, stageV2I.y);

        if (PortalObject[0] != null && !stageManager.surroundStagePositions.Contains(frontStageV2I) && stageType != StageType.Boss)
            PortalObject[0].SetActive(false);

        if (PortalObject[1] != null && !stageManager.surroundStagePositions.Contains(backStageV2I) && stageType != StageType.Boss)
            PortalObject[1].SetActive(false);

        if (PortalObject[2] != null && !stageManager.surroundStagePositions.Contains(leftStageV2I) && stageType != StageType.Boss)
            PortalObject[2].SetActive(false);

        if (PortalObject[3] != null && !stageManager.surroundStagePositions.Contains(rightStageV2I) && stageType != StageType.Boss)
            PortalObject[3].SetActive(false);
    }
}