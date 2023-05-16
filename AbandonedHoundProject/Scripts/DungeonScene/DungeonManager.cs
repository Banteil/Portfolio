using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Xml;

///<summary>
///위치 이동 이벤트 대응으로 타일이 존재하는 좌표를 저장하기 위한 구조체
///</summary>
public struct TileLocation
{
    public int x;
    public int y;
    public bool isStart; //시작 위치인지 여부를 판단하기 위한 bool
};

///<summary>
///현재 좌표, 타일 상태, 몬스터 상태, 습득 아이템 저장, 몬스터 스폰, UI 상태 조정 등 탐색 씬에서 일어나는 정보 및 이벤트를 담당하는 매니저 클래스
///</summary>
public class DungeonManager : Singleton<DungeonManager>
{
    const int maxTileAry = 30; //최대 Tile 배열 수

    [SerializeField]
    List<DungeonMonster> monsterList = new List<DungeonMonster>(); //현재 구역 내 몬스터 리스트    
    AudioSource BGMAudio;
    AudioClip dungeonBGM;
    AudioClip battleBGM;

    public bool isNext; //다음 구역으로 가는것인지 이전 구역으로 돌아가는 것인지 판단하는 bool    
    public int x, y; //현재 플레이어 좌표를 확인하기 위한 int
    public int moveCount; //주사위 굴림으로 반환받는 이동력 카운트
    public int areaNum; //현재 구역의 번호를 나타내는 변수  
    public int maxAreaNum; //최대로 이동한 구역의 번호를 나타내는 변수  

    public Tile[,] currTileInfo = new Tile[maxTileAry, maxTileAry]; //현재 타일 정보(미니맵, 스테이지 클리어 전 던전 상태 저장에 사용)
    public Text moveCountText; //이동력을 표시하는 텍스트
    public Text currPositionXText, currPositionYText; //현재 좌표를 표시하는 텍스트
    public Text getGemText, getMoneyText; //소지 아이템, 돈을 표시하는 텍스트
    public Text getPotionText, getFoodText, getBombText; //소지 소모품량을 표시하는 텍스트
    public Text eventText; //이벤트 발동 시 문구를 표시하는 텍스트
    public Text stageInfoText; //스테이지 정보를 표시하는 텍스트
    public Text stageNameText, areaNumText; //전체맵 UI에서 스테이지 이름, 구역 인덱스를 표시하는 텍스트
    public Image hpBarUI, staminaBarUI, expBarUI; //HP, 스태미나 게이지를 표시하는 이미지
    public Text hpText, staminaText;
    public Image damageUI; //함정 등으로 데미지를 입었을 때 효과를 표시하기 위한 이미지
    public RawImage fadeImage; //화면 페이드 효과를 위한 이미지
    public GameObject dungeonMainUICanvas; //던전 씬에서 표시되는 UI의 캔버스 상태를 변경하기 위해 저장하는 Canvas 변수
    public GameObject mapUI; //미니맵을 확대했을 때 메뉴를 표시하기 위해 저장하는 GameObject 변수
    public GameObject resultAccountCanvas; //결과창의 active 여부를 변경하여 표시하기 위한 변수
    public GameObject gameOverCanvas;
    public Transform resultContent; //결과창의 젬 획득 리스트

    public List<TileLocation> tileLocationList = new List<TileLocation>();
    //위치 이동 이벤트 대응으로 실제 타일이 존재하는 좌표로만 이동하기 위해 생성된 좌표 저장 리스트
    public Transform mapHolder;

    int getGem = 0;
    int getMoney = 0;
    public int GetGem
    {
        get { return getGem; }
        set
        {
            getGem = value;
            if (getGem < 0) getGem = 0;
            getGemText.text = getGem.ToString();
        }
    } //던전 진행 중 획득한 젬의 개수를 나타내는 변수
    public int GetMoney
    {
        get { return getMoney; }
        set
        {
            getMoney = value;
            if (getMoney < 0) getMoney = 0;
            getMoneyText.text = getMoney.ToString();
        }
    } //던전 진행 중 획득한 돈의 액수를 나타내는 변수

    bool dungeonEnd; //던전 종료 여부 판단
    bool effectSkip; //연출 스킵을 위한 bool 변수
    bool tutorialEvent; //튜토리얼 이벤트 연출을 위한 bool
    XmlNodeList tileEventNodeList; //타일맵 이벤트의 정보를 저장

