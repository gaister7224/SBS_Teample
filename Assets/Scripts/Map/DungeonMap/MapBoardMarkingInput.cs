using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// M키 대형 지도: 클릭 위치로 셀을 찾아 마킹/삭제합니다.
/// </summary>
public class MapBoardMarkingInput : MonoBehaviour, IPointerClickHandler
{
    static readonly List<RaycastResult> RaycastHits = new();

    DungeonMapGridBuilder grid;
    Image clickSurface;

    void Awake()
    {
        // GridBuilder는 BindGrid()로 주입될 수도 있으므로 Awake에서는 없어도 동작해야 한다.
        grid = GetComponent<DungeonMapGridBuilder>();
        EnsureClickSurface();
    }

    void EnsureClickSurface()
    {
        clickSurface = GetComponent<Image>();
        if (clickSurface == null)
            clickSurface = gameObject.AddComponent<Image>();

        clickSurface.sprite = MapUiSpriteUtil.White;
        clickSurface.color = new Color(1f, 1f, 1f, 0.004f);
        clickSurface.raycastTarget = true;
    }

    public void BindGrid(DungeonMapGridBuilder gridBuilder)
    {
        grid = gridBuilder != null ? gridBuilder : GetComponent<DungeonMapGridBuilder>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
  
        if (MapBoardPanelView.ActiveMarkingPanel == null)
        {
            return;
        }

        var service = DungeonMapService.Instance;
        if (service == null)
        {
            return;
        }
        if (service.IsReadOnly)
        {
            return;
        }
        if (grid == null)
        {
            return;
        }

        var cell = ResolveCell(eventData);
        if (cell == null)
            return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            cell.RemoveMark();
            return;
        }

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (service.PendingMarkType.HasValue)
            cell.ApplyMarkClick();
        else
            cell.SelectIfRevealed();
    }

    DungeonMapCellView ResolveCell(PointerEventData eventData)
    {
        var byPosition = grid.PickCellAtScreen(eventData.position, eventData.pressEventCamera);
        if (byPosition != null)
            return byPosition;

        RaycastHits.Clear();
        EventSystem.current.RaycastAll(eventData, RaycastHits);

        foreach (var hit in RaycastHits)
        {
            if (hit.gameObject == gameObject)
                continue;

            var cell = hit.gameObject.GetComponent<DungeonMapCellView>();
            if (cell != null)
                return cell;

            cell = hit.gameObject.GetComponentInParent<DungeonMapCellView>();
            if (cell != null)
                return cell;
        }

        return null;
    }
}
