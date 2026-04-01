using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    private static ResourceManager instance = null;

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            SettingResource();
        }
        else
        {
            Destroy(this.gameObject);
        }        
    }

    public static ResourceManager Instance
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

    [SerializeField]
    bool settingComplete;
    public bool SettingComplete { get { return settingComplete; } }

    public void SettingResource()
    {
        if (settingComplete) return;
        Debug.Log("리소스 세팅 시작");
        treeSprite = Resources.LoadAll<Sprite>("Sprites/Tiles/Trees");
        treeSprite = SortSpriteArray(treeSprite);

        houseOuterWallSprites = Resources.LoadAll<Sprite>("Sprites/Tiles/House/OuterWall");
        houseRoofSprites = Resources.LoadAll<Sprite>("Sprites/Tiles/House/Roof");
        houseDoorSprites = Resources.LoadAll<Sprite>("Sprites/Tiles/House/HouseDoor");
        talkBubble = Resources.LoadAll<Sprite>("Sprites/UI/Bubble");
        numberSprites = Resources.LoadAll<Sprite>("Sprites/UI/Number");

        harvestAxe = Resources.LoadAll<Sprite>("Sprites/Player/Hand/HarvestAxe");
        harvestPickaxe = Resources.LoadAll<Sprite>("Sprites/Player/Hand/HarvestMining");
        harvestHoe = Resources.LoadAll<Sprite>("Sprites/Player/Hand/HarvestHoe");
        buildHouse = Resources.LoadAll<Sprite>("Sprites/Player/Hand/Build");

        dropItemObj = Resources.Load<GameObject>("Prefabs/Object/DropItem");

        damageText = Resources.Load<GameObject>("Prefabs/UI/DamageText");
        interactionUI = Resources.Load<GameObject>("Prefabs/UI/InteractionUI");
        workGaugeObject = Resources.Load<GameObject>("Prefabs/UI/WorkGauge");
        enemyHpUI = Resources.Load<GameObject>("Prefabs/UI/EnemyHpUI");
        coinUI = Resources.Load<GameObject>("Prefabs/UI/CoinAcquisitionUI");
        questItemObj = Resources.Load<GameObject>("Prefabs/UI/QuestItem");

        urpSprite = Resources.Load<Material>("Materials/URPOutline");
        flashWhite = Resources.Load<Material>("Materials/FlashWhite");

        hitEffect = Resources.Load<GameObject>("Prefabs/Effect/HitEffect");
        runDustEffect = Resources.Load<GameObject>("Prefabs/Effect/RunDust");

        settingComplete = true;
    }

    //아이템 관련 스프라이트
    [SerializeField]
    Sprite[] itemSprites;
    public Sprite[] ItemSprites { get { return itemSprites; } }

    //UI 관련 스프라이트, 오브젝트
    GameObject damageText;
    public GameObject DamageText { get { return damageText; } }
    Sprite[] talkBubble;
    public Sprite[] TalkBubble { get { return talkBubble; } }

    Sprite[] numberSprites;
    public Sprite[] NumberSprites { get { return numberSprites; } }

    GameObject interactionUI;
    public GameObject InterUI { get { return interactionUI; } }

    GameObject workGaugeObject;
    public GameObject WorkGaugeObject { get { return workGaugeObject; } }

    GameObject enemyHpUI;
    public GameObject EnemyHpUI { get { return enemyHpUI; } }
    GameObject coinUI;
    public GameObject CoinUI { get { return coinUI; } }
    GameObject questItemObj;
    public GameObject QuestItemObj { get { return questItemObj; } }

    //작업 관련 스프라이트
    [SerializeField]
    Sprite[] houseOuterWallSprites;
    public Sprite[] HouseOuterWallSprites { get { return houseOuterWallSprites; } }
    [SerializeField]
    Sprite[] houseRoofSprites;
    public Sprite[] HouseRoofSprites { get { return houseRoofSprites; } }
    [SerializeField]
    Sprite[] houseDoorSprites;
    public Sprite[] HouseDoorSprites { get { return houseDoorSprites; } }
    Sprite[] harvestAxe;
    public Sprite[] HarvestAxe { get { return harvestAxe; } }
    Sprite[] harvestPickaxe;
    public Sprite[] HarvestPickaxe { get { return harvestPickaxe; } }
    Sprite[] harvestHoe;
    public Sprite[] HarvestHoe { get { return harvestHoe; } }
    Sprite[] buildHouse;
    public Sprite[] BuildHouse { get { return buildHouse; } }

    //타일 관련 스프라이트
    [SerializeField]
    Sprite[] treeSprite;
    public Sprite[] TreeSprite { get { return treeSprite; } }

    //드롭 아이템 객체
    GameObject dropItemObj;
    public GameObject DropItemObj { get { return dropItemObj; } }

    //마테리얼
    Material urpSprite;
    public Material URPSprite { get { return urpSprite; } }
    Material flashWhite;
    public Material FlashWhite { get { return flashWhite; } }

    //이펙트
    GameObject hitEffect;
    public GameObject HitEffectObj { get { return hitEffect; } }
    GameObject runDustEffect;
    public GameObject RunDustEffect { get { return runDustEffect; } }

    //대화 관련 객체
    ConversationContent conversation = new ConversationContent();

    public List<string> GetConversation(string npcName, int switchNum)
    {
        List<string> conversationList = new List<string>();

        for (int i = 0; i < conversation.Content.Length; i++)
        {
            string[] splitStr = conversation.Content[i].Split(';');
            string splitName = splitStr[0];
            int splitSwitchNum = int.Parse(splitStr[1]);
            string splitContent = splitStr[2];

            if (npcName.Equals(splitName) && switchNum.Equals(splitSwitchNum))
                conversationList.Add(splitContent);
        }

        return conversationList;
    }

    Sprite[] SortSpriteArray(Sprite[] array)
    {
        Sprite[] tempArray = new Sprite[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            string expName = Regex.Replace(array[i].name, @"\D", "");
            int expNum = int.Parse(expName);
            tempArray[expNum] = array[i];
        }
        return tempArray;
    }

    public void SaveJson(object saveClass, string fileName)
    {   
        string json = JsonUtility.ToJson(saveClass);
        string path = Application.dataPath + "/" + fileName + ".Json";
        File.WriteAllText(path, json);
    }
}
