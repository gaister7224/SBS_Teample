using System.Collections;
using UnityEngine;

public class ArrowStormSkill : MonoBehaviour
{
    private GameObject player;
    private PlayerProfile playerProfile;
    private PlayerAttack playerAttack;

    private float damage;

    private float stopDist = 10.0f;
    private float moveSpeed = 10.0f;
    private float rotateSpeed = 20.0f;
    private float targetDist = 6f;

    private Vector3 firstPos;
    private Vector3 targetPos;
    private float dist;

    private bool rush = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerProfile = player.GetComponent<PlayerProfile>();
        playerAttack = player.GetComponent<PlayerAttack>();

        firstPos = transform.position;
        targetPos = playerAttack.AttackPos.transform.position + (playerAttack.AttackPos.transform.up * targetDist);
        targetPos.y = transform.position.y;

        if (playerProfile != null)
        {
            bool critical = playerProfile.CriticalProbability();
            if (critical)
            {
                damage = playerProfile.CriticalBuff(playerProfile.ATK(150f));
            }
            else
                damage = playerProfile.ATK(150f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        dist = Vector3.Distance(firstPos, transform.position);

        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        if (dist > stopDist)
        {
            Destroy(gameObject);
        }

        //if (dist < 5 && !rush)
        //{

        //}
        //else if (dist >= 5 && !rush)
        //{
        //    rush = true;
        //}

        //if (dist < stopDist && rush)
        //{
        //    Vector3 dir = (targetPos - transform.position).normalized;
        //    if (dir != Vector3.zero)
        //    {
        //        Quaternion targetRotation = Quaternion.LookRotation(dir);
        //        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        //    }

        //    transform.position += transform.forward * moveSpeed * Time.deltaTime;

        //    if (Vector3.Distance(transform.position, targetPos) < 0.2f)
        //    {
        //        playerProfile.SkillStart = false;
        //        Destroy(gameObject);
        //    }
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            playerProfile.BowSkillHit(hitPoint);
            if (other.CompareTag("Boss"))
            {
                Debug.Log("��ų : ȭ�� ��ǳ" + other.gameObject.name + "��(��) �����߽��ϴ�!" + "damage = " + damage);
                other.gameObject.GetComponent<BossStatus>().GetDamage(damage);
            }
            else if (other.CompareTag("Enemy"))
            {
                Debug.Log("��ų : ȭ�� ��ǳ" + other.gameObject.name + "��(��) �����߽��ϴ�!" + "damage = " + damage);
                if (other.gameObject.GetComponent<MonsterBehavior>() != null)
                    other.gameObject.GetComponent<MonsterBehavior>().TakeDamage(damage);
                if (other.gameObject.GetComponent<SealStoneManager>() != null)
                    other.gameObject.GetComponent<SealStoneManager>().Damage(damage);
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
        enemyRb.AddForce(dist * 7f, ForceMode.Impulse);
        yield return new WaitForSeconds(0.5f);
        enemyRb.linearVelocity = Vector3.zero;
    }
}
