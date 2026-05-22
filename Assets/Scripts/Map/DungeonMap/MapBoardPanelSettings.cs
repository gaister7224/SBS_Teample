/// <summary>
/// 마을 지도 게시판(F키) / 던전 전체 지도(M키) 대형 패널.
/// </summary>
public static class MapBoardPanelSettings
{
    public const float PanelSize = 600f;
    /// <summary>셀 중심 간격(그리드 피치).</summary>
    public const float CellSize = 75f;
    /// <summary>방 타일 표시 크기. CellSize보다 작아야 셀 사이 여백이 생깁니다.</summary>
    public const float CellGap = 12f;
    public static float CellVisualSize => CellSize - CellGap;
    public const bool ShowPlayerPin = true;
    public const float MarkToolbarButtonSize = 84f;
    public const float MarkToolbarGapBelowMap = 56f;
}
