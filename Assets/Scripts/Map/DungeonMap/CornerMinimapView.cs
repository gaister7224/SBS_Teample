using UnityEngine;

/// <summary>
/// 우측 상단 코너 미니맵. DungeonMapService 데이터와 동기화됩니다.
/// </summary>
public class CornerMinimapView : MonoBehaviour
{
    [SerializeField] public DungeonMapGridBuilder gridBuilder;
    private bool _isBuilt = false;

    void Start()
    {
        if (gridBuilder != null)
            gridBuilder.ConfigureForCornerMinimap();

        //if (DungeonMapService.Instance != null)
        //{
        //    DungeonMapService.Instance.Current.OnChanged += Refresh;
        //    //DungeonMapService.Instance.OnDungeonLoaded += OnDungeonLoaded;
        //    DungeonMapService.OnMapLoaded += OnDungeonLoaded;
        //}

        //if (DungeonMapService.Instance != null && DungeonMapService.Instance.ActiveData == null)
        //{
        //    Debug.Log("데이터가 아직 없어서 이벤트를 기다립니다...");
        //    DungeonMapService.OnMapLoaded += OnDungeonLoaded;
        //}
        //else if (DungeonMapService.Instance != null && DungeonMapService.Instance.ActiveData != null)
        //{
        //    OnDungeonLoaded(DungeonMapService.Instance.ActiveData);
        //}
        //else
        //{
        //    // 2. 인자를 받는 Action<DungeonMapData> 형태로 구독
        //    DungeonMapService.OnMapLoaded += OnDungeonLoaded;
        //}
        if (DungeonMapService.Instance != null && DungeonMapService.Instance.ActiveData != null)
        {
            BuildMap(DungeonMapService.Instance.ActiveData);
        }

        // 2. 데이터가 나중에 로드될 경우를 대비해 이벤트 구독
        DungeonMapService.OnMapLoaded += BuildMap;
    }

    private void BuildMap(DungeonMapData data)
    {
        if (gridBuilder != null && data != null)
        {
            gridBuilder.BuildGrid(data);
            _isBuilt = true;
        }
    }

    void OnDestroy()
    {
        //if (DungeonMapService.Instance != null)
        //{
        //    DungeonMapService.Instance.Current.OnChanged -= Refresh;
        //    // 인자 받는 이벤트 구독 해제
        //    DungeonMapService.OnMapLoaded -= OnDungeonLoaded;
        //}

        DungeonMapService.OnMapLoaded -= BuildMap;
    }

    void OnDungeonLoaded(DungeonMapData data = null)
    {
        //if (gridBuilder != null)
        //    gridBuilder.ForceRebuild();

        //Refresh();
        if (_isBuilt) return; // 이미 그려졌으면 무시

        if (gridBuilder != null)
        {
            gridBuilder.BuildGrid(data); // RefreshAll 대신 BuildGrid를 직접 호출
            _isBuilt = true;
        }
    }

    void Refresh()
    {
        gridBuilder?.RefreshAll();
        GameplayInputUtility.ReleaseUiFocus();
    }
}
