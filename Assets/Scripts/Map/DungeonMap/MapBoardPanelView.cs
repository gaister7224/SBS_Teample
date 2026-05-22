using UnityEngine;

/// <summary>
/// 마을 지도 게시판(F) / 던전 지도 제작 UI(M) 공통 대형 지도 패널.
/// </summary>
public class MapBoardPanelView : MonoBehaviour
{
    [SerializeField] public GameObject panelRoot;
    [SerializeField] public GameObject mapContentRoot;
    [SerializeField] public GameObject markingToolbar;
    [SerializeField] public DungeonMapGridBuilder gridBuilder;
    [SerializeField] public GameObject mapStagePrefab;
    [SerializeField] public MapMarkSpriteSet markSpriteSet;
    [SerializeField] int dungeonIdOverride;

    bool built;
    bool isOpen;

    public bool IsOpen => isOpen;

    public void Configure(GameObject panel, GameObject content, GameObject toolbar,
        DungeonMapGridBuilder grid, GameObject stagePrefab, MapMarkSpriteSet sprites)
    {
        panelRoot = panel;
        mapContentRoot = content;
        markingToolbar = toolbar;
        gridBuilder = grid;
        mapStagePrefab = stagePrefab;
        markSpriteSet = sprites;
    }

    public void Toggle(bool readOnly)
    {
        if (isOpen)
            Close();
        else
            Open(readOnly);
    }

    public void Open(bool readOnly)
    {
        EnsureService();
        DungeonMapService.Instance.IsReadOnly = readOnly;

        var dungeonId = dungeonIdOverride > 0
            ? dungeonIdOverride
            : DungeonMapService.Instance.GetCurrentDungeonId();
        DungeonMapService.Instance.LoadDungeon(dungeonId);

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (markingToolbar != null)
            markingToolbar.SetActive(!readOnly);

        EnsureGrid(readOnly);
        gridBuilder?.RefreshAll();
        isOpen = true;
    }

    public void Close()
    {
        isOpen = false;
        GameplayInputUtility.ReleaseUiFocus();

        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (markingToolbar != null)
            markingToolbar.SetActive(false);

        DungeonMapService.Instance?.FlushSave();
    }

    void EnsureService()
    {
        if (DungeonMapService.Instance != null)
            return;

        var serviceObject = new GameObject("DungeonMapService");
        serviceObject.AddComponent<DungeonMapService>();
    }

    void EnsureGrid(bool allowMarking)
    {
        if (gridBuilder == null && mapContentRoot != null)
            gridBuilder = mapContentRoot.GetComponent<DungeonMapGridBuilder>();

        if (gridBuilder == null && mapContentRoot != null)
            gridBuilder = mapContentRoot.AddComponent<DungeonMapGridBuilder>();

        if (gridBuilder == null)
            return;

        gridBuilder.ConfigureForMapBoard(allowMarking);

        if (mapStagePrefab != null)
            gridBuilder.SetCellPrefab(mapStagePrefab);

        if (markSpriteSet != null)
            gridBuilder.SetMarkSpriteSet(markSpriteSet);

        if (!built)
        {
            gridBuilder.BuildGrid();
            built = true;
        }
    }
}
