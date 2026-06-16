using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// MapBoard 트리거 근처에서 F키를 누르면 화면 중앙 대형 미니맵을 토글합니다.
/// </summary>
public class MapBoardInteract : MonoBehaviour
{
    [SerializeField] private GameObject fKeyHint;
    [SerializeField] private MapBoardPanelView mapBoardPanel;

    private bool playerInRange;
    private InventoryMain inventory;

    private void Awake()
    {
        if (fKeyHint == null && transform.childCount > 0)
            fKeyHint = transform.GetChild(0).gameObject;

        if (fKeyHint != null)
            fKeyHint.SetActive(false);

        if (mapBoardPanel == null)
            mapBoardPanel = Object.FindAnyObjectByType<MapBoardPanelView>(FindObjectsInactive.Include);
    }

    private void Start()
    {
        inventory = GameObject.Find("InventorySystem").GetComponent<InventoryMain>();
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            // 💡 만약 지도 게시판 UI가 이미 켜져 있다면 F키로 닫을 수 있게 예외 처리
            if (mapBoardPanel != null && mapBoardPanel.gameObject.activeSelf)
            {
                CloseMapBoardPanel();
                return;
            }

            // 기존 인벤토리 및 플레이어 상태 제어 코드 유지
            if (inventory != null)
            {
                inventory.playerProfile.SetActive(false);
                inventory.playerAttack.uiClicking = true;
            }

            Time.timeScale = 0f;

            // 기존 마을 큰 지도 토글 코드 유지
            //var villageUi = VillageMinimapUI.Instance
            //    ?? Object.FindAnyObjectByType<VillageMinimapUI>();
            //villageUi?.ToggleLargeMap();

            if (mapBoardPanel != null)
            {
                mapBoardPanel.OpenMapBoard();
            }
            else
            {
                Debug.LogError("[MapBoardInteract] MapBoardPanelView(지도 게시판 UI)를 찾을 수 없습니다! 인스펙터 슬롯을 확인하세요.");
            }
        }
    }

    private void CloseMapBoardPanel()
    {
        if (mapBoardPanel != null) mapBoardPanel.CloseMapBoard();
        if (inventory != null)
        {
            inventory.playerAttack.uiClicking = false;
            inventory.playerProfile.SetActive(true);
        }
        Time.timeScale = 1f;
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
