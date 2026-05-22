using System.Collections;
using UnityEngine;

public class StrongFireSkill : MonoBehaviour
{
    private PlayerProfile playerProfile;

    private float damage;

    private float stopDist = 25.0f;
    private float moveSpeed = 30.0f;

    private Vector3 firstPos;
    private float dist;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>();
        firstPos = transform.position;

        if (playerProfile != null)
        {
            bool critical = playerProfile.CriticalProbability();
            if (critical)
                damage = playerProfile.CriticalBuff(playerProfile.ATK(1600f));
            else
                damage = playerProfile.ATK(1600f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
        dist = Vector3.Distance(firstPos, transform.position);

        if (dist >= stopDist)
        {
            playerProfile.SkillStart = false;
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            playerProfile.BowSkillHit(hitPoint);
            playerProfile.ShakeCamera(0.2f, 3.0f, 15.0f);
            if (other.CompareTag("Boss"))
            {
                Debug.Log("��ų : ū�� �ѹ�" + other.gameObject.name + "��(��) �����߽��ϴ�!" + "damage = " + damage);
                other.gameObject.GetComponent<BossStatus>().GetDamage(damage);
            }
            else if (other.CompareTag("Enemy"))
            {
                Debug.Log("��ų : ū�� �ѹ�" + other.gameObject.name + "��(��) �����߽��ϴ�!" + "damage = " + damage);
                if (other.gameObject.GetComponent<MonsterBehavior>() != null)
                    other.gameObject.GetComponent<MonsterBehavior>().TakeDamage(damage);
                if (other.gameObject.GetComponent<SealStoneManager>() != null)
                    other.gameObject.GetComponent<SealStoneManager>().Damage(damage);
                if (other.gameObject.GetComponent<SealedStone>() != null)
                    other.gameObject.GetComponent<SealedStone>().TakeDamage(damage);

                StartCoroutine(NuckBack(other.GetComponent<Rigidbody>(), other));
            }
            if (playerProfile.BloodHeal)
            {
                playerProfile.BloodHealHp(10, damage);
            }
        }
        if (other.CompareTag("Wall") || other.CompareTag("Storage"))
        {
            //Destroy(gameObject);
        }
    }

    IEnumerator NuckBack(Rigidbody enemyRb, Collider enemy)
    {
        enemyRb.linearVelocity = Vector3.zero;
        Vector3 dist = enemy.transform.position - transform.position;
        enemyRb.AddForce(dist * 5f, ForceMode.Impulse);
        yield return new WaitForSeconds(0.5f);
        enemyRb.linearVelocity = Vector3.zero;
    }
}
