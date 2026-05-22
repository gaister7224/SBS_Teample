using UnityEngine;

public class SplitFireSkill : MonoBehaviour
{
    private PlayerProfile playerProfile;

    private float stopDist = 13.0f;
    private float moveSpeed = 10.0f;

    private float damage;

    private int enemyHitCount = 0;

    private Vector3 firstPos;
    private float dist;
    void Start()
    {
        playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>();
        firstPos = transform.position;

        if (playerProfile != null)
        {
            bool critical = playerProfile.CriticalProbability();
            if (critical)
                damage = playerProfile.CriticalBuff(playerProfile.ATK(70f));
            else
                damage = playerProfile.ATK(70f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
        dist = Vector3.Distance(firstPos, transform.position);

        if (dist >= stopDist)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            //넉백추가
            enemyHitCount++;
            if (enemyHitCount == 1)
            {
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                playerProfile.BowSkillHit(hitPoint);
                if (other.CompareTag("Boss"))
                {
                    Debug.Log("스킬 : 분할 사격" + other.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                    other.gameObject.GetComponent<BossStatus>().GetDamage(damage);
                }
                else if (other.CompareTag("Enemy"))
                {
                    Debug.Log("스킬 : 분할 사격" + other.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                    if (other.gameObject.GetComponent<MonsterBehavior>() != null)
                        other.gameObject.GetComponent<MonsterBehavior>().TakeDamage(damage);
                    if (other.gameObject.GetComponent<SealStoneManager>() != null)
                        other.gameObject.GetComponent<SealStoneManager>().Damage(damage);
                    if (other.gameObject.GetComponent<SealedStone>() != null)
                        other.gameObject.GetComponent<SealedStone>().TakeDamage(damage);
                }
                if (playerProfile.BloodHeal)
                {
                    playerProfile.BloodHealHp(10, damage);
                }
            }
            else if (enemyHitCount == 2)
            {
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                playerProfile.BowSkillHit(hitPoint);
                if (other.CompareTag("Boss"))
                {
                    Debug.Log("스킬 : 분할 사격" + other.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                    other.gameObject.GetComponent<BossStatus>().GetDamage(damage);
                }
                else if (other.CompareTag("Enemy"))
                {
                    Debug.Log("스킬 : 분할 사격" + other.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                }
                if (playerProfile.BloodHeal)
                {
                    playerProfile.BloodHealHp(10, damage);
                }
                Destroy(gameObject);
            }
        }
        if (other.CompareTag("Wall") || other.CompareTag("Storage"))
        {
            Destroy(gameObject);
        }
    }
}