    private void Awake()
    {
        TextAsset textAsset = Resources.Load("XML/TileEventDB") as TextAsset;
        XmlDocument tileEvnetDB = new XmlDocument();
        tileEvnetDB.LoadXml(textAsset.text);
        tileEventNodeList = tileEvnetDB.SelectNodes("rows/row");
        BGMAudio = GetComponent<AudioSource>();
        fadeImage.color = new Color32(0, 0, 0, 255);
    }

    void Start()
    {
        StartCoroutine(DungeonStartSetting());
    }

    ///<summary>
    ///던전 입장 시 실행되는 이벤트
    ///</summary>
    IEnumerator DungeonDialogEvent(int num)
    {
        dungeonMainUICanvas.SetActive(false);
        DataPassing.isStory = true;
        string name = SceneManager.GetActiveScene().name;
        DataPassing.getSceneName = name;
        DataPassing.storyKind = "Stage_" + DataPassing.stageNum + "_" + num;
        SceneManager.LoadScene("StoryScene", LoadSceneMode.Additive); //인카운트 스토리 씬 로딩
        while (DataPassing.isStory) yield return null;
        dungeonMainUICanvas.SetActive(true);
    }

    ///<summary>
    ///던전 입장 시 필요한 세팅을 진행하는 함수
    ///</summary>
    IEnumerator DungeonStartSetting()
    {
        PlayerState.Instance.ResetState();

        if (DataPassing.stageNum.Equals(0))
        {
            PlayerState.Instance.CurrHp -= 15;
            PlayerState.Instance.CurrStamina -= 30;
            PlayerState.Instance.GetPotion = 0;
            PlayerState.Instance.GetFood = 0;
            PlayerState.Instance.GetBomb = 0;
            yield return StartCoroutine(DungeonDialogEvent(1));
        }   

        MoveUIUpdate();
        ItemUIUpdate();
        FullMapSetting();
        yield return StartCoroutine(EffectManager.Instance.FadeIn(fadeImage, 1f));
        fadeImage.raycastTarget = true;
        //페이드 인 후 자동으로 레이캐스트 타겟이 꺼지므로 연출 중 조작을 막기 위해 임의로 레이캐스트 타겟 true 설정

        stageInfoText.text = "- " + DataPassing.stageName + " -";
        stageNameText.text = DataPassing.stageName;
        yield return StartCoroutine(EffectManager.Instance.FadeOut(stageInfoText, 0.5f));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(EffectManager.Instance.FadeIn(stageInfoText, 0.5f));

        dungeonBGM = Resources.Load("Sound/BGM/BGM_Stage" + DataPassing.stageNum) as AudioClip;
        battleBGM = Resources.Load("Sound/BGM/BGM_Battle" + DataPassing.stageNum) as AudioClip;
        //스테이지에 맞는 BGM 로딩
        BGMAudio.clip = dungeonBGM;
        BGMAudio.Play();

        areaNum = 1;
        isNext = true;
        StartNewTurn();

        fadeImage.raycastTarget = false;
    }

    ///<summary>
    ///전체맵의 세팅을 현재 타일 정보에 맞춰 세팅, 변경하는 함수
    ///</summary>
    public void FullMapSetting()
    {
        for (int i = 0; i < mapHolder.childCount; i++)
        {
            Destroy(mapHolder.GetChild(i).gameObject);
        }

        GameObject fullMapTile = Resources.Load("FullMapTile") as GameObject;

        for (int i = 0; i < tileLocationList.Count; i++)
        {
            if (!currTileInfo[tileLocationList[i].y, tileLocationList[i].x].miniMapCheck) continue;

            GameObject tile = Instantiate(fullMapTile);
            tile.transform.SetParent(mapHolder);
            tile.transform.localPosition = new Vector3(240f + (tileLocationList[i].x * 40f) - 560f, -240f - (tileLocationList[i].y * 40f) + 920f);

            if (tileLocationList[i].y.Equals(y) && tileLocationList[i].x.Equals(x))
            {
                tile.GetComponent<Image>().color = new Color32(0, 255, 44, 255);
            }
            else if (currTileInfo[tileLocationList[i].y, tileLocationList[i].x].activeTileEvent)
            {
                tile.GetComponent<Image>().color = new Color32(255, 0, 0, 255);
            }
        }
    }

