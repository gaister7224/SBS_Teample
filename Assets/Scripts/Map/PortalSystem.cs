using System.Collections;
using System.Linq;
using UnityEngine;

public class PortalSystem : MonoBehaviour
{
    [SerializeField] PortalManager portalManager;
    StageManager stageManager;

    GameObject player;
    PlayerProfile playerProfile;

    [SerializeField] PortalDirection direction;

    void Update()
    {
        if (portalManager == null)
            portalManager = GetComponentInParent<PortalManager>();

        if (stageManager == null && StageManager.instance != null)
            stageManager = StageManager.instance;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        player = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.transform.root.gameObject;

        playerProfile = PlayerLocomotion.GetProfile(player);
        if (playerProfile == null)
            return;

        if (direction != PortalDirection.Random && direction != PortalDirection.Clear)
        {
            if (playerProfile.ActCount > 0)
                StartCoroutine(Teleport());
        }
        else if (direction == PortalDirection.Random)
        {
            int randomIndex = Random.Range(1, stageManager.StagePositions.Count);
            Vector2 randomPos = stageManager.StagePositions.ElementAt(randomIndex);

            var target = new Vector3(
                randomPos.x * stageManager.spacing,
                1.9f,
                randomPos.y * stageManager.spacing - 9f);

            PlayerLocomotion.Teleport(player.transform, target);
            playerProfile.ResetLocomotion();
            GameplayInputUtility.ReleaseUiFocus();
            GameManager.instance.OnRandomPortalEnter?.Invoke();
        }
    }

    IEnumerator Teleport()
    {
        PlayerLocomotion.Teleport(player.transform, portalManager.PlayerTpSpotTransform.position);
        portalManager.MainCameraObject.transform.position = portalManager.MainCameraTpSpotTransform.position;
        portalManager.isCleared = true;

        switch (direction)
        {
            case PortalDirection.Front:
                PlayerLocomotion.TeleportOffset(player.transform, new Vector3(0f, 0f, StageManager.instance.spacing - 9f));
                portalManager.MainCameraObject.transform.position += new Vector3(0f, 0f, StageManager.instance.spacing - 9f);
                break;
            case PortalDirection.Back:
                PlayerLocomotion.TeleportOffset(player.transform, new Vector3(0f, 0f, -StageManager.instance.spacing + 9f));
                portalManager.MainCameraObject.transform.position += new Vector3(0f, 0f, -StageManager.instance.spacing + 9f);
                break;
            case PortalDirection.Left:
                PlayerLocomotion.TeleportOffset(player.transform, new Vector3(-StageManager.instance.spacing + 9f, 0f, 0f));
                portalManager.MainCameraObject.transform.position += new Vector3(-StageManager.instance.spacing + 9f, 0f, 0f);
                break;
            case PortalDirection.Right:
                PlayerLocomotion.TeleportOffset(player.transform, new Vector3(StageManager.instance.spacing - 9f, 0f, 0f));
                portalManager.MainCameraObject.transform.position += new Vector3(StageManager.instance.spacing - 9f, 0f, 0f);
                break;
        }

        playerProfile.UseActCount(1);
        playerProfile.ResetLocomotion();
        GameplayInputUtility.ReleaseUiFocus();
        GameManager.instance.OnPortalEnter?.Invoke();
        yield return null;
    }
}
