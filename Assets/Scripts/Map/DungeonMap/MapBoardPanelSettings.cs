/// <summary>
/// 마을 지도 게시판(F키) / 던전 전체 지도(M키) 대형 패널.
/// </summary>
public static class MapBoardPanelSettings
{
    public const float PanelSize = 900f;
    /// <summary>셀 중심 간격(그리드 피치).</summary>
    public const float CellSize = 75f;
    /// <summary>방 타일 표시 크기. CellSize보다 작아야 셀 사이 여백이 생깁니다.</summary>
    public const float CellGap = 12f;
    public static float CellVisualSize => CellSize - CellGap;
    public const bool ShowPlayerPin = true;
    public const float MarkToolbarButtonSize = 56f;
    public const float MarkToolbarGapBelowMap = 0f;

    // Scroll background artwork guide regions.
    //public const float MapAreaInsetX = 88f;
    //public const float MapAreaTopInset = 96f;
    //public const float MapAreaBottomInset = 196f;

    public const float MapAreaInsetX = 60f;
    public const float MapAreaTopInset = 80f;
    public const float MapAreaBottomInset = 90f;

    public const float ToolbarInsetX = 88f;
    public const float ToolbarBottomInset = 78f;
    public const float ToolbarHeight = 64f;

    public static float MapAreaWidth => PanelSize - MapAreaInsetX * 2f;
    public static float ToolbarWidth => PanelSize - ToolbarInsetX * 2f;
}