    ///<summary>
    ///플레이어가 1칸 이동하여 이동력을 소모한 것을 나타내고, 이동력이 0이 되었을 경우 타일 이벤트를 발동시키는 함수<br/>
    ///이동 도중 몬스터와 같은 칸이 될 경우 BattleScene으로 전환
    ///</summary>
    public void SetMoveInfo()
    {
        int rand = Random.Range(0, 100);
        if (rand >= PlayerState.Instance.YellowGemOptionValue(0)) PlayerState.Instance.SetStamina(-1);

        moveCount--;
        MoveUIUpdate();

        if (moveCount <= 0)
        {
            StartCoroutine(TileEvent(currTileInfo[y, x].eventType));
        }
        //moveCount가 0이하라면 행동을 마친 타일에 맞는 이벤트를 발동시킴
        //단, 이미 이벤트가 발동된 타일이라면 곧바로 주사위 굴림으로 이동
    }

    ///<summary>
    ///현재 타일의 이벤트 타입 정보를 받아 해당 이벤트 및 애니메이션을 실행하는 코루틴 함수
    ///</summary>
    public IEnumerator TileEvent(int eventNum)
    {
        string tileLocation = y + "," + x;
        Transform tileMap = GameObject.Find("TileMap").transform;
        SpriteRenderer tileMark = tileMap.Find(tileLocation).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        tileMark.color = new Color32(255, 0, 0, 255);
        //밟은 타일 마크 색상 변경

        if (DataPassing.stageNum.Equals(0))
        {
            if (!tutorialEvent)
            {
                currTileInfo[y, x].activeTileEvent = false;
                currTileInfo[y, x].eventType = 3;
                eventNum = 3;
            }
            else
            {
                StartNewTurn();
                yield break;
            }
        }

        fadeImage.raycastTarget = true; //fadeImage에 레이를 받게 처리하여 이벤트동안 동작 불가 처리
        PlayerMovement player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        player.TurnOffTextUI(); //상호작용 UI 일시 제거

        if (currTileInfo[y, x].activeTileEvent || currTileInfo[y, x].eventType.Equals(0))
        {
            string[] dialog = tileEventNodeList[0].SelectSingleNode("dialog").InnerText.Split('@');
            int rand = Random.Range(0, dialog.Length);
            yield return StartCoroutine(EventTextDisplay(dialog[rand]));
        } //이미 이벤트를 발동시킨 타일이거나 이벤트가 없는 타일일 경우 랜덤 독백 멘트를 표시하고 함수 종료          
        else
        {
            Debug.Log("이벤트 발동");
            string type = tileEventNodeList[eventNum].SelectSingleNode("type").InnerText; //현재 이벤트 넘버의 타입

            switch (type)
            {
                case "Battle":
                    yield return StartCoroutine(BattleEventProcess());
                    break;
                case "SetState":
                    Debug.Log("스탯 이벤트");
                    yield return StartCoroutine(SetStateEventProcess());
                    break;
                case "SetPosition":
                    yield return StartCoroutine(SetPositionEventProcess());
                    break;
            }
        }

        currTileInfo[y, x].activeTileEvent = true;

        if (DataPassing.stageNum.Equals(0) && !tutorialEvent)
        {
            tutorialEvent = true;
            PlayerState.Instance.GetPotion++;
            PlayerState.Instance.GetFood++;
            ItemUIUpdate();
            yield return StartCoroutine(DungeonDialogEvent(3));

        }

        fadeImage.raycastTarget = false;
        if (dungeonEnd || DataPassing.playerDie) yield break;
        player.TurnOnTextUI();
        StartNewTurn();
    }

    ///<summary>
    ///타일 이벤트의 멘트 표시 효과를 담당하는 함수
    ///</summary>
    IEnumerator EventTextDisplay(string dialog)
    {
        eventText.text = dialog;
        yield return StartCoroutine(EffectManager.Instance.FadeOut(eventText, 0.5f));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(EffectManager.Instance.FadeIn(eventText, 0.5f));

        if (PlayerState.Instance.CurrHp <= 0) StartCoroutine(GameOver());
    }

    ///<summary>
    ///전투 씬으로 전환하는 타일 이벤트
    ///</summary>
    IEnumerator BattleEventProcess()
    {
        string dialog = tileEventNodeList[currTileInfo[y, x].eventType].SelectSingleNode("dialog").InnerText;
        yield return StartCoroutine(EventTextDisplay(dialog));
        BattleStart(false);
        while (DataPassing.isBattle) yield return null;
    }

