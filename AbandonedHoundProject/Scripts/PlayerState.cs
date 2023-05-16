using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///<summary>
///플레이어의 상태변화를 나타내는 enum<br/>
///states List에 상태가 저장됨
///</summary>
public enum State
{
    RUN, GUARD, BREAK, EXHAUST, POISON
}

///<summary>
///플레이어의 HP, STAMINA 등 능력치나 상태를 저장, 반환하는 클래스
///</summary>
public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance = null;
    const int maximumItem = 9; //최대 아이템 소지량

    ///////////////////플레이어의 생명, 스태미나, 행동 게이지    
    int maxHp = 40; //플레이어의 최대 hp
    int currHp; //플레이어의 현재 hp
    int maxStamina = 150; //플레이어의 최대 Stamina
    int itemStamina = 0; //아이템 효과로 증가하는 Stamina
    int currStamina; //플레이어의 현재 Stamina 
    const int maxAp = 99; //플레이어의 최대 Ap;
    const int basicAp = 6; //기본적으로 보장되는 Ap 수치
    public int residualAp; //전투 행동 후 잔여 AP를 저장하는 변수
    public int currAp; //플레이어의 현재 Ap;

    public int MaxHp
    {
        get
        {
            int value;
            if (StateCheck(State.EXHAUST)) value = (int)((maxHp + (level * 10) + (CurrVitality * 10) + YellowGemOptionValue(1)) * 0.7f);
            else value = maxHp + (level * 10) + (CurrVitality * 10) + YellowGemOptionValue(1);

            if (value > 999) value = 999;
            return value;
        }
    }

    public int CurrHp
    {
        get
        {
            if (StateCheck(State.EXHAUST)) return (int)(currHp * 0.7f);
            else return currHp;
        }
        set
        {
            currHp = value;
            if (currHp > MaxHp) currHp = MaxHp;
            else if (currHp < 0) currHp = 0;
        }
    }

    public int MaxStamina
    {
        get
        {
            int value;
            if (StateCheck(State.EXHAUST)) value = (int)((maxStamina + (level * 10) + (CurrVitality * 10) + itemStamina) * 0.7f);
            else value = maxStamina + (level * 10) + (CurrVitality * 10) + itemStamina;

            if (value > 999) value = 999;
            return value;
        }
    }

    public int CurrStamina
    {
        get
        {
            if (StateCheck(State.EXHAUST)) return (int)(currStamina * 0.7f);
            else return currStamina;
        }
        set
        {
            currStamina = value;
            if (currStamina > MaxStamina) currStamina = MaxStamina;
            else if (currStamina < 0) currStamina = 0;
        }
    }
    ///////////////////////////////////////////////

    ///////////////////////플레이어의 힘, 민첩, 지능, 체력 스탯
    int currStrength;//상태이상으로 변하는 힘 스탯
    int currDexterity;//상태이상으로 변하는 민첩 스탯
    int currIntelligence;//상태이상으로 변하는 지능 스탯
    int currVitality; //상태이상으로 변하는 체력 스탯

    public int CurrStrength
    {
        get
        {
            if (StateCheck(State.EXHAUST))
            {
                if (redGem != null) return (int)((level + currStrength + redGem.statValue) * 0.5f);
                else return (int)((level + currStrength) * 0.5f);
            }
            else
            {
                if (redGem != null) return level + currStrength + redGem.statValue;
                else return level + currStrength;
            }
            //순수 능력치 + 현재(상태변화) 능력치 + 장비 능력치 = 반환값
        }
        set { currStrength = value; }
    }

    public int CurrDexterity
    {
        get
        {
            if (StateCheck(State.EXHAUST))
            {
                if (greenGem != null) return (int)((level + currDexterity + greenGem.statValue) * 0.5f);
                else return (int)((level + currDexterity) * 0.5f);
            }
            else
            {
                if (greenGem != null) return level + currDexterity + greenGem.statValue;
                else return level + currDexterity;
            }
        }
        set { currDexterity = value; }
    }

    public int CurrIntelligence
    {
        get
        {
            if (StateCheck(State.EXHAUST))
            {
                if (blueGem != null) return (int)((level + currIntelligence + blueGem.statValue) * 0.5f);
                else return (int)((level + currIntelligence) * 0.5f);
            }
            else
            {
                if (blueGem != null) return level + currIntelligence + blueGem.statValue;
                else return level + currIntelligence;
            }
        }
        set { currIntelligence = value; }
    }

    public int CurrVitality
    {
        get
        {
            if (StateCheck(State.EXHAUST))
            {
                if (yellowGem != null) return (int)((level + currVitality + yellowGem.statValue) * 0.5f);
                else return (int)((level + currVitality) * 0.5f);
            }
            else
            {
                if (yellowGem != null) return level + currVitality + yellowGem.statValue;
                else return level + currVitality;
            }
        }
        set { currVitality = value; }
    }

    ///////////////////////////////////////////////////////


    /////////////////////////플레이어의 소지 소모품 개수 및 등급
    public int potionGrade; //포션 등급
    public int foodGrade; //음식 등급
    public int bombGrade; //폭탄 등급

    int money; //골드
    int getPotion; //포션 소지량
    int getFood; //음식 소지량
    int getBomb; //폭탄 소지량

    public int GetPotion
    {
        get { return getPotion; }
        set
        {
            getPotion = value;
            if (getPotion < 0) getPotion = 0;
            else if (getPotion > maximumItem) getPotion = maximumItem;
            //value 대입 시 최소, 최대 수치 조절
        }
    }
    public int GetFood
    {
        get { return getFood; }
        set
        {
            getFood = value;
            if (getFood < 0) getFood = 0;
            else if (getFood > maximumItem) getFood = maximumItem;
        }
    }
    public int GetBomb
    {
        get { return getBomb; }
        set
        {
            getBomb = value;
            if (getBomb < 0) getBomb = 0;
            else if (getBomb > maximumItem) getBomb = maximumItem;
        }
    }
    public int Money
    {
        get { return money; }
        set
        {
            money = value;
            if (money > 9999999) money = 9999999;
            else if (money < 0) money = 0;
        }
    }
    /////////////////////////////////////////////////////////

    public int WallAtk
    {
        get
        {
            int value = (int)(CurrStrength / 3f) + RedGemOptionValue(0);
            if (value <= 0) value = 1;
            return value;
        }
    }

    ////////////////////////////////////////젬 옵션 관련 변수
    public int RedGemOptionValue(int kind)
    {
        if (redGem != null)
        {
            int value = 0;
            foreach (GemOption option in redGem.gemOptionList)
            {
                if (option.kind.Equals(kind))
                {
                    value = option.value;
                    break;
                }
            }
            return value;
        }
        else return 0;
    }

    public int GreenGemOptionValue(int kind)
    {
        if (greenGem != null)
        {
            int value = 0;
            foreach (GemOption option in greenGem.gemOptionList)
            {
                if (option.kind.Equals(kind))
                {
                    value = option.value;
                    break;
                }
            }
            return value;
        }
        else return 0;
    }

    public int BlueGemOptionValue(int kind)
    {
        if (blueGem != null)
        {
            int value = 0;
            foreach (GemOption option in blueGem.gemOptionList)
            {
                if (option.kind.Equals(kind))
                {
                    value = option.value;
                    break;
                }
            }
            return value;
        }
        else return 0;
    }

    public int YellowGemOptionValue(int kind)
    {
        if (yellowGem != null)
        {
            int value = 0;
            foreach (GemOption option in yellowGem.gemOptionList)
            {
                if (option.kind.Equals(kind))
                {
                    value = option.value;
                    break;
                }
            }
            return value;
        }
        else return 0;
    }
    /////////////////////////////////////////////////////////

    //////////////////////////////////플레이어의 레벨 및 경험치
    public int level; //플레이어의 레벨
    public int currExp; //플레이어의 현재 경험치

   public int MaxExp
    {
        get
        {
            int value = (int)(level * (20 * 1.5f));
            return value;
        }
    }

    //플레이어의 현재 레벨을 기준으로 한 최대 경험치
    /////////////////////////////////////////////////////////

    [HideInInspector]
    public List<int> actList = new List<int>(); //플레이어가 소지한 행동(스킬) 리스트를 저장하는 List
    public List<Gem> getGemList = new List<Gem>(); //플레이어가 소지한 젬의 리스트를 저장하는 List
    public List<State> states = new List<State>(); //플레이어의 상태를 저장하는 List

    public Gem redGem = null;
    public Gem blueGem = null;
    public Gem greenGem = null;
    public Gem yellowGem = null;

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        //DontDestroyOnLoad 객체이므로 싱글톤 패턴이 다소 달라 별도로 관리

        actList.Clear();
        getGemList.Clear();
        states.Clear();

        FirstStart();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DungeonManager.Instance.moveCount = 99;
        }
        //임시로 무브 카운트를 99로 만드는 디버그용 키

        if (Input.GetKeyDown(KeyCode.Backspace)) CurrHp = 1;
        if (Input.GetKeyDown(KeyCode.K))
        {
            Enemy enemy = GameObject.Find("Enemy").GetComponent<Enemy>();
            enemy.currHp = 1;
        }

        if (Input.GetKey(KeyCode.P)) Time.timeScale = 3f;
        else Time.timeScale = 1f;

        if (Input.GetKeyDown(KeyCode.N)) StartCoroutine(DungeonManager.Instance.AreaMove(true));

        //Debug.Log("현재 HP : " + CurrHp + ", 현재 스태미나 : " + CurrStamina);
    }
    //디버그용 조작 넣을 예정

    public void FirstStart()
    {
        for (int i = 0; i < 8; i++) actList.Add(i);
        //플레이어 행동(스킬)에 기본 행동들 추가
        level = 1;
        currStrength = 0;
        currDexterity = 0;
        currIntelligence = 0;
        currVitality = 0;

        currExp = 0;
        CurrHp = MaxHp;
        CurrStamina = MaxStamina;
        currAp = basicAp;
        //현재 Hp, Stamina, Ap 초기화

        Money = 3000;
        getPotion = 1;
        getFood = 1;
        getBomb = 1;
        potionGrade = 1;
        foodGrade = 1;
        bombGrade = 1;

        ////////
        Money = 999999;
        Gem tempRed = new Gem();
        tempRed.GemKind = GemKind.RED;
        tempRed.statValue = Random.Range(1, (1 * 3) + 1);
        redGem = tempRed;
        for (int i = 0; i < 5; i++)
        {
            Gem temp = new Gem();
            temp.GemKind = (GemKind)Random.Range(0, 4);
            temp.statValue = Random.Range(1, (1 * 3) + 1);
            getGemList.Add(temp);
        }
        ///////임시로 소유 젬, 돈 넣음 (삭제 예정)
    }

    public void ResetState()
    {
        CurrHp = MaxHp;
        CurrStamina = MaxStamina;
        states.Clear();
    }

    ///<summary>
    ///플레이어의 HP 상태를 업데이트 해주는 함수
    ///</summary>
    ///<param name="value">플레이어의 체력에 더해지거나 빼지는 값</param> 
    public void SetHp(int value)
    {
        CurrHp += value;
    }

    public void SetHp(string method, int value)
    {
        switch (method)
        {
            case "p":
                CurrHp += (int)(CurrHp / 100f * value);
                break;
            case "c":
                CurrHp += value;
                break;
        }
    }

    ///<summary>
    ///플레이어의 스테미너 상태를 업데이트 해주는 함수
    ///</summary>
    ///<param name="value">플레이어의 스테미너에 더해지거나 빼지는 값</param> 
    public void SetStamina(int value)
    {
        CurrStamina += value;

        if (!states.Contains(State.EXHAUST) && CurrStamina <= 0)
        {
            states.Add(State.EXHAUST);
        }
    }

    public void SetStamina(string method, int value)
    {
        switch (method)
        {
            case "p":
                CurrStamina += (int)(CurrStamina / 100f * value);
                break;
            case "c":
                CurrStamina += value;
                break;
        }

        if (!states.Contains(State.EXHAUST) && CurrStamina <= 0)
        {
            states.Add(State.EXHAUST);
        }
        else if(CurrStamina > 0) RemoveState(State.EXHAUST);
    }

    ///<summary>
    ///플레이어에게 특정 상태가 있는지 여부를 확인하는 함수
    ///</summary>
    public bool StateCheck(State checkState)
    {
        foreach (State state in states)
        {
            if (state.Equals(checkState)) return true;
        }       

        return false;
    }

    public void RemoveState(State checkState)
    {
        for (int i = 0; i < states.Count; i++)
        {
            if (states[i].Equals(checkState))
            {
                states.RemoveAt(i);
                break;
            }
        }
    }

    ///<summary>
    ///레벨업시 스탯에 변화를 주는 함수
    ///</summary>
    public void LevelUp()
    {
        level++;
        currExp = 0;
        DungeonManager.Instance.expBarUI.fillAmount = 0;
    }

    ///<summary>
    ///플레이어의 상태, 소지 아이템, 스탯 전달 함수
    ///</summary>
    public string GetPlayerStatus()
    {
        string info = "LV: " + level +
             "\n\nSTR: " + CurrStrength +
            "\nDEX: " + CurrDexterity +
            "\nINT: " + CurrIntelligence +
            "\nVIT: " + CurrVitality;

        return info;
    }

    void ResetAllState()
    {
        actList.Clear();
        getGemList.Clear();
        states.Clear();
        redGem = null;
        blueGem = null;
        greenGem = null;
        yellowGem = null;
    }

    public string SaveStateData()
    {
        //level, exp, money, expendablesValue, expendablesGrade, actList, getGemList

        string saveToData = null;
        saveToData += level.ToString() + ";";
        saveToData += currExp.ToString() + ";";
        saveToData += money.ToString() + ";";
        saveToData += GetPotion.ToString() + ";";
        saveToData += GetFood.ToString() + ";";
        saveToData += GetBomb.ToString() + ";";
        saveToData += potionGrade.ToString() + ";";
        saveToData += foodGrade.ToString() + ";";
        saveToData += bombGrade.ToString() + ";";
        for(int i = 0; i < actList.Count; i++)
        {
            saveToData += actList[i].ToString();
            if(i == actList.Count - 1) saveToData += ";";
            else saveToData += ",";
        }

        if (redGem != null) saveToData += redGem.GemData() + "T;";
        if (blueGem != null) saveToData += blueGem.GemData() + "T;";
        if (greenGem != null) saveToData += greenGem.GemData() + "T;";
        if (yellowGem != null) saveToData += yellowGem.GemData() + "T;";

        for (int i = 0; i < getGemList.Count; i++)
        {
            saveToData += getGemList[i].GemData() + "F;";
        }

        return saveToData;
    }

    public void LoadStateData(string dataToLoad)
    {        
        ResetAllState();

        string[] data = dataToLoad.Split(';');
        level = int.Parse(data[0]);
        currExp = int.Parse(data[1]);
        money = int.Parse(data[2]);
        GetPotion = int.Parse(data[3]);
        GetFood = int.Parse(data[4]);
        GetBomb = int.Parse(data[5]);
        potionGrade = int.Parse(data[6]);
        foodGrade = int.Parse(data[7]);
        bombGrade = int.Parse(data[8]);

        string[] act = data[9].Split(',');
        
        for(int i = 0; i < act.Length; i++)
        {
            int actNum = int.Parse(act[i]);
            actList.Add(actNum);
        }

        for(int i = 10; i < data.Length; i++)
        {
            string[] gemData = data[i].Split('.');
            
            GemKind kind;
            switch(gemData[0])
            {
                case "RED":
                    kind = GemKind.RED;
                    break;
                case "BLUE":
                    kind = GemKind.BLUE;
                    break;
                case "GREEN":
                    kind = GemKind.GREEN;
                    break;
                case "YELLOW":
                    kind = GemKind.YELLOW;
                    break;
                default:
                    continue;
            }
            int value = int.Parse(gemData[1]);
            Gem gem = new Gem();
            gem.GemKind = kind;
            gem.statValue = value;
            gem.gemOptionList.Clear();

            for(int j = 2; j < gemData.Length - 1; j++)
            {
                string[] gemOptionData = gemData[j].Split('_');
                int optionKind = int.Parse(gemOptionData[0]);
                int optionValue = int.Parse(gemOptionData[1]);

                GemOption gemOption = gem.CreateGemOption(optionKind, optionValue);
            }

            if(gemData[gemData.Length - 1].Equals("F"))
            {
                getGemList.Add(gem);
            }
            else
            {
                switch(kind)
                {
                    case GemKind.RED:
                        redGem = gem;
                        break;
                    case GemKind.BLUE:
                        blueGem = gem;
                        break;
                    case GemKind.GREEN:
                        greenGem = gem;
                        break;
                    case GemKind.YELLOW:
                        yellowGem = gem;
                        break;
                }
            }
        }
    }
}
