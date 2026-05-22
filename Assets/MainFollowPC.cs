using UnityEngine;

public class MainFollowPC : MonoBehaviour
{
    [SerializeField] Transform playerCameraTransform;

    public void BindPlayerCamera(Transform playerCamera)
    {
        playerCameraTransform = playerCamera;
        enabled = playerCameraTransform != null;
    }

    void Awake()
    {
        if (playerCameraTransform == null)
        {
            var playerCamera = GameObject.Find("PlayerCamera");
            if (playerCamera != null)
                playerCameraTransform = playerCamera.transform;
        }

        enabled = playerCameraTransform != null;
    }

    void LateUpdate()
    {
        if (playerCameraTransform == null)
            return;

        transform.SetPositionAndRotation(
            playerCameraTransform.position,
            playerCameraTransform.rotation);
    }
}
