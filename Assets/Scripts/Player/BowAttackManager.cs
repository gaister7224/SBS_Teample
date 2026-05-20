using Unity.VisualScripting;
using UnityEngine;

public class BowAttackManager : MonoBehaviour
{
    private Vector3 startPos;
    private float distance;

    [SerializeField] private string hitTag;
    [SerializeField] private GameObject bowExplosionObj;
    [SerializeField] private GameObject hitPrefab;

    private PlayerAttack playerAttack;
    private PlayerProfile playerProfile;

    private float damage1;
    private float damage2;
    private int throughCount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerAttack = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAttack>();
        playerProfile = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerProfile>();
        startPos = transform.position;

        if (!playerAttack.bowPassiveSkill3)
        {
            bool critical = playerProfile.CriticalProbability();
            if (critical)
            {
                damage1 = playerProfile.CriticalBuff(playerProfile.BasicATK(300));
                damage2 = playerProfile.CriticalBuff(playerProfile.BasicATK(200));
            }
            else
            {
                damage1 = playerProfile.BasicATK(300);
                damage2 = playerProfile.BasicATK(200);
            }
        }
        else if (playerAttack.bowPassiveSkill3)
        {
            if (playerProfile.CriticalProbability())
            {
                damage1 = playerProfile.CriticalBuff(playerProfile.BasicATK(350));
                damage2 = playerProfile.CriticalBuff(playerProfile.BasicATK(230));
            }
            else
            {
                damage1 = playerProfile.BasicATK(350);
                damage2 = playerProfile.BasicATK(230);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.up * playerAttack.power * Time.deltaTime;

        distance = Vector3.Distance(startPos, transform.position);

        if (distance > playerAttack.shotDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            if (playerAttack.bowExplosion)
            {
                int number = Random.Range(1, 101);
                Debug.Log("BowExplosionRandomNumber : " + number);
                if (number > 0 && number <= 30)
                {
                    Instantiate(bowExplosionObj, transform.position, transform.rotation);
                }
            }

            if (playerAttack.through)
            {
                throughCount++;
                if (throughCount == 1)
                {
                    if (other.CompareTag("Boss"))
                    {
                        Debug.Log("궁수 기본 공격" + other.gameObject.name + "을(를) 공격했습니다!" + "damage1 = " + damage1);
                        other.gameObject.GetComponent<BossStatus>().GetDamage(damage1);
                    }
                    else if (other.CompareTag("Enemy"))
                    {
                        Debug.Log("궁수 기본 공격" + other.gameObject.name + "을(를) 공격했습니다!" + "damage1 = " + damage1);
                        other.gameObject.GetComponent<MonsterBehavior>().TakeDamage(damage1);
                        other.gameObject.GetComponent<SealStoneManager>().Damage(damage1);
                        other.gameObject.GetComponent<SealedStone>().TakeDamage(damage1);
                    }
                    Vector3 hitPoint = other.ClosestPoint(transform.position);
                    Instantiate(hitPrefab, hitPoint, Quaternion.identity);
                }
                else if (throughCount == 2)
                {
                    if (other.CompareTag("Boss"))
                    {
                        Debug.Log("궁수 기본 공격" + other.gameObject.name + "을(를) 공격했습니다!" + "damage2 = " + damage2);
                        other.gameObject.GetComponent<BossStatus>().GetDamage(damage2);
                    }
                    else if (other.CompareTag("Enemy"))
                    {
                        Debug.Log("궁수 기본 공격" + other.gameObject.name + "을(를) 공격했습니다!" + "damage2 = " + damage2);
                        other.gameObject.GetComponent<MonsterBehavior>().TakeDamage(damage2);
                        other.gameObject.GetComponent<SealStoneManager>().Damage(damage2);
                        other.gameObject.GetComponent<SealedStone>().TakeDamage(damage2);
                    }
                    Vector3 hitPoint = other.ClosestPoint(transform.position);
                    Instantiate(hitPrefab, hitPoint, Quaternion.identity);
                    Destroy(gameObject);
                }
            }
            else
            {
                if (other.CompareTag("Boss"))
                {
                    Debug.Log("궁수 기본 공격" + other.gameObject.name + "을(를) 공격했습니다!" + "damage1 = " + damage1);
                    other.gameObject.GetComponent<BossStatus>().GetDamage(damage1);
                }
                else if (other.CompareTag("Enemy"))
                {
                    Debug.Log("궁수 기본 공격" + other.gameObject.name + "을(를) 공격했습니다!" + "damage1 = " + damage1);
                    if (other.gameObject.GetComponent<MonsterBehavior>() != null)
                        other.gameObject.GetComponent<MonsterBehavior>().TakeDamage(damage1);
                    if (other.gameObject.GetComponent<SealStoneManager>() != null)
                        other.gameObject.GetComponent<SealStoneManager>().Damage(damage1);
                    if (other.gameObject.GetComponent<SealedStone>() != null)
                        other.gameObject.GetComponent<SealedStone>().TakeDamage(damage1);
                }
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Instantiate(hitPrefab, hitPoint, Quaternion.identity);
                Destroy(gameObject);
            }
        }

        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
