using UnityEngine;

/// <summary>
/// 레거시 마킹 버튼. 신규 UI는 MapMarkButton을 사용하세요.
/// </summary>
public class PingSelectButton : MonoBehaviour
{
    [SerializeField] MapMarkButton markButton;

    void Awake()
    {
        if (markButton == null)
            markButton = GetComponent<MapMarkButton>();
    }

    public void OnButtonClick()
    {
        // MapMarkButton이 Button.onClick에 연결되어 처리합니다.
    }
}
