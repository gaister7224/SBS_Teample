using System.Collections;
using TMPro;
using UnityEngine;

public class BarrierSkill : MonoBehaviour
{
    private PlayerProfile playerProfile;
    private PlayerAttack playerAttack;
    private GameObject player;

    private SpriteRenderer spriteRenderer;
    private SphereCollider sphereCollider;

    [SerializeField] private GameObject shockWave;
    [SerializeField] private GameObject bombObj;

    [SerializeField] private float barrierHp;

    private float stopTime = 5;
    private float time;
    private int random;

    private bool selfBomb;
    private TextMeshProUGUI hpText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerProfile = player.GetComponent<PlayerProfile>();
        playerAttack = player.GetComponent<PlayerAttack>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        sphereCollider = GetComponent<SphereCollider>();

        if (UIManager.Instance != null)
            hpText = UIManager.Instance.barrierHpText.GetComponent<TextMeshProUGUI>();
        else if (UIManager.Instance == null)
            Debug.Log("UIManager null");

        if (playerProfile != null)
        {
            playerProfile.UseMP(1);
            playerProfile.Barrier = true;
            barrierHp = playerProfile.MaxHp * 0.2f;
        }
        if (playerAttack.stampPassiveSkill2)
        {
            random = Random.Range(1, 101);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (playerAttack.stampPassiveSkill2)
        {
            if (random >= 1 && random <= 90)
            {
                selfBomb = true;
                StartCoroutine(SelfBomb());
            }
            else if (random > 90)
            {
                selfBomb = false;
            }
        }

        if (!playerAttack.stampPassiveSkill2 || (playerAttack.stampPassiveSkill2 && !selfBomb))
        {
            transform.position = player.transform.position;
            time += Time.deltaTime;
            if (barrierHp <= 0 || time > stopTime)
            {
                barrierHp = 0;
                Instantiate(shockWave, transform.position, shockWave.transform.rotation);
                playerProfile.SkillStart = false;
                playerProfile.Barrier = false;
                UIManager.Instance.barrierHpText.SetActive(false);
                Destroy(gameObject);
            }
        }

        hpText.text = "+ " + barrierHp.ToString();
    }

    private float healTime;
    private bool bumbApear = false;
    private bool hpDamage = false;
    IEnumerator SelfBomb()
    {
        yield return new WaitForSeconds(0.2f);
        if (!hpDamage)
        {
            playerProfile.SelfHpDamage(0.05f);
            hpDamage = true;
        }
        spriteRenderer.enabled = false;
        sphereCollider.enabled = false;
        yield return new WaitForSeconds(2);
        if (!bumbApear)
        {
            Instantiate(bombObj, transform.position, bombObj.transform.rotation);
            bumbApear = true;
        }
        healTime += Time.deltaTime;
        if (healTime >= 5)
        {
            playerProfile.HPBuff(0.013f);
            healTime = 0;
        }
        yield return new WaitForSeconds(25);
        playerProfile.SkillStart = false;
        playerProfile.Barrier = false;
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        //공격당하는걸 임시로 적에 닿았을경우로 함
        if (other.CompareTag("EnemyAttack"))
        {
            barrierHp -= 5;
        }
    }
}