    ///<summary>
    ///플레이어 상태에 영향을 주는 이벤트
    ///</summary>
    IEnumerator SetStateEventProcess()
    {
        string dialog = tileEventNodeList[currTileInfo[y, x].eventType].SelectSingleNode("dialog").InnerText;
        string[] valueStr = tileEventNodeList[currTileInfo[y, x].eventType].SelectSingleNode("value").InnerText.Split('\x020');
        string[] calculation = valueStr[2].Split('_');
        string method = calculation[0];
        int maxValue = int.Parse(calculation[1]);
        int value = Random.Range(1, maxValue + 1);
        if (method.Equals("p"))
        {
            value %= 5;
            if (value.Equals(0)) value = 1;
            value *= 5;
        }

        switch (valueStr[0])
        {
            case "hp":
                {
                    GameObject trapResource = Resources.Load("DungeonObject/Trap_1") as GameObject;
                    GameObject trap = Instantiate(trapResource, trapResource.transform.position + new Vector3(x * 4f, 0f, y * -4f), Quaternion.identity);
                    trap.transform.SetParent(GameObject.Find(y + "," + x).transform);
                    yield return new WaitForSeconds(0.5f);

                    int rand = Random.Range(0, 100);
                    if (rand < PlayerState.Instance.BlueGemOptionValue(0))
                    {
                        dialog = "함정이다!\n하지만 위기에 대비되있던 내 몸은 자연스럽게 이를 회피했다.";
                    }
                    else
                    {
                        if (valueStr[1].Equals("-"))
                        {
                            value *= -1;
                            yield return StartCoroutine(EffectManager.Instance.FadeOut(damageUI, 0.3f));
                            yield return StartCoroutine(EffectManager.Instance.FadeIn(damageUI, 0.3f));
                        }
                        PlayerState.Instance.SetHp(method, value);
                    }
                    MoveUIUpdate();
                }
                break;
            case "st":
                {
                    if (valueStr[1].Equals("-")) value *= -1;
                    PlayerState.Instance.SetStamina(method, value);
                    staminaBarUI.fillAmount = (float)PlayerState.Instance.CurrStamina / PlayerState.Instance.MaxStamina;
                    break;
                }
            case "mo":
                {
                    int rand = Random.Range(0, 100);
                    if (GetMoney.Equals(0) || rand < PlayerState.Instance.BlueGemOptionValue(1))
                    {
                        dialog = "골드 주머니를 스치는 느낌에 몸을 피했다.\n...알 수 없는 그 기운은 재빠르게 멀어져갔다.";
                    }
                    else
                    {
                        if (valueStr[1].Equals("-")) value *= -1;
                        switch (method)
                        {
                            case "c":
                                GetMoney += value;
                                break;
                            case "p":
                                GetMoney += (GetMoney / 100 * value);
                                break;
                        }
                    }
                }
                break;
            case "it":
                {
                    int rand = Random.Range(0, 100);
                    if (GetGem.Equals(0) || rand < PlayerState.Instance.BlueGemOptionValue(1))
                    {
                        dialog = "품 속을 스치는 느낌에 몸을 피했다.\n...알 수 없는 그 기운은 재빠르게 멀어져갔다.";
                    }
                    else
                    {
                        if (valueStr[1].Equals("-")) value *= -1;
                        switch (method)
                        {
                            case "c":
                                GetGem += value;
                                break;
                            case "p":
                                GetGem += (GetGem / 100 * value);
                                break;
                        }
                       
                    }
                    break;
                }
        }

        yield return StartCoroutine(EventTextDisplay(dialog));
    }

