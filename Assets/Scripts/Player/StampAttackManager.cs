using System.Collections;
using UnityEngine;

public class StampAttackManager : MonoBehaviour
{
    private Vector3 startPos;
    private float distance;
    private bool bomb = false;
    private bool bombStart = false;

    private SpriteRenderer sr;
    private PlayerAttack playerAttack;
    private PlayerProfile playerProfile;

    private float bombScale = 0.5f;
    private float bombStartScale;
    private float damage1;
    private float damage2;

    [SerializeField] private GameObject hitPrefab;
    [SerializeField] private GameObject bombEffect;
    private GameObject newBomb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position;
        sr = GetComponentInChildren<SpriteRenderer>();
        playerAttack = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAttack>();
        playerProfile = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerProfile>();

        bombStartScale = bombScale;

        if (playerProfile != null)
        {
            if (!playerAttack.stampPassiveSkill1)
            {
                bool critical = playerProfile.CriticalProbability();
                if (critical)
                {
                    damage1 = playerProfile.CriticalBuff(playerProfile.BasicATK(250));
                    damage2 = playerProfile.CriticalBuff(playerProfile.BasicATK(180));
                }
                else
                {
                    damage1 = playerProfile.BasicATK(250);
                    damage2 = playerProfile.BasicATK(180);
                }
            }
            else if (playerAttack.stampPassiveSkill1)
            {
                bool critical = playerProfile.CriticalProbability();
                if (critical)
                {
                    damage1 = playerProfile.CriticalBuff(playerProfile.BasicATK(280));
                    damage2 = playerProfile.CriticalBuff(playerProfile.BasicATK(220));
                }
                else
                {
                    damage1 = playerProfile.BasicATK(280);
                    damage2 = playerProfile.BasicATK(220);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!bomb)
        {
            transform.position += transform.up * playerAttack.power * Time.deltaTime;

            distance = Vector3.Distance(startPos, transform.position);

            if (distance > playerAttack.shotDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    

    private Collider _other;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy" || other.tag == "Boss")
        {
            if (other.CompareTag("Boss"))
            {
                Debug.Log("스탬프 기본 직격 공격" + other.gameObject.name + "을(를) 공격했습니다!" + "damage1 = " + damage1);
                other.gameObject.GetComponent<BossStatus>().GetDamage(damage1);
            }
            else if (other.CompareTag("Enemy"))
            {
                Debug.Log("스탬프 기본 직격 공격" + other.gameObject.name + "을(를) 공격했습니다!" + "damage1 = " + damage1);
                if (other.gameObject.GetComponent<MonsterBehavior>() != null)
                    other.gameObject.GetComponent<MonsterBehavior>().TakeDamage(damage1);
                if (other.gameObject.GetComponent<SealStoneManager>() != null)
                    other.gameObject.GetComponent<SealStoneManager>().Damage(damage1);
            }
            //Instantiate(hitPrefab, transform.position, Quaternion.identity);
            bomb = true;
            _other = other;
            if (!playerAttack.stampPassiveSkill2)
            {
                StartCoroutine(BombDestroy(_other));
            }
            else if (playerAttack.stampPassiveSkill2)
            {
                Destroy(gameObject);
            }
        }
    }

    IEnumerator BombDestroy(Collider other)
    {
        if (!bombStart)
        {
            newBomb = Instantiate(bombEffect, transform.position, Quaternion.identity);
            bombStart = false;
        }
        sr.enabled = false;
        yield return new WaitForSeconds(0.2f);
        //Color32 orange = new Color32(255, 160, 0, 255);
        //GetComponent<SpriteRenderer>().color = orange;
        
        //transform.localScale = new Vector3(bombScale, bombScale, bombScale);

        
        yield return new WaitForSeconds(0.5f);
        Destroy(newBomb);
        Destroy(gameObject);
    }
}
