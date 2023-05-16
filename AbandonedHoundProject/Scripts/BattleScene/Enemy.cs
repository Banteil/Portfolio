using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class Enemy : MonoBehaviour
{
    public int maxHp; //적의 최대 체력
    public int currHp; //적의 현재 체력
    public string enemyName; //적의 이름

    public int pureStrength = 1; //순수 플레이어의 힘 스탯
    public int pureDexterity = 1; //순수 플레이어의 민첩 스탯
    public int pureIntelligence = 1; //순수 플레이어의 지능 스탯
    public int pureVitality = 1; //순수 플레이어의 체력 스탯

    public int currStrength;//상태이상으로 변하는 힘 스탯
    public int currDexterity;//상태이상으로 변하는 민첩 스탯
    public int currIntelligence;//상태이상으로 변하는 지능 스탯
    public int currVitality; //상태이상으로 변하는 체력 스탯

    public int getItemPercentage; //아이템 획득 확률
    public int getExp; //경험치
    public int getMoney; //보상금
    XmlNode enemyNode; //선택된 적 정보가 담기는 XmlNode;
    Sprite monsterImage; //적의 이미지

    List<int> actList = new List<int>(); //적이 소지한 행동(스킬) 리스트를 저장하는 List
    List<string> patternList = new List<string>(); //만약 적이 고정 패턴을 지닌 적이라면, 해당 패턴을 저장하는 List

    void Awake()
    {
        LoadXMLData();
        SetEnemyInfo();
    }

    void LoadXMLData()
    {
        TextAsset textAsset = Resources.Load("XML/EnemyDB") as TextAsset;
        XmlDocument enemyDB = new XmlDocument();
        enemyDB.LoadXml(textAsset.text);
        XmlNodeList stageTable = enemyDB.SelectNodes("rows/row");
        List<XmlNode> enemyNodeList = new List<XmlNode>();

        if (!DataPassing.isBoss)
        {
            foreach (XmlNode node in stageTable)
            {
                if (!node.SelectSingleNode("stage").InnerText.Contains("Boss_") && 
                    int.Parse(node.SelectSingleNode("stage").InnerText).Equals(DataPassing.stageNum))
                    enemyNodeList.Add(node);
            }
            int select = Random.Range(0, enemyNodeList.Count);
            enemyNode = enemyNodeList[select];
        }
        else
        {
            foreach (XmlNode node in stageTable)
            {
                if (node.SelectSingleNode("stage").InnerText.Equals("Boss_" + DataPassing.stageNum))
                {
                    enemyNode = node;
                    break;
                }                
            }
        }
    }

    void SetEnemyInfo()
    {
        maxHp = int.Parse(enemyNode.SelectSingleNode("hp").InnerText);
        enemyName = enemyNode.SelectSingleNode("name").InnerText;
        pureStrength = int.Parse(enemyNode.SelectSingleNode("str").InnerText);
        pureDexterity = int.Parse(enemyNode.SelectSingleNode("dex").InnerText);
        pureIntelligence = int.Parse(enemyNode.SelectSingleNode("int").InnerText);
        pureVitality = int.Parse(enemyNode.SelectSingleNode("vit").InnerText);
        getItemPercentage = int.Parse(enemyNode.SelectSingleNode("item").InnerText);
        getExp = int.Parse(enemyNode.SelectSingleNode("exp").InnerText);
        getMoney = Random.Range(50 * DataPassing.stageNum, DataPassing.stageNum * 300);

        currHp = maxHp;
        currStrength = pureStrength;
        currDexterity = pureDexterity;
        currIntelligence = pureIntelligence;
        currVitality = pureVitality;

        string[] strAct = enemyNode.SelectSingleNode("skill").InnerText.Split(',');
        for (int i = 0; i < strAct.Length; i++) actList.Add(int.Parse(strAct[i]));

        string ImageNum = enemyNode.SelectSingleNode("num").InnerText;
        if(!DataPassing.isBoss) monsterImage = Resources.Load<Sprite>("Sprite/Enemy_" + ImageNum);
        else monsterImage = Resources.Load<Sprite>("Sprite/Boss_" + DataPassing.stageNum);
        GetComponent<SpriteRenderer>().sprite = monsterImage;
    }

    ///<summary>
    ///6개의 행동 슬롯에 적의 행동 패턴을 삽입하는 함수
    ///</summary>
    public void SetPattern()
    {
        for (int i = 0; i < 6; i++)
        {
            int pattern = actList[Random.Range(0, actList.Count)];
            BattleManager.Instance.SetEnemySlot(i, pattern);
        }
        //적의 행동 넘버를 랜덤으로 돌리고, 1~6번째 슬롯에 차례로 집어넣음

        if (BattleManager.Instance.turn == 7)
            BattleManager.Instance.enemyActNum[5] = 8;
    }
}
