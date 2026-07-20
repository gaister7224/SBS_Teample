using UnityEngine;

public class SealedStone : MonoBehaviour
{
    StageManager stageManager;
    PortalManager portalManager;
    [SerializeField] float HP;
    [SerializeField] float MaxHP;

    bool destroyed = false;

    private void Awake()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();

        portalManager = GetComponentInParent<PortalManager>();
        stageManager.SealedStoneLeft++;
    }

    void Start()
    {

    }

    void Update()
    {
        if (stageManager == null || destroyed)
            return;

        if (HP <= 0)
        {
            destroyed = true;
            stageManager.SealedStoneLeft--;
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        HP -= damage;
    }
}