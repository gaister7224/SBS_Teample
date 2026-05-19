using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PortalSystem : MonoBehaviour
{
    [SerializeField] PortalManager portalManager;
    StageManager stageManager;

    GameObject player;

    //0:��, 1:��, 2:��, 3:��
    [SerializeField] PortalDirection direction;

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

            if (direction != PortalDirection.Random && direction != PortalDirection.Clear)
            {
                if (other.GetComponent<PlayerProfile>().ActCount > 0)
                {
                    StartCoroutine(Teleport());
                }
            }
            else if (direction == PortalDirection.Random)
            {
                int randomIndex = Random.Range(1, stageManager.StagePositions.Count);
                Vector2 randomPos = stageManager.StagePositions.ElementAt(randomIndex);

                player.transform.position = new Vector3(randomPos.x * stageManager.spacing, 1.9f, randomPos.y * stageManager.spacing - 9f);
                player.GetComponent<PlayerProfile>()?.ResetLocomotion();

                GameManager.instance.OnRandomPortalEnter?.Invoke();
                //������Ż
            }
            else if (direction == PortalDirection.Return)
            {

            }
            else if (portalManager.PlayerObject.GetComponent<PlayerProfile>().ActCount <=
                portalManager.PlayerObject.GetComponent<PlayerProfile>().actCountMin)
            {
                //���Ʈ����
            }
        }
        
    }

    IEnumerator Teleport()
    {
        //Image img = UIManager.Instance.fade.GetComponent<Image>();
        //img.gameObject.SetActive(true);
        player.transform.position = portalManager.PlayerTpSpotTransform.position;
        portalManager.MainCameraObject.transform.position = portalManager.MainCameraTpSpotTransform.position;
        portalManager.isCleared = true;

        switch (direction)
        {
            case PortalDirection.Front:
                player.transform.position += new Vector3(0f, 0f, StageManager.instance.spacing - 9f);
                portalManager.MainCameraObject.transform.position += new Vector3(0f, 0f, StageManager.instance.spacing - 9f);
                break;
            case PortalDirection.Back:
                player.transform.position += new Vector3(0f, 0f, -StageManager.instance.spacing + 9f);
                portalManager.MainCameraObject.transform.position += new Vector3(0f, 0f, -StageManager.instance.spacing + 9f);
                break;
            case PortalDirection.Left:
                player.transform.position += new Vector3(-StageManager.instance.spacing + 9f, 0f, 0f);
                portalManager.MainCameraObject.transform.position += new Vector3(-StageManager.instance.spacing + 9f, 0f, 0f);
                break;
            case PortalDirection.Right:
                player.transform.position += new Vector3(StageManager.instance.spacing - 9f, 0f, 0f);
                portalManager.MainCameraObject.transform.position += new Vector3(StageManager.instance.spacing - 9f, 0f, 0f);
                break;
            case PortalDirection.Clear:
                //player.transform.position += new Vector3(StageManager.instance.spacing - 9f, 0f, 0f);
                //portalManager.MainCameraObject.transform.position += new Vector3(StageManager.instance.spacing - 9f, 0f, 0f);
                break;
        }

        var profile = player.GetComponent<PlayerProfile>();
        profile?.UseActCount(1);
        profile?.ResetLocomotion();
        GameplayInputUtility.ReleaseUiFocus();
        GameManager.instance.OnPortalEnter?.Invoke();
        yield return null;
    }
}
