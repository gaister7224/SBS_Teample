using System.Collections;
using UnityEngine;

public class MonsterBehavior : MonoBehaviour
{
    MonsterSpawnManager monsterSpawnManager;
    Rigidbody rb;
    bool isMoving = false;
    [SerializeField] MonsterStatData MonsterData;
    [SerializeField] GameObject SectorAttackAoE;
    [SerializeField] GameObject SectorAttackEffect;
    [SerializeField] GameObject ThrustAttackAoE;
    [SerializeField] GameObject ThrustAttackEffect;

    [SerializeField] float attackTimer = 0f;
    [SerializeField] bool isAttacking;

    public GameObject Player;
    private bool itemSpawn;

    private void Awake()
    {
        monsterSpawnManager = GameObject.Find("MonsterSpawnManager").GetComponent<MonsterSpawnManager>();
        MonsterData.CurHP = MonsterData.MaxHP;
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (MonsterData.CurHP <= 0)
        {
            if (!itemSpawn)
            {
                monsterSpawnManager.MonsterDead(gameObject);
                itemSpawn = true;
            }
            Invoke("ObjDestroy", 0.5f);
        }

        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }

        if (!isMoving)
        {
            StartCoroutine(Move());
        }

        if (Player.GetComponent<PlayerProfile>().PlayerDeadCheck())
        {
            Destroy(gameObject);
        }
    }

    private void ObjDestroy()
    {
        Destroy(gameObject);
    }

    IEnumerator Move()
    {
        isMoving = true;
        int randomAttack = Random.Range(0, 10);

        if (Vector3.Distance(transform.localPosition, Player.transform.localPosition) <= MonsterData.AttackRange && !isAttacking && attackTimer >= MonsterData.AttackDelay)
        {
            isAttacking = true;
            if (randomAttack >= 0 && randomAttack <= 2)
            {
                StartCoroutine(ThrustAttack());
            }
            else
            {
                StartCoroutine(SectorAttack());
            }

            yield return new WaitForSeconds(0f);
        }
        
        if (!isAttacking && Vector3.Distance(transform.localPosition, Player.transform.localPosition) > MonsterData.AttackRange)
        {
            MoveToPlayer();
            GetComponentInChildren<SkeletonMonsterImage>().animstate = AnimState.Walk;

            if (Player.transform.position.x - transform.position.x > 0)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
                //transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
                //transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
        else if (rb.linearVelocity.magnitude <= 0f && !isAttacking)
        {
            GetComponentInChildren<SkeletonMonsterImage>().animstate = AnimState.Idle;
        }

        attackTimer += Time.deltaTime;
        isMoving = false;
    }

    void MoveToPlayer()
    {
        Vector3 direction = Player.transform.position - transform.position;
        direction.y = 0f;
        float randomMoveSpeed = Random.Range(MonsterData.MoveSpeed * 0.8f, MonsterData.MoveSpeed * 1.2f);

        if (direction != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(direction);
            transform.rotation = rot;
        }

        transform.Translate(Vector3.forward * Time.deltaTime * randomMoveSpeed);
    }

    IEnumerator ThrustAttack()
    {
        Sprite sprite = ThrustAttackEffect.GetComponent<SpriteRenderer>().sprite;

        GetComponentInChildren<SkeletonMonsterImage>().animstate = AnimState.Idle;
        ThrustAttackEffect.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        GetComponentInChildren<SkeletonMonsterImage>().animstate = AnimState.Attack;
        yield return new WaitForSeconds(0.3f);
        ThrustAttackEffect.SetActive(false);
        ThrustAttackAoE.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        ThrustAttackAoE.SetActive(false);

        isAttacking = false;
        attackTimer = 0f;
    }

    IEnumerator SectorAttack()
    {
        Sprite sprite = SectorAttackEffect.GetComponent<SpriteRenderer>().sprite;

        GetComponentInChildren<SkeletonMonsterImage>().animstate = AnimState.Idle;
        SectorAttackEffect.SetActive(true);
        yield return new WaitForSeconds(1f);
        GetComponentInChildren<SkeletonMonsterImage>().animstate = AnimState.Attack;
        yield return new WaitForSeconds(0.3f);
        SectorAttackEffect.SetActive(false);
        SectorAttackAoE.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        SectorAttackAoE.SetActive(false);

        isAttacking = false;
        attackTimer = 0f;
    }

    public void TakeDamage(float damage)
    {
        MonsterData.CurHP -= damage;
    }

    private void OnTriggerEnter(Collider other)
    {
    }
}
