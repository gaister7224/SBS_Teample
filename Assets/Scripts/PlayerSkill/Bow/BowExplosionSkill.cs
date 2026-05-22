using UnityEngine;

public class BowExplosionSkill : MonoBehaviour
{
    private PlayerProfile playerProfile;

    private float damage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerProfile = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerProfile>();

        if (playerProfile != null)
        {
            bool critical = playerProfile.CriticalProbability();
            if (critical)
                damage = playerProfile.CriticalBuff(playerProfile.BasicATK(60));
            else
                damage = playerProfile.BasicATK(60);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, 1);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            if (other.CompareTag("Boss"))
            {
                Debug.Log("스킬 : 활 폭발" + other.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                other.gameObject.GetComponent<BossStatus>().GetDamage(damage);
            }
            else if (other.CompareTag("Enemy"))
            {
                Debug.Log("스킬 : 활 폭발" + other.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
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
}
