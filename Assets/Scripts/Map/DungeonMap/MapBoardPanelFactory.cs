using UnityEngine;
using UnityEngine.UI;

public struct MarkToolbarLayout
{
    public float Width;
    public float Height;
    public float ButtonSize;
    public float Spacing;
    public Vector2 AnchoredPosition;
    public bool StartActive;
    public bool AnchorBelowCenteredMap;

    public static MarkToolbarLayout MapBoard => new()
    {
        Width = 1000f,
        Height = 96f,
        ButtonSize = MapBoardPanelSettings.MarkToolbarButtonSize,
        Spacing = 10f,
        AnchoredPosition = new Vector2(0f, -(MapBoardPanelSettings.PanelSize * 0.5f
            + MapBoardPanelSettings.MarkToolbarGapBelowMap)),
        StartActive = false,
        AnchorBelowCenteredMap = true
    };

    public static MarkToolbarLayout CornerMinimap => new()
    {
        Width = CornerMinimapSettings.PanelSize,
        Height = CornerMinimapSettings.ToolbarHeight,
        ButtonSize = CornerMinimapSettings.ToolbarButtonSize,
        Spacing = 4f,
        AnchoredPosition = Vector2.zero,
        StartActive = true
    };
}

public static class MapBoardPanelFactory
{
    public static MapBoardPanelView Create(Transform parent, GameObject mapStagePrefab, MapMarkSpriteSet markSprites,
        bool includeMarkingToolbar, string panelName = "MapBoardPanel")
    {
        var panel = CreateRectObject(panelName, parent);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var dim = panel.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.55f);
        dim.raycastTarget = false;

        var content = CreateRectObject("MapContent", panel.transform);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(MapBoardPanelSettings.PanelSize, MapBoardPanelSettings.PanelSize);

        var grid = content.AddComponent<DungeonMapGridBuilder>();
        grid.ConfigureForMapBoard(includeMarkingToolbar);
        grid.SetCellPrefab(mapStagePrefab);
        grid.SetMarkSpriteSet(markSprites);

        if (includeMarkingToolbar)
            content.AddComponent<MapBoardMarkingInput>();

        GameObject toolbar = null;
        if (includeMarkingToolbar)
            toolbar = CreateMarkingToolbar(panel.transform, markSprites, MarkToolbarLayout.MapBoard);

        var view = panel.AddComponent<MapBoardPanelView>();
        view.Configure(panel, content, toolbar, grid, mapStagePrefab, markSprites);
        panel.SetActive(false);
        return view;
    }

    public static GameObject CreateMarkingToolbar(Transform parent, MapMarkSpriteSet markSprites,
        MarkToolbarLayout layoutStyle)
    {
        var toolbar = CreateRectObject("MarkingToolbar", parent);
        var toolbarRect = toolbar.GetComponent<RectTransform>();
        if (layoutStyle.AnchorBelowCenteredMap)
        {
            toolbarRect.anchorMin = new Vector2(0.5f, 0.5f);
            toolbarRect.anchorMax = new Vector2(0.5f, 0.5f);
            toolbarRect.pivot = new Vector2(0.5f, 1f);
        }
        else
        {
            toolbarRect.anchorMin = new Vector2(0.5f, 0f);
            toolbarRect.anchorMax = new Vector2(0.5f, 0f);
            toolbarRect.pivot = new Vector2(0.5f, 0f);
        }

        toolbarRect.anchoredPosition = layoutStyle.AnchoredPosition;
        toolbarRect.sizeDelta = new Vector2(layoutStyle.Width, layoutStyle.Height);

        var layout = toolbar.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = layoutStyle.Spacing;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var markTypes = new[]
        {
            DungeonMapMarkType.Trap,
            DungeonMapMarkType.Treasure,
            DungeonMapMarkType.SealedStone,
            DungeonMapMarkType.Shop,
            DungeonMapMarkType.Bonfire,
            DungeonMapMarkType.BuffStatue,
            DungeonMapMarkType.ReturnPortal,
            DungeonMapMarkType.RandomPortal
        };

        foreach (var markType in markTypes)
        {
            var buttonObject = CreateRectObject($"Mark_{markType}", toolbar.transform);
            ApplyToolbarButtonLayout(buttonObject.GetComponent<RectTransform>(), layoutStyle.ButtonSize);

            var sprite = markSprites != null ? markSprites.GetSprite(markType) : null;
            var buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.sprite = sprite;
            buttonImage.color = Color.white;
            buttonImage.preserveAspect = true;
            buttonImage.raycastTarget = true;

            buttonObject.AddComponent<Button>();
            var markButton = buttonObject.AddComponent<MapMarkButton>();
            markButton.Configure(markType, sprite);
        }

        toolbar.transform.SetAsLastSibling();
        toolbar.SetActive(layoutStyle.StartActive);
        return toolbar;
    }

    static void ApplyToolbarButtonLayout(RectTransform rect, float size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(size, size);

        var layoutElement = rect.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = size;
        layoutElement.preferredHeight = size;
        layoutElement.minWidth = size;
        layoutElement.minHeight = size;
    }

    static GameObject CreateRectObject(string objectName, Transform parent)
    {
        var go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
}
