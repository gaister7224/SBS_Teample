using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 우측 상단 미니맵은 항상 표시, MapBoard F키로 화면 중앙 대형 미니맵을 토글합니다.
/// </summary>
public class VillageMinimapUI : MonoBehaviour
{
    public static VillageMinimapUI Instance { get; private set; }

    [SerializeField] private GameObject largeMapPanel;

    private bool isLargeMapOpen;

    private void Awake()
    {
        Instance = this;

        if (largeMapPanel != null)
            largeMapPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isLargeMapOpen)
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseLargeMap();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ToggleLargeMap()
    {
        isLargeMapOpen = !isLargeMapOpen;

        if (largeMapPanel != null)
            largeMapPanel.SetActive(isLargeMapOpen);
    }

    public void CloseLargeMap()
    {
        isLargeMapOpen = false;

        if (largeMapPanel != null)
            largeMapPanel.SetActive(false);
    }
}
