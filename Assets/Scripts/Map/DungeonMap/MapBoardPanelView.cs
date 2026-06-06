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
    [SerializeField] List<Button> dungeonButtons;
    [SerializeField] TMPro.TMP_Dropdown floorDropdown;

    int selectedDungeonIndex = 0;

    // 마을 보관함 static 캐시 구조
    private static Dictionary<int, HashSet<Vector2Int>> townMapRevealedCache = new Dictionary<int, HashSet<Vector2Int>>();
    private bool isOpenedAsTownBoard = false;

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

    public void OpenMapBoard()
    {
        gameObject.SetActive(true);
        ActiveMarkingPanel = this;
        isOpenedAsTownBoard = true;

        BackupDungeonDataToTownCache();

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

    private void BackupDungeonDataToTownCache()
    {
        if (DungeonMapService.Instance != null && DungeonMapService.Instance.Current != null)
        {
            var curData = DungeonMapService.Instance.Current;
            if (curData.Revealed != null && curData.Revealed.Count > 0)
            {
                townMapRevealedCache[curData.DungeonId] = new HashSet<Vector2Int>(curData.Revealed);
                Debug.Log($"[마을 캐싱] 던전 고유 ID {curData.DungeonId} 상태 백업 완료.");

                //인스펙터 업데이트용으로 ID 리스트 갱신
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
    }

    void OnDungeonSelected(int dungeonIndex)
    {
        selectedDungeonIndex = dungeonIndex;
        int currentFloor = GetCurrentSelectedFloor();

        // 고유 ID 결합 공식 동기화
        int calculatedId = (dungeonIndex == 0) ? currentFloor : (dungeonIndex * 100) + currentFloor;

        Debug.Log($"[마을 게시판 UI] 선택 던전 ID: {calculatedId}");

        if (DungeonMapService.Instance != null)
        {
            DungeonMapService.Instance.EnsureLoaded(calculatedId);

            if (townMapRevealedCache.ContainsKey(calculatedId))
            {
                DungeonMapService.Instance.Current.Revealed.Clear();
                foreach (var cell in townMapRevealedCache[calculatedId])
                {
                    DungeonMapService.Instance.Current.Revealed.Add(cell);
                }
            }
        }

        RefreshCurrentMap();
    }

    void OnFloorChanged(int dropdownIndex) => OnDungeonSelected(selectedDungeonIndex);

    public void RefreshCurrentMap()
    {
        if (gridBuilder == null) return;

        ClearExistingCells();

        gridBuilder.BindServiceEventsForPanel();
        gridBuilder.ForceRebuild();

        // 🚀 [복구 및 정밀화] 가본 방만 걸러내는 필터를 다시 안전하게 적용합니다.
        if (isOpenedAsTownBoard)
        {
            PostProcessTownMapVisibility();
            CancelInvoke(nameof(PostProcessTownMapVisibility));
            Invoke(nameof(PostProcessTownMapVisibility), 0.02f);
        }
    }

    private void ClearExistingCells()
    {
        if (gridBuilder == null) return;

        for (int i = gridBuilder.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = gridBuilder.transform.GetChild(i);
            if (child.name != "MarkingToolbar")
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// 플레이어가 실제로 탐험하며 밝힌(Revealed) 방 정보만 필터링하여 온전히 켜주는 핵심 로직
    /// </summary>
    private void PostProcessTownMapVisibility()
    {
        if (DungeonMapService.Instance == null || DungeonMapService.Instance.Current == null || gridBuilder == null)
            return;

        var data = DungeonMapService.Instance.Current;
        bool hasRevealedData = data.Revealed != null && data.Revealed.Count > 0;

        foreach (Transform child in gridBuilder.transform)
        {
            var cellView = child.GetComponent<DungeonMapCellView>();
            if (cellView == null) continue;

            Vector2Int cellPos = GetCellPositionFromComponent(cellView, child.name);

            // 세이브 데이터에 존재하는 좌표의 방 오브젝트만 활성화 처리합니다.
            if (hasRevealedData && data.Revealed.Contains(cellPos))
            {
                child.gameObject.SetActive(true);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private Vector2Int GetCellPositionFromComponent(DungeonMapCellView cellView, string defaultName)
    {
        var type = cellView.GetType();
        string[] possibleFields = { "cellPos", "position", "pos", "Coordinate", "coordinate" };
        foreach (var fieldName in possibleFields)
        {
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null && field.FieldType == typeof(Vector2Int)) return (Vector2Int)field.GetValue(cellView);

            var prop = type.GetProperty(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prop != null && prop.PropertyType == typeof(Vector2Int)) return (Vector2Int)prop.GetValue(cellView);
        }

        try
        {
            string[] tokens = defaultName.Split('_');
            if (tokens.Length >= 3)
            {
                return new Vector2Int(int.Parse(tokens[1]), int.Parse(tokens[2]));
            }
        }
        catch { }

        return Vector2Int.zero;
    }

    int GetCurrentSelectedFloor() => floorDropdown != null ? floorDropdown.value : 0;

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

        int inGameDungeonId = 1;
        if (StageManager.instance != null)
        {
            if (StageManager.instance.Tutorial)
            {
                inGameDungeonId = GameManager.instance.curDungeonFloorNumber;
            }
            else
            {
                int dungeonIndex = GameManager.instance.curDungeonNumber;
                inGameDungeonId = (dungeonIndex * 100) + GameManager.instance.curDungeonFloorNumber;
            }
        }
        else
        {
            inGameDungeonId = dungeonIdOverride > 0 ? dungeonIdOverride : DungeonMapService.Instance.GetCurrentDungeonId();
        }

        Debug.Log($"[인게임 M키] 트래킹 ID: {inGameDungeonId}");
        DungeonMapService.Instance.EnsureLoaded(inGameDungeonId);

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

        EnsureMarkingToolbar(!readOnly);
        EnsureGrid(readOnly);

        if (!readOnly)
            ActiveMarkingPanel = this;

        if (gridBuilder != null)
        {
            gridBuilder.ConfigureForMapBoard(allowMarking: !readOnly);

            var serializedObj = new UnityEditor.SerializedObject(gridBuilder);
            var showPinProp = serializedObj.FindProperty("showPlayerPin");
            if (showPinProp != null)
            {
                showPinProp.boolValue = true;
                serializedObj.ApplyModifiedProperties();
            }

            gridBuilder.BindServiceEventsForPanel();
            gridBuilder.ForceRebuild();
            built = gridBuilder.HasCells;
            EnsurePanelLayout();
            gridBuilder.RefreshAll();
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
