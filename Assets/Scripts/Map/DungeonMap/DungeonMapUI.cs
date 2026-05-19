using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 던전에서 M키로 지도 게시판(대형 지도 + 마킹) UI를 엽니다.
/// </summary>
public class DungeonMapUI : MonoBehaviour
{
    [SerializeField] MapBoardPanelView mapBoardPanel;

    void Awake()
    {
        if (mapBoardPanel == null)
            mapBoardPanel = GetComponent<MapBoardPanelView>();

        if (mapBoardPanel == null)
            mapBoardPanel = GetComponentInChildren<MapBoardPanelView>(true);
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
