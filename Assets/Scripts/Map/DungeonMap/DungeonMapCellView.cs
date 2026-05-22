using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
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
    [SerializeField] float playerPinScale = 0.35f;

    public Vector2Int Index { get; private set; }

    Button button;
    MapMarkSpriteSet spriteSet;
    bool isRevealed;
    bool isSelected;
    bool showPlayer;

    void Awake()
    {
        button = GetComponent<Button>();
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (markIcon == null)
            markIcon = CreateChildImage("MarkIcon", 14f);

        if (selectionOutline == null)
            selectionOutline = CreateChildImage("SelectionOutline", 22f);

        if (playerPin == null)
            playerPin = CreatePlayerPin();

        if (revealedImage == null)
            revealedImage = backgroundImage;

        markIcon.enabled = false;
        selectionOutline.enabled = false;
        playerPin.enabled = false;

        button.onClick.AddListener(OnClicked);

        var nav = button.navigation;
        nav.mode = Navigation.Mode.None;
        button.navigation = nav;
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
        var pin = CreateChildImage("PlayerPin", 8f);
        pin.color = playerPinColor;
        return pin;
    }

    void UpdatePlayerPinVisual(bool visible)
    {
        if (playerPin == null)
            return;

        playerPin.enabled = visible;
        if (!visible)
            return;

        playerPin.color = playerPinColor;
        var cellRect = transform as RectTransform;
        if (cellRect != null)
        {
            var pinSize = Mathf.Min(cellRect.sizeDelta.x, cellRect.sizeDelta.y) * playerPinScale;
            pinSize = Mathf.Max(pinSize, 6f);
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

    public void Refresh(DungeonMapData data, Vector2Int? selectedCell, MapMarkSpriteSet marks,
        bool showPlayerPin = true, bool allowInteraction = true)
    {
        if (marks != null)
            spriteSet = marks;

        isRevealed = data != null && data.IsRevealed(Index);
        isSelected = selectedCell.HasValue && selectedCell.Value == Index;
        showPlayer = showPlayerPin && data != null && data.PlayerPosition.HasValue && data.PlayerPosition.Value == Index;

        if (revealedImage != null)
            revealedImage.color = isRevealed ? revealedColor : hiddenColor;

        if (backgroundImage != null && backgroundImage != revealedImage)
            backgroundImage.color = isRevealed ? revealedColor : hiddenColor;

        var canInteract = allowInteraction && isRevealed && DungeonMapService.Instance != null
            && !DungeonMapService.Instance.IsReadOnly;
        button.interactable = canInteract;

        if (backgroundImage != null)
            backgroundImage.raycastTarget = canInteract;

        if (markIcon != null)
        {
            if (data != null && data.TryGetMark(Index, out var markType) && spriteSet != null)
            {
                var sprite = spriteSet.GetSprite(markType);
                markIcon.sprite = sprite;
                markIcon.enabled = sprite != null;
            }
            else
            {
                markIcon.enabled = false;
            }
        }

        if (selectionOutline != null)
            selectionOutline.enabled = isSelected;

        UpdatePlayerPinVisual(showPlayer);
    }

    public void PlayMarkFeedback()
    {
        StopAllCoroutines();
        StartCoroutine(MarkFeedbackRoutine());
    }

    IEnumerator MarkFeedbackRoutine()
    {
        if (markIcon != null)
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

    void OnClicked()
    {
        if (DungeonMapService.Instance == null || !isRevealed)
            return;

        DungeonMapService.Instance.SelectCell(Index);
    }
}
