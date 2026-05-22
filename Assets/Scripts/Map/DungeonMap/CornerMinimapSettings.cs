/// <summary>
/// 우측 상단 HUD 코너 미니맵 (원본 크기).
/// </summary>
public static class CornerMinimapSettings
{
    public const float PanelSize = 200f;
    public const float CellSize = 25f;
    public const float CellGap = 5f;
    public static float CellVisualSize => CellSize - CellGap;
    public const bool ShowPlayerPin = true;
    public const bool AllowCellInteraction = false;
    public const bool ShowMarkingToolbar = false;
    public const float ToolbarHeight = 44f;
    public const float ToolbarButtonSize = 32f;
}
