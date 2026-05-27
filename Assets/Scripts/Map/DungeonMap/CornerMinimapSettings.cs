using UnityEngine;

/// <summary>
/// 우측 상단 HUD 코너 미니맵 (원본 크기).
/// </summary>
public static class CornerMinimapSettings
{
    public const int VisibleRadius = 1; // 플레이어 기준 반경 1칸 => 3x3
    public const int VisibleCellCountPerAxis = VisibleRadius * 2 + 1;
    public const float CellSize = 100f;
    public const float CellGap = 15f;
    public const float BackgroundPadding = 20f;
    public static float PanelSize => VisibleCellCountPerAxis * CellSize;
    public static float TotalPanelSize => PanelSize + BackgroundPadding * 2f;
    public static float CellVisualSize => CellSize - CellGap;
    public const bool ShowPlayerPin = true;
    public const bool AllowCellInteraction = false;
    public const bool ShowMarkingToolbar = false;
    public const float ToolbarHeight = 44f;
    public const float ToolbarButtonSize = 32f;
    public static readonly Color BackgroundColor = new(0f, 0f, 0f, 0.35f);
}
