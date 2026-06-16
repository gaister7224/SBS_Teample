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
        Width = MapBoardPanelSettings.ToolbarWidth,
        Height = MapBoardPanelSettings.ToolbarHeight,
        ButtonSize = MapBoardPanelSettings.MarkToolbarButtonSize,
        Spacing = 6f,
        AnchoredPosition = new Vector2(0f, MapBoardPanelSettings.ToolbarBottomInset),
        StartActive = false,
        AnchorBelowCenteredMap = false
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
    public const string MapBoardBackgroundResourcePath = "MapBoardBackground";
    const string MapContentBackgroundName = "Background";

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
        dim.sprite = null;
        dim.color = Color.clear;
        dim.raycastTarget = false;

        var content = CreateRectObject("MapContent", panel.transform);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = new Vector2(0, 109f);
        contentRect.sizeDelta = new Vector2(MapBoardPanelSettings.PanelSize, MapBoardPanelSettings.PanelSize);

        ApplyMapContentBackground(content, warnOnFailure: true);
        content.AddComponent<RectMask2D>();

        var gridRoot = CreateRectObject("Grid", content.transform);
        var gridRootRect = gridRoot.GetComponent<RectTransform>();
        gridRootRect.anchorMin = new Vector2(0.5f, 0.5f);
        gridRootRect.anchorMax = new Vector2(0.5f, 0.5f);
        gridRootRect.pivot = new Vector2(0.5f, 0.5f);
        gridRootRect.sizeDelta = new Vector2(
            MapBoardPanelSettings.MapAreaWidth,
            MapBoardPanelSettings.PanelSize - MapBoardPanelSettings.MapAreaTopInset - MapBoardPanelSettings.MapAreaBottomInset);
        gridRootRect.anchoredPosition = new Vector2(
            0f,
            (MapBoardPanelSettings.MapAreaBottomInset - MapBoardPanelSettings.MapAreaTopInset) * 0.5f);
        gridRoot.AddComponent<RectMask2D>();

        var grid = gridRoot.AddComponent<DungeonMapGridBuilder>();
        grid.ConfigureForMapBoard(includeMarkingToolbar);
        grid.SetCellPrefab(mapStagePrefab);
        grid.SetMarkSpriteSet(markSprites);

        if (includeMarkingToolbar)
            content.AddComponent<MapBoardMarkingInput>();

        GameObject toolbar = null;
        if (includeMarkingToolbar)
            toolbar = CreateMarkingToolbar(content.transform, markSprites, MarkToolbarLayout.MapBoard);

        var view = panel.AddComponent<MapBoardPanelView>();
        view.Configure(panel, content, toolbar, grid, mapStagePrefab, markSprites);
        panel.SetActive(false);
        return view;
    }

    public static void ApplyPanelBackground(GameObject panelRoot, bool warnOnFailure = true)
    {
        if (panelRoot == null || panelRoot.transform == null)
            return;

        var mapContent = panelRoot.transform.Find("MapContent");
        if (mapContent == null)
            return;

        ApplyMapContentBackground(mapContent.gameObject, warnOnFailure);
    }

    public static void ApplyPanelBackground(Image panelImage, bool warnOnFailure = true)
    {
        ApplyPanelBackground(panelImage != null ? panelImage.gameObject : null, warnOnFailure);
    }

    public static bool TryLoadMapBoardBackground(out Sprite sprite)
    {
        sprite = Resources.Load<Sprite>(MapBoardBackgroundResourcePath);
        if (sprite != null)
            return true;

        var sprites = Resources.LoadAll<Sprite>(MapBoardBackgroundResourcePath);
        if (sprites != null && sprites.Length > 0)
        {
            sprite = sprites[0];
            return true;
        }

        sprite = null;
        return false;
    }

    static void LogMapBoardBackgroundLoadFailure()
    {
        var resourcePath = $"Resources/{MapBoardBackgroundResourcePath}";

        var texture = Resources.Load<Texture2D>(MapBoardBackgroundResourcePath);
        if (texture != null)
        {
            Debug.LogWarning(
                $"[MapBoard] '{resourcePath}' is imported as a texture but could not be loaded as Sprite. " +
                "Set Texture Type to 'Sprite (2D and UI)' and apply. Using dim fallback.",
                texture);
            return;
        }

        Debug.LogWarning(
            $"[MapBoard] Background not found at '{resourcePath}'. " +
            "Add Assets/Resources/MapBoardBackground.png (Sprite 2D and UI). Using dim fallback.");
    }

    static void ApplyMapContentBackground(GameObject mapContentRoot, bool warnOnFailure)
    {
        if (mapContentRoot == null)
            return;

        Transform targetParent = mapContentRoot.transform.parent != null 
            ? mapContentRoot.transform.parent : mapContentRoot.transform;

        var background = targetParent.Find(MapContentBackgroundName);
        RectTransform backgroundRect;
        Image backgroundImage;

        if (background == null)
        {
            var backgroundObject = new GameObject(
                MapContentBackgroundName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            //backgroundObject.transform.SetParent(mapContentRoot.transform, false);
            
            backgroundObject.transform.SetParent(targetParent, false);
            backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundImage = backgroundObject.GetComponent<Image>();
        }
        else
        {
            backgroundRect = background as RectTransform;
            backgroundImage = background.GetComponent<Image>();
            if (backgroundImage == null)
                backgroundImage = background.gameObject.AddComponent<Image>();
        }

        backgroundRect.anchorMin = new Vector2(0.5f, 0.5f); // ¾ÞÄ¿¸¦ Áß¾ÓÀ¸·Î ¼³Á¤
        backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);

        backgroundRect.sizeDelta = new Vector2(752f, 900f);

        //backgroundRect.offsetMin = Vector2.zero;
        //backgroundRect.offsetMax = Vector2.zero;
        backgroundRect.anchoredPosition = Vector2.zero;

        backgroundRect.SetAsFirstSibling();

        if (TryLoadMapBoardBackground(out var sprite))
        {
            backgroundImage.sprite = sprite;
            backgroundImage.type = Image.Type.Simple;
            backgroundImage.preserveAspect = false;
            backgroundImage.color = Color.white;
            backgroundImage.raycastTarget = false;
            return;
        }

        backgroundImage.sprite = null;
        backgroundImage.color = Color.clear;
        backgroundImage.raycastTarget = false;

        if (warnOnFailure)
            LogMapBoardBackgroundLoadFailure();
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
