using UnityEngine;

/// <summary>
/// 우측 상단 코너 미니맵. DungeonMapService 데이터와 동기화됩니다.
/// </summary>
public class CornerMinimapView : MonoBehaviour
{
    [SerializeField] public DungeonMapGridBuilder gridBuilder;

    void Start()
    {
        if (gridBuilder != null)
            gridBuilder.ConfigureForCornerMinimap();

        if (DungeonMapService.Instance != null)
        {
            DungeonMapService.Instance.Current.OnChanged += Refresh;
            DungeonMapService.Instance.OnDungeonLoaded += OnDungeonLoaded;
        }

        OnDungeonLoaded();
    }

    void OnDestroy()
    {
        if (DungeonMapService.Instance == null)
            return;

        DungeonMapService.Instance.Current.OnChanged -= Refresh;
        DungeonMapService.Instance.OnDungeonLoaded -= OnDungeonLoaded;
    }

    void OnDungeonLoaded()
    {
        if (gridBuilder != null)
            gridBuilder.BuildGrid();

        Refresh();
    }

    void Refresh()
    {
        gridBuilder?.RefreshAll();
        GameplayInputUtility.ReleaseUiFocus();
    }
}
