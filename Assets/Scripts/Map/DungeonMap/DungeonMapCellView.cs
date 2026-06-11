using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DungeonMapCellView : MonoBehaviour
{
    [SerializeField] Image backgroundImage;
    [SerializeField] Image revealedImage;
    [SerializeField] Image markIcon;
    [SerializeField] Image selectionOutline;
    [SerializeField] Image playerPin;
    [SerializeField] Color hiddenColor = new(0.92f, 0.9f, 0.85f, 1f);
    [SerializeField] Color revealedColor = new(0.55f, 0.52f, 0.48f, 1f);
    [SerializeField] Color selectedFlashColor = new(1f, 0.95f, 0.6f, 1f);
    [SerializeField] Color playerPinColor = Color.red;
    [SerializeField] float playerPinScale = 0.55f;
    const float MarkIconSize = 28f * 1.2f;
    const float PlayerPinMinSize = 10f;

    public Vector2Int Index { get; private set; }

    MapMarkSpriteSet spriteSet;
    bool isRevealed;
    bool isSelected;
    bool showPlayer;

    void Awake()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        var legacyButton = GetComponent<Button>();
        if (legacyButton != null)
            Destroy(legacyButton);

        if (markIcon == null)
            markIcon = CreateChildImage("MarkIcon", MarkIconSize);

        if (selectionOutline == null)
            selectionOutline = CreateChildImage("SelectionOutline", 22f);

        if (playerPin == null)
            playerPin = CreatePlayerPin();

        if (revealedImage == null)
            revealedImage = backgroundImage;

        EnsureRaycastGraphic(backgroundImage);
        EnsureRaycastGraphic(revealedImage);

        markIcon.enabled = false;
        selectionOutline.enabled = false;
        playerPin.enabled = false;
    }

    Image CreateChildImage(string childName, float size)
    {
        var go = new GameObject(childName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(size, size);
        var img = go.GetComponent<Image>();
        img.raycastTarget = false;
        return img;
    }

    Image CreatePlayerPin()
    {
        var pin = CreateChildImage("PlayerPin", PlayerPinMinSize);
        pin.color = playerPinColor;
        EnsureRaycastGraphic(pin);
        return pin;
    }

    void UpdatePlayerPinVisual(bool visible)
    {
        if (playerPin == null)
            return;

        playerPin.enabled = visible;
        if (!visible)
            return;

        EnsureRaycastGraphic(playerPin);
        playerPin.color = playerPinColor;
        var cellRect = transform as RectTransform;
        if (cellRect != null)
        {
            var pinSize = Mathf.Min(cellRect.sizeDelta.x, cellRect.sizeDelta.y) * playerPinScale;
            pinSize = Mathf.Max(pinSize, PlayerPinMinSize);
            var pinRect = playerPin.rectTransform;
            pinRect.sizeDelta = new Vector2(pinSize, pinSize);
        }

        playerPin.transform.SetAsLastSibling();
    }

    public void Initialize(Vector2Int index, MapMarkSpriteSet marks)
    {
        Index = index;
        spriteSet = marks;
    }

    public void SetPlayerIcon(bool active)
    {
        playerPin.enabled = true;
    }

    public void Refresh(DungeonMapData data, Vector2Int? selectedCell, MapMarkSpriteSet marks,
        bool showPlayerPin = true, bool allowInteraction = true)
    {
        if (data == null) return;
        if (marks != null) spriteSet = marks;

        isRevealed = data.IsRevealed(Index);
        isSelected = selectedCell.HasValue && selectedCell.Value == Index;
        showPlayer = showPlayerPin && data.PlayerPosition.HasValue && data.PlayerPosition.Value == Index;

        // 색상 적용 (기존 로직)
        var targetColor = isRevealed ? revealedColor : hiddenColor;
        if (revealedImage != null) revealedImage.color = targetColor;
        if (backgroundImage != null && backgroundImage != revealedImage) backgroundImage.color = targetColor;

        // [중요] 마크 갱신 로직: 데이터에서 직접 가져오기
        if (data.TryGetMark(Index, out var markType))
        {
            SetMark(markType);
        }
        else
        {
            SetMark(null); // 데이터에 마크가 없으면 끄기
        }

        UpdatePlayerPinVisual(showPlayer);

        if (selectionOutline != null)
            selectionOutline.enabled = isSelected; // 선택된 셀 하이라이트

        if (backgroundImage != null) backgroundImage.raycastTarget = isRevealed;
    }

    public void PlayMarkFeedback()
    {
        StopAllCoroutines();
        StartCoroutine(MarkFeedbackRoutine());
    }

    IEnumerator MarkFeedbackRoutine()
    {
        if (markIcon != null && markIcon.enabled)
        {
            markIcon.transform.localScale = Vector3.zero;
            var elapsed = 0f;
            const float duration = 0.15f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var scale = t < 0.6f
                    ? Mathf.Lerp(0f, 1.1f, t / 0.6f)
                    : Mathf.Lerp(1.1f, 1f, (t - 0.6f) / 0.4f);
                markIcon.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            markIcon.transform.localScale = Vector3.one;
        }

        if (selectionOutline != null)
        {
            selectionOutline.enabled = true;
            selectionOutline.color = selectedFlashColor;
            yield return new WaitForSecondsRealtime(0.1f);
            selectionOutline.color = Color.white;
        }
    }

    public void SetVisibility(bool isVisible)
    {
        // 밝혀지지 않은 칸은 투명하게 하거나, 아주 연한 색으로 표시
        var img = GetComponent<Image>();
        img.color = isRevealed ? Color.white : new Color(0, 0, 0, 0.3f);
    }

    public void ApplyMarkClick()
    {
        var service = DungeonMapService.Instance;
        //if (service == null || service.IsReadOnly || !service.PendingMarkType.HasValue)
        //    return;

        if (service == null || service.IsReadOnly || !service.PendingMarkType.HasValue)
            return;

        var pending = service.PendingMarkType.Value;
        if (service.Current.TryGetMark(Index, out var existing) && existing == pending)
        {
            service.ClearMark(Index); // 내부에서 OnMapDataChanged 발생
        }
        else
        {
            service.ApplyMark(Index, pending); // 내부에서 OnMapDataChanged 발생
        }

        // 각 셀은 자기 자신의 애니메이션만 재생합니다.
        PlayMarkFeedback();
        //PlayMarkFeedbackOnAllGrids();
    }

    public void RemoveMark()
    {
        var service = DungeonMapService.Instance;
        if (service == null || service.IsReadOnly)
            return;

        service.ClearMark(Index);

        //RefreshAllGrids();
    }

    public void SelectIfRevealed()
    {
        if (!isRevealed || DungeonMapService.Instance == null)
            return;

        DungeonMapService.Instance.SelectCell(Index);
    }

    void ApplyMarkIconLayout()
    {
        if (markIcon == null)
            return;

        var cellRect = transform as RectTransform;
        var size = MarkIconSize;
        if (cellRect != null)
        {
            var cellSize = Mathf.Min(cellRect.sizeDelta.x, cellRect.sizeDelta.y);
            size = Mathf.Min(MarkIconSize, cellSize * 0.9f);
        }

        var iconRect = markIcon.rectTransform;
        iconRect.sizeDelta = new Vector2(size, size);
        iconRect.anchoredPosition = Vector2.zero;
        markIcon.preserveAspect = true;
        markIcon.transform.SetAsLastSibling();
    }

    static void EnsureRaycastGraphic(Image image)
    {
        if (image == null)
            return;

        if (image.sprite == null)
            image.sprite = MapUiSpriteUtil.White;
    }

    //void PlayMarkFeedbackOnAllGrids()
    //{
    //    var grids = UnityEngine.Object.FindObjectsByType<DungeonMapGridBuilder>(FindObjectsSortMode.None);
    //    foreach (var grid in grids)
    //        grid.GetCell(Index)?.PlayMarkFeedback();
    //}

    //void RefreshAllGrids()
    //{
    //    var grids = UnityEngine.Object.FindObjectsByType<DungeonMapGridBuilder>(FindObjectsSortMode.None);
    //    foreach (var grid in grids)
    //        grid.RefreshAll();
    //}

    public void SetMark(DungeonMapMarkType? markType)
    {
        if (markIcon == null) return;

        if (markType.HasValue && spriteSet != null)
        {
            var sprite = spriteSet.GetSprite(markType.Value);
            markIcon.sprite = sprite;
            markIcon.enabled = sprite != null;

            // 아이콘 크기 재조정
            ApplyMarkIconLayout();
        }
        else
        {
            markIcon.enabled = false;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}

