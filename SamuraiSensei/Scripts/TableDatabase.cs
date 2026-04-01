using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableDatabase : MonoBehaviour
{
    private static TableDatabase instance = null;
    public static TableDatabase Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SettingTable();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //////////정보 테이블

    //플레이어블 정보 테이블
    PlayerbleInfoTalbe playerbleTable;
    public PlayerbleInfoTalbe PlayerbleTable { get { return playerbleTable; } }

    //적 정보 테이블
    EnemyInfoTable enemyTable;
    public EnemyInfoTable EnemyTable { get { return enemyTable; } }

    //스테이지 별 스폰 정보 테이블
    SpawnEventTable spawnEventTable;
    public SpawnEventTable SpawnTable { get { return spawnEventTable; } }

    //스킬 테이블
    SkillTable skillTable;
    public SkillTable AllSkillTable { get { return skillTable; } }

    //스테이지 정보 테이블
    StageInfoTable stageInfoTable;
    public StageInfoTable StageTable { get { return stageInfoTable; } }

    //유저 정보 테이블
    UserPlayerbleInfoTable userPlayerbleInfotable;
    public UserPlayerbleInfoTable UserPlayerbleTable { get { return userPlayerbleInfotable; } }
    UserStageInfoTable userStageInfoTable;
    public UserStageInfoTable UserStageTable { get { return userStageInfoTable; } }

    //////////

    void SettingTable()
    {
        TextAsset json;
        //플레이어블 관련 리소스 로드
        json = Resources.Load<TextAsset>("Json/PlayerbleInfoTable");
        playerbleTable = JsonUtility.FromJson<PlayerbleInfoTalbe>(json.ToString());

        //유저 소유 캐릭터 관련 테이블 정보 로드
        json = Resources.Load<TextAsset>("Json/UserPlayerbleInfoTable");
        userPlayerbleInfotable = JsonUtility.FromJson<UserPlayerbleInfoTable>(json.ToString());

        //스테이지 정보 로드
        json = Resources.Load<TextAsset>("Json/StageInfoTable");
        stageInfoTable = JsonUtility.FromJson<StageInfoTable>(json.ToString());

        //임시로 유저 스테이지 정보 세팅
        json = Resources.Load<TextAsset>("Json/UserStageInfoTable");
        userStageInfoTable = JsonUtility.FromJson<UserStageInfoTable>(json.ToString());

        //스테이지 스폰 이벤트 정보 로드
        json = Resources.Load<TextAsset>("Json/SpawnEventTable");
        spawnEventTable = JsonUtility.FromJson<SpawnEventTable>(json.ToString());

        //스킬 정보 로드
        json = Resources.Load<TextAsset>("Json/SkillTable");
        skillTable = JsonUtility.FromJson<SkillTable>(json.ToString());

        //적 관련 정보 로드
        json = Resources.Load<TextAsset>("Json/EnemyInfoTable");
        enemyTable = JsonUtility.FromJson<EnemyInfoTable>(json.ToString());

    }
}
