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

    void Start()
    {
    }

    void Update()
    {
        if (portalManager == null)
        {
            portalManager = GetComponentInParent<PortalManager>();
        }

        if (stageManager == null && StageManager.instance != null)
        {
            stageManager = StageManager.instance;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            if (other.GetComponent<PlayerProfile>().ActCount > 0)
            {
                StartCoroutine(Teleport());
            }
            else if (portalManager.PlayerObject.GetComponent<PlayerProfile>().ActCount <= 0)
            {
            }
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
                player.transform.position += new Vector3(0f, 0f, StageManager.instance.spacing - distance);
                player.GetComponent<PlayerProfile>().UseActCount(1);
                break;
            case PortalDirection.Back:
                player.transform.position += new Vector3(0f, 0f, -StageManager.instance.spacing + distance);
                player.GetComponent<PlayerProfile>().UseActCount(1);
                break;
            case PortalDirection.Left:
                player.transform.position += new Vector3(-StageManager.instance.spacing + distance, 0f, 0f);
                player.GetComponent<PlayerProfile>().UseActCount(1);
                break;
            case PortalDirection.Right:
                player.transform.position += new Vector3(StageManager.instance.spacing - distance, 0f, 0f);
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

                player.transform.position = new Vector3(randomPos.x * stageManager.spacing, 1.9f, randomPos.y * stageManager.spacing - distance);
                //������Ż
                break;
            case PortalDirection.Clear:
                //Debug.Log(stageManager);
                //stageManager.curFloorCleared = true;
                ////Ŭ������Ż
                //if (!stageManager.Tutorial)
                //{
                //    int countHalf = (stageManager.StageCount % 2 == 1) ? stageManager.StageCount / 2 + 1 : stageManager.StageCount / 2;
                //    player.transform.position = new Vector3(-countHalf * stageManager.spacing, 1.9f, -countHalf * stageManager.spacing);
                //}
                //else
                //{
                //    player.transform.position = new Vector3(0f, 1.9f, -distance);
                //}
                break;
            case PortalDirection.Return:
                Vector3 returnPos = new Vector3(stageManager.StagePositions.ElementAt(0).x * stageManager.spacing, 1.9f, stageManager.StagePositions.ElementAt(0).y * stageManager.spacing);
                player.transform.position = returnPos;
                break;
        }

        GameManager.instance.OnPortalEnter?.Invoke();
        yield return null;
    }
}
