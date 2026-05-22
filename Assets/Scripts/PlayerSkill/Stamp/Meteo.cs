using UnityEngine;

public class Meteo : MonoBehaviour
{
    private PlayerProfile playerProfile;

    private float damage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>(); ;

        if (playerProfile != null)
        {
            bool critical = playerProfile.CriticalProbability();
            if (critical)
                damage = playerProfile.CriticalBuff(playerProfile.ATK(900));
            else
                damage = playerProfile.ATK(900);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            playerProfile.ShakeCamera(0.2f, 3.0f, 15.0f);
            if (other.CompareTag("Boss"))
            {
                Debug.Log("스킬 : 메테오" + other.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                other.gameObject.GetComponent<BossStatus>().GetDamage(damage);
            }
            else if (other.CompareTag("Enemy"))
            {
                Debug.Log("스킬 : 메테오" + other.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                if (other.gameObject.GetComponent<MonsterBehavior>() != null)
                    other.gameObject.GetComponent<MonsterBehavior>().TakeDamage(damage);
                if (other.gameObject.GetComponent<SealStoneManager>() != null)
                    other.gameObject.GetComponent<SealStoneManager>().Damage(damage);
                if (other.gameObject.GetComponent<SealedStone>() != null)
                    other.gameObject.GetComponent<SealedStone>().TakeDamage(damage);
                //기절상태 추가
            }
            playerProfile.SkillStart = false;
            Destroy(gameObject);
            if (playerProfile.BloodHeal)
                playerProfile.BloodHealHp(10, damage);
        }
        if (other.CompareTag("Place"))
        {
            playerProfile.ShakeCamera(0.2f, 3.0f, 15.0f);
            playerProfile.SkillStart = false;
            Destroy(gameObject);
        }
    }
}
