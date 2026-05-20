using UnityEngine;

/// <summary>
/// 레거시 호환. MapBoardPanelView를 사용하세요.
/// </summary>
public class VillageMapBoardView : MonoBehaviour
{
    [SerializeField] MapBoardPanelView mapBoardPanel;

    public void Show()
    {
        if (mapBoardPanel == null)
            mapBoardPanel = GetComponent<MapBoardPanelView>();
        mapBoardPanel?.Open(readOnly: true);
    }

    public void Hide() => mapBoardPanel?.Close();
}