    ///<summary>
    ///플레이어 좌표 이동을 진행하는 이벤트
    ///</summary>
    IEnumerator SetPositionEventProcess()
    {
        string dialog = tileEventNodeList[currTileInfo[y, x].eventType].SelectSingleNode("dialog").InnerText;
        yield return StartCoroutine(EventTextDisplay(dialog));

        string value = tileEventNodeList[currTileInfo[y, x].eventType].SelectSingleNode("value").InnerText;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        switch (value)
        {
            case "start":
                yield return StartCoroutine(EffectManager.Instance.FadeOut(fadeImage, 1f));
                for (int i = 0; i < tileLocationList.Count; i++)
                {
                    if (tileLocationList[i].isStart)
                    {
                        y = tileLocationList[i].y;
                        x = tileLocationList[i].x;
                        player.GetComponent<PlayerMovement>().x = x;
                        player.GetComponent<PlayerMovement>().y = y;
                        player.transform.position = new Vector3(x * 4f, 1.5f, y * -4f);
                    }
                }
                yield return StartCoroutine(EffectManager.Instance.FadeIn(fadeImage, 1f));
                break;
            case "random":
                yield return StartCoroutine(EffectManager.Instance.FadeOut(fadeImage, 1f));
                int rand = Random.Range(0, tileLocationList.Count);
                y = tileLocationList[rand].y;
                x = tileLocationList[rand].x;
                player.GetComponent<PlayerMovement>().x = x;
                player.GetComponent<PlayerMovement>().y = y;
                player.transform.position = new Vector3(x * 4f, 1.5f, y * -4f);
                yield return StartCoroutine(EffectManager.Instance.FadeIn(fadeImage, 1f));
                break;
            case "town":
                StartCoroutine(AccountResult());
                dungeonEnd = true;
                break;
        }
    }

    ///<summary>
    ///타일 이벤트 종료 후 새로운 플레이어 턴을 실행하기 위해 준비하는 함수(주사위 굴림)
    ///</summary>
    void StartNewTurn()
    {
        dungeonMainUICanvas.SetActive(false);
        moveCount = DiceManager.Instance.DiceRoll(2) + PlayerState.Instance.GreenGemOptionValue(0); //이동력을 얻기 위함이므로 주사위를 2개 굴림
        MoveUIUpdate();
    }

    ///<summary>
    ///움직임과 관련된 정보 UI를 최신 정보로 업데이트하는 함수
    ///</summary>
    public void MoveUIUpdate()
    {
        moveCountText.text = moveCount.ToString();
        currPositionXText.text = x.ToString();
        currPositionYText.text = y.ToString();
        hpText.text = PlayerState.Instance.CurrHp + " / " + PlayerState.Instance.MaxHp;
        staminaText.text = PlayerState.Instance.CurrStamina + " / " + PlayerState.Instance.MaxStamina;
        hpBarUI.fillAmount = (float)PlayerState.Instance.CurrHp / PlayerState.Instance.MaxHp;
        staminaBarUI.fillAmount = (float)PlayerState.Instance.CurrStamina / PlayerState.Instance.MaxStamina;
    }

    ///<summary>
    ///아이템 사용과 관련된 정보 UI를 최신 정보로 업데이트하는 함수
    ///</summary>
    public void ItemUIUpdate()
    {
        getPotionText.text = PlayerState.Instance.GetPotion.ToString();
        getFoodText.text = PlayerState.Instance.GetFood.ToString();
        getBombText.text = PlayerState.Instance.GetBomb.ToString();
    }

    ///<summary>
    ///마주치거나 이벤트로 활성화된 몬스터와 전투를 실행하기 위해 전투 씬을 호출하는 함수
    ///bool형 매개변수를 통해 현재 전투가 인카운트로 인한 것인지, 타일 이벤트로 인한 것인지 확인
    ///</summary>
    public void BattleStart(bool isEncounter)
    {
        StartCoroutine(EffectManager.Instance.BattleStartEffect(Camera.main));
        BGMAudio.clip = battleBGM;
        BGMAudio.Play();
        DataPassing.isEncounter = isEncounter; //인카운트로 시작된 전투라면 DataPassing 내의 isEncounter에 정보를 전달
        DataPassing.isBattle = true; //배틀 시작을 알리는 bool을 true로 변경
        dungeonMainUICanvas.SetActive(false);
        SceneManager.LoadScene("BattleScene", LoadSceneMode.Additive); //전투 씬 로딩    
    }

    ///<summary>
    ///전투 종료 처리를 진행하는 함수
    ///</summary>
    public void BattleEnd()
    {
        if (DataPassing.stageNum.Equals(0) && DataPassing.tutorialEnd.Equals(true))
        {
            DataPassing.isBattle = false;
            return;
        }

        if (!DataPassing.playerDie)
        {
            StartCoroutine(EffectManager.Instance.BattleEndEffect(Camera.main));
            DataPassing.isBattle = false;

            if (DataPassing.isEncounter)
            {
                dungeonMainUICanvas.SetActive(true);
                DataPassing.isEncounter = false;
            }

            ItemUIUpdate();
            BGMAudio.clip = dungeonBGM;
            BGMAudio.Play();
        }
        else
        {
            StartCoroutine(GameOver());
        }
    }

