using UnityEngine;

/// <summary>
/// 레거시 맵 셀. DungeonMapCellView가 지도 표시를 담당합니다.
/// </summary>
public class MinimapStage : MonoBehaviour
{
    public Vector2Int index;
    public StageType stageType;

    public void PingUiOn()
    {
        if (DungeonMapService.Instance != null)
            DungeonMapService.Instance.SelectCell(index);
    }

    public void OnCellClicked() => PingUiOn();
}
