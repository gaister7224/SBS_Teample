using System.Collections;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.UI;

public class BossAttack : MonoBehaviour
{
    private GameObject player;
    private BehaviorGraphAgent agent;

    [SerializeField] private float dist;
    private float distRange = 10f;

    [SerializeField] private GameObject attackRange;
    [SerializeField] private GameObject skill1Prefab;
    [SerializeField] private GameObject skill2Prefab;
    [SerializeField] private GameObject skill3Prefab;
    [SerializeField] private GameObject skill4Prefab;
    [SerializeField] private GameObject skill5Prefab;

    [SerializeField] private Slider hpSlider;
    [SerializeField] private GameObject[] dropItems;

    public bool isAttacking = false;
    private int attack2Count = 0;
    private float range = 0;

    private bool spawn;

    private BossStatus bossStatus;
    private void Awake()
    {
        agent = GetComponent<BehaviorGraphAgent>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        bossStatus = GetComponent<BossStatus>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        dist = Vector3.Distance(transform.position, player.transform.position);

        agent.SetVariableValue("isTargetAppear", dist <= distRange);

        UpdateActCountBar();
        Die();
    }

    private void Die()
    {
        if(bossStatus.curHp <= 0)
        {
            GameObject map = GameObject.FindGameObjectWithTag("Map");
            if (!spawn)
            {
                for (int i = 0; i < dropItems.Length; i++)
                {
                    Instantiate(dropItems[i], transform.position, Quaternion.identity, map.transform);
                }
                spawn = true;
            }

            Invoke("ObjDestroy", 0.5f);
        }
    }

    private void ObjDestroy()
    {
        Destroy(gameObject);
    }

    private void UpdateActCountBar()
    {
        //acText.text = string.Format("{0} / {1}", curActCount, maxActCount);

        float _curActCount = (float)bossStatus.curHp / (float)bossStatus.maxHp;

        hpSlider.value = Mathf.Lerp(hpSlider.value, _curActCount, Time.deltaTime * 5);
    }
    public void Attack1()
    {
        StartCoroutine(Attack1Routine());
        agent.SetVariableValue("coolTime", 1.5f);
    }

    public void Attack2()
    {
        //StartCoroutine(Attack2Routine());
        attack2Count = 0;
        agent.SetVariableValue("coolTime", 1.5f);
        InvokeRepeating("Attack2Create", 0.1f, 0.1f);
    }

    public void Attack3()
    {
        StartCoroutine(Attack3Routine());
        agent.SetVariableValue("coolTime", 3.0f);
    }

    public void Attack4()
    {
        StartCoroutine(Attack4Routine());
        agent.SetVariableValue("coolTime", 1.5f);
    }
    public void Attack5()
    {
        StartCoroutine(Attack5Routine());
        agent.SetVariableValue("coolTime", 3.0f);
    }

