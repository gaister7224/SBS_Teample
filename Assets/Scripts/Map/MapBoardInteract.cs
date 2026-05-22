using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// MapBoard 트리거 근처에서 F키를 누르면 화면 중앙 대형 미니맵을 토글합니다.
/// </summary>
public class MapBoardInteract : MonoBehaviour
{
    [SerializeField] private GameObject fKeyHint;

    private bool playerInRange;

    private void Awake()
    {
        if (fKeyHint == null && transform.childCount > 0)
            fKeyHint = transform.GetChild(0).gameObject;

        if (fKeyHint != null)
            fKeyHint.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (VillageMinimapUI.Instance != null)
                VillageMinimapUI.Instance.ToggleLargeMap();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        if (fKeyHint != null)
            fKeyHint.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        if (fKeyHint != null)
            fKeyHint.SetActive(false);
    }
}
