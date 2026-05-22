using System.Collections;
using UnityEngine;

public class ConcentrationOfBattleSkillParent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private GameObject skill;
    [SerializeField] SwordAttackManager swordAttackManager;

    private GameObject player;
    private PlayerProfile playerProfile;
    private PlayerAttack playerAttack;
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerProfile = player.GetComponent<PlayerProfile>();
        playerAttack = player.GetComponent<PlayerAttack>();
        playerProfile.SwordAttackCount = 0;
        if (swordAttackManager != null)
        {
            swordAttackManager.IncreasedColliderSize(30);
        }
        else
        {
            Debug.Log("swordAttackManager : null");
        }

        if (playerProfile != null)
        {
            playerProfile.UseMP(1);
            playerProfile.ChangeBasicATK(25f);
        }
        Invoke("CameraShake", 1.0f);
    }

    private void CameraShake()
    {
        playerProfile.ShakeCamera(0.2f, 2.0f, 10.0f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position;
        transform.rotation = Quaternion.Euler(0, playerAttack.AttackPos.transform.localRotation.eulerAngles.y, 0);
        if (playerProfile.SwordAttackCount == 3)
        {
            Instantiate(skill, transform.position, transform.rotation);
            playerProfile.SwordAttackCount = 0;
        }

        StartCoroutine(ObjectDestory());
    }

    IEnumerator ObjectDestory()
    {
        yield return new WaitForSeconds(20);
        playerProfile.SkillStart = false;
        swordAttackManager.IncreasedColliderSize(0);
        playerProfile.ChangeATK(0);
        Destroy(gameObject);
    }
}
