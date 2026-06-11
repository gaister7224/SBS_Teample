using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 마을 지도 게시판(F) / 던전 지도 제작 UI(M) 공통 대형 지도 패널.
/// </summary>
public class MapBoardPanelView : MonoBehaviour
{
    public static MapBoardPanelView ActiveMarkingPanel { get; private set; }

    [SerializeField] public GameObject panelRoot;
    [SerializeField] public GameObject mapContentRoot;
    [SerializeField] public GameObject markingToolbar;
    [SerializeField] public DungeonMapGridBuilder gridBuilder;
    [SerializeField] public GameObject mapStagePrefab;
    [SerializeField] public MapMarkSpriteSet markSpriteSet;
    [SerializeField] int dungeonIdOverride;

    InventoryMain inventory;

    bool built;
    bool isOpen;

    public bool IsOpen => isOpen;

    [Header("Grid Setup")]
    [SerializeField] MapBoardMarkingInput markingInput;

    [Header("Dungeon Selection Buttons")]
    [SerializeField] List<Button> dungeonButtons = new List<Button>();
    [SerializeField] TMPro.TMP_Dropdown floorDropdown;

    int selectedDungeonIndex = 0;

    private static Dictionary<int, HashSet<Vector2Int>> townMapRevealedCache = new Dictionary<int, HashSet<Vector2Int>>();
    private static HashSet<int> visitedDungeonIds = new HashSet<int>(); // 실제로 방문한 던전 ID 목록

    private bool isOpenedAsTownBoard = false;

    [Header("Debug Section")]
    [SerializeField] private List<int> debugCachedDungeonIds = new List<int>();

    void Awake()
    {
        if (markingInput == null) markingInput = GetComponentInChildren<MapBoardMarkingInput>();
        if (gridBuilder == null) gridBuilder = GetComponentInChildren<DungeonMapGridBuilder>();

        if (markingInput != null && gridBuilder != null)
        {
            markingInput.BindGrid(gridBuilder);
        }

        BindUiComponents();
    }

    void BindUiComponents()
    {
        dungeonButtons.Clear();

        Transform dungeonButtonRoot = transform.Find("DungeonButton");
        if (dungeonButtonRoot != null)
        {
            foreach (Transform child in dungeonButtonRoot)
            {
                Button btn = child.GetComponent<Button>();
                if (btn != null) dungeonButtons.Add(btn);
            }
        }

        for (int i = 0; i < dungeonButtons.Count; i++)
        {
            int fixedIndex = i;

            dungeonButtons[i].onClick.RemoveAllListeners();
            dungeonButtons[i].onClick.AddListener(() =>
            {
                Debug.Log($"[UI 클릭] {fixedIndex}번째 던전 버튼이 눌렸습니다.");
                OnDungeonSelected(fixedIndex);
            });
        }

        Transform dropdownTransform = transform.Find("Dropdown");
        if (dropdownTransform != null) floorDropdown = dropdownTransform.GetComponent<TMPro.TMP_Dropdown>();

        if (floorDropdown != null)
        {
            floorDropdown.onValueChanged.RemoveAllListeners();
            floorDropdown.onValueChanged.AddListener(OnFloorChanged);
        }
    }

    public static void UpdateVisitedCacheDirectly(int dungeonId, HashSet<Vector2Int> revealedCells)
    {
        if (revealedCells == null || revealedCells.Count == 0) return;

        // 방문 ID 등록
        if (!visitedDungeonIds.Contains(dungeonId))
        {
            visitedDungeonIds.Add(dungeonId);
        }

        // 격자 데이터 실시간 동기화/백업
        if (!townMapRevealedCache.ContainsKey(dungeonId))
        {
            townMapRevealedCache[dungeonId] = new HashSet<Vector2Int>();
        }

        foreach (var cell in revealedCells)
        {
            townMapRevealedCache[dungeonId].Add(cell);
        }
    }