    IEnumerator GameOver()
    {
        DataPassing.playerDie = true;
        BGMAudio.Stop();
        dungeonMainUICanvas.SetActive(false);
        gameOverCanvas.SetActive(true);

        RawImage fade = gameOverCanvas.transform.GetChild(0).GetComponent<RawImage>();
        yield return StartCoroutine(EffectManager.Instance.FadeOut(fade, 1f));
        yield return new WaitForSeconds(0.5f);

        Text text = gameOverCanvas.transform.GetChild(1).GetComponent<Text>();
        yield return StartCoroutine(EffectManager.Instance.FadeOut(text, 1f));
        yield return new WaitForSeconds(2f);

        gameOverCanvas.transform.GetChild(2).gameObject.SetActive(true);
        gameOverCanvas.transform.GetChild(3).gameObject.SetActive(true);
    }

    public void ContinueGame()
    {
        StartCoroutine(EndGameEffect(true));
    }

    public void EndGame()
    {
        StartCoroutine(EndGameEffect(false));
    }

    IEnumerator EndGameEffect(bool isContinue)
    {
        DataPassing.Reset();
        PlayerState.Instance.ResetState();
        gameOverCanvas.transform.GetChild(2).gameObject.SetActive(false);
        gameOverCanvas.transform.GetChild(3).gameObject.SetActive(false);
        Text text = gameOverCanvas.transform.GetChild(1).GetComponent<Text>();
        yield return StartCoroutine(EffectManager.Instance.FadeIn(text, 0.5f));
        TileMapManager.Instance.DeleteMapData(false);
        if (isContinue) SceneManager.LoadScene("TownScene");
        else SceneManager.LoadScene("MainScene");
    }

    ///<summary>
    ///전체 맵 UI를 활성화할 때 호출되는 함수
    ///</summary>
    public void MapUITurnOn()
    {
        FullMapSetting();
        mapUI.SetActive(true);
    }

    ///<summary>
    ///전체 맵 UI를 비활성화 할 때 호출되는 함수
    ///</summary>
    public void MapUITurnOff()
    {
        mapUI.SetActive(false);
    }

    ///<summary>
    ///현재 타일들의 정보(벽 파괴 등)를 확인하고자 정보를 전달하는 함수
    ///</summary>
    public string CurrentTileInfo(int i, int j)
    {
        string tile;
        if (currTileInfo[i, j].tile == TileType.SWITCH)
        {
            if (GameObject.Find(i + "," + j).transform.GetComponentInChildren<SwitchAct>().active)
                tile = "6 ";
            else tile = TileMapManager.Instance.GetSDConnectInfo(i, j, 0) + " ";
        }
        else if (currTileInfo[i, j].tile == TileType.TREASURE)
        {
            Transform chest = GameObject.Find(i + "," + j).transform.Find("Chest_" + DataPassing.stageNum + "(Clone)");
            if (chest != null)
            {
                if (chest.GetComponent<ChestAct>().open)
                    tile = "4_T ";
                else tile = "4 ";
            }
            else tile = "4_T ";
        }
        else
        {
            int temp = (int)currTileInfo[i, j].tile;
            tile = temp.ToString() + " ";
        }

        string[] wall = new string[4];

        for (int w = 0; w < 4; w++)
        {
            if (currTileInfo[i, j].wall[w] == WallType.DOOR)
            {
                if (!GameObject.Find(i + "," + j).transform.GetComponentInChildren<DoorAct>().doorLock)
                    wall[w] = "2 ";
                else wall[w] = TileMapManager.Instance.GetSDConnectInfo(i, j, w + 1) + " ";
            }
            else
            {
                int temp = (int)currTileInfo[i, j].wall[w];
                wall[w] = temp.ToString() + " ";
            }
        }

        string minimapInfo = currTileInfo[i, j].miniMapCheck ? "T" : "F";
        string tileInfo = tile + wall[0] + wall[1] + wall[2] + wall[3] + minimapInfo;

        return tileInfo;
    }

