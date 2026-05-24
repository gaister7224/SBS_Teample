using UnityEngine;

/// <summary>
/// MiniMapCanvas 및 코너 HUD 요소의 화면 배치.
/// </summary>
public static class MinimapHudLayout
{
    public const float Margin = 16f;
    public const float VillageMinimapSize = 200f;

    public static void ApplyFullscreenCanvas(RectTransform canvasRect)
    {
        if (canvasRect == null)
            return;

        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.offsetMin = Vector2.zero;
        canvasRect.offsetMax = Vector2.zero;
        canvasRect.pivot = new Vector2(0.5f, 0.5f);
        canvasRect.localScale = Vector3.one;
    }

    public static void ApplyTopRight(RectTransform rect, float width, float height, float margin = Margin)
    {
        if (rect == null)
            return;

        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one;
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(-margin, -margin);
        rect.localScale = Vector3.one;
    }

    public static void ApplyFullscreenPanel(RectTransform panelRect)
    {
        if (panelRect == null)
            return;

        var canvas = FindMainOverlayCanvas();
        if (canvas == null)
            return;

        EnsureRootCanvasReady(canvas);

        if (panelRect.parent != canvas.transform)
            panelRect.SetParent(canvas.transform, false);

        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panelRect.localScale = Vector3.one;
        panelRect.anchoredPosition = Vector2.zero;
    }

    public static void EnsureRootCanvasReady(Canvas canvas)
    {
        if (canvas == null)
            return;

        var canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null)
            return;

        ApplyFullscreenCanvas(canvasRect);
    }

    public static void ApplyCenteredMapContent(RectTransform contentRect, float size)
    {
        if (contentRect == null)
            return;

        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(size, size);
        contentRect.localScale = Vector3.one;
    }

    public static Canvas FindMainOverlayCanvas()
    {
        var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Canvas bestNamed = null;
        Canvas bestRoot = null;

        foreach (var canvas in allCanvases)
        {
            if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                continue;

            if (canvas.gameObject.name.Contains("MiniMap"))
                continue;

            if (!IsRootOverlayCanvas(canvas))
                continue;

            if (canvas.gameObject.name == "Canvas")
                bestNamed = canvas;

            if (bestRoot == null || canvas.sortingOrder >= bestRoot.sortingOrder)
                bestRoot = canvas;
        }

        return bestNamed != null ? bestNamed : bestRoot;
    }

    static bool IsRootOverlayCanvas(Canvas canvas)
    {
        if (canvas == null)
            return false;

        var parent = canvas.transform.parent;
        while (parent != null)
        {
            if (parent.GetComponent<Canvas>() != null)
                return false;

            parent = parent.parent;
        }

        return true;
    }

    public static void EnsureMiniMapCanvas()
    {
        var canvas = GameObject.Find("MiniMapCanvas");
        if (canvas == null)
            return;

        ApplyFullscreenCanvas(canvas.transform as RectTransform);
    }
}
