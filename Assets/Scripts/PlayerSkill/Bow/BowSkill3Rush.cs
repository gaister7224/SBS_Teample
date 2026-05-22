using System.Collections;
using UnityEngine;

public class BowSkill3Rush : MonoBehaviour
{
    PlayerProfile playerProfile;
    PlayerAttack playerAttack;
    Rigidbody playerRid;

    bool rush;

    private float rushSpeed = 10;
    private void OnEnable()
    {
        playerProfile = GetComponent<PlayerProfile>();
        playerAttack = GetComponent<PlayerAttack>();
        playerRid = GetComponent<Rigidbody>();
        rush = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (rush)
        {
            playerProfile.moveSpeed = 0;
            Vector3 rushDir = playerAttack.AttackPos.transform.up;
            rushDir.y = 0;
            rushDir.Normalize();
            playerRid.AddForce(rushDir * rushSpeed
                        , ForceMode.Impulse);
            Invoke("RushClose", 0.1f);
        }
        else 
        {
            playerRid.linearVelocity = Vector3.zero;
            playerProfile.ChangeMoveSpeed(1);
            playerProfile.SkillStart = false;
            this.enabled = false;
        }
    }

    private void RushClose()
    {
        rush = false;
    }
}