    public void OpenMapBoard()
    {
        gameObject.SetActive(true);
        ActiveMarkingPanel = this;
        isOpenedAsTownBoard = true;

        EnsureService();
        //ForceSyncCurrentDungeonData();
        BackupDungeonDataToTownCache();

        if (GameManager.instance != null)
        {
            int currentId = DungeonMapService.GetDungeonMapId(GameManager.instance.curDungeonNumber, GetCurrentSelectedFloor());
            DungeonMapService.Instance.EnsureLoaded(currentId);
        }

        if (gridBuilder != null)
        {
            gridBuilder.ConfigureForMapBoard(allowMarking: true);

            var serializedObj = new UnityEditor.SerializedObject(gridBuilder);
            var showPinProp = serializedObj.FindProperty("showPlayerPin");
            if (showPinProp != null)
            {
                showPinProp.boolValue = false;
                serializedObj.ApplyModifiedProperties();
            }
        }

        OnDungeonSelected(selectedDungeonIndex);
    }

    private void ForceSyncCurrentDungeonData()
    {
        var service = DungeonMapService.Instance;
        if (service != null && service.Current != null)
        {
            int currentId = service.Current.DungeonId;

            // 방문 기록에 강제 추가 (중간에 나왔어도 방문은 한 것이므로)
            if (!visitedDungeonIds.Contains(currentId))
                visitedDungeonIds.Add(currentId);

            // 현재 Revealed 데이터를 캐시에 강제 저장
            townMapRevealedCache[currentId] = new HashSet<Vector2Int>(service.Current.Revealed);

            Debug.Log($"[강제 동기화] 던전 ID {currentId}의 데이터가 맵 보드 캐시에 저장되었습니다.");
        }
    }

    private void BackupDungeonDataToTownCache()
    {
        if (DungeonMapService.Instance != null && DungeonMapService.Instance.Current != null)
        {
            var curData = DungeonMapService.Instance.Current;

            // 인게임에서 현재 로드되어 정상 작동 중인 던전 ID는 무조건 방문한 것으로 기록함
            if (curData.DungeonId > 0)
            {
                visitedDungeonIds.Add(curData.DungeonId);
            }

            if (curData.Revealed != null && curData.Revealed.Count > 0)
            {
                townMapRevealedCache[curData.DungeonId] = new HashSet<Vector2Int>(curData.Revealed);
                debugCachedDungeonIds = new List<int>(townMapRevealedCache.Keys);
            }
        }
    }

    public void CloseMapBoard()
    {
        if (ActiveMarkingPanel == this) ActiveMarkingPanel = null;
        Time.timeScale = 1f;

        var invObj = GameObject.Find("InventorySystem");
        if (invObj != null)
        {
            var inventory = invObj.GetComponent<InventoryMain>();
            if (inventory != null)
            {
                inventory.playerProfile.SetActive(true);
                inventory.playerAttack.uiClicking = false;
            }
        }

        isOpenedAsTownBoard = false;
        gameObject.SetActive(false);

        CancelInvoke(nameof(PostProcessTownMapVisibility));
    }

    void OnDungeonSelected(int dungeonIndex)
    {
        selectedDungeonIndex = dungeonIndex;
        int currentFloor = GetCurrentSelectedFloor();
        int calculatedId = DungeonMapService.GetDungeonMapId(selectedDungeonIndex, currentFloor);

        Debug.Log($"[UI] 던전 {selectedDungeonIndex}층 {currentFloor} 선택 -> ID: {calculatedId}");

        DungeonMapService.Instance.EnsureLoaded(calculatedId);
        var data = DungeonMapService.Instance.GetDungeonData(calculatedId);

        if (data == null) Debug.LogError("데이터 자체가 null입니다.");
        else Debug.Log($"데이터 가져옴. ID: {data.DungeonId}, 셀 개수: {(data.ValidCells != null ? data.ValidCells.Count : 0)}");

        // 3. 맵 표시
        if (data != null && data.ValidCells != null && data.ValidCells.Count > 0)
        {
            gridBuilder.BuildGrid(data);
        }
        else
        {
            Debug.Log("방문하지 않았거나 셀 정보가 없습니다. 그리드 초기화.");
            gridBuilder.ClearGrid();
        }
    }
    void OnFloorChanged(int dropdownIndex) => OnDungeonSelected(selectedDungeonIndex);

