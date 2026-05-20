using System;
using System.Collections.Generic;
using UnityEngine;

public enum MapState
{
    None,
    Stage,
    Village
}
public class GameManager : MonoBehaviour
{
    public static GameManager instance; //어디서든 접근 가능

    [Header("PlayerData")]
    public string nickName;
    public int level;
    public int curLevel;
    public Job job;
    public int hpPoint;
    public int atkPoint;
    public float defPoint;
    public float criticalPoint;
    public int profileIndex;
    public float e_hp;
    public float a_hp;
    public float e_atk;
    public float a_atk;
    public float e_def;
    public float a_def;
    public int a_mp;
    public float a_skillCoolTime;
    public float e_critical;
    public float a_critical;
    public RuntimeAnimatorController curAnimation;
    public int gold;
    public int skillPoint;
    public int statusPoint;
    public int maxActCount;
    public int curActCount;
    [Header("etcData")]
    public List<int> canBuyCount = new List<int>();
    public bool character1Spawn;
    public bool tutorialClear;
    public bool storageTutorial;
    public bool inventoryTutorial;
    public bool dayTutorial;
    public int dayCount;
    public int artifactInstallImpossibleDay;
    public bool dayEnd; //일차 종료 확인
    public bool[] possibleDungeon;
    public bool itemGetAll;
    public int curDungeonNumber;
    public bool installImpossibleStart;
    public float goldMultiplier;
    public int buffStoneGetStatusNumber;
    public float recoveryMultiplier;
    public bool shelterActCountBan;
    public bool shelterHpBan;

    [Header("유물 Action")]
    public Action OnShelterEnter;
    public Action OnRandomPortalEnter;
    public Action OnPortalEnter;
    public Action OnActCountDeath;
    public Action OnDayChange;

    public MapState mapState = MapState.Village;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); //오브젝트 파괴 금지
        }
        else
        {
            Destroy(gameObject); //이미 있으면 파괴
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        profileIndex = 0;
        goldMultiplier = 1;
        buffStoneGetStatusNumber = 0;
        recoveryMultiplier = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
