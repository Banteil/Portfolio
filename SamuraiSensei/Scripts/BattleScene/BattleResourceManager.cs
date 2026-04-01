using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleResourceManager : MonoBehaviour
{
    private static BattleResourceManager instance = null;
    public static BattleResourceManager Instance
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
            ResourceSetting();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //플레이어블 객체 프리팹 리스트
    GameObject[] playerbleObjectList;
    public GameObject[] PlayerbleObjectList { get { return playerbleObjectList; } }

    //적 객체 프리팹 리스트
    GameObject[] enemyObjectList;
    public GameObject[] EnemyObjectList { get { return enemyObjectList; } }

    //스킬 효과 오브젝트 프리팹 리스트
    GameObject[] skillEffectList;
    public GameObject[] SkillEffectList { get { return skillEffectList; } }
    //스킬 범위 오브젝트 프리팹 리스트
    GameObject[] skillRangeList;
    public GameObject[] SkillRangeList { get { return skillRangeList; } }

    //기타 오브젝트
    GameObject bulletObject;
    public GameObject BulletObject { get { return bulletObject; } }

    GameObject damageTextObject;
    public GameObject DamageTextObject { get { return damageTextObject; } }

    GameObject enemyHpBarObject;
    public GameObject EnemyHpBarObject { get { return enemyHpBarObject; } }

    GameObject bossHpBarObject;
    public GameObject BossHpBarObject { get { return bossHpBarObject; } }

    Sprite[] bulletSprites;
    public Sprite[] BulletSprites { get { return bulletSprites; } }

    void ResourceSetting()
    {
        //플레이어블 관련 리소스 로드
        playerbleObjectList = Resources.LoadAll<GameObject>("Prefabs/Playerble");

        //적 관련 리소스 로드
        enemyObjectList = Resources.LoadAll<GameObject>("Prefabs/Enemy");

        //스킬 정보 로드
        skillEffectList = Resources.LoadAll<GameObject>("Prefabs/Skill/Effect");
        skillRangeList = Resources.LoadAll<GameObject>("Prefabs/Skill/Range");

        //기타 오브젝트 로드
        bulletObject = Resources.Load<GameObject>("Prefabs/Object/Bullet");
        damageTextObject = Resources.Load<GameObject>("Prefabs/UI/DamageText");
        enemyHpBarObject = Resources.Load<GameObject>("Prefabs/UI/EnemyHpBar");
        bossHpBarObject = Resources.Load<GameObject>("Prefabs/UI/BossHpBar");

        bulletSprites = Resources.LoadAll<Sprite>("Sprites/Bullet");
    }
}