    public void RefreshCurrentMap()
    {
        if (gridBuilder == null) return;

        ClearExistingCells();

        gridBuilder.ForceRebuild();
        gridBuilder.BindServiceEventsForPanel();

        if (isOpenedAsTownBoard)
        {
            PostProcessTownMapVisibility();
            CancelInvoke(nameof(PostProcessTownMapVisibility));
            Invoke(nameof(PostProcessTownMapVisibility), 0.02f);
        }
    }

    /// <summary>
    /// Grid 내부의 방 타일 오브젝트들을 싹 밀어주는 청소 함수
    /// </summary>
    private void ClearExistingCells()
    {
        if (gridBuilder == null) return;

        // 지연 호출(Invoke)로 인해 꼬이는 것을 방지하기 위해 
        // 맵을 새로 지울 때 기존 예약된 포스트 프로세싱을 강제로 취소합니다.
        CancelInvoke(nameof(PostProcessTownMapVisibility));

        for (int i = gridBuilder.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = gridBuilder.transform.GetChild(i);
            if (child != null && child.name != "MarkingToolbar")
            {
                child.SetParent(null); // 즉시 부모 관계를 끊어 갱신 연산에서 제외시킵니다.
                Destroy(child.gameObject);
            }
        }
    }

    private void PostProcessTownMapVisibility()
    {
        if (DungeonMapService.Instance == null || DungeonMapService.Instance.Current == null || gridBuilder == null)
            return;

        foreach (Transform child in gridBuilder.transform)
        {
            if (child == null) continue;

            var cellView = child.GetComponent<DungeonMapCellView>();
            if (cellView == null) continue;

            // 이미 파괴 절차에 들어간 MissingReference 상태인지 유니티 널 체크 수행
            if (!cellView) continue;

            child.gameObject.SetActive(true);
        }
    }