    ///<summary>
    ///정보가 전달된 몬스터를 리스트에 추가하는 함수
    ///</summary>
    public void AddMonsterToList(DungeonMonster script)
    {
        monsterList.Add(script);
    }

    ///<summary>
    ///정보가 전달된 몬스터를 리스트에서 제거하는 함수
    ///</summary>
    public void DeleteEnemyToList(DungeonMonster script)
    {
        for (int i = 0; i < monsterList.Count; i++)
        {
            if (monsterList[i].Equals(script))
            {
                monsterList.RemoveAt(i);
                break;
            }
        }
    }

    ///<summary>
    ///리스트에 존재하는 몬스터들의 객체 및 리스트를 모두 삭제하는 함수
    ///</summary>
    void DeleteAllEnemyToList()
    {
        foreach (DungeonMonster dm in monsterList)
        {
            Destroy(dm.gameObject);
        }
        monsterList.Clear();
        DataPassing.tempDM = null;
    }

    ///<summary>
    ///리스트에 존재하는 몬스터들을 움직이게 하는 함수
    ///</summary>
    public void MonstersMove()
    {
        if (monsterList.Count == 0) return;

        foreach (DungeonMonster dm in monsterList)
        {
            StartCoroutine(dm.MonsterMove());
        }
    }

    ///<summary>
    ///몬스터들 중 움직이고 있는 객체가 있는지 여부를 확인하는 함수<br/>
    ///움직이고 있는 몬스터가 있을 경우 플레이어가 조작을 받지 못하게 하기 위함<br/>
    ///DataPassing 클래스 내 tempDM에 정보가 있다면 현재 플레이어와 인카운트하여 전투를 실행했다는<br/>
    ///의미이므로 몬스터 리스트를 모두 확인한 시점 이후 타이밍에 인카운트한 몬스터를 리스트에서 제외함
    ///</summary>
    public bool IsMonsterMove()
    {
        if (monsterList.Count == 0) return false;

        bool isMove = false;
        foreach (DungeonMonster dm in monsterList)
        {
            if (dm.isMove)
            {
                isMove = true;
                break;
            }
        }

        if (DataPassing.tempDM != null)
        {
            DeleteEnemyToList(DataPassing.tempDM);
            Destroy(DataPassing.tempDM.gameObject);
            DataPassing.tempDM = null;
        }

        return isMove;
    }

    ///<summary>
    ///구역을 넘어가는 행동을 진행하는 코루틴 함수
    ///</summary>
    public IEnumerator AreaMove(bool isNext)
    {
        TileMapManager.Instance.SaveTempMapData(DataPassing.stageNum, areaNum); //이동 시 임시 맵 정보 저장
        areaNum += isNext ? 1 : -1; //다음 구역으로 간다면 areaNum을 1 증가, 반대라면 1 감소
        this.isNext = isNext; //던전 매니저의 isNext bool 변수에 매개변수 bool 대입
        if (isNext) maxAreaNum = areaNum;

        if (!TileMapManager.Instance.CheckNextAreaData(DataPassing.stageNum, areaNum))
        {
            DataPassing.isClear = true;
            StartCoroutine(AccountResult());
            yield break;
        }

        yield return StartCoroutine(EffectManager.Instance.FadeOut(fadeImage, 2f));
        DeleteAllEnemyToList(); //현재 구역의 모든 몬스터 삭제
        tileLocationList.Clear(); //현재 구역 좌표 리스트 초기화
        TileMapManager.Instance.DeleteCurrntMap(); //현재 맵 삭제
        TileMapManager.Instance.LoadXMLData(DataPassing.stageNum, areaNum); //새로운 구역 맵 정보 로딩
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().CheckMiniMapTile();
        //맵 이동 후 미니맵 시야 정보 체크
        areaNumText.text = areaNum + "구역";
        MoveUIUpdate(); //UI정보 초기화
        yield return StartCoroutine(EffectManager.Instance.FadeIn(fadeImage, 2f));
    }

    ///<summary>
    ///던전 결과창 표시를 확인했을 때 실행되는 함수
    ///</summary>
    public void DungeonEscape()
    {
        StartCoroutine(StageEscape());
    }

