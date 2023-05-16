using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public enum GemKind
{
    RED, BLUE, GREEN, YELLOW
}

public struct GemOption
{
    public int kind;
    public int value;
    public string dialog;
};

public class Gem
{
    GemKind gemKind; //젬의 종류
    public string gemKindName; //젬 종류를 나타낼 이름
    public Sprite gemSprite; //젬 스프라이트
    public int statValue; //젬 능력 수치
    public List<GemOption> gemOptionList = new List<GemOption>(); //젬 옵션 리스트
    public GemKind GemKind //외부에서 호출될 젬의 종류
    {
        get { return gemKind; }
        set
        {
            Sprite[] icon = Resources.LoadAll<Sprite>("Icons/IconSet");
            gemKind = value;
            switch (gemKind)
            {
                case GemKind.RED:
                    gemKindName = "레드 젬";
                    gemSprite = icon[71];
                    break;
                case GemKind.BLUE:
                    gemKindName = "블루 젬";
                    gemSprite = icon[72];
                    break;
                case GemKind.GREEN:
                    gemKindName = "그린 젬";
                    gemSprite = icon[73];
                    break;
                case GemKind.YELLOW:
                    gemKindName = "옐로 젬";
                    gemSprite = icon[74];
                    break;
            }
            //젬의 타입을 설정하는 순간 젬의 이름과 아이콘 이미지가 저장됨

            GemOptionAdd();
        }
    }

    void GemOptionAdd()
    {
        int getPer = 60; //옵션이 붙는 기본 확률
        for (int i = 0; i < 5; i++)
        {
            List<int> percentage = new List<int>();
            for (int j = 0; j < 100; j++)
            {
                if (j < 100 - getPer) percentage.Add(0); //100 - getper 숫자만큼 percentage 리스트에 0 추가
                else percentage.Add(1); //그 외엔 1 추가
            }

            int rand = Random.Range(0, percentage.Count); //0 ~ precentage.Count == 99사이의 숫자를 랜덤으로 받음

            if (percentage[rand].Equals(0)) break; //만약 percentage[rand]의 숫자가 0이면 옵션을 부여하지 않고 탈출
            else //1이면 옵션을 부여
            {
                TextAsset textAsset = Resources.Load("XML/GemOptionDB") as TextAsset;
                XmlDocument gemOptionDB = new XmlDocument();
                gemOptionDB.LoadXml(textAsset.text);

                string type = null;
                switch (GemKind)
                {
                    case GemKind.RED:
                        type = "r_";
                        break;
                    case GemKind.BLUE:
                        type = "b_";
                        break;
                    case GemKind.GREEN:
                        type = "g_";
                        break;
                    case GemKind.YELLOW:
                        type = "y_";
                        break;
                }

                XmlNodeList gemOptionList = gemOptionDB.SelectNodes("rows/row");
                int typeNum = Random.Range(0, gemOptionList.Count / 4);
                type += typeNum.ToString();

                bool isDuplicate = false; //이미 동일한 종류의 옵션이 있는지 여부를 판단하는 bool
                if (this.gemOptionList.Count > 0)
                {
                    for (int j = 0; j < this.gemOptionList.Count; j++)
                    {
                        if (this.gemOptionList[j].kind.Equals(typeNum))
                        {
                            foreach (XmlNode node in gemOptionList)
                            {
                                if (node.SelectSingleNode("typeNum").InnerText.Equals(type))
                                {
                                    GemOption gemOption = new GemOption();
                                    gemOption = this.gemOptionList[j];

                                    int maxValue = int.Parse(node.SelectSingleNode("maxValue").InnerText);
                                    int randValue = Random.Range(1, maxValue + 1);
                                    gemOption.value += randValue;

                                    string dialog;
                                    string[] temp = node.SelectSingleNode("dialog").InnerText.Split('@');
                                    dialog = temp[0] + gemOption.value + temp[1];
                                    gemOption.dialog = dialog;

                                    this.gemOptionList[j] = gemOption;

                                    isDuplicate = true;
                                    break;
                                }
                            }

                            if (isDuplicate) break;
                        }
                    }
                }

                if (isDuplicate) continue;

                foreach (XmlNode node in gemOptionList)
                {
                    if (node.SelectSingleNode("typeNum").InnerText.Equals(type))
                    {
                        GemOption gemOption = new GemOption();
                        gemOption.kind = typeNum;

                        int maxValue = int.Parse(node.SelectSingleNode("maxValue").InnerText);
                        int randValue = Random.Range(1, maxValue + 1);
                        gemOption.value = randValue;

                        string dialog;
                        string[] temp = node.SelectSingleNode("dialog").InnerText.Split('@');
                        dialog = temp[0] + gemOption.value + temp[1];
                        gemOption.dialog = dialog;

                        this.gemOptionList.Add(gemOption);
                        break;
                    }
                }
            }

            getPer = (int)(getPer / 2f);
        }
    }

    public string GemData()
    {
        //kind, value, optionKind, optionValue;
        string gemData = null;
        gemData += gemKind.ToString() + ".";
        gemData += statValue.ToString() + ".";
        for(int i = 0; i < gemOptionList.Count; i++)
        {
            gemData += gemOptionList[i].kind.ToString() + "_";
            gemData += gemOptionList[i].value.ToString() + ".";
        }

        return gemData;
    }

    public GemOption CreateGemOption(int kind, int value)
    {
        GemOption gemOption = new GemOption();
        TextAsset textAsset = Resources.Load("XML/GemOptionDB") as TextAsset;
        XmlDocument gemOptionDB = new XmlDocument();
        gemOptionDB.LoadXml(textAsset.text);

        string type = null;
        switch (GemKind)
        {
            case GemKind.RED:
                type = "r_";
                break;
            case GemKind.BLUE:
                type = "b_";
                break;
            case GemKind.GREEN:
                type = "g_";
                break;
            case GemKind.YELLOW:
                type = "y_";
                break;
        }

        XmlNodeList gemOptionList = gemOptionDB.SelectNodes("rows/row");
        int typeNum = Random.Range(0, gemOptionList.Count / 4);
        type += typeNum.ToString();

        foreach (XmlNode node in gemOptionList)
        {
            if (node.SelectSingleNode("typeNum").InnerText.Equals(type))
            {                
                gemOption.kind = kind;
                gemOption.value = value;

                string dialog;
                string[] temp = node.SelectSingleNode("dialog").InnerText.Split('@');
                dialog = temp[0] + gemOption.value + temp[1];
                gemOption.dialog = dialog;

                this.gemOptionList.Add(gemOption);
                break;
            }
        }    

        return gemOption;
    }
}
