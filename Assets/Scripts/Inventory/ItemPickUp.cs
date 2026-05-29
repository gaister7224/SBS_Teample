using UnityEngine;

/// <summary>
/// Item을 넣는 공간 프리펩에 컴포넌트로 추가하고 인스펙터에 아이템을 할당
/// </summary>

public class ItemPickUp : MonoBehaviour
{
    public bool canPickUp = false;
    private void Start()
    {
        Invoke("EnablePickUp", 1);
    }

    private void EnablePickUp()
    {
        canPickUp = true;
    }
    [Header("해당 오브젝트에 할당되는 아이템")]
    [SerializeField] private Item item;
    /// <summary>
    /// 상호작용 가능한 객체가 가지고 있는 아이템
    /// /// </summary>
    /// <value></value>
    public Item Item
    {
        get
        {
            return item;
        }
    }

    [Header("해당 오브젝트에 상호작용 시, 보유줄 인디케이터의 높이")]
    [SerializeField] private float indicatorHeight;

    public float IndicatorHeight
    {
        get
        {
            return indicatorHeight;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Place"))
        {
            GameObject shadow = transform.GetChild(1).gameObject;
            if (shadow != null)
            {
                shadow.SetActive(true);
            }
        }
    }
}
