using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ArtiFactManager : MonoBehaviour
{
    public static ArtiFactManager instance;

    private List<int> equippedArtifactIDs = new List<int>();

    private bool artifactImpassible = false;
    private int setCount = 0;
    private void Awake()
    {
        instance = this;
    }

    public void EquipArtifact(int itemID) //효과 등록
    {
        if (equippedArtifactIDs.Contains(itemID)) return;

        equippedArtifactIDs.Add(itemID);
        RegisterArtifactEffect(itemID, true);
    }

    public void UnequipArtifact(int itemID) //효과해제
    {
        if (equippedArtifactIDs.Remove(itemID))
        {
            RegisterArtifactEffect(itemID, false);
        }
    }

    private void RegisterArtifactEffect(int id, bool isEquip)
    {
        switch(id)
        {
            case 700:
                {
                    if (!artifactImpassible)
                    {
                        ApplyLanternStaminaBonus(isEquip, 5);
                    }
                }
                break;
            case 701:
                {
                    if (isEquip) GameManager.instance.OnShelterEnter += RecoverStaminaOnShelter;
                    else GameManager.instance.OnShelterEnter -= RecoverStaminaOnShelter;
                }
                break;
            case 702:
                {
                    if (isEquip) GameManager.instance.OnRandomPortalEnter += RecoverStaminaOnShelter;
                    else GameManager.instance.OnRandomPortalEnter -= RecoverStaminaOnShelter;
                }
                break;
            case 703:
                {
                    DoubleStaminaRecovery(isEquip, 5);
                }
                break;
            case 704:
                {
                    if (isEquip) GameManager.instance.OnPortalEnter += ChanceToSkipStaminaCost;
                    else GameManager.instance.OnPortalEnter -= ChanceToSkipStaminaCost;
                }
                break;
            case 705:
                {
                    if (isEquip) GameManager.instance.OnPortalEnter += ApplyChaosStaminaCost;
                    else GameManager.instance.OnPortalEnter -= ApplyChaosStaminaCost;
                }
                break;
            case 706:
                {
                    if (isEquip) GameManager.instance.OnActCountDeath += EmergencyEscapeOnDeath;
                    else GameManager.instance.OnActCountDeath -= EmergencyEscapeOnDeath;
                }
                break;
            case 707:
                {
                    LoanStaminaOnMove(isEquip);
                    if (isEquip) GameManager.instance.OnActCountDeath += LoanStaminaUse;
                    else GameManager.instance.OnActCountDeath -= LoanStaminaUse;
                }
                break;
            case 708:
                {
                    SealReturnPortal(isEquip);
                }
                break;
            case 709:
                {
                    SealShelterRest(isEquip);
                }
                break;
            case 710:
                {
                    if (isEquip)
                    {
                        GameManager.instance.OnPortalEnter += RegenerateHpOnMove;
                        GameManager.instance.shelterHpBan = true;
                    }
                    else
                    {
                        GameManager.instance.OnPortalEnter -= RegenerateHpOnMove;
                        GameManager.instance.shelterHpBan = false;
                    }
                }
                break;
            case 711:
                {
                    ApplyGreedBuffAndDebuff(isEquip);
                }
                break;
        }
    }

    private void ApplyLanternStaminaBonus(bool isEquip, int amount)
    {
        if (isEquip) GameManager.instance.maxActCount += amount;
        else GameManager.instance.maxActCount -= amount;
    }
    private void RecoverStaminaOnShelter()
    {
        PlayerProfile playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>();
        playerProfile.ActCountPlus(3, GameManager.instance.recoveryMultiplier);
        Debug.Log("RecoverStaminaOnShelter");
    }
    private void DoubleStaminaRecovery(bool isEquip, int amount)
    {
        if (isEquip)
        {
            GameManager.instance.maxActCount -= amount;
            GameManager.instance.recoveryMultiplier *= 2.0f;
        }
        else
        {
            GameManager.instance.maxActCount += amount;
            GameManager.instance.recoveryMultiplier /= 2.0f;
        }
    }
    private void ChanceToSkipStaminaCost()
    {
        PlayerProfile playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>();
        int random = Random.Range(0, 100);
        Debug.Log(random);
        if (random >= 0 && random < 10)
        {
            playerProfile.BuffActCount(1);
            playerProfile.NotUseActCount = true;
            
        }
        else
        {
            playerProfile.NotUseActCount = false;
        }
    }
    private void ApplyChaosStaminaCost()
    {
        PlayerProfile playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>();
        int random = Random.Range(0, 100);
        Debug.Log(random);
        if (random >= 0 && random < 70)
        {
            
        }
        else if(random >= 70 && random < 90)
        {
            playerProfile.NotUseActCount = false;
            playerProfile.UseActCount(1);
        }
        else
        {
            if(!playerProfile.NotUseActCount)
            {
                playerProfile.BuffActCount(1);
            }
        }
    }

    private void EmergencyEscapeOnDeath()
    {
        PlayerProfile playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>();
        playerProfile.EmergencyEscape = true;
        GameManager.instance.installImpossibleStart = true;
        //생환의 부적 만들기
    }

    private void LoanStaminaOnMove(bool isEquip)
    {
        PlayerProfile playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>();
        if (isEquip)
        {
            playerProfile.actCountMin = -5;
        }
        else
        {
            playerProfile.actCountMin = 0;
        }
    }

    private void LoanStaminaUse()
    {
        PlayerProfile playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>();
        playerProfile.LoanActCount();
    }

    private void SealReturnPortal(bool isEquip)
    {
        //귀한 포탈 사용 불가 넣어야함

        if(isEquip)
        {
            setCount++;

            if (setCount >= 2)
            {
                GameManager.instance.maxActCount += 6;
            }
            else if(setCount < 2)
            {
                GameManager.instance.maxActCount += 3;
            }
        }
        else
        {
            if (setCount >= 2)
            {
                GameManager.instance.maxActCount -= 6;
            }
            else if (setCount < 2)
            {
                GameManager.instance.maxActCount -= 3;
            }
            setCount--;
        }
    }
    private void SealShelterRest(bool isEquip)
    {
        PlayerProfile playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>();

        if (isEquip)
        {
            artifactImpassible = true;
            GameManager.instance.shelterActCountBan = true;
            setCount++;
            if(setCount >= 2)
            {
                GameManager.instance.maxActCount += 5;
                if(playerProfile != null)
                {
                    playerProfile.BuffActCount(5);
                }
            }
            else if(setCount < 2)
            {
                GameManager.instance.maxActCount += 2;
                if (playerProfile != null)
                {
                    playerProfile.BuffActCount(2);
                }
            }
        }
        else
        {
            artifactImpassible = false;
            GameManager.instance.shelterActCountBan = false;
            if (setCount >= 2)
            {
                GameManager.instance.maxActCount -= 5;
            }
            else if (setCount < 2)
            {
                GameManager.instance.maxActCount -= 2;
            }
            setCount--;
        }
    }
    private void RegenerateHpOnMove()
    {
        PlayerProfile playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>();
        playerProfile.HPBuff(0.1f);
    }
    private void ApplyGreedBuffAndDebuff(bool isEquip)
    {
        PlayerProfile playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>();
        if (isEquip)
        {
            playerProfile.ArtifactDEFDebuff(-10);
            GameManager.instance.goldMultiplier += 0.2f;
        }
        else
        {
            playerProfile.ArtifactDEFDebuff(+10);
            GameManager.instance.goldMultiplier -= 0.2f;
        }
    }
}
