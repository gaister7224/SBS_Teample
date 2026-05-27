using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PortalSystem : MonoBehaviour
{
    [SerializeField] PortalManager portalManager;
    StageManager stageManager;

    GameObject player;

    [SerializeField] PortalDirection direction;

    public bool toBoss;
    public float distance = 9f;

    void Awake()
    {
        // Update 지연 할당 대신 Awake 에서 즉시 확보합니다.
        if (portalManager == null)
            portalManager = GetComponentInParent<PortalManager>();

        if (stageManager == null)
            stageManager = StageManager.instance;
    }

    void Start()
    {
    }

    void Update()
    {
        // Awake 시점에 못 찾았을 경우 재시도
        if (portalManager == null)
            portalManager = GetComponentInParent<PortalManager>();

        if (stageManager == null && StageManager.instance != null)
            stageManager = StageManager.instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // 진입 직전에 null 재확인
        if (portalManager == null)
            portalManager = GetComponentInParent<PortalManager>();
        if (stageManager == null)
            stageManager = StageManager.instance;

        if (portalManager == null)
        {
            Debug.LogError("[PortalSystem] OnTriggerEnter: PortalManager를 찾을 수 없습니다.");
            return;
        }

        player = other.gameObject;
        if (other.GetComponent<PlayerProfile>().ActCount > 0)
        {
            StartCoroutine(Teleport());
        }
    }

    IEnumerator Teleport()
    {
        // Teleport 시작 시점에도 null 재확인
        if (portalManager == null)
            portalManager = GetComponentInParent<PortalManager>();
        if (stageManager == null)
            stageManager = StageManager.instance;

        if (portalManager == null)
        {
            Debug.LogError("[PortalSystem] Teleport: PortalManager가 null입니다.");
            yield break;
        }

        Image img = UIManager.Instance.fade.GetComponent<Image>();
        img.gameObject.SetActive(true);
        player.transform.position = portalManager.PlayerTpSpotTransform.position;
        portalManager.MainCameraObject.transform.position = portalManager.MainCameraTpSpotTransform.position;
        portalManager.isCleared = true;

        switch (direction)
        {
            case PortalDirection.Front:
                player.transform.position += new Vector3(0f, 0f, stageManager.spacing - distance);
                player.GetComponent<PlayerProfile>().UseActCount(1);
                break;
            case PortalDirection.Back:
                player.transform.position += new Vector3(0f, 0f, -stageManager.spacing + distance);
                player.GetComponent<PlayerProfile>().UseActCount(1);
                break;
            case PortalDirection.Left:
                player.transform.position += new Vector3(-stageManager.spacing + distance, 0f, 0f);
                player.GetComponent<PlayerProfile>().UseActCount(1);
                break;
            case PortalDirection.Right:
                player.transform.position += new Vector3(stageManager.spacing - distance, 0f, 0f);
                player.GetComponent<PlayerProfile>().UseActCount(1);
                break;
            case PortalDirection.toBoss:
                player.transform.position = GameObject.FindWithTag("BossRoom").transform.position + new Vector3(0f, 1.9f, -distance);
                player.GetComponent<PlayerProfile>().UseActCount(1);
                break;
            case PortalDirection.Random:
                int randomIndex;
                Vector2 randomPos;
                do
                {
                    randomIndex = Random.Range(0, stageManager.StagePositions.Count);
                    randomPos = stageManager.StagePositions.ElementAt(randomIndex);
                } while (randomPos.x == 0 && randomPos.y == 0);

                var randomGrid = new Vector2Int(Mathf.RoundToInt(randomPos.x), Mathf.RoundToInt(randomPos.y));
                var randomWorld = stageManager.GridToWorld(randomGrid, 1.9f);
                randomWorld.z -= distance;
                player.transform.position = randomWorld;
                break;
            case PortalDirection.Clear:
                if (stageManager.Tutorial)
                    yield return stageManager.AdvanceTutorialFloorRoutine();
                else
                    stageManager.curFloorCleared = true;
                break;
            case PortalDirection.Return:
                var returnGrid = stageManager.StagePositions.ElementAt(0);
                Vector3 returnPos = stageManager.GridToWorld(returnGrid, 1.9f);
                player.transform.position = returnPos;
                break;
        }

        GameManager.instance.OnPortalEnter?.Invoke();
        stageManager.SyncPlayerToMinimap();

        yield return null;
    }
}