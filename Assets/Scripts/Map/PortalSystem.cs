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
    float distance = 12f;

    void Awake()
    {
        if (portalManager == null)
            portalManager = GetComponentInParent<PortalManager>();

        if (stageManager == null)
            stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
    }

    void Start()
    {
    }

    void Update()
    {
        if (portalManager == null)
            portalManager = GetComponentInParent<PortalManager>();

        if (stageManager == null)
            stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (portalManager == null)
            portalManager = GetComponentInParent<PortalManager>();
        if (stageManager == null)
            stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();

        player = other.gameObject;
        if (other.GetComponent<PlayerProfile>().ActCount > 0)
        {
            StartCoroutine(Teleport());
        }
    }

    IEnumerator Teleport()
    {
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
                var bossRoom = GameObject.FindWithTag("BossRoom");
                if (bossRoom != null)
                {
                    player.transform.position = bossRoom.transform.position + new Vector3(0f, 1.9f, -distance);
                    player.GetComponent<PlayerProfile>().UseActCount(1);
                }
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
                int half = stageManager.StageCount / 2;
                var returnWorld = stageManager.GridToWorld(new Vector2Int(-half, -half), 1.9f);
                player.transform.position = returnWorld;
                break;
            case PortalDirection.Villiage:
                player.GetComponent<PlayerProfile>().PlayerMoveToVillage();
                break;
        }

        GameManager.instance.OnPortalEnter?.Invoke();
        stageManager.SyncPlayerToMinimap();

        yield return null;
    }
}