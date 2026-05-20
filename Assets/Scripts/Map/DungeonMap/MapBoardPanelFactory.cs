using UnityEngine;
using UnityEngine.UI;

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

        GameObject toolbar = null;
        if (includeMarkingToolbar)
            toolbar = CreateMarkingToolbar(panel.transform, markSprites);

        var view = panel.AddComponent<MapBoardPanelView>();
        view.Configure(panel, content, toolbar, grid, mapStagePrefab, markSprites);
        panel.SetActive(false);
        return view;
    }

    static GameObject CreateMarkingToolbar(Transform parent, MapMarkSpriteSet markSprites)
    {
        var toolbar = CreateRectObject("MarkingToolbar", parent);
        var toolbarRect = toolbar.GetComponent<RectTransform>();
        toolbarRect.anchorMin = new Vector2(0.5f, 0f);
        toolbarRect.anchorMax = new Vector2(0.5f, 0f);
        toolbarRect.pivot = new Vector2(0.5f, 0f);
        toolbarRect.anchoredPosition = new Vector2(0f, 40f);
        toolbarRect.sizeDelta = new Vector2(700f, 60f);

        var layout = toolbar.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 8f;
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
            DungeonMapMarkType.BackPortal,
            DungeonMapMarkType.RandomPortal
        };

        foreach (var markType in markTypes)
        {
            var buttonObject = CreateRectObject($"Mark_{markType}", toolbar.transform);
            var buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(48f, 48f);

            buttonObject.AddComponent<Image>().color = Color.white;

            var iconObject = CreateRectObject("Icon", buttonObject.transform);
            var iconImage = iconObject.AddComponent<Image>();
            var sprite = markSprites != null ? markSprites.GetSprite(markType) : null;
            if (sprite != null)
                iconImage.sprite = sprite;
            iconImage.raycastTarget = false;

            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(4f, 4f);
            iconRect.offsetMax = new Vector2(-4f, -4f);

            buttonObject.AddComponent<Button>();
            var markButton = buttonObject.AddComponent<MapMarkButton>();
            markButton.Configure(markType, sprite);
        }

        toolbar.SetActive(false);
        return toolbar;
    }

    static GameObject CreateRectObject(string objectName, Transform parent)
    {
        var go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
}
