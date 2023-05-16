using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Xml;

///<summary>
///현재 스탯을 저장하는 구조체
///</summary>
struct Stat
{
    public int currStr;
    public int currInt;
    public int currDex;
    public int currVit;
}

///<summary>
///행동 정보를 저장하는 구조체
///</summary>
struct ActInfo
{
    public string name;
    public int priority;
    public int succesRate;
    public string type;
    public string animation;
    public string special;
    public string icon;
    public int formulaValue;
};

///<summary>
///전투 씬에서 일어나는 이벤트, 행동 등 전반적인 처리를 담당하는 클래스
///</summary>
public class BattleManager : Singleton<BattleManager>
{
    public GameObject battleSceneCanvas; //배틀씬에서 처음 보여지는 UI Canvas
    public GameObject actCanvas; //행동 슬롯을 클릭할 때 보여지는 UI Canvas
    public GameObject actCloseBack; //행동 슬롯이 활성화 상태일 때 끄기 위한 터치 영역
    public GameObject slotHolder; //행동 슬롯과 관련된 UI
    public GameObject explanationWin; //행동 리스트를 한번 클릭 했을 때 보여지는 행동 설명창
    public Image playerHpbar; //플레이어의 Hp 상태를 표시하는 UI
    public Image enemyHpbar; //적의 Hp 상태를 표시하는 UI
    public GameObject enemyName; //해당 배틀에서 플레이어와 교전하는 적 개체 이름
    public GameObject enemyActDisplay; //교전 중 적의 행동이 표시되는 UI
    public GameObject playerActDisplay; //교전 중 플레이어의 행동이 표시되는 UI
    public GameObject[] playerActSlot = new GameObject[6]; //플레이어의 행동 슬롯[슬롯 번호]
    public GameObject[] enemyActSlot = new GameObject[6]; //적의 행동 슬롯[슬롯 번호]
    public GameObject[] itemUI = new GameObject[3]; //플레이어가 소지한 아이템 수량을 업데이트 해주는 UI
    public GameObject battleStartBt; //배틀 시작 버튼
    public GameObject ResultWin; //전투가 끝났을 때 띄워지는 결과창
    public List<GameObject> selectActList = new List<GameObject>(); //현재 플레이어가 선택할 수 있는 스킬 목록
    public Transform content; //스킬 목록 content
    public GameObject player;
    public Enemy enemy; //적의 정보가 저장되어 있는 enemy 스크립트

    public int[] selectedActNum = new int[6]; //행동할 액션 [넣어진 슬롯 번호];
    public int[] enemyActNum = new int[6]; //적이 행동할 액션 [넣어진 슬롯 번호];

    bool[] actComplete = new bool[6]; //행동할 액션 [넣어진 슬롯 번호];   
    bool enemyDie; //적이 죽었는지 여부 확인
    bool playerDie; //플레이어가 죽었는지 여부 확인
    bool isItem = false; //아이템 사용 여부 확인
    bool isEnd = false; //결과창이 닫히고 탐색 씬으로 돌아가기 위한 bool 변수
    bool islevelUp = false; //레벨업 상태일 떄(CurrExp가 MaxExp보다 높을 때)
    public int turn; //한 번의 턴을 구분하는 변수
    int selectedSlotNum; //현재 선택한 슬롯의 번호
    int clickCount = 0; //선택창에서 해당 행동을 클릭한 횟수
    int backclickAct; //이전에 클릭한 행동
    string[] itemAct = new string[6]; //Act DB에서 받아온 아이템 종류를 저장하는 string 변수[넣어진 슬롯 번호]
    int[] backActCost = new int[6]; //이전에 선택했던 행동의 비용
    int backSlotNum; //이전의 선택했던 슬롯 번호[현재 선택한 슬롯 번호]
    int readyCount = 0; //현재 선택이 완료된 슬롯의 숫자를 판단하는 int 변수
    int tempPotion, tempFood, tempBomb; //던전에서 쓰여지는 포션, 음식, 폭탄 변수
    public Text apText; //Ap가 보여지는 텍스트
    public Text turnText; //현재 턴을 표시하는 텍스트
    public Text turnCheckText;
    public Text escapeText; //도주할 때 표시되는 텍스트
    public Text MissText; //대상이 공격을 회피했을 때 표시되는 텍스트
    XmlDocument skillDB; //스킬 정보가 저장되는 xml Doc
    Coroutine explain; //정보 UI가 출력되는 코루틴 변수
    List<GameObject> specialAnimOb = new List<GameObject>(); //spcial 타입 행동 애니메이션 오브젝트

    //액션 1= 공격, 2= 방어, 3= 강타, 4= 포션, 5= 음식, 6= 도주
    //ex)clickActnum[0] = 1 (첫번째 슬롯에 공격 행동)

