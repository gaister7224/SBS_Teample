using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PortalSystem : MonoBehaviour
{
    [SerializeField] PortalManager portalManager;
    StageManager stageManager;

    GameObject player;

    //0:¾Õ, 1:µÚ, 2:¿Þ, 3:¿À
    [SerializeField] PortalDirection direction;

    public float distance = 9f;
    public bool toBoss;

    void Start()
    {
        if (portalManager == null)
        {
            portalManager = GetComponentInParent<PortalManager>();
        }

        if (stageManager == null)
        {
            stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        }
    }

    void Update()
    {
        if (toBoss)
        {
            direction = PortalDirection.toBoss;
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
                //»ç¸ÁÆ®¸®°Å
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
                break;
            case PortalDirection.Back:
                player.transform.position += new Vector3(0f, 0f, -StageManager.instance.spacing + distance);
                break;
            case PortalDirection.Left:
                player.transform.position += new Vector3(-StageManager.instance.spacing + distance, 0f, 0f);
                break;
            case PortalDirection.Right:
                player.transform.position += new Vector3(StageManager.instance.spacing - distance, 0f, 0f);
                break;
            case PortalDirection.toBoss :
                player.transform.position = GameObject.FindWithTag("BossRoom").transform.position + new Vector3(0f, 1.9f, -distance);
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
                //·£´ýÆ÷Å»
                break;
            case PortalDirection.Clear:
                Debug.Log(stageManager);
                stageManager.curFloorCleared = true;
                //Å¬¸®¾îÆ÷Å»
                if (!stageManager.Tutorial)
                {
                    int countHalf = (stageManager.StageCount % 2 == 1) ? stageManager.StageCount / 2 + 1 : stageManager.StageCount / 2;
                    player.transform.position = new Vector3(-countHalf * stageManager.spacing, 1.9f, -countHalf * stageManager.spacing);
                }
                else
                {
                    player.transform.position = new Vector3(0f, 1.9f, -distance);
                }
                break;
        }

        player.GetComponent<PlayerProfile>().UseActCount(1);
        yield return null;
    }
}