    int GetCurrentSelectedFloor() => floorDropdown != null ? floorDropdown.value + 1 : 1;

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
        if (isOpen) Close();
        else Open(readOnly);
    }

    public void Open(bool readOnly)
    {
        inventory = GameObject.Find("InventorySystem").GetComponent<InventoryMain>();
        inventory.playerProfile.SetActive(false);
        inventory.playerAttack.uiClicking = true;
        Time.timeScale = 0f;

        EnsureService();
        DungeonMapService.Instance.IsReadOnly = readOnly;

        // 2. 던전 ID 계산 및 로드
        // [중요] LoadDungeon을 호출하면 내부에서 OnMapLoaded 이벤트를 발사합니다.
        // 씬에 있는 모든 GridBuilder(미니맵, 큰 지도 등)가 이를 듣고 자동으로 빌드됩니다.
        //int inGameDungeonId = (GameManager.instance != null && GameManager.instance.mapState == MapState.Stage)
        //    ? DungeonMapService.GetDungeonMapId(GameManager.instance.curDungeonNumber, GameManager.instance.curDungeonFloorNumber)
        //    : (dungeonIdOverride > 0 ? dungeonIdOverride : 1001);
        int inGameDungeonId;
        if (GameManager.instance.mapState == MapState.Stage)
        {
            // StageManager에서 사용하는 방식과 완벽하게 동일하게 계산
            if (StageManager.instance.Tutorial)
                inGameDungeonId = (GameManager.instance.curDungeonNumber * 100) + GameManager.instance.curDungeonFloorNumber;
            else
                inGameDungeonId = DungeonMapService.GetDungeonMapId(GameManager.instance.curDungeonNumber, GameManager.instance.curDungeonFloorNumber);
        }
        else
        {
            inGameDungeonId = (dungeonIdOverride > 0 ? dungeonIdOverride : 1001);
        }

        DungeonMapService.Instance.LoadDungeon(inGameDungeonId);
        visitedDungeonIds.Add(inGameDungeonId);

        Debug.Log($"[인게임 M키] 트래킹 ID: {inGameDungeonId}");
        DungeonMapService.Instance.EnsureLoaded(inGameDungeonId);

        // 3. 기록 및 패널 설정
        if (readOnly)
            DungeonMapService.Instance.SetPendingMark(null);

        BackupDungeonDataToTownCache();
        isOpenedAsTownBoard = false;
        EnsurePanelLayout();

        if (panelRoot != null)
        {
            MapBoardPanelFactory.ApplyPanelBackground(panelRoot);
            panelRoot.SetActive(true);
            panelRoot.transform.SetAsLastSibling();
        }

        // 4. 입력/그리드 설정
        EnsureMarkingToolbar(!readOnly);
        EnsureGrid(readOnly);

        if (!readOnly)
            ActiveMarkingPanel = this;

        // 5. 그리드 로직 (간소화 완료!)
        if (gridBuilder != null)
        {
            // 이제 수동으로 Clear하거나 ForceRebuild할 필요가 없습니다.
            // GridBuilder가 스스로 데이터를 받아서 처리합니다.
            gridBuilder.ConfigureForMapBoard(allowMarking: !readOnly);

            // 추가적인 설정이 있다면 여기에 작성
            EnsureMarkingInput();
        }

        isOpen = true;
    }

    public void Close()
    {
        BackupDungeonDataToTownCache();

        isOpen = false;
        inventory = GameObject.Find("InventorySystem").GetComponent<InventoryMain>();
        inventory.playerProfile.SetActive(true);
        inventory.playerAttack.uiClicking = false;
        Time.timeScale = 1f;

        if (ActiveMarkingPanel == this) ActiveMarkingPanel = null;

        DungeonMapService.Instance?.SetPendingMark(null);
        GameplayInputUtility.ReleaseUiFocus();

        if (panelRoot != null) panelRoot.SetActive(false);

        EnsureMarkingToolbar(false);
        isOpenedAsTownBoard = false;

        DungeonMapService.Instance?.FlushSave();

        CancelInvoke(nameof(PostProcessTownMapVisibility));
    }

    void EnsureService()
    {
        if (DungeonMapService.Instance != null) return;
        var serviceObject = new GameObject("DungeonMapService");
        serviceObject.AddComponent<DungeonMapService>();
    }

    void EnsureGrid(bool allowMarking)
    {
        if (mapContentRoot != null)
        {
            var gridTransform = mapContentRoot.transform.Find("Grid");
            if (gridTransform != null) gridBuilder = gridTransform.GetComponent<DungeonMapGridBuilder>();

            var legacyBuilder = mapContentRoot.GetComponent<DungeonMapGridBuilder>();
            if (legacyBuilder != null)
            {
                if (gridBuilder == null)
                {
                    var gridObject = new GameObject("Grid", typeof(RectTransform));
                    gridObject.transform.SetParent(mapContentRoot.transform, false);
                    var gridRect = gridObject.GetComponent<RectTransform>();
                    gridRect.anchorMin = Vector2.zero;
                    gridRect.anchorMax = Vector2.one;
                    gridRect.offsetMin = Vector2.zero;
                    gridRect.offsetMax = Vector2.zero;
                    gridRect.pivot = new Vector2(0.5f, 0.5f);
                    gridRect.anchoredPosition = Vector2.zero;
                    gridObject.AddComponent<RectMask2D>();
                    gridBuilder = gridObject.AddComponent<DungeonMapGridBuilder>();
                }
                Destroy(legacyBuilder);
            }

            var panelImage = mapContentRoot.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(1f, 1f, 1f, 0.004f);
                panelImage.raycastTarget = true;
            }
        }

        if (gridBuilder == null && mapContentRoot != null)
        {
            var gridObject = new GameObject("Grid", typeof(RectTransform));
            gridObject.transform.SetParent(mapContentRoot.transform, false);
            var gridRect = gridObject.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.5f, 0.5f);
            gridRect.anchorMax = new Vector2(0.5f, 0.5f);
            gridRect.pivot = new Vector2(0.5f, 0.5f);
            gridRect.sizeDelta = new Vector2(
                MapBoardPanelSettings.MapAreaWidth,
                MapBoardPanelSettings.PanelSize - MapBoardPanelSettings.MapAreaTopInset - MapBoardPanelSettings.MapAreaBottomInset);
            gridRect.anchoredPosition = new Vector2(
                0f,
                (MapBoardPanelSettings.MapAreaBottomInset - MapBoardPanelSettings.MapAreaTopInset) * 0.5f);
            gridObject.AddComponent<RectMask2D>();
            gridBuilder = gridObject.AddComponent<DungeonMapGridBuilder>();
        }

        if (gridBuilder == null) return;

        var allBuilders = mapContentRoot.GetComponentsInChildren<DungeonMapGridBuilder>(true);
        foreach (var b in allBuilders)
        {
            if (b != null && b != gridBuilder) Destroy(b);
        }

        gridBuilder.ConfigureForMapBoard(allowMarking);

        if (mapStagePrefab == null) mapStagePrefab = MinimapManager.ResolveMapStagePrefab();
        gridBuilder.SetCellPrefab(mapStagePrefab);

        if (markSpriteSet == null) markSpriteSet = MapMarkSpriteSet.LoadFromResources();
        gridBuilder.SetMarkSpriteSet(markSpriteSet);
    }

    void EnsurePanelLayout()
    {
        if (panelRoot == null || mapContentRoot == null) return;

        var panelRect = panelRoot.GetComponent<RectTransform>();
        if (panelRect != null) MinimapHudLayout.ApplyFullscreenPanel(panelRect);

        MapBoardPanelFactory.ApplyPanelBackground(panelRoot, warnOnFailure: false);

        var contentRect = mapContentRoot.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            MinimapHudLayout.ApplyCenteredMapContent(contentRect, MapBoardPanelSettings.PanelSize);
            if (mapContentRoot.GetComponent<RectMask2D>() == null) mapContentRoot.AddComponent<RectMask2D>();

            var panelImage = mapContentRoot.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(1f, 1f, 1f, 0.004f);
                panelImage.raycastTarget = true;
            }
        }

        if (isOpenedAsTownBoard)
        {
            contentRect.anchoredPosition = Vector2.zero;
        }
        else
        {
            float targetX = -contentRect.sizeDelta.x * 0.5f + MapBoardPanelSettings.MapAreaInsetX;
            float targetY = contentRect.sizeDelta.y * 0.5f - MapBoardPanelSettings.MapAreaTopInset;

            if (StageManager.instance != null && StageManager.instance.Tutorial)
                contentRect.anchoredPosition = new Vector2(targetX + 180, targetY - 60);
            else
                contentRect.anchoredPosition = new Vector2(targetX + 393, targetY - 267);
        }
    }

    void EnsureMarkingInput()
    {
        if (mapContentRoot == null || gridBuilder == null) return;
        var input = mapContentRoot.GetComponent<MapBoardMarkingInput>();
        if (input == null) input = mapContentRoot.AddComponent<MapBoardMarkingInput>();
        input.BindGrid(gridBuilder);
    }

    void EnsureMarkingToolbar(bool visible)
    {
        if (panelRoot == null) return;

        if (mapContentRoot != null)
        {
            var misplaced = mapContentRoot.transform.Find("MarkingToolbar");
            if (misplaced != null) Destroy(misplaced.gameObject);
        }

        var existingToolbar = panelRoot.transform.Find("MarkingToolbar");
        if (existingToolbar != null)
        {
            if (!visible)
            {
                markingToolbar = existingToolbar.gameObject;
                markingToolbar.SetActive(false);
                return;
            }
            Destroy(existingToolbar.gameObject);
            markingToolbar = null;
        }

        if (visible)
        {
            var sprites = markSpriteSet != null ? markSpriteSet : MapMarkSpriteSet.LoadFromResources();
            markingToolbar = MapBoardPanelFactory.CreateMarkingToolbar(panelRoot.transform, sprites, MarkToolbarLayout.MapBoard);
        }

        if (markingToolbar == null) return;

        markingToolbar.transform.SetAsLastSibling();
        markingToolbar.SetActive(visible);

        if (visible)
        {
            var toolbarRect = markingToolbar.GetComponent<RectTransform>();
            if (toolbarRect != null) toolbarRect.anchoredPosition += new Vector2(0f, 180f);
            RefreshToolbarSprites();
        }
    }

    void RefreshToolbarSprites()
    {
        if (markingToolbar == null) return;
        var sprites = markSpriteSet != null ? markSpriteSet : MapMarkSpriteSet.LoadFromResources();
        foreach (var markButton in markingToolbar.GetComponentsInChildren<MapMarkButton>(true))
            markButton.Configure(markButton.MarkType, sprites.GetSprite(markButton.MarkType));
    }
}
