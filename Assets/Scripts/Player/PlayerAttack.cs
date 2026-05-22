using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Job
{
    Sword,
    Bow,
    Stamp
}
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private GameObject attackPos;
    private GameObject jobChoiceUI;
    private JobChoice jobChoice;
    [Header("SkillObject")]
    [SerializeField] private GameObject swordAttackObj;
    [SerializeField] private GameObject bowAttackObj;
    [SerializeField] private GameObject stampAttackObj;
    [Header("???? ?? ?????")]
    [SerializeField] private Sprite[] profileImages;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [Header("???? ?? ??????? ??????")]
    [SerializeField] private RuntimeAnimatorController swordAnimation;
    [SerializeField] private RuntimeAnimatorController stampAnimation;
    [SerializeField] private RuntimeAnimatorController bowAnimation;

    float rotateSpeed = 100f;
    float maxRotateSpeed = 100f;

    Vector3 targetDir;
    float angle;

    float attackTime;
    [SerializeField] private float attackDelay = 1;
    public float attackStartDelay = 0;
    float originattackDelay = 1;
    private float passiveDelay;
    bool attack = false;
    public bool uiClicking = false;
    public float shotDistance;
    private float originShotDistance;
    public float power;
    private float originPower;
    private float passivePower;

    public float scalePercent = 0;

    public bool through = false;
    public bool bowExplosion = false;
    public bool bowPassiveSkill3 = false;
    public bool stampSkill6 = false;
    public bool stampPassiveSkill1 = false;
    public bool stampPassiveSkill2 = false;

    //private int skillCount;

    private PlayerProfile playerProfile;
    [SerializeField] private Transform pSprite;

    public Job curJob = Job.Sword;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerProfile = GetComponent<PlayerProfile>();

        if (UIManager.Instance != null)
        {
            jobChoiceUI = UIManager.Instance.jobChoiceUI;
            jobChoice = UIManager.Instance.jobChoice;

            if (UIManager.Instance.swordJobButton != null)
                UIManager.Instance.swordJobButton.onClick.AddListener(SwordChoice);
            if (UIManager.Instance.bowJobButton != null)
                UIManager.Instance.bowJobButton.onClick.AddListener(BowChoice);
            if (UIManager.Instance.stampJobButton != null)
                UIManager.Instance.stampJobButton.onClick.AddListener(StampChoice);
        }

        //????? ???
        StateDecision(1f, 0f, 0f, false, Job.Sword, 10, 3, 0, 0, swordAnimation);
    }

    // Update is called once per frame
    void Update()
    {
        AttackArrow();

        if (attack)
        {
            attackTime += Time.deltaTime;

            if (attackTime > attackDelay)
            {
                attack = false;
                attackTime = 0;
            }
        }
        if (playerProfile != null)
            playerProfile.PlayerDie();
    }

    public void ChangeAttackDelay(float changePercent)
    {
        attackDelay = passiveDelay * (1f + (changePercent / 100f));
    }

    public void PassiveDelay(float changePercent)
    {
        passiveDelay = originattackDelay * (1f + (changePercent / 100f));
        attackDelay = passiveDelay;
    }

    public void ChangeShotDistance(float changePercent)
    {
        shotDistance = originShotDistance * (1f + (changePercent / 100f));
    }

    public void AttackPosRotation(float changePercent)
    {
        rotateSpeed = maxRotateSpeed * (1f + (changePercent / 100f));
    }

    public void ChangePower(float changePercent)
    {
        power = passivePower * (1f + (changePercent / 100f));
    }

    public void PassivePower(float changePercent)
    {
        passivePower = originPower * (1f + (changePercent / 100f));
        power = passivePower;
    }

    void AttackArrow()
    {
        if (attackPos == null || Camera.main == null || Mouse.current == null)
            return;

        Vector3 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            //Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);
            targetDir = hit.point - attackPos.transform.position; //???? ????
            angle = Mathf.Atan2(targetDir.x, targetDir.z) * Mathf.Rad2Deg; //????? ??? ?? ?? ???????? ???
            Quaternion targetRotation = Quaternion.Euler(90f, 0f, -angle);
            attackPos.transform.localRotation = Quaternion.Slerp(
                attackPos.transform.localRotation,
                targetRotation,
                rotateSpeed * Time.deltaTime);
        }
    }

    public GameObject AttackPos
    {
        get { return attackPos; }
    }

    //??? ???? ????
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!attack && !uiClicking)
        {
            switch (curJob)
            {
                case Job.Sword:
                    {
                        Instantiate(swordAttackObj, transform.position, attackPos.transform.rotation);
                        playerProfile.ani.SetTrigger("Attack1");
                        PlayerAttackDirection();
                        playerProfile.currentState = PlayerSituation.Attack;
                        playerProfile.ChangeMoveSpeed(-100);
                        attack = true;
                    }
                    break;
                case Job.Bow:
                    {
                        StartCoroutine(BowAttack());
                        playerProfile.ani.SetTrigger("Attack1");
                        PlayerAttackDirection();
                        playerProfile.currentState = PlayerSituation.Attack;
                        playerProfile.ChangeMoveSpeed(-100);
                    }
                    break;
                case Job.Stamp:
                    {
                        Instantiate(stampAttackObj, transform.position, attackPos.transform.rotation);
                        playerProfile.ani.SetTrigger("Attack1");
                        PlayerAttackDirection();
                        playerProfile.currentState = PlayerSituation.Attack;
                        playerProfile.ChangeMoveSpeed(-100);
                        attack = true;
                    }
                    break;
            }
        }
    }

    public void PlayerAttackDirection()
    {
        if (attackPos.transform.eulerAngles.y >= 1 && attackPos.transform.eulerAngles.y <= 180)
        {
            pSprite.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (attackPos.transform.eulerAngles.y > 180 && attackPos.transform.eulerAngles.y <= 360)
        {
            pSprite.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    IEnumerator BowAttack()
    {
        attack = true;
        if (attackStartDelay > 0)
        {
            playerProfile.ChangeMoveSpeed(-20);
        }
        yield return new WaitForSeconds(attackStartDelay);
        GameObject newBow;
        Vector3 bowScale;
        newBow = Instantiate(bowAttackObj, transform.position, attackPos.transform.rotation);
        bowScale = newBow.transform.localScale;
        newBow.transform.localScale = bowScale * (1f + (scalePercent / 100f));
    }

    private void StateDecision(float _attackDelay, float _shotDistance, float _power, bool _attack, Job _curJob,
        float setHp, float setATK, float setDEF, int profile, RuntimeAnimatorController animation)
    {
        playerProfile = GetComponent<PlayerProfile>();
        originattackDelay = _attackDelay;
        attackDelay = originattackDelay;
        passiveDelay = originattackDelay;
        originShotDistance = _shotDistance;
        shotDistance = originShotDistance;
        originPower = _power;
        power = originPower;
        passivePower = originPower;
        attack = _attack;
        curJob = _curJob;

        if (GameManager.instance != null)
        {
            GameManager.instance.job = curJob;
            GameManager.instance.hpPoint = (int)setHp;
            GameManager.instance.atkPoint = (int)setATK;
            GameManager.instance.defPoint = (int)setDEF;
            GameManager.instance.profileIndex = profile;
            GameManager.instance.curAnimation = animation;
        }

        if (playerProfile != null)
        {
            playerProfile.SetMaxHp(setHp, 0, 0);
            playerProfile.SetMaxATK(setATK, 0, 0);
            playerProfile.SetMaxDEF(setDEF, 0, 0);

            if (animation != null && playerProfile.ani != null)
                playerProfile.ani.runtimeAnimatorController = animation;
        }

        if (playerSpriteRenderer != null && profileImages != null && profile >= 0 && profile < profileImages.Length)
            playerSpriteRenderer.sprite = profileImages[profile];
    }

    public void SwordChoice()
    {
        Debug.Log("swordskill");

        StateDecision(1f, 0f, 0f, false, Job.Sword, 10, 3, 0, 0, swordAnimation);
        JobSelect();
    }
    public void BowChoice()
    {
        Debug.Log("bowskill");

        StateDecision(0.5f, 10.0f, 10.0f, false, Job.Bow, 8, 3, -10, 1, bowAnimation);
        JobSelect();
    }
    public void StampChoice()
    {
        Debug.Log("stampskill");

        StateDecision(2.5f, 10.0f, 5.0f, false, Job.Stamp, 7, 4, 0, 2, stampAnimation);
        JobSelect();
        //playerProfile.ChangeMoveSpeed(0);
    }

    private void JobSelect()
    {
        if (UIManager.Instance == null)
            return;

        jobChoiceUI = UIManager.Instance.jobChoiceUI;
        jobChoice = UIManager.Instance.jobChoice;
        Time.timeScale = 1;

        if (jobChoiceUI != null)
            jobChoiceUI.SetActive(false);
        if (jobChoice != null)
            jobChoice.enabled = true;

        uiClicking = false;

        if (UIManager.Instance.villagePos != null)
            transform.position = UIManager.Instance.villagePos.position;

        if (UIManager.Instance.inventory != null && UIManager.Instance.inventory.playerProfile != null)
            UIManager.Instance.inventory.playerProfile.SetActive(true);

        if (GameManager.instance != null)
            GameManager.instance.mapState = MapState.Village;

        if (UIManager.Instance.virtualCamera != null && UIManager.Instance.villageCollider != null)
        {
            UIManager.Instance.virtualCamera.GetComponent<CinemachineConfiner3D>().BoundingVolume
                = UIManager.Instance.villageCollider;
        }

        Invoke("StoreExplainDialogue", 0.5f);
    }

    private void StoreExplainDialogue()
    {
        if (!DialogueManager.instance.start)
        {
            DialogueManager.instance.OnDialogue(UIManager.Instance.storeExplainDialogue);
            DialogueManager.instance.OnDialogueComplete += StorageZoom;
        }
    }

    private void StorageZoom()
    {
        Time.timeScale = 0;
        UIManager.Instance.storageDirector.Play();
        StartCoroutine(PlayAndCheck());
    }

    IEnumerator PlayAndCheck()
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime((float)UIManager.Instance.storageDirector.duration);
        DialogueManager.instance.OnDialogueComplete -= StorageZoom;
        UIManager.Instance.storageDirector.gameObject.SetActive(false);
        Time.timeScale = 1;
    }
}
