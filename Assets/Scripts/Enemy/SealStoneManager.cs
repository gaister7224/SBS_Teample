using NUnit.Framework.Interfaces;
using UnityEngine;

public class SealStoneManager : MonoBehaviour
{
    [SerializeField] private float curHp;
    [SerializeField] private float maxHp;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite[] sprites;

    public ChestData chestData;

    [SerializeField] private GameObject[] itemObjects;
    private bool itemSpawn = false;

    [SerializeField] private bool fakeSealStone;

    private BoxCollider boxCollider;

    [SerializeField] StageManager stageManager;

    private void Awake()
    {
        
    }
    private void Start()
    {
        stageManager = StageManager.instance;

        if (stageManager == null)
        {
            stageManager = GameObject.FindWithTag("Map").GetComponent<StageManager>();
        }

        stageManager.SealedStoneLeft++;
        boxCollider = GetComponent<BoxCollider>();
    }
    private void Update()
    {
        if (curHp <= 0)
        {
            spriteRenderer.sprite = sprites[2];
            if (fakeSealStone && !itemSpawn)
            {
                for (int i = 0; i < 4; i++)
                {
                    itemObjects[i] = RollByChance();
                    GameObject newItem = Instantiate(itemObjects[i], transform.position, Quaternion.identity);
                    Rigidbody rb = newItem.GetComponent<Rigidbody>();

                    if (rb != null)
                    {
                        Vector2 randomCircle = Random.insideUnitCircle.normalized;

                        float spread = 0.5f;

                        Vector3 jumpDir = new Vector3(randomCircle.x * spread, 1.5f, randomCircle.y * spread).normalized;
                        rb.AddForce(jumpDir * 5f, ForceMode.Impulse);
                    }

                }
                itemSpawn = true;
            }
            if (!fakeSealStone)
            {
                stageManager.SealedStoneLeft--;
            }
            this.enabled = false;
            boxCollider.enabled = false;
            stageManager.curStageCleared = true;
        }
        if (curHp <= maxHp * 0.8f && curHp >= maxHp * 0.4f)
        {
            spriteRenderer.sprite = sprites[0];
        }
        else if (curHp <= maxHp * 0.4f && curHp > 0)
        {
            spriteRenderer.sprite = sprites[1];
        }
    }
    public void Damage(float damage)
    {
        curHp -= damage;
    }

    private GameObject RollByChance()
    {
        float roll = Random.Range(0f, 100f);
        float cumulativeChance = 0f;

        //어떤 등급이 당첨되었는지
        foreach (var pool in chestData.itemPools)
        {
            cumulativeChance += pool.dropChance;
            if (roll <= cumulativeChance)
            {
                //당첨된 등급의 아이템 리스트 중 하나를 무작위로 반환
                if (pool.items.Count > 0)
                {
                    int randomIndex = Random.Range(0, pool.items.Count);
                    return pool.items[randomIndex];
                }
            }
        }
        return null;
    }
}
