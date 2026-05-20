using UnityEngine;
using UnityEngine.UI;

public class MapMarkButton : MonoBehaviour
{
    [SerializeField] DungeonMapMarkType markType;
    [SerializeField] Image iconImage;

    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    public void Configure(DungeonMapMarkType type, Sprite sprite)
    {
        markType = type;
        if (iconImage != null && sprite != null)
        {
            iconImage.sprite = sprite;
            iconImage.enabled = true;
        }
    }

    void OnClick()
    {
        if (DungeonMapService.Instance == null || DungeonMapService.Instance.IsReadOnly)
            return;

        if (!DungeonMapService.Instance.SelectedCell.HasValue)
            return;

        var cell = DungeonMapService.Instance.SelectedCell.Value;
        if (!DungeonMapService.Instance.ApplyMark(cell, markType))
            return;

        var grids = FindObjectsByType<DungeonMapGridBuilder>(FindObjectsSortMode.None);
        foreach (var grid in grids)
            grid.GetCell(cell)?.PlayMarkFeedback();
    }
}
