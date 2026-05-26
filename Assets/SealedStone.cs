using UnityEngine;

public class SealedStone : MonoBehaviour
{
    StageManager stageManager;
    PortalManager portalManager;
    [SerializeField] float HP;
    [SerializeField] float MaxHP;

    private void Awake()
    {
        stageManager = StageManager.instance;

        if (stageManager == null)
        {
            Debug.LogError("[SealedStone] StageManager.instance 가 null입니다.");
            return;
        }

        portalManager = GetComponentInParent<PortalManager>();
        stageManager.SealedStoneLeft++;
    }

    void Start()
    {

    }

    void Update()
    {
        if (stageManager == null)
            return;

        if (HP <= 0)
        {
            stageManager.SealedStoneLeft--;
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        HP -= damage;
    }
}