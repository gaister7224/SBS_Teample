using UnityEngine;

public class BlowSkill : MonoBehaviour
{
    private GameObject player;
    private Rigidbody playerRid;
    private PlayerAttack playerAttack;
    private PlayerProfile playerProfile;

    private float rushSpeed = 15.0f;
    private float stopDist = 10.0f;

    private float damage;

    private Vector3 firstPos;
    private float dist;

    private bool rush = false;
    private bool attack = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerRid = player.GetComponent<Rigidbody>();
        playerAttack = player.GetComponent<PlayerAttack>();
        playerProfile = player.GetComponent<PlayerProfile>();
        firstPos = transform.position;
        if (playerProfile != null)
        {
            playerProfile.UseMP(2);
            bool critical = playerProfile.CriticalProbability();
            if (critical)
                damage = playerProfile.CriticalBuff(playerProfile.ATK(1900f));
            else
                damage = playerProfile.ATK(1900f);
            playerProfile.ChangeMoveSpeed(-100);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position;
        dist = Vector3.Distance(firstPos, transform.position);
        if (!attack)
        {
            if (!rush)
            {
                transform.rotation = Quaternion.Euler(0, playerAttack.AttackPos.transform.localRotation.eulerAngles.y, 0);
                playerProfile.ChangeMoveSpeed(-100);
                playerProfile.CameraZoom(2, 3, 45);
                playerProfile.currentState = PlayerSituation.Attack;
                Invoke("RushStart", 2);
            }
            else //돌격
            {
                Debug.Log("rushStart");

                playerRid.AddForce(transform.forward * rushSpeed
                    , ForceMode.Force);
            }
        }

        if (dist >= stopDist)
        {
            Hit();
        }
    }

    private void RushStart()
    {
        rush = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            playerProfile.ShakeCamera(0.2f, 3.0f, 15.0f);
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            playerProfile.SwordSkillHit(hitPoint);
            if (other.CompareTag("Boss"))
            {
                Debug.Log("스킬 : 회심의 일격" + other.gameObject.name + "을(를) 공격했습니다!");
                other.gameObject.GetComponent<BossStatus>().GetDamage(damage);
            }
            else if (other.CompareTag("Enemy"))
            {
                Debug.Log("스킬 : 회심의 일격" + other.gameObject.name + "을(를) 공격했습니다!");
                if (other.gameObject.GetComponent<MonsterBehavior>() != null)
                    other.gameObject.GetComponent<MonsterBehavior>().TakeDamage(damage);
                if (other.gameObject.GetComponent<SealStoneManager>() != null)
                    other.gameObject.GetComponent<SealStoneManager>().Damage(damage);
                if (other.gameObject.GetComponent<SealedStone>() != null)
                    other.gameObject.GetComponent<SealedStone>().TakeDamage(damage);
            }
            if (playerProfile.BloodHeal)
                playerProfile.BloodHealHp(10, damage);
            Hit();
        }
        if (other.CompareTag("Wall") || other.CompareTag("Storage"))
        {
            Hit();
        }

    }

    private void Hit()
    {
        playerRid.linearVelocity = Vector3.zero;
        //playerProfile.NoDamage = false;
        attack = true;
        playerProfile.ChangeMoveSpeed(1);
        playerProfile.SkillStart = false;
        playerProfile.currentState = PlayerSituation.Idle;
        Destroy(this.gameObject);
    }
}