    void Awake()
    {
        turn = 1;
        DataLoading();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("BattleScene"));
        //오픈된 전투 씬에 포커스를 맞춰서 해당 씬에 객체 생성 등 이벤트가 일어나게 함
        StartCoroutine(Battle());
        //전투가 실행될 때 탐색씬의 아이템 정보를 전투씬에 전달
        tempPotion = PlayerState.Instance.GetPotion;
        tempFood = PlayerState.Instance.GetFood;
        tempBomb = PlayerState.Instance.GetBomb;
        itemUI[0].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = tempPotion.ToString();
        itemUI[1].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = tempFood.ToString();
        itemUI[2].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = tempBomb.ToString();
        //스테이지가 0(즉, 튜토리얼 스테이지일 경우)일 때 튜토리얼 상태 활성화
    }

    ///<summary>
    ///전투에 필요한 DB 등 데이터를 로딩하는 함수
    ///</summary>
    void DataLoading()
    {        
        TextAsset textAsset = Resources.Load("XML/BattleActDB") as TextAsset;
        skillDB = new XmlDocument();
        skillDB.LoadXml(textAsset.text);
    }

    ///<summary>
    ///매개변수로 넘겨진 int와 동일한 num을 지닌 act table이 담긴 XmlNodeList를 반환하는 함수
    ///</summary>
    XmlNode SelectXmlNode(int select)
    {
        XmlNodeList cost_Table = skillDB.SelectNodes("rows/row");
        return cost_Table[select];
    }

    ///<summary>
    ///전투 전체 흐름을 조정하는 코루틴 함수<br/>
    ///플레이어, 또는 적이 사망할 때 까지 실행
    ///</summary>
    IEnumerator Battle()
    {
        if (DataPassing.stageNum == 0)
        {
            enemy.currHp = 1000;
            enemyHpbar.fillAmount = (float)enemy.currHp / enemy.maxHp;
            yield return StartCoroutine(BattleTutorial(8));
        }

        Debug.Log("배틀 시작");
        playerHpbar.fillAmount = (float)PlayerState.Instance.CurrHp / PlayerState.Instance.MaxHp;
        yield return StartCoroutine(EnemyEmergenceEffect());
        yield return StartCoroutine(TurnDisplay());
        enemy.SetPattern();
        EnemyPatternCheck();

        while (!enemyDie && !playerDie) yield return null;

        //플레이어와 적의 특수 애니메이션 오브젝트를 모두 제거
        for (int i = 0; i < specialAnimOb.Count; i++)
        {
            Destroy(specialAnimOb[i].gameObject);
        }
        specialAnimOb.Clear();

        if (enemyDie)
        {
            yield return EffectManager.Instance.FadeIn(enemy.gameObject.GetComponent<SpriteRenderer>(), 1.5f);
            StartCoroutine(SetResultWin());
        }
        else if(playerDie)
        {
            BattleEnd();
        }
    }

    ///<summary>
    ///적을 조우했을 때 실행되는 연출 코루틴 함수<br/>
    ///</summary>
    IEnumerator EnemyEmergenceEffect()
    {
        enemyName.SetActive(true);
        slotHolder.SetActive(false);
        enemyName.GetComponent<Text>().text = enemy.enemyName + " 출현!";
        RectTransform rect = enemyName.GetComponent<RectTransform>();
        while (rect.anchoredPosition.x < 0f)
        {
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, new Vector2(100f, 0f), Time.deltaTime);
            yield return null;
        }

        float speed = 500;
        while (rect.anchoredPosition.x < 1900)
        {
            rect.anchoredPosition += new Vector2(speed * Time.deltaTime, 0f);
            speed += 100f;
            yield return null;
        }

        rect.anchoredPosition = new Vector2(-1900f, 0f);
        enemyName.SetActive(false);
    }

    ///<summary>
    ///현재 턴 숫자를 표시하는 코루틴 함수
    ///</summary>
    IEnumerator TurnDisplay()
    {
        turnText.text = turn + "번째 턴";
        yield return StartCoroutine(EffectManager.Instance.FadeOut(turnText, 1f));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(EffectManager.Instance.FadeIn(turnText, 1f));
        slotHolder.SetActive(true);
        turnCheckText.text = "Turn " + turn;
        NewTurn();
    }
    ///<summary>
    ///새로운 턴이 시작됐을 때 이전 턴에서 불필요한 값들을 초기화 시키는 함수
    ///</summary>
    void NewTurn()
    {
        playerHpbar.fillAmount = (float)PlayerState.Instance.CurrHp / PlayerState.Instance.MaxHp;
        Sprite[] spr = Resources.LoadAll<Sprite>("Icons/slotFont");
        //플레이어 슬롯 이미지를 모두 초기 상태로 되돌림
        for (int i = 0; i < 6; i++)
        {
            SpriteRenderer slotsp = slotHolder.transform.GetChild(0).transform.GetChild(0).transform.GetChild(i).transform.GetChild(0).GetComponent<SpriteRenderer>();
            slotsp.sprite = spr[i];
            slotHolder.transform.GetChild(0).transform.GetChild(0).transform.GetChild(i).transform.GetChild(0).localScale = new Vector3(100, 100, 1);
            SpriteRenderer eslotsp = slotHolder.transform.GetChild(0).transform.GetChild(1).transform.GetChild(i).transform.GetChild(0).GetComponent<SpriteRenderer>();
            eslotsp.sprite = spr[6];
            slotHolder.transform.GetChild(0).transform.GetChild(1).transform.GetChild(i).transform.GetChild(0).localScale = new Vector3(100, 100, 1);
            backSlotNum = 99;
            selectedActNum[i] = 99;
        }
        //슬롯에 저장된 예전 정보들을 전부 초기화
        readyCount = 0;
        backSlotNum = 99;
        
        for (int i = 0; i < 6; i++)
        {
            backActCost[i] = 0;
            itemAct[i] = null;
        }
        //AP 회복(기본 회복량(6) + 지능 + 이전 턴 잔여AP)
        PlayerState.Instance.currAp = 6 + PlayerState.Instance.CurrIntelligence + PlayerState.Instance.residualAp;
        apText.text = "AP: " + PlayerState.Instance.currAp;
        enemy.GetComponent<Enemy>().SetPattern();

    }

    ///<summary>
    ///주사위를 던져 나온 숫자만큼 적의 패턴을 확인하는 함수<br/>
    ///</summary>
    void EnemyPatternCheck()
    {
        battleSceneCanvas.SetActive(false);
        int diceValue = DiceManager.Instance.DiceRoll(1);
        List<int> checkList = new List<int>();

        for (int i = 0; i < diceValue; i++)
        {
            int rand = Random.Range(0, 6);

            if (!checkList.Contains(rand)) checkList.Add(rand);
            else i--;
        }

        for (int i = 0; i < checkList.Count; i++)
        {
            XmlNode selectAct = SelectXmlNode(enemyActNum[checkList[i]]); //패턴의 스킬 정보를 가져옴
            string icon = selectAct.SelectSingleNode("icon").InnerText;
            enemyActSlot[checkList[i]].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite
             = Resources.Load<Sprite>("Icons/Act_" + icon);
            enemyActSlot[checkList[i]].transform.GetChild(0).localScale = new Vector3(45, 45, 1);
        }
        if(enemyActNum[5] == 8)
        {
            enemyActSlot[5].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite
         = Resources.Load<Sprite>("Icons/Act_" + "finishMove");
            enemyActSlot[5].transform.GetChild(0).localScale = new Vector3(45, 45, 1);
        }
    }

    ///<summary>
    ///전투가 끝난 후 보상목록 UI를 보여주는 함수
    ///</summary>
    IEnumerator SetResultWin()
    {
        ResultWin.SetActive(true);
        Image resWinImage = ResultWin.GetComponent<Image>();
        //결과창 FadeOut
        yield return StartCoroutine(EffectManager.Instance.FadeOut(resWinImage, 0.4f)); 
        ResultWin.transform.GetChild(0).gameObject.SetActive(true);
        Text resTitle = ResultWin.transform.GetChild(0).GetComponent<Text>();
        yield return StartCoroutine(EffectManager.Instance.FadeOut(resTitle, 0.4f));
        ResultWin.transform.GetChild(1).gameObject.SetActive(true);
        //결과창에 물리친 몬스터의 보상 값 입력
        RewardApply();
        ResultWin.transform.GetChild(2).gameObject.SetActive(true);
        if (PlayerState.Instance.currExp >= PlayerState.Instance.MaxExp)
        {
            PlayerState.Instance.LevelUp();
            ResultWin.transform.GetChild(3).gameObject.SetActive(true);
            islevelUp = true;
        }
        while (!isEnd)
        {
            if (islevelUp == true)
            {
                ResultWin.transform.GetChild(3).GetComponent<Text>().color = Color.red;
                yield return new WaitForSeconds(0.3f);
                ResultWin.transform.GetChild(3).GetComponent<Text>().color = Color.black;
                yield return new WaitForSeconds(0.3f);
            }
            yield return null;
        }
        islevelUp = false;
        for (int i = 0; i < 4; i++)
        {
            ResultWin.transform.GetChild(i).gameObject.SetActive(false);
        }
        yield return StartCoroutine(EffectManager.Instance.FadeIn(resWinImage, 0.4f));
        ResultWin.SetActive(false);

        //모든 연출이 끝나면 전투를 마무리하고 탐색씬으로 이동하는 BattleEnd 함수 실행 
        BattleEnd();
    }

    ///<summary>
    ///획득한 보상을 플레이어 상태에 적용 시켜주는 함수
    ///</summary>
    void RewardApply()
    {
        //잼 획득 확률
       float acheivetemp = Random.Range(0, 100);
        //잼 획득 확률에 들어갔을 때 잼을 생성하여 플레이어에게 정보 전달
        if (acheivetemp <= enemy.getItemPercentage)
        {
            GameObject gemSlot = Instantiate(Resources.Load("GemSlot") as GameObject);
            gemSlot.transform.SetParent(ResultWin.transform.GetChild(1).GetChild(2).transform);
            DungeonManager.Instance.GetGem += 1;
            gemSlot.transform.localScale = Vector3.one;
        }
        //획득 골드, 경험치 정보 전달
        Text MoneyText = ResultWin.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>();
        Text expText = ResultWin.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>();

        DungeonManager.Instance.GetMoney += enemy.getMoney;
        PlayerState.Instance.currExp += enemy.getExp;
        DungeonManager.Instance.expBarUI.fillAmount = (float)PlayerState.Instance.currExp / PlayerState.Instance.MaxExp;
        MoneyText.text = enemy.getMoney.ToString();
        expText.text = enemy.getExp.ToString();
    }

    ///<summary>
    ///배틀 종료 처리를 진행하는 함수
    ///</summary>
    void BattleEnd()
    {
        PlayerState.Instance.currAp = 0;
        PlayerState.Instance.residualAp = 0;
        if (DataPassing.isBoss) DataPassing.isBoss = false;
        Debug.Log("배틀 종료");
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("DungeonScene"));
        DataPassing.playerDie = playerDie;
        DungeonManager.Instance.BattleEnd();
        SceneManager.UnloadSceneAsync("BattleScene");
    }

    ///<summary>
    ///전투 결과 화면을 종료하는 버튼 기능
    ///</summary>
    public void ExitBattle()
    {
        isEnd = true;
    }

    ///<summary>
    ///선택한 Act슬롯 번호를 받은 후 플레이어가 소유한 Act 리스트를 표시해주어 행동을 저장하는 함수
    ///</summary>
    public void SetUpActWindow(int slotnum)
    {
        for (int i = 0; i < PlayerState.Instance.actList.Count; i++) //플레이어가 소유한 행동 리스트 개수만큼 for문 돌림
        {
            int num = PlayerState.Instance.actList[i]; //플레이어의 행동 i번째 스킬(번호)을 저장
            XmlNode selectAct = SelectXmlNode(num); //num번째 스킬 정보를 가져옴
            GameObject slot = Instantiate(Resources.Load<GameObject>("Act_Slot"));
            string icon = selectAct.SelectSingleNode("icon").InnerText;
            string type = selectAct.SelectSingleNode("type").InnerText;
            slot.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Icons/Act_" + icon);
            //슬롯의 아이콘 이미지를 스킬과 맞는 것으로 변경
            slot.transform.GetChild(1).GetComponent<Text>().text = selectAct.SelectSingleNode("name").InnerText;
            //슬롯 이름을 DB 내 스킬 이름으로 변환
            slot.transform.GetChild(2).GetComponent<Text>().text = selectAct.SelectSingleNode("ap").InnerText;
            //AP소모 text를 DB 내 소모량으로 변환
            slot.GetComponent<ActSlot>().actValue = num;
            slot.transform.SetParent(content);
            slot.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            backclickAct = 0;

            //행동 선택창을 열 때 아이템 수량을 검사, 수량이 0이하라면 해당 아이템 버튼 이벤트 비활성화
            if(type == "ItemUse" || DataPassing.isBoss)
            {
                switch(icon)
                {
                    case"potion":
                        if (tempPotion <= 0)
                        {
                            slot.GetComponent<Image>().color = Color.red;
                            Destroy(slot.GetComponent<ActSlot>());
                            break;
                        }
                        else break;
                    case "food":
                        if (tempFood <= 0)
                        {
                            slot.GetComponent<Image>().color = Color.red;
                            Destroy(slot.GetComponent<ActSlot>());
                            break;
                        }
                        else break;
                    case "bomb":
                        if (tempBomb <= 0)
                        {
                            slot.GetComponent<Image>().color = Color.red;
                            Destroy(slot.GetComponent<ActSlot>());
                            break;
                        }
                        else break;
                    case "run":
                        slot.GetComponent<Image>().color = Color.red;
                        Destroy(slot.GetComponent<ActSlot>());
                        break;
                }
            }

            selectActList.Add(slot); //스킬 리스트 객체를 생성
        }

        actCanvas.SetActive(true);
        actCloseBack.SetActive(true);

        selectedSlotNum = slotnum;
    }

    ///<summary>
    ///해당 슬롯에 행동할 액션을 집어넣는 함수<br/>
    ///ex) 0 = 공격, 1 = 방어, 2 = 강타, 3 = 포션, 4 = 음식, 5 = 폭탄, 6 = 도주
    ///</summary>
    public void SelectAct(int act)
    {
        XmlNode selectAct = SelectXmlNode(act);
        string ex = selectAct.SelectSingleNode("explanation").InnerText;
        string icon = selectAct.SelectSingleNode("icon").InnerText;
        string type = selectAct.SelectSingleNode("type").InnerText;
        //두 번째 클릭이고, 선택한 act가 같을 때 두 번 클릭으로 인정.
        //같은 act 두 번 클릭이 아닐 경우 다시 첫 번째 클릭으로 판별.
        if (clickCount == 1 && backclickAct == act)
            clickCount++;
        else
        {
            //행동 설명 코루틴이 실행되고 있을 때 해당 코루틴 정지
            if (explain != null) StopCoroutine(explain);
            clickCount = 0;
            selectActList[backclickAct].GetComponent<Image>().color = new Color32(84, 84, 84, 255);
        }

        //첫 번째 클릭에 해당 행동의 설명창을 띄움
        if (clickCount == 0)
        {
            //XmlNode selectAct = SelectXmlNode(act);
            selectActList[act].GetComponent<Image>().color = new Color32(0, 0, 0, 255);
            backclickAct = act;
            clickCount++;

            explanationWin.SetActive(true);
            explanationWin.transform.GetChild(0).GetComponent<Text>().text = ex;

            explain = StartCoroutine(SetUpExplanation());
        }
        //같은 행동을 두 번 클릭했을 경우, 슬롯 이미지를 해당 행동 이미지로 변경, 저장
        else if (clickCount == 2)   
        {
            //현재 클릭한 슬롯 넘버와 전에 클릭했던 슬롯 넘버가 같을 경우
            //현재 클릭한 슬롯 넘버와 전에 클릭했던 슬롯 넘버가 다르고 해당 슬롯에 담긴 예전 행동의 비용이 0이 아닐 때
            //소모되었던 AP 반환, 준비 카운트 차감
            if ((backSlotNum == selectedSlotNum) || ((backSlotNum != selectedSlotNum) && backActCost[selectedSlotNum] != 0))
            {
                PlayerState.Instance.currAp += backActCost[selectedSlotNum];

                if (readyCount != 0)
                    readyCount--;
                Debug.Log("readyCount: " + readyCount);
            }
            //해당 행동의 타입이 아이템 사용이고 슬롯에 아이템 타입 정보가 담겨져 있지 않은 경우(다른 타입 행동이거나, 비었거나)
            if (type == "ItemUse" && itemAct[selectedSlotNum] == null)
            {
                switch (icon)
                {
                    case "potion":
                        tempPotion--;
                        itemAct[selectedSlotNum] = "potion";
                        itemUI[0].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = tempPotion.ToString();
                        break;
                    case "food":
                        tempFood--;
                        itemAct[selectedSlotNum] = "food";
                        itemUI[1].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = tempFood.ToString();
                        break;
                    case "bomb":
                        itemAct[selectedSlotNum] = "bomb";
                        tempBomb--;
                        itemUI[2].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = tempBomb.ToString();
                        break;
                }
            }
            //해당 슬롯에 아이템 정보가 담겨있을 때
            else if (itemAct[selectedSlotNum] != null)
            {
                switch(itemAct[selectedSlotNum])
                {
                    case "potion":
                        tempPotion++;
                        itemAct[selectedSlotNum] = icon;
                        itemUI[0].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = tempPotion.ToString();
                        break;
                    case "food":
                        tempFood++;
                        itemAct[selectedSlotNum] = icon;
                        itemUI[1].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = tempFood.ToString();
                        break;
                    case "bomb":
                        itemAct[selectedSlotNum] = icon;
                        tempBomb++;
                        itemUI[2].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = tempBomb.ToString();
                        break;
                }
               //해당 슬롯에 아이템 정보가 담겨있고, 선택한 행동도 아이템 사용 타입일 때
                if (type == "ItemUse")
                {
                    switch (icon)
                    {
                        case "potion":
                            tempPotion--;
                            itemAct[selectedSlotNum] = "potion";
                            itemUI[0].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = tempPotion.ToString();
                            break;
                        case "food":
                            tempFood--;
                            itemAct[selectedSlotNum] = "food";
                            itemUI[1].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = tempFood.ToString();
                            break;
                        case "bomb":
                            itemAct[selectedSlotNum] = "bomb";
                            tempBomb--;
                            itemUI[2].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = tempBomb.ToString();
                            break;
                    }
                }
            }
            string apcost = selectAct.SelectSingleNode("ap").InnerText;
            int apcostnum = int.Parse(apcost);
            PlayerState.Instance.currAp -= apcostnum;
            apText.text = "AP: " + PlayerState.Instance.currAp;

            //플레이어의 현재 AP가 음수일 때 (할당된 AP를 초과했을 때)
            if (PlayerState.Instance.currAp <= 0)
            {
                apText.color = Color.red;
            }
            else if (PlayerState.Instance.currAp > 0)
            {
                apText.color = Color.white;
            }
            StopCoroutine(explain);
            playerActSlot[selectedSlotNum].transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().sprite
                = Resources.Load<Sprite>("Icons/Act_" + icon);
            playerActSlot[selectedSlotNum].transform.GetChild(0).localScale = new Vector3(45, 45, 1);

            //선택이 끝나면 행동 선택과 관련된 모든 창 false, 행동 리스트 content Destory
            selectedActNum[selectedSlotNum] = act;
            actComplete[selectedSlotNum] = true;
            explanationWin.SetActive(false);
            actCanvas.SetActive(false);
            actCloseBack.SetActive(false);
            for (int i = 0; i < PlayerState.Instance.actList.Count; i++) Destroy(content.GetChild(i).gameObject);
            selectActList.Clear();
            //클릭 횟수 초기화, 현재 행동 비용, 행동 슬롯 저장(다음 선택, 선택 번복 위함)
            clickCount = 0;
            backActCost[selectedSlotNum] = apcostnum;
            backSlotNum = selectedSlotNum;
            readyCount++;

            //준비완료
            if (readyCount == 6 && PlayerState.Instance.currAp >= 0)
                battleStartBt.SetActive(true);
            else
                battleStartBt.SetActive(false);
        }
        Debug.Log("readyCount: " + readyCount);
    }

    public void AutoActSelect()
    {
        for (int i = 0; i < 6; i++)
        {
            if (selectedActNum[i] == 99)
            {
                int rand = Random.Range(0, 3);
                XmlNode selectAct = SelectXmlNode(rand);
                string icon = selectAct.SelectSingleNode("icon").InnerText;
                selectedActNum[i] = rand;
                playerActSlot[i].transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().sprite
                       = Resources.Load<Sprite>("Icons/Act_" + icon);
                playerActSlot[i].transform.GetChild(0).localScale = new Vector3(45, 45, 1);
                string apcost = selectAct.SelectSingleNode("ap").InnerText;
                int apcostnum = int.Parse(apcost);
                PlayerState.Instance.currAp -= apcostnum;
                backActCost[i] = apcostnum;
                backSlotNum = i;
                apText.text = "AP: " + PlayerState.Instance.currAp;
            }
            readyCount = 6;

            if (PlayerState.Instance.currAp >= 0)
            {
                apText.color = Color.white;
                battleStartBt.SetActive(true);
            }
            else
            {
                apText.color = Color.red;
                battleStartBt.SetActive(false);
            }
        }
    }
   
    ///<summary>
    ///행동 리스트 한 번 클릭시 해당 행동에 관한 설명창을 띄워주는 코루틴 함수<br/>
    ///</summary>
    IEnumerator SetUpExplanation()
    {
        RectTransform rect = explanationWin.transform.GetChild(0).GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-1000, 0);
        Vector2 temp = rect.anchoredPosition;
        float speed = 300f;
        while (true)
        {
            if (rect.anchoredPosition.x < 2000)
            {
                rect.anchoredPosition += new Vector2(speed * Time.deltaTime, 0f);
                temp.x = -1500;
                yield return null;
            }
            else
            {
                rect.anchoredPosition = temp;
            }
        }
    }

    ///<summary>
    ///행동을 선택하지 않고 actCloseBack 영역을 눌렀을 때 실행되는 함수<br/>
    ///행동 목록 창을 닫는 함수
    ///</summary>
    public void DontSelectAct()
    {
        if(explain != null)
        StopCoroutine(explain);
        actCanvas.SetActive(false);
        actCloseBack.SetActive(false);
        explanationWin.SetActive(false);
        for (int i = 0; i < PlayerState.Instance.actList.Count; i++) Destroy(content.GetChild(i).gameObject);
        selectActList.Clear();
        clickCount = 0;
    }
    ///<summary>
    ///전투 씬에서 Start 버튼을 클릭했을 때 실행되는 함수
    ///</summary>
    public void ClickStartButton()
    {
        PlayerState.Instance.residualAp = PlayerState.Instance.currAp;
        readyCount = 0;
        battleStartBt.SetActive(false);
        slotHolder.SetActive(false);
        StartCoroutine(Engagement());
    }
    ///<summary>
    ///슬롯에 저장된 행동대로 교전 코루틴을 실행하는 함수
    ///</summary>
    public IEnumerator Engagement()
    {
        ActInfo[] playerAct = Act("player");
        ActInfo[] enemyAct = Act("enemy");

        for (int i = 0; i < 6; i++)
        {
            yield return StartCoroutine(Engage(playerAct[i], enemyAct[i]));
            if (playerDie || enemyDie) yield break;
        }

        turn++;
        yield return StartCoroutine(TurnDisplay());
        enemy.SetPattern();
        EnemyPatternCheck();
    }

    ///<summary>
    ///플레이어, 에너미 두 객체의 행동 정보를 비교하여 행동에 따른 실제 수치 처리 및 애니메이션을 재생하는 코루틴 함수
    ///</summary>
    IEnumerator Engage(ActInfo playerAct, ActInfo enemyAct)
    {
        playerActDisplay.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Icons/Act_" + playerAct.icon);
        playerActDisplay.transform.GetChild(1).GetComponent<Text>().text = playerAct.name;
        enemyActDisplay.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("Icons/Act_" + enemyAct.icon);
        enemyActDisplay.transform.GetChild(0).GetComponent<Text>().text = enemyAct.name;
        playerActDisplay.SetActive(true);
        enemyActDisplay.SetActive(true);
        RectTransform rect = playerActDisplay.GetComponent<RectTransform>();
        RectTransform rect2 = enemyActDisplay.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-600f, 0);
        rect2.anchoredPosition = new Vector2(600f, 0);
        float speed = 1200f;
        while (rect.anchoredPosition.x < -20f && rect2.anchoredPosition.x > 20f)
        {
            rect.anchoredPosition += new Vector2(speed * Time.deltaTime, 0f);
            rect2.anchoredPosition -= new Vector2(speed * Time.deltaTime, 0f);
            yield return null;
        }
        yield return new WaitForSeconds(1.0f);
        playerActDisplay.SetActive(false);
        enemyActDisplay.SetActive(false);
        //서로의 우선순위를 가져오고 높은쪽이 선공, 같으면 플레이어부터 행동
        //플레이어가 선공
        if (playerAct.priority >= enemyAct.priority)
        {
            //플레이어와 적의 행동 확률을 계산할 랜덤 int
            int playerRand = Random.Range(0, 100);
            int enemyRand = Random.Range(0, 100);
            switch (playerAct.type)
            {
                case "HPDamage":
                    if (playerRand <= playerAct.succesRate + PlayerState.Instance.CurrDexterity)
                    {
                        if (playerAct.formulaValue <= 0)
                            playerAct.formulaValue = 1;
                        enemy.currHp -= playerAct.formulaValue;
                    }
                    break;
                case "Special":
                    switch (playerAct.special)
                    {
                        case "0":
                            if (playerRand <= playerAct.succesRate + PlayerState.Instance.CurrDexterity)
                            {
                                escapeText.text = "도주에 성공했다!";
                                yield return StartCoroutine(EscapeProd());
                                BattleEnd();
                                yield break;
                            }
                            else
                                break;
                        case "1":
                            if (enemyAct.special == null)
                            {
                                enemyAct.formulaValue = (enemyAct.formulaValue / 3) - PlayerState.Instance.GreenGemOptionValue(1);
                                if (enemyAct.formulaValue < 0) enemyAct.formulaValue = 1;
                            }
                            break;
                        case "3":
                            if (enemyAct.type == "HPDamage")
                            {
                                enemy.currHp -= enemyAct.formulaValue;
                            }
                            break;
                    }
                    break;
                case "ItemUse":
                    isItem = true;
                    switch(playerAct.name)
                    {
                        case "포션":
                            PlayerState.Instance.CurrHp += Expendables.UsePotion;
                            break;
                        case "음식":
                            PlayerState.Instance.residualAp += Expendables.BattleUseFood;
                            break;
                        case "폭탄":
                            enemy.currHp -= Expendables.AttackBomb;
                            break;
                    }
                    break;
            }
            //행동 성공 확률에 만족하면 해당 행동 애니메이션 코루틴 실행
            if(playerRand <= playerAct.succesRate + PlayerState.Instance.CurrDexterity)
            yield return StartCoroutine(PlayerActionProd(playerAct, enemyAct));
            //만족하지 않으면 해당 행동 실패 애니메이션 코루틴 실행
            else yield return StartCoroutine(PlayerFailActProd(playerAct, enemyAct));

            if (enemy.currHp <= 0)
            {
                enemyDie = true;
                yield break;
            }            

            switch (enemyAct.type)
            {
                case "HPDamage":
                    if (enemyRand <= enemyAct.succesRate)
                    {
                        if (enemyAct.formulaValue <= 0)
                            enemyAct.formulaValue = 1;
                        PlayerState.Instance.CurrHp -= enemyAct.formulaValue;
                    }
                    break;
                case "Special":
                    switch (enemyAct.special)
                    {
                        case "1":
                            if (playerAct.special == null)
                            {
                                playerAct.formulaValue = playerAct.formulaValue / 3;
                                if (playerAct.formulaValue < 0) playerAct.formulaValue = 1;
                            }
                            break;
                        case "3":
                            if (playerAct.type == "HPDamage")
                            {
                                PlayerState.Instance.CurrHp -= playerAct.formulaValue / 2;
                            }
                            break;
                    }
                    break;
            }

            if (enemyRand <= enemyAct.succesRate)
                yield return StartCoroutine(EnemyActionProd(enemyAct, playerAct));
            //만족하지 않으면 해당 행동 실패 애니메이션 코루틴 실행
            else
                yield return StartCoroutine(EnemyFailActProd(enemyAct, playerAct));

            if (playerAct.special != "0" && playerAct.special != null || enemyAct.special != null)
            {
                for (int i = 0; i < specialAnimOb.Count; i++)
                {
                    Destroy(specialAnimOb[i].gameObject);
                }
                specialAnimOb.Clear();
            }
        }
        //적이 선공(playerAct.priority < enemyAct.priority)
        else
        {
            int playerRand = Random.Range(0, 100);
            int enemyRand = Random.Range(0, 100);
            switch (enemyAct.type)
            {
                case "HPDamage":
                    if (enemyRand <= enemyAct.succesRate)
                    {
                        if (enemyAct.formulaValue <= 0)
                            enemyAct.formulaValue = 1;
                        PlayerState.Instance.CurrHp -= enemyAct.formulaValue;
                    }
                    break;
                case "Special":
                    switch (enemyAct.special)
                    {
                        case "1":
                            if (playerAct.special == null)
                            {
                                playerAct.formulaValue = playerAct.formulaValue / 3;
                                if (playerAct.formulaValue < 0) playerAct.formulaValue = 1;
                            }
                            break;
                        case "3":
                            if (playerAct.type == "HPDamage")
                            {
                                PlayerState.Instance.CurrHp -= playerAct.formulaValue / 2;
                            }
                            break;
                    }
                    break;
            }

            if (enemyRand <= enemyAct.succesRate)
                yield return StartCoroutine(EnemyActionProd(enemyAct, playerAct));
            //만족하지 않으면 해당 행동 실패 애니메이션 코루틴 실행
            else
                yield return StartCoroutine(EnemyFailActProd(enemyAct, playerAct));

            switch (playerAct.type)
            {
                case "HPDamage":
                    if (playerRand <= playerAct.succesRate + PlayerState.Instance.CurrDexterity)
                    {
                        if (playerAct.formulaValue <= 0)
                            playerAct.formulaValue = 1;
                        if (enemyAct.special == "2") playerAct.formulaValue += PlayerState.Instance.RedGemOptionValue(1);
                        enemy.currHp -= playerAct.formulaValue;
                    }
                    break;
                case "Special":
                    switch (playerAct.special)
                    {
                        case "0":
                            if (playerRand <= playerAct.succesRate + PlayerState.Instance.CurrDexterity)
                            {
                                BattleEnd();
                                yield break;
                            }
                            else
                                break;
                        case "1":
                            if (enemyAct.special == null)
                                enemyAct.formulaValue = enemyAct.formulaValue / 3;
                            if (enemyAct.formulaValue < 0) enemyAct.formulaValue = 1;
                            break;
                        case "3":
                            if (enemyAct.type == "HPDamage")
                            {
                                enemy.currHp -= enemyAct.formulaValue;
                            }
                            break;
                    }
                    break;
                case "ItemUse":
                    isItem = true;
                    switch (playerAct.name)
                    {
                        case "포션":
                            PlayerState.Instance.CurrHp += Expendables.UsePotion;
                            break;
                        case "음식":
                            PlayerState.Instance.residualAp += Expendables.BattleUseFood;
                            break;
                        case "폭탄":
                            enemy.currHp -= Expendables.AttackBomb;
                            break;
                    }
                    break;
            }

            if (playerRand <= playerAct.succesRate + PlayerState.Instance.CurrDexterity)
                yield return StartCoroutine(PlayerActionProd(playerAct, enemyAct));
            //만족하지 않으면 해당 행동 실패 애니메이션 코루틴 실행
            else
                yield return StartCoroutine(PlayerFailActProd(playerAct, enemyAct));

            if (enemy.currHp <= 0)
            {
                enemyDie = true;
                yield break;
            }
            if (playerAct.special != "0" && playerAct.special != null || enemyAct.special != null)
            {
                for (int i = 0; i < specialAnimOb.Count; i++)
                {
                    Destroy(specialAnimOb[i].gameObject);
                }
                specialAnimOb.Clear();
            }
        }

        if (PlayerState.Instance.CurrHp <= 0)
        {
            if(DataPassing.stageNum.Equals(0))
            {
                DataPassing.tutorialEnd = true;
                enemyDie = true;
                BattleEnd();
                yield break;
            }

            playerDie = true;
            yield break;
        }
    }
    ///<summary>
    ///플레이어 행동 연출 코루틴
    ///</summary>
    IEnumerator PlayerActionProd(ActInfo playerAct, ActInfo enemyAct)
    {
        //피격 연출을 위해 대상인 enemy의 SpriteRenderer와 Tramsform를 변수에 저장
        SpriteRenderer enemySp = enemy.gameObject.GetComponent<SpriteRenderer>();
        Transform enemyTr = enemy.gameObject.transform;

        if (playerAct.special == "0")
        {
            escapeText.text = "도주에 실패했다!";
            yield return StartCoroutine(EscapeProd());
            yield break;
        }

        //아이템 사용 상태인 경우
        if (isItem == true)
        {
            GameObject itemObject = Instantiate(Resources.Load("PlayerAnimPrefabs/Anim_" + playerAct.animation) as GameObject,
            new Vector3(enemy.transform.position.x, enemy.transform.position.y, 1), Quaternion.identity);
            Animator itemanim = itemObject.GetComponent<Animator>();
            while (itemanim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
            Destroy(itemObject);
            playerHpbar.fillAmount = (float)PlayerState.Instance.CurrHp / PlayerState.Instance.MaxHp;
            enemyHpbar.fillAmount = (float)enemy.currHp / enemy.maxHp;
            apText.text = "AP: " + PlayerState.Instance.residualAp;
            switch(playerAct.animation)
            {
                case "Potion":
                    PlayerState.Instance.GetPotion--;
                    break;
                case "Food":
                    PlayerState.Instance.GetFood--;
                    break;
                case "Bomb":
                    PlayerState.Instance.GetBomb--;
                    StartCoroutine(EffectManager.Instance.EnemyHitEffect(enemySp, 0.3f));
                    StartCoroutine(EffectManager.Instance.ShakeEffect(enemyTr, 0.1f, 0.3f));
                    break;
            }
            isItem = false;
            yield break;
        }
        //플레이어가 방어 타입 상태 일 경우
        else if (playerAct.special == "1" || playerAct.special == "3")
        {
            GameObject specialObject = Instantiate(Resources.Load("PlayerAnimPrefabs/Anim_" + playerAct.animation) as GameObject,
                new Vector3(enemy.transform.position.x, enemy.transform.position.y, enemy.transform.position.z - 2), Quaternion.identity);
            specialAnimOb.Add(specialObject);
            Animator specialanim = specialObject.GetComponent<Animator>();
            while (specialanim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
        }
        //아이템 사용 상태, 방어상태가 아닐 경우. (other special)
        else
        {
            //플레이어가 강타 타입, 적이 공격 타입일 경우와 적이 반격을 사용했을 경우 플레이어 공격 실패 연출 코루틴 실행
            if (playerAct.special == "2" && enemyAct.special == null || playerAct.type =="HPDamage" && enemyAct.special == "3")
            {
                enemy.currHp += playerAct.formulaValue;
                yield return StartCoroutine(PlayerFailActProd(playerAct, enemyAct));
                yield break;
            }

            GameObject animObject = Instantiate(Resources.Load("PlayerAnimPrefabs/Anim_" + playerAct.animation) as GameObject,
                    new Vector3(enemy.transform.position.x, enemy.transform.position.y, enemy.transform.position.z - 2), Quaternion.identity);
            Animator actanim = animObject.GetComponent<Animator>();
            GameObject specialObject = null;
            //적의 방어가 활성화일때
            if (enemyAct.special == "1")
            {
                //playerAct.special/ 1= 방어, 2= 강타, default= 공격
                switch (playerAct.special)
                {
                    case "1":
                        break;
                    case "2":
                        specialObject = Instantiate(Resources.Load("PlayerAnimPrefabs/Anim_GuardBreak") as GameObject);
                        StartCoroutine(EffectManager.Instance.EnemyHitEffect(enemySp, 0.3f));
                        StartCoroutine(EffectManager.Instance.ShakeEffect(enemyTr, 0.1f, 0.3f));
                        break;
                    default:
                        specialObject = Instantiate(Resources.Load("PlayerAnimPrefabs/Anim_Block") as GameObject);
                        StartCoroutine(EffectManager.Instance.EnemyHitEffect(enemySp, 0.1f));
                        break;
                }
            }
            else
            {
                StartCoroutine(EffectManager.Instance.EnemyHitEffect(enemySp, 0.3f));
                StartCoroutine(EffectManager.Instance.ShakeEffect(enemyTr, 0.1f, 0.3f));
            }
            //플레이어의 애니메이션이 끝나는 타이밍에 적의 HP를 깎아줌
            if (specialObject != null)
                specialObject.transform.parent = animObject.GetComponent<Transform>();
            while (actanim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
            Destroy(animObject);
            enemyHpbar.fillAmount = (float)enemy.currHp / enemy.maxHp;
            playerHpbar.fillAmount = (float)PlayerState.Instance.CurrHp / PlayerState.Instance.MaxHp;
        }
    }
    ///<summary>
    ///적 행동 연출 코루틴
    ///</summary>
    IEnumerator EnemyActionProd(ActInfo enemyAct, ActInfo playerAct)
    {
        //플레이어 피격 연출을 위해 Transform 변수 저장
        Transform playerTr = player.transform;
        //적이 방어 타입 상태인 경우
        if (enemyAct.special == "1" || enemyAct.special == "3")
        {
            GameObject spcialObject = Instantiate(Resources.Load("EnemyAnimPrefabs/Anim_" + enemyAct.animation) as GameObject,
                new Vector3(enemy.transform.position.x, enemy.transform.position.y, enemy.transform.position.z - 2), Quaternion.identity);
            specialAnimOb.Add(spcialObject);
            Animator specialanim = spcialObject.GetComponent<Animator>();
            while (specialanim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
        }
        //데미지 스킬이고 방어가 공격 대상(플레이어)의 방어가 활성화 상태일 때
        else
        {
            //아이템을 쓰지 않았고, 적이 강타, 플레이어가 공격일 경우와 플레이어가 반격일 경우 적 연출 종료, 데미지 값 반환
            if (enemyAct.special == "2" && playerAct.special == null && playerAct.type != "ItemUse" || enemyAct.type == "HPDamage" && playerAct.special == "3")
            {
                PlayerState.Instance.CurrHp += enemyAct.formulaValue;
                yield return StartCoroutine(EnemyFailActProd(enemyAct, playerAct));
                yield break;
            }
            StartCoroutine(EffectManager.Instance.ScaleUpDown(enemy.gameObject, 10, 1, 3, Vector3.one));
            GameObject specialObject = null;
            Transform enemyTr = enemy.gameObject.transform;
            if (playerAct.special == "1")
            {
                switch (enemyAct.special)
                {
                    case "1":
                        break;
                    case "2":
                            specialObject = Instantiate(Resources.Load("EnemyAnimPrefabs/Anim_GuardBreak") as GameObject);
                            StartCoroutine(EffectManager.Instance.ShakeEffect(playerTr, 0.1f, 0.3f));
                        break;
                    case "8":
                        specialObject = Instantiate(Resources.Load("EnemyAnimPrefabs/Anim_Finish") as GameObject);
                        StartCoroutine(EffectManager.Instance.ShakeEffect(playerTr, 0.5f, 2f));
                        break;
                    default:
                            specialObject = Instantiate(Resources.Load("EnemyAnimPrefabs/Anim_Block") as GameObject);
                            StartCoroutine(EffectManager.Instance.ShakeEffect(playerTr, 0.05f, 0.2f));
                        break;
                }
            }
            // 그 외 나머지 연출
            else
            {
                if(enemyAct.special == "8") StartCoroutine(EffectManager.Instance.ShakeEffect(playerTr, 0.5f, 2f));
                else StartCoroutine(EffectManager.Instance.ShakeEffect(playerTr, 0.1f, 0.3f));
                GameObject animObject = Instantiate(Resources.Load("EnemyAnimPrefabs/Anim_" + enemyAct.animation) as GameObject,
                new Vector3(enemy.transform.position.x, enemy.transform.position.y, enemy.transform.position.z - 2), Quaternion.identity);
                Animator actanim = animObject.GetComponent<Animator>();
                while (actanim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
                Destroy(animObject);
            }
            if (specialObject != null)
            {
                specialObject.transform.position = new Vector3(enemyTr.position.x, enemyTr.position.y, enemyTr.position.z - 1);
                Animator spcialAnim = specialObject.GetComponent<Animator>();
                while (spcialAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
                Destroy(specialObject);
            }
            enemyHpbar.fillAmount = (float)enemy.currHp / enemy.maxHp;
            playerHpbar.fillAmount = (float)PlayerState.Instance.CurrHp / PlayerState.Instance.MaxHp;
        }
    }
    ///<summary>
    ///플레이어 행동 실패 연출 코루틴
    ///</summary>
    IEnumerator PlayerFailActProd(ActInfo playerAct, ActInfo enemyAct)
    {
        Transform playerTr = player.transform;
        GameObject animObject = Instantiate(Resources.Load("PlayerAnimPrefabs/Anim_" + playerAct.animation) as GameObject,
                  new Vector3(enemy.transform.position.x, enemy.transform.position.y, enemy.transform.position.z - 2), Quaternion.identity);
        Animator actanim = animObject.GetComponent<Animator>();
        //적이 반격을 사용했을 때 연출
        if (enemyAct.special == "3")
        {
            while (actanim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
            Destroy(animObject);
            GameObject specialObject = Instantiate(Resources.Load("EnemyAnimPrefabs/Anim_CounterEf") as GameObject);
            Animator specialanim = specialObject.GetComponent<Animator>();
            while (specialanim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
            Destroy(specialObject);
            StartCoroutine(EffectManager.Instance.ScaleUpDown(enemy.gameObject, 10, 1, 3, Vector3.one));
            GameObject animObject2 = Instantiate(Resources.Load("EnemyAnimPrefabs/Anim_" + playerAct.animation) as GameObject,
                 new Vector3(enemy.transform.position.x, enemy.transform.position.y, enemy.transform.position.z - 2), Quaternion.identity);
            StartCoroutine(EffectManager.Instance.ShakeEffect(playerTr, 0.1f, 0.3f));
            Animator actanim2 = animObject2.GetComponent<Animator>();
            while (actanim2.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
            playerHpbar.fillAmount = (float)PlayerState.Instance.CurrHp / PlayerState.Instance.MaxHp;
            yield break;
        }
        while (actanim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
        Destroy(animObject);

        switch (playerAct.type)
            {
                case "HPDamage":
                    yield return StartCoroutine(EffectManager.Instance.EnemyEvade(MissText));
                    break;
                case "Special":
                    break;
            }
    }

    IEnumerator EnemyFailActProd(ActInfo enemyAct, ActInfo playerAct)
    {
        SpriteRenderer enemySp = enemy.gameObject.GetComponent<SpriteRenderer>();
        Transform enemyTr = enemy.gameObject.transform;

        switch (enemyAct.type)
        {
            case "HPDamage":
                yield return StartCoroutine(EffectManager.Instance.ScaleUpDown(enemy.gameObject, 10, 1, 3, Vector3.one));
                if (playerAct.special == "3")
                {
                    GameObject animObject = Instantiate(Resources.Load("PlayerAnimPrefabs/Anim_CounterEf") as GameObject,
                         new Vector3(enemy.transform.position.x, enemy.transform.position.y, enemy.transform.position.z - 2), Quaternion.identity);
                    Animator actanim = animObject.GetComponent<Animator>();
                    while (actanim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
                    Destroy(animObject);
                    GameObject animObject2 = Instantiate(Resources.Load("PlayerAnimPrefabs/Anim_" + enemyAct.animation) as GameObject,
                  new Vector3(enemy.transform.position.x, enemy.transform.position.y, enemy.transform.position.z - 2), Quaternion.identity);
                    Animator actanim2 = animObject2.GetComponent<Animator>();
                    StartCoroutine(EffectManager.Instance.EnemyHitEffect(enemySp, 0.3f));
                    StartCoroutine(EffectManager.Instance.ShakeEffect(enemyTr, 0.1f, 0.3f));
                    while (actanim2.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
                    Destroy(animObject2);
                    enemyHpbar.fillAmount = (float)enemy.currHp / enemy.maxHp;
                    yield break;
                }
                StartCoroutine(EffectManager.Instance.PlayerEvade(MissText));
                break;
            case "Special":
                break;
        }
        yield return null;
    }

    ///<summary>
    ///도주 연출
    ///</summary>
    IEnumerator EscapeProd()
    {
        yield return StartCoroutine(EffectManager.Instance.FadeOut(escapeText, 1f));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(EffectManager.Instance.FadeIn(escapeText, 1f));
        yield return null;
    }

    ///<summary>
    ///플레이어, 또는 적의 행동,스킬 정보를 DB에서 참조하여 반환하는 함수
    ///</summary>
    ActInfo[] Act(string subject)
    {
        ActInfo[] returnAct = new ActInfo[6];

        //플레이어 행동
        for (int i = 0; i < 6; i++)
        {
            XmlNode selectAct = null;
            switch (subject)
            {
                case "player":
                    selectAct = SelectXmlNode(selectedActNum[i]);
                    break;
                case "enemy":
                    selectAct = SelectXmlNode(enemyActNum[i]);
                    break;
            }
            returnAct[i].name = selectAct.SelectSingleNode("name").InnerText;
            returnAct[i].priority = int.Parse(selectAct.SelectSingleNode("priority").InnerText);
            returnAct[i].succesRate = int.Parse(selectAct.SelectSingleNode("successRate").InnerText);
            returnAct[i].type = selectAct.SelectSingleNode("type").InnerText;
            returnAct[i].animation = selectAct.SelectSingleNode("animation").InnerText;
            returnAct[i].icon = selectAct.SelectSingleNode("icon").InnerText;
            returnAct[i].special = selectAct.SelectSingleNode("special").InnerText;
            if (returnAct[i].special == "")
                returnAct[i].special = null;
            string formula = selectAct.SelectSingleNode("formula").InnerText;

            switch (returnAct[i].type)
            {
                case "ItemUse":
                    returnAct[i].formulaValue = 0; //수정 예정
                    break;
                case "Special":
                    returnAct[i].formulaValue = 0; //수정 예정
                    break;
                default:
                    returnAct[i].formulaValue = GetFormulaValue(formula, subject);
                    break;
            }
        }

        return returnAct;
    }

    ///<summary>
    ///행동, 스킬 DB에 저장된 공식을 수치로 변환하여 반환하는 함수
    ///</summary>
    int GetFormulaValue(string formula, string subject)
    {
        Stat main = new Stat();
        Stat other = new Stat();
        switch (subject)
        {
            case "player":
                main.currStr = PlayerState.Instance.CurrStrength;
                main.currInt = PlayerState.Instance.CurrIntelligence;
                main.currDex = PlayerState.Instance.CurrDexterity;
                main.currVit = PlayerState.Instance.CurrVitality;
                other.currStr = enemy.currStrength;
                other.currInt = enemy.currIntelligence;
                other.currDex = enemy.currDexterity;
                other.currVit = enemy.currVitality;
                break;
            case "enemy":
                main.currStr = enemy.currStrength;
                main.currInt = enemy.currIntelligence;
                main.currDex = enemy.currDexterity;
                main.currVit = enemy.currVitality;
                other.currStr = PlayerState.Instance.CurrStrength;
                other.currInt = PlayerState.Instance.CurrIntelligence;
                other.currDex = PlayerState.Instance.CurrDexterity;
                other.currVit = PlayerState.Instance.CurrVitality;
                break;
        }
        //subject 속성에 따른 main, other의 데이터 대입

        string strValue = null;
        string[] splitFormula = formula.Split('\x020');
        //우선 공식의 띄어쓰기를 기준으로 split 처리하여 splitFormula string배열에 삽입 

        for (int j = 0; j < splitFormula.Length; j++) //공식의 처음부터 끝까지 탐색
        {
            if (j % 2 == 0) //공식의 0, 2 ,4 .. 번째의 글자는 모두 숫자이므로, 숫자만을 따로 판별함
            {
                if (splitFormula[j].Contains(".")) //만약 해당 글자에 . 문자가 포함되어 있다면 대상을 구별해야하는 공식이므로 따로 판별 필요
                {
                    string[] check = splitFormula[j].Split('.'); //.을 기준으로 문자를 나눔
                    switch (check[0]) //. 앞의 문자가 a냐 b냐에 따라 스탯을 확인하는 주체가 달라짐 
                    {
                        case "a":
                            switch (check[1])
                            {
                                case "str":
                                    strValue += main.currStr.ToString();
                                    break;
                                case "dex":
                                    strValue += main.currDex.ToString();
                                    break;
                                case "int":
                                    strValue += main.currInt.ToString();
                                    break;
                                case "vit":
                                    strValue += main.currVit.ToString();
                                    break;
                            }
                            break;
                        case "b":
                            switch (check[1])
                            {
                                case "str":
                                    strValue += other.currStr.ToString();
                                    break;
                                case "dex":
                                    strValue += other.currDex.ToString();
                                    break;
                                case "int":
                                    strValue += other.currInt.ToString();
                                    break;
                                case "vit":
                                    strValue += other.currVit.ToString();
                                    break;
                            }
                            break;
                    }
                }
                else
                {
                    switch (splitFormula[j])
                    {
                        case "str":
                            strValue += main.currStr.ToString();
                            break;
                        case "dex":
                            strValue += main.currDex.ToString();
                            break;
                        case "int":
                            strValue += main.currInt.ToString();
                            break;
                        case "vit":
                            strValue += main.currVit.ToString();
                            break;
                        default:
                            strValue += splitFormula[j];
                            break;
                    }
                }
            }
            else
            {
                strValue += splitFormula[j];
            }

            strValue += " ";
        }

        string[] queue = strValue.Split('\x020');
        Queue<int> numQueue = new Queue<int>();
        Queue<string> signQueue = new Queue<string>();

        for (int i = 0; i < queue.Length; i++)
        {
            if (i % 2 == 0) numQueue.Enqueue(int.Parse(queue[i]));
            else signQueue.Enqueue(queue[i]);
        }

        int value = 0;
        bool isFirst = false;
        while (numQueue.Count != 0)
        {
            int firstNum = 0;
            int secondNum = 0;

            if (!isFirst)
            {
                firstNum = numQueue.Dequeue();
                secondNum = numQueue.Dequeue();
                isFirst = true;
            }
            else
            {
                firstNum = value;
                secondNum = numQueue.Dequeue();
            }

            switch (signQueue.Dequeue())
            {
                case "+":
                    value = firstNum + secondNum;
                    break;
                case "-":
                    value = firstNum - secondNum;
                    break;
                case "*":
                    value = firstNum * secondNum;
                    break;
                case "/":
                    value = (int)(firstNum / (float)secondNum);
                    break;
            }
        }

        return value;
    }

    public void SetEnemySlot(int num, int pattern)
    {
        enemyActNum[num] = pattern;

    }

    IEnumerator BattleTutorial(int eventNum)
    {
        DataPassing.isStory = true;
        string name = SceneManager.GetActiveScene().name;
        DataPassing.getSceneName = name;
        battleSceneCanvas.SetActive(false);
        enemy.gameObject.SetActive(false);
        DataPassing.storyKind = "Stage_" + DataPassing.stageNum + "_" + eventNum;
        SceneManager.LoadScene("StoryScene", LoadSceneMode.Additive); //인카운트 스토리 씬 로딩
        while (DataPassing.isStory) yield return null;
        battleSceneCanvas.SetActive(true);
        enemy.gameObject.SetActive(true);

    }
}
