using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSkill : MonoBehaviour
{
    [SerializeField] private GameObject laser;
    private BoxCollider objCollider;

    private PlayerAttack playerAttack;
    private PlayerProfile playerProfile;
    private GameObject player;

    Quaternion firstRotation;

    private float angleDiff;
    private float clampdDiff;

    private float damage;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerAttack = player.GetComponent<PlayerAttack>();
        playerProfile = player.GetComponent<PlayerProfile>();
        objCollider = GetComponent<BoxCollider>();
        objCollider.enabled = false;

        if (playerProfile != null)
        {
            playerProfile.ChangeMoveSpeed(-100f);
            bool critical = playerProfile.CriticalProbability();
            if (critical)
                damage = playerProfile.CriticalBuff(playerProfile.ATK(190));
            else
                damage = playerProfile.ATK(190);

            playerAttack.AttackPosRotation(-99.5f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(LaserStart());
    }

    IEnumerator LaserStart()
    {
        firstRotation = transform.rotation;
        playerProfile.CameraZoom(2, 5, 45);
        yield return new WaitForSeconds(2);
        objCollider.enabled = true;
        laser.SetActive(true);
        angleDiff = Mathf.DeltaAngle(firstRotation.y, playerAttack.AttackPos.transform.eulerAngles.y);
        clampdDiff = Mathf.Clamp(angleDiff, -25f, 25f);
        transform.rotation = Quaternion.Euler(transform.rotation.x,
             firstRotation.y + clampdDiff, transform.rotation.z);
        yield return new WaitForSeconds(3);
        playerProfile.ChangeMoveSpeed(0f);
        playerAttack.AttackPosRotation(0f);
        playerProfile.SkillStart = false;
        Destroy(gameObject);
    }

    private Dictionary<GameObject, float> enemyTimers = new Dictionary<GameObject, float>();
    private float attackTime = 0.3f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            playerProfile.ShakeCamera(0.2f, 3.0f, 15.0f);
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            playerProfile.StampSkillHit(hitPoint);
            if (other.CompareTag("Boss"))
            {
                Debug.Log("스킬 : 레이저" + other.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                other.gameObject.GetComponent<BossStatus>().GetDamage(damage);
            }
            else if (other.CompareTag("Enemy"))
            {
                Debug.Log("스킬 : 레이저" + other.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                if (other.gameObject.GetComponent<MonsterBehavior>() != null)
                    other.gameObject.GetComponent<MonsterBehavior>().TakeDamage(damage);
                if (other.gameObject.GetComponent<SealStoneManager>() != null)
                    other.gameObject.GetComponent<SealStoneManager>().Damage(damage);
                if (other.gameObject.GetComponent<SealedStone>() != null)
                    other.gameObject.GetComponent<SealedStone>().TakeDamage(damage);
            }
            if (playerProfile.BloodHeal)
                playerProfile.BloodHealHp(10, damage);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            GameObject enemy = other.gameObject;

            if (!enemyTimers.ContainsKey(enemy))
            {
                enemyTimers.Add(enemy, 0f);
            }

            enemyTimers[enemy] += Time.deltaTime;
            if (enemyTimers[enemy] > attackTime)
            {
                if (other.CompareTag("Boss"))
                {
                    Debug.Log("스킬 : 레이저" + other.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                    other.gameObject.GetComponent<BossStatus>().GetDamage(damage);
                }
                else if (other.CompareTag("Enemy"))
                {
                    Debug.Log("스킬 : 레이저" + other.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                }
                if (playerProfile.BloodHeal)
                    playerProfile.BloodHealHp(10, damage);
                enemyTimers[enemy] = 0;
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (enemyTimers.ContainsKey(other.gameObject))
            {
                enemyTimers.Remove(other.gameObject);
            }
        }
    }
}