    ///<summary>
    ///최종적인 던전 탈출, 클리어 판정을 진행하는 코루틴 함수
    ///</summary>
    public IEnumerator StageEscape()
    {
        if (!DataPassing.isClear) TileMapManager.Instance.SaveMapData(DataPassing.stageNum, maxAreaNum);
        //던전 탈출 시점일 때엔 현재 구역 데이터 저장
        else
        {
            DataPassing.stageClear[DataPassing.stageNum] = true; //stageNum번째 던전에 영구 클리어 판정 부여
            TileMapManager.Instance.DeleteMapData(true);
        }
        //던전 클리어 시점일 때엔 해당 던전의 모든 구역 데이터 초기화

        RawImage raFadeImage = resultAccountCanvas.transform.GetChild(1).GetComponent<RawImage>();
        DataPassing.Reset();
        PlayerState.Instance.ResetState();
        yield return StartCoroutine(EffectManager.Instance.FadeOut(raFadeImage, 2f));
        SceneManager.LoadScene("TownScene");
    }

    ///<summary>
    ///던전 탈출, 클리어 시 결과를 보여주는 코루틴 함수
    ///</summary>
    public IEnumerator AccountResult()
    {
        dungeonMainUICanvas.SetActive(false);
        resultAccountCanvas.SetActive(true);
        GameObject ResultImage = resultAccountCanvas.transform.GetChild(0).gameObject;

        if (DataPassing.isClear) ResultImage.transform.GetChild(1).GetComponent<Text>().text = "- 던전 클리어 -";
        else ResultImage.transform.GetChild(1).GetComponent<Text>().text = "- 던전 탈출 -";

        int moneyValue = 0;
        Text moneyText = ResultImage.transform.GetChild(2).GetComponentInChildren<Text>();

        RawImage fadeImage = GameObject.Find("ResultAccountCanvas").transform.GetChild(1).GetComponent<RawImage>();
        fadeImage.raycastTarget = true;
        EventTrigger.Entry interactionUI_Entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        interactionUI_Entry.callback.AddListener((data) => { CancleTouch(); });
        fadeImage.gameObject.GetComponent<EventTrigger>().triggers.Add(interactionUI_Entry);

        while (moneyValue < getMoney)
        {
            if (effectSkip)
            {
                moneyValue = getMoney;
                moneyText.text = moneyValue + " 골드 획득";
                fadeImage.gameObject.GetComponent<EventTrigger>().triggers.Clear();
                fadeImage.raycastTarget = false;
                break;
            }

            moneyValue += 10;
            if (moneyValue > getMoney) moneyValue = getMoney;

            moneyText.text = moneyValue + " 골드 획득";
            yield return null;
        }

        if (!effectSkip)
        {
            fadeImage.gameObject.GetComponent<EventTrigger>().triggers.Clear();
            fadeImage.raycastTarget = false;
        }

        PlayerState.Instance.Money += getMoney;

        for (int i = 0; i < getGem; i++)
        {
            Gem gem = new Gem();
            gem.GemKind = (GemKind)Random.Range(0, 4);
            gem.statValue = Random.Range(1, (DataPassing.stageNum * 3) + 1);

            PlayerState.Instance.getGemList.Add(gem);
            GameObject gemList = Instantiate(Resources.Load("GemList") as GameObject);
            gemList.GetComponent<GemListCheck>().gemOptionList = new List<GemOption>(gem.gemOptionList);

            Sprite[] icon = Resources.LoadAll<Sprite>("Icons/IconSet");
            switch (gem.GemKind)
            {
                case GemKind.RED:
                    gemList.transform.GetChild(2).GetComponent<Text>().text = "Str + " + gem.statValue;
                    break;
                case GemKind.BLUE:
                    gemList.transform.GetChild(2).GetComponent<Text>().text = "Int + " + gem.statValue;
                    break;
                case GemKind.GREEN:
                    gemList.transform.GetChild(2).GetComponent<Text>().text = "Dex + " + gem.statValue;
                    break;
                case GemKind.YELLOW:
                    gemList.transform.GetChild(2).GetComponent<Text>().text = "Vit + " + gem.statValue;
                    break;
            }

            gemList.transform.GetChild(0).GetComponent<Image>().sprite = gem.gemSprite;
            gemList.transform.GetChild(1).GetComponent<Text>().text = gem.gemKindName;

            gemList.transform.SetParent(resultContent);

            yield return new WaitForSeconds(0.2f);
        }
    }

    void CancleTouch()
    {
        effectSkip = true;
    }
}
