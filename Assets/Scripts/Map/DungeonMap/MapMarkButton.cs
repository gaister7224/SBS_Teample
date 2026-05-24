using UnityEngine;
using UnityEngine.UI;

public class MapMarkButton : MonoBehaviour
{
    static readonly Color NormalBg = new(1f, 1f, 1f, 1f);
    static readonly Color SelectedBg = new(1f, 0.92f, 0.55f, 1f);

    [SerializeField] DungeonMapMarkType markType;

    public DungeonMapMarkType MarkType => markType;
    [SerializeField] Image iconImage;
    [SerializeField] Image backgroundImage;

    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    void OnEnable()
    {
        if (DungeonMapService.Instance != null)
            DungeonMapService.Instance.OnPendingMarkChanged += OnPendingMarkChanged;

        OnPendingMarkChanged(DungeonMapService.Instance?.PendingMarkType);
    }

    void OnDisable()
    {
        if (DungeonMapService.Instance != null)
            DungeonMapService.Instance.OnPendingMarkChanged -= OnPendingMarkChanged;
    }

    public void Configure(DungeonMapMarkType type, Sprite sprite)
    {
        markType = type;

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (backgroundImage == null)
            return;

        backgroundImage.sprite = sprite;
        backgroundImage.preserveAspect = true;
        backgroundImage.color = NormalBg;
    }

    void OnClick()
    {
        if (DungeonMapService.Instance == null || DungeonMapService.Instance.IsReadOnly)
            return;

        var next = DungeonMapService.Instance.PendingMarkType == markType
            ? (DungeonMapMarkType?)null
            : markType;
        DungeonMapService.Instance.SetPendingMark(next);
    }

    void OnPendingMarkChanged(DungeonMapMarkType? activeType)
    {
        if (backgroundImage == null)
            return;

        var selected = activeType.HasValue && activeType.Value == markType;
        backgroundImage.color = selected
            ? SelectedBg
            : backgroundImage.sprite != null ? Color.white : NormalBg;
    }
}
