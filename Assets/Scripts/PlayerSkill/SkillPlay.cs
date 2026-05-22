using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SkillPlay : MonoBehaviour
{
    private int actSkill1Number = 0;
    private int actSkill2Number = 0;
    private int passiveSkillNumber = 0;

    private GameObject slotParent;
    private PlayerProfileSkill[] slots;

    [Header("?????")]
    private Slider[] coolTimeSlider;
    [SerializeField] private float coolTimeSkill1;
    [SerializeField] private float coolTimeSkill2;
    [SerializeField] private float passiveCoolTimeSkill1;
    [SerializeField] private float passiveCoolTimeSkill2;
    [SerializeField] private float curCoolTimeSkill1;
    [SerializeField] private float curCoolTimeSkill2;

    private float a_coolTimeSkill1;
    private float a_coolTimeSkill2;

    [SerializeField] private bool skill1Start = false;
    [SerializeField] private bool skill2Start = false;

    [Header("? ???????")]
    [SerializeField] private GameObject sword1;
    [SerializeField] private GameObject sword2;
    [SerializeField] private GameObject sword3;
    [SerializeField] private GameObject sword4;
    [SerializeField] private GameObject sword5;
    [SerializeField] private GameObject sword6;

    [Header("? ???????")]
    [SerializeField] private GameObject bow1;
    [SerializeField] private GameObject bow2;
    [SerializeField] private GameObject bow3;
    [SerializeField] private GameObject bow4;
    [SerializeField] private GameObject bow5;
    [SerializeField] private GameObject bow6;

    [Header("?????? ???????")]
    [SerializeField] private GameObject stamp1;
    [SerializeField] private GameObject stamp2;
    [SerializeField] private GameObject stamp3;
    [SerializeField] private GameObject stamp4;
    [SerializeField] private GameObject stamp5;
    [SerializeField] private GameObject stamp6;

    private PlayerAttack playerAttack;
    private PlayerProfile playerProfile;
    private Rigidbody playerRid;
    [SerializeField] private SwordAttackManager swordAttackManager;

    private float mpBuffTime;
    private void Start()
    {
        playerAttack = GetComponent<PlayerAttack>();
        playerProfile = GetComponent<PlayerProfile>();
        playerRid = GetComponent<Rigidbody>();

        if (UIManager.Instance == null)
            return;

        slotParent = UIManager.Instance.slotParent;
        coolTimeSlider = UIManager.Instance.coolTimeSlider;
    }

    private void Update()
    {
        if (coolTimeSlider == null)
            return;

        CoolTime();

        if (playerAttack != null && playerAttack.stampPassiveSkill2)
        {
            MpHeal();
        }
    }

    private void CoolTime()
    {
        if (skill1Start)
        {
            if (curCoolTimeSkill1 <= 0)
            {
                skill1Start = false;
                curCoolTimeSkill1 = 0f;
                if (coolTimeSlider != null) coolTimeSlider[0].value = 0f;
            }
            else
            {
                curCoolTimeSkill1 -= Time.deltaTime;
                UpdateSlider(0, curCoolTimeSkill1);
            }
        }

        if (skill2Start)
        {
            if (curCoolTimeSkill2 <= 0)
            {
                skill2Start = false;
                curCoolTimeSkill2 = 0f;
                if (coolTimeSlider != null) coolTimeSlider[1].value = 0f;
            }
            else
            {
                curCoolTimeSkill2 -= Time.deltaTime;
                UpdateSlider(1, curCoolTimeSkill2);
            }
        }
    }

    private void MpHeal()
    {
        if (playerProfile.MPBuffStart())
        {
            mpBuffTime += Time.deltaTime;

            if (mpBuffTime >= 10)
            {
                playerProfile.MPBuff(1);
                mpBuffTime = 0;
            }
        }
        else
        {
            mpBuffTime = 0;
        }
    }

    public void SetSkillCoolTimeBuff(float a_skillCoolTime)
    {
        coolTimeSkill1 = a_coolTimeSkill1 * (1 - (a_skillCoolTime / 100));
        coolTimeSkill2 = a_coolTimeSkill2 * (1 - (a_skillCoolTime / 100));

        curCoolTimeSkill1 = coolTimeSkill1;
        curCoolTimeSkill2 = coolTimeSkill2;
    }

    private void UpdateSlider(int number, float curCollTime)
    {
        if (coolTimeSlider != null)
        {
            coolTimeSlider[number].value = curCollTime;
        }
    }
    public void SkillNumberSetting()
    {
        slots = slotParent.GetComponentsInChildren<PlayerProfileSkill>();
        actSkill1Number = slots[0].choiceNumber;
        actSkill2Number = slots[1].choiceNumber;
        passiveSkillNumber = slots[2].choiceNumber;
        a_coolTimeSkill1 = slots[0].coolTime;
        a_coolTimeSkill2 = slots[1].coolTime;
        coolTimeSlider[0].value = 0;
        coolTimeSlider[1].value = 0;

        SetSkillCoolTimeBuff(GameManager.instance.a_skillCoolTime);
    }

    public void OnSkillAttack(InputAction.CallbackContext context)
    {
        if (context.started && actSkill1Number > 0 && !playerProfile.SkillStart)
        {
            if (context.control.name == "1" && slots[0].coolTime > 0)
            {
                if (skill1Start) return;

                skill1Start = true;
                if (passiveCoolTimeSkill1 > 0)
                    curCoolTimeSkill1 = passiveCoolTimeSkill1;
                else
                    curCoolTimeSkill1 = coolTimeSkill1;
                coolTimeSkill1 = slots[0].coolTime;
                coolTimeSlider[0].maxValue = curCoolTimeSkill1;
                coolTimeSlider[0].value = curCoolTimeSkill1;
                playerProfile.SkillStart = true;
                JobSkillChoice(actSkill1Number);
            }

            if (context.control.name == "2" && slots[1].coolTime > 0)
            {
                if (skill2Start) return;

                skill2Start = true;
                if (passiveCoolTimeSkill2 > 0)
                    curCoolTimeSkill2 = passiveCoolTimeSkill2;
                else
                    curCoolTimeSkill2 = coolTimeSkill2;
                coolTimeSkill2 = slots[1].coolTime;
                coolTimeSlider[1].maxValue = curCoolTimeSkill2;
                coolTimeSlider[1].value = curCoolTimeSkill2;
                playerProfile.SkillStart = true;
                JobSkillChoice(actSkill2Number);
            }
        }
    }

    private void JobSkillChoice(int number)
    {
        if (playerAttack.curJob == Job.Sword)
        {
            SwordActiveSkill(number);
        }
        else if (playerAttack.curJob == Job.Bow)
        {
            BowActiveSkill(number);
        }
        else if (playerAttack.curJob == Job.Stamp)
        {
            StampActiveSkill(number);
        }
    }

    private void SwordActiveSkill(int number)
    {
        switch (number)
        {
            case 1:
                {
                    Instantiate(sword1, transform.position, sword1.transform.rotation);
                    SkillAnimation("Attack2");
                }
                break;
            case 2:
                {
                    Instantiate(sword2, transform.position,
                        Quaternion.Euler(0, playerAttack.AttackPos.transform.eulerAngles.y, 0));
                    SkillAnimation("Attack1");
                    playerAttack.PlayerAttackDirection();
                }
                break;
            case 3:
                {
                    Instantiate(sword3, transform.position, sword3.transform.rotation);
                    SkillAnimation("Attack2");
                }
                break;
            case 4:
                {
                    Instantiate(sword4, transform.position, Quaternion.Euler(0, playerAttack.AttackPos.transform.eulerAngles.y, 0));
                    SkillAnimation("Attack1");
                    playerAttack.PlayerAttackDirection();
                }
                break;
            case 5:
                {
                    Instantiate(sword5, transform.position, Quaternion.Euler(0, playerAttack.AttackPos.transform.eulerAngles.y, 0));
                    Invoke("SwordSkill5Animation", 0.45f);
                }
                break;
            case 6:
                {
                    Instantiate(sword6, transform.position, sword6.transform.rotation);
                    SkillAnimation("Attack2");
                }
                break;
        }

    }

    private void SkillAnimation(string animation)
    {
        playerProfile.currentState = PlayerSituation.Attack;
        playerProfile.ani.SetTrigger(animation);
        playerProfile.ChangeMoveSpeed(-100);
    }

    private void SwordSkill5Animation()
    {
        playerProfile.currentState = PlayerSituation.Attack;
        playerProfile.ani.SetTrigger("Attack1");
        playerAttack.PlayerAttackDirection();
    }

    float rotation;
    private void BowActiveSkill(int number)
    {
        switch (number)
        {
            case 1:
                {
                    rotation = playerAttack.AttackPos.transform.eulerAngles.y;
                    playerProfile.UseMP(1);

                    StartCoroutine(BowSkill1Routine());
                }
                break;
            case 2:
                {
                    rotation = playerAttack.AttackPos.transform.eulerAngles.y;
                    playerProfile.UseMP(1);

                    StartCoroutine(BowSkill2Routine());
                }
                break;
            case 3:
                {
                    int pos = 0;
                    playerProfile.UseMP(1);
                    StartCoroutine(BowSkill3Rush());
                    for (int i = 0; i < 10; i++)
                    {
                        Instantiate(bow3, transform.position, Quaternion.Euler(0, rotation + pos, 0));
                        pos += 36;
                    }
                }
                break;
            case 4:
                {
                    StartCoroutine(BowSkill4Create());
                }
                break;
            case 5:
                {
                    Instantiate(bow5, transform.position, bow5.transform.rotation);
                }
                break;
            case 6:
                {
                    Instantiate(bow6, transform.position, bow6.transform.rotation);
                }
                break;
        }
    }

    private void StampActiveSkill(int number)
    {
        switch (number)
        {
            case 1:
                {
                    playerProfile.UseMP(1);
                    Instantiate(stamp1, transform.position, stamp1.transform.rotation);
                }
                break;
            case 2:
                {
                    Instantiate(stamp2, transform.position,
                        Quaternion.Euler(0, playerAttack.AttackPos.transform.eulerAngles.y, 0));
                    SkillAnimation("Attack2");
                    playerAttack.PlayerAttackDirection();
                }
                break;
            case 3:
                {
                    Instantiate(stamp3, transform.position, stamp3.transform.rotation);
                }
                break;
            case 4:
                {
                    playerProfile.UseMP(1);
                    Instantiate(stamp4, transform.position, stamp4.transform.rotation);
                }
                break;
            case 5:
                {
                    playerProfile.UseMP(2);
                    Instantiate(stamp5, transform.position,
                        Quaternion.Euler(0, playerAttack.AttackPos.transform.eulerAngles.y, 0));
                }
                break;
            case 6:
                {
                    playerProfile.UseMP(1);
                    playerAttack.stampSkill6 = true;
                    Instantiate(stamp6, transform.position, stamp6.transform.rotation);
                }
                break;
        }
    }
    IEnumerator BowSkill1Routine()
    {
        int count = 0;
        while (count < 5)
        {
            BowSkill1Create(); // ??? ????
            count++;
            yield return new WaitForSeconds(0.1f); // 0.1?? ???
        }

        // 5?? ?? ???? ?? ???? ????
        playerProfile.SkillStart = false;
    }
    private void BowSkill1Create()
    {
        Instantiate(bow1, transform.position, Quaternion.Euler(0, rotation, 0));
    }

    IEnumerator BowSkill2Routine()
    {
        int count = 0;
        while (count < 3)
        {
            BowSkill2Create(); // ??? ????
            count++;
            yield return new WaitForSeconds(0.1f); // 0.1?? ???
        }

        // 5?? ?? ???? ?? ???? ????
        playerProfile.SkillStart = false;
    }
    private void BowSkill2Create()
    {
        Instantiate(bow2, transform.position, Quaternion.Euler(0, rotation, 0));
        Instantiate(bow2, transform.position, Quaternion.Euler(0, rotation + 45, 0));
        Instantiate(bow2, transform.position, Quaternion.Euler(0, rotation - 45, 0));
    }

    private float rushSpeed = 10;
    IEnumerator BowSkill3Rush()
    {
        yield return new WaitForSeconds(0.2f);
        playerProfile.moveSpeed = 0;
        Vector3 rushDir = playerAttack.AttackPos.transform.up;
        rushDir.y = 0;
        rushDir.Normalize();
        playerRid.AddForce(rushDir * rushSpeed
                    , ForceMode.Impulse);
        yield return new WaitForSeconds(1.2f);
        playerRid.linearVelocity = Vector3.zero;
        playerProfile.ChangeMoveSpeed(1);
        playerProfile.SkillStart = false;
    }

    IEnumerator BowSkill4Create()
    {
        playerProfile.UseMP(1);
        playerProfile.ChangeMoveSpeed(-100f);
        playerProfile.CameraZoom(3, 5, 45);

        yield return new WaitForSeconds(3);
        rotation = playerAttack.AttackPos.transform.eulerAngles.y;
        transform.GetComponent<Rigidbody>().AddForce(-transform.forward * 5, ForceMode.Impulse);
        Instantiate(bow4, transform.position, Quaternion.Euler(0, rotation, 0));

        yield return new WaitForSeconds(0.5f);
        playerProfile.ChangeMoveSpeed(0);
    }

    public void SwordPassiveSkill()
    {
        switch (passiveSkillNumber)
        {
            case 1:
                {
                    SwordPassiveBuff(30, -30, -30, -20, 0, 0, 0, false, 0, 0);
                }
                break;
            case 2:
                {
                    SwordPassiveBuff(-25, 25, 25, 0, 0, 0, 0, true, 0, 0);
                }
                break;
            case 3:
                {
                    if (actSkill1Number == 6)
                    {
                        SwordPassiveBuff(0, 10, 15, 0, 20, -25, 100, false, 10, 0);
                    }
                    else if (actSkill2Number == 6)
                        SwordPassiveBuff(0, 10, 15, 0, 20, -25, 100, false, 0, 10);
                    else
                        SwordPassiveBuff(0, 10, 15, 0, 20, -25, 100, false, 0, 0);
                }
                break;
        }
    }

    public void BowPassiveSkill()
    {
        switch (passiveSkillNumber)
        {
            case 1:
                {
                    BowPassiveBuff(-20, -20, 20, true, false, 0, false, false);
                }
                break;
            case 2:
                {
                    BowPassiveBuff(0, 0, 0, false, true, 0, false, false);
                }
                break;
            case 3:
                {
                    BowPassiveBuff(0, 200, 0, false, false, 250, true, true);
                }
                break;
        }
    }

    public void StampPassiveSkill()
    {
        switch (passiveSkillNumber)
        {
            case 1:
                {
                    StampPassiveBuff(50f, true, 20f, -20f, 0f, 0f, false, 0f, false);
                }
                break;
            case 2:
                {
                    StampPassiveBuff(0f, false, 0f, 10f, -50f, 20f, true, -60f, false);
                }
                break;
            case 3:
                {
                    StampPassiveBuff(0f, false, 0f, 210f, 100f, 0f, false, -15f, true);
                }
                break;
        }
    }

    private void StampPassiveBuff(float shotDistance, bool stampPassiveSkill1, float passiveDelay, float passiveATK,
        float coolTimePersent, float passiveMoveSpeed, bool stampPassiveSkill2, float passiveBasicATK
        , bool stampPassiveSKill3)
    {
        playerAttack.ChangeShotDistance(shotDistance);
        playerAttack.stampPassiveSkill1 = stampPassiveSkill1;
        playerAttack.PassiveDelay(passiveDelay);
        playerProfile.PassiveATK(passiveATK);

        passiveCoolTimeSkill1 = (coolTimeSkill1 * (1f + (coolTimePersent / 100)));
        passiveCoolTimeSkill2 = (coolTimeSkill2 * (1f + (coolTimePersent / 100)));
        playerProfile.PassiveMoveSpeed(passiveMoveSpeed);
        playerAttack.stampPassiveSkill2 = stampPassiveSkill2;
        playerProfile.PassiveBasicATK(passiveBasicATK);

        playerProfile.StampPassiveSkill3 = stampPassiveSKill3;
    }

    private void BowPassiveBuff(float passiveDef, float attackDelay, float power, bool bloodHeal, bool bowExplosion,
        float passiveBasicAtk, bool through, bool passiveSkill3)
    {
        playerProfile.PassiveDEF(passiveDef);
        playerAttack.PassiveDelay(attackDelay);
        playerAttack.PassivePower(power);
        playerProfile.BloodHeal = bloodHeal;
        playerAttack.bowExplosion = bowExplosion;
        playerProfile.PassiveBasicATK(passiveBasicAtk);
        playerAttack.through = through;
        playerAttack.bowPassiveSkill3 = passiveSkill3;
    }
    private void SwordPassiveBuff(float passiveDef, float passiveBasicAtk, float passiveAtk, float passiveMoveSpeed,
        float increasedColliderSize, float changeAttackDelay, float coolTimePersent, bool bloodHeal, float coolTimePlue1, float coolTimePlue2)
    {
        playerProfile.PassiveDEF(passiveDef);
        playerProfile.PassiveBasicATK(passiveBasicAtk);
        playerProfile.PassiveATK(passiveAtk);
        playerProfile.PassiveMoveSpeed(passiveMoveSpeed);
        if (swordAttackManager != null)
        {
            swordAttackManager.IncreasedColliderSize(increasedColliderSize);
        }
        playerAttack.ChangeAttackDelay(changeAttackDelay);
        passiveCoolTimeSkill1 = (coolTimeSkill1 * (1f + (coolTimePersent / 100))) + coolTimePlue1;
        passiveCoolTimeSkill2 = (coolTimeSkill2 * (1f + (coolTimePersent / 100))) + coolTimePlue2;
        playerProfile.BloodHeal = bloodHeal;
    }
}
