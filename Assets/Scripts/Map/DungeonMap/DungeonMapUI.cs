using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 던전에서 M키로 지도 게시판(대형 지도 + 마킹) UI를 엽니다.
/// </summary>
public class DungeonMapUI : MonoBehaviour
{
    public static DungeonMapUI Instance { get; private set; }

    [SerializeField] MapBoardPanelView mapBoardPanel;

    public static DungeonMapUI EnsureSingleInstance(Transform parent)
    {
        if (Instance != null)
            return Instance;

        var existing = Object.FindAnyObjectByType<DungeonMapUI>();
        if (existing != null)
        {
            Instance = existing;
            return existing;
        }

        var mapUiObject = new GameObject("DungeonMapUI");
        if (parent != null)
            mapUiObject.transform.SetParent(parent, false);

        return mapUiObject.AddComponent<DungeonMapUI>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (mapBoardPanel == null)
            mapBoardPanel = ResolveDungeonMapBoardPanel();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    static MapBoardPanelView ResolveDungeonMapBoardPanel()
    {
        foreach (var panel in Object.FindObjectsByType<MapBoardPanelView>(FindObjectsSortMode.None))
        {
            if (panel != null && panel.gameObject.name.Contains("DungeonMapBoard"))
                return panel;
        }

        return null;
    }

    void Update()
    {
        if (Keyboard.current == null || mapBoardPanel == null)
            return;

        if (!IsDungeonPlayActive())
            return;

        if (Keyboard.current.mKey.wasPressedThisFrame)
            mapBoardPanel.Toggle(readOnly: false);

        if (mapBoardPanel.IsOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            mapBoardPanel.Close();
    }

    static bool IsDungeonPlayActive()
    {
        if (GameManager.instance != null)
            return GameManager.instance.mapState == MapState.Stage;

        return StageManager.instance != null;
    }

    public void SetMapBoardPanel(MapBoardPanelView panel) => mapBoardPanel = panel;
}
