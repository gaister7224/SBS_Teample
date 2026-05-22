using UnityEngine;

public class BumbManager : MonoBehaviour
{
    private PlayerAttack playerAttack;
    private PlayerProfile playerProfile;

    private float bombScale = 0.5f;
    private float bombStartScale;

    private float damage2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerAttack = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAttack>();
        playerProfile = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerProfile>();

        bombStartScale = bombScale;
        transform.localScale = new Vector3(bombStartScale, bombStartScale, bombStartScale);

        if (playerProfile != null)
        {
            if (!playerAttack.stampPassiveSkill1)
            {
                bool critical = playerProfile.CriticalProbability();
                if (critical)
                {
                    damage2 = playerProfile.CriticalBuff(playerProfile.BasicATK(180));
                }
                else
                {
                    damage2 = playerProfile.BasicATK(180);
                }
            }
            else if (playerAttack.stampPassiveSkill1)
            {
                bool critical = playerProfile.CriticalProbability();
                if (critical)
                {
                    damage2 = playerProfile.CriticalBuff(playerProfile.BasicATK(220));
                }
                else
                {
                    damage2 = playerProfile.BasicATK(220);
                }
            }
        }
        else
        {
            Debug.Log("playerprofile null");
        }

        if ((playerAttack.stampSkill6 && !playerAttack.stampPassiveSkill1)
        || (!playerAttack.stampSkill6 && playerAttack.stampPassiveSkill1))
        {
            IncreasedColliderSize(0.5f);
        }
        else if (playerAttack.stampSkill6 && playerAttack.stampPassiveSkill1)
        {
            IncreasedColliderSize(1.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreasedColliderSize(float size)
    {
        bombScale = bombStartScale + size;
        transform.localScale = new Vector3(bombScale, bombScale, bombScale);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boss"))
        {
            Debug.Log("스탬프 기본 폭발 공격" + other.gameObject.name + "을(를) 공격했습니다!" + "damage2 = " + damage2);
            other.gameObject.GetComponent<BossStatus>().GetDamage(damage2);
        }
        else if (other.CompareTag("Enemy"))
        {
            Debug.Log("스탬프 기본 폭발 공격" + other.gameObject.name + "을(를) 공격했습니다!" + "damage2 = " + damage2);
            if (other.gameObject.GetComponent<MonsterBehavior>() != null)
                other.gameObject.GetComponent<MonsterBehavior>().TakeDamage(damage2);
            if (other.gameObject.GetComponent<SealStoneManager>() != null)
                other.gameObject.GetComponent<SealStoneManager>().Damage(damage2);
        }
    }
}