    IEnumerator Attack1Routine()
    {
        isAttacking = true;
        GameObject[] newattackRange = new GameObject[4];
        newattackRange[0] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y, 0), transform);
        newattackRange[1] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y + 90, 0), transform);
        newattackRange[2] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y + 180, 0), transform);
        newattackRange[3] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y + 270, 0), transform);
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < newattackRange.Length; i++)
        {
            Destroy(newattackRange[i]);
        }
        int count = 0;
        while (count < 3)
        {
            Attack1Create(); // 화살 생성
            count++;
            yield return new WaitForSeconds(0.2f); // 0.2초 대기
        }
        isAttacking = false; //공격종료
    }
    private void Attack1Create()
    {
        Instantiate(skill1Prefab, transform.position, Quaternion.Euler(0, transform.position.y, 0));
        Instantiate(skill1Prefab, transform.position, Quaternion.Euler(0, transform.position.y + 90, 0));
        Instantiate(skill1Prefab, transform.position, Quaternion.Euler(0, transform.position.y + 180, 0));
        Instantiate(skill1Prefab, transform.position, Quaternion.Euler(0, transform.position.y + 270, 0));
    }

    //IEnumerator Attack2Routine()
    //{
    //    isAttacking = true;
    //    GameObject[] newattackRange = new GameObject[4];
    //    newattackRange[0] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y + 45, 0));
    //    newattackRange[1] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y + 135, 0));
    //    newattackRange[2] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y + 225, 0));
    //    newattackRange[3] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y + 315, 0));
    //    yield return new WaitForSeconds(1f);
    //    for (int i = 0; i < newattackRange.Length; i++)
    //    {
    //        Destroy(newattackRange[i]);
    //    }
    //    int count = 0;
    //    while (count < 3)
    //    {
    //        Attack2Create(); // 화살 생성
    //        count++;
    //        yield return new WaitForSeconds(0.2f); // 0.2초 대기
    //    }
    //    isAttacking = false; //공격종료
    //}
    private void Attack2Create()
    {
        isAttacking = true;

        Instantiate(skill2Prefab, transform.position, Quaternion.Euler(0, transform.position.y + range, 0));
        attack2Count++;
        range += 10;

        if(attack2Count >= 40)
        {
            isAttacking = false;
            CancelInvoke();
        }
        //Instantiate(skill2Prefab, transform.position, Quaternion.Euler(0, transform.position.y + 135, 0));
        //Instantiate(skill2Prefab, transform.position, Quaternion.Euler(0, transform.position.y + 225, 0));
        //Instantiate(skill2Prefab, transform.position, Quaternion.Euler(0, transform.position.y + 315, 0));
    }
    IEnumerator Attack3Routine()
    {
        isAttacking = true;
        GameObject newattackRange;
        newattackRange = Instantiate(attackRange, transform.position, Quaternion.identity);
        float timer = 0;
        float trackDuration = 2f;

        while (timer < trackDuration) //플레이어를 바라보게 함
        {
            if (player != null)
            {
                Vector3 direction = transform.position - player.transform.position;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    newattackRange.transform.rotation = Quaternion.LookRotation(direction);
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(newattackRange);
        Quaternion finalRotation = newattackRange.transform.rotation;
        int count = 0;
        while (count < 3)
        {
            Attack3Create(finalRotation); // 화살 생성
            count++;
            yield return new WaitForSeconds(0.2f); // 0.2초 대기
        }
        isAttacking = false; //공격종료
    }
    private void Attack3Create(Quaternion finalRotation)
    {
        Instantiate(skill3Prefab, transform.position, finalRotation);
    }

    IEnumerator Attack4Routine()
    {
        isAttacking = true;
        GameObject[] newattackRange = new GameObject[8];
        newattackRange[0] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y + 45, 0), transform);
        newattackRange[1] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y + 135, 0), transform);
        newattackRange[2] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y + 225, 0), transform);
        newattackRange[3] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y + 315, 0), transform);
        newattackRange[4] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y, 0), transform);
        newattackRange[5] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y + 90, 0), transform);
        newattackRange[6] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y + 180, 0), transform);
        newattackRange[7] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y + 270, 0), transform);
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < newattackRange.Length; i++)
        {
            Destroy(newattackRange[i]);
        }
        int count = 0;
        while (count < 3)
        {
            Attack4Create(); // 화살 생성
            count++;
            yield return new WaitForSeconds(0.2f); // 0.2초 대기
        }
        isAttacking = false; //공격종료
    }
    private void Attack4Create()
    {
        Instantiate(skill4Prefab, transform.position, Quaternion.Euler(0, transform.position.y + 45, 0));
        Instantiate(skill4Prefab, transform.position, Quaternion.Euler(0, transform.position.y + 135, 0));
        Instantiate(skill4Prefab, transform.position, Quaternion.Euler(0, transform.position.y + 225, 0));
        Instantiate(skill4Prefab, transform.position, Quaternion.Euler(0, transform.position.y + 315, 0));
        Instantiate(skill1Prefab, transform.position, Quaternion.Euler(0, transform.position.y, 0));
        Instantiate(skill1Prefab, transform.position, Quaternion.Euler(0, transform.position.y + 90, 0));
        Instantiate(skill1Prefab, transform.position, Quaternion.Euler(0, transform.position.y + 180, 0));
        Instantiate(skill1Prefab, transform.position, Quaternion.Euler(0, transform.position.y + 270, 0));
    }

    IEnumerator Attack5Routine()
    {
        isAttacking = true;
        GameObject[] newattackRange = new GameObject[3];
        newattackRange[0] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y, 0), transform);
        newattackRange[1] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y + 120, 0), transform);
        newattackRange[2] = Instantiate(attackRange, transform.position, Quaternion.Euler(0, transform.position.y + 240, 0), transform);
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < newattackRange.Length; i++)
        {
            Destroy(newattackRange[i]);
        }
        Attack5Create();
        yield return new WaitForSeconds(4f);
        isAttacking = false; //공격종료
    }
    private void Attack5Create()
    {
        Instantiate(skill5Prefab, transform.position, Quaternion.Euler(0, transform.position.y, 0));
        Instantiate(skill5Prefab, transform.position, Quaternion.Euler(0, transform.position.y + 120, 0));
        Instantiate(skill5Prefab, transform.position, Quaternion.Euler(0, transform.position.y + 240, 0));
    }

}
