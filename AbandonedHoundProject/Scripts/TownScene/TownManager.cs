using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

///<summary>
//마을 씬에서 할 수 있는 행동의 처리를 관리하는 클래스
///</summary>
public class TownManager : Singleton<TownManager>
{
    const int minlocation = 0; //위치 최소값
    const int maxlocation = 4; //위치 최대값

    public GameObject maintainPage, shopPage, dungeonPage, combinationPage;
    //각 기능(상점, 정비 등)들의 메인 UI
    public GameObject quitPanel;
    public GameObject[] stagePoint = new GameObject[5]; //스테이지 선택 시 위치를 나타내는 포인트
    public GameObject admissionCheck; //던전에 입장할 것인지 여부를 체크하기 위한 UI
    public GameObject uiPanel; //메인 로비의 위치 선택, 버튼 ui를 묶고 있는 UI 
    public RawImage fadeImage; //메인 UI의 페이드 효과를 담당하는 UI
    public RectTransform content; //배경 화면의 위치 계산을 위한 객체
    public Image background; //배경 화면의 이미지를 변경하기 위한 변수    
    public Text saveText;
    public bool openUI = false; //특정 UI의 켜짐 여부를 판단하여 장소 이동 등의 행동을 방지하는 bool
    public int location; //현재 장소가 어딘지 판단하는 변수

    float pos, movePos; //스크롤 위치 거리에 연관된 변수
    bool isScroll = false; //스크롤 중에 또 다시 스크롤을 하지 못하도록 방지하는 bool
    bool locationInfoTouch = false; //위치 정보 UI의 켜짐 여부를 판단하는 bool
    int clickCount = 0; //스테이지 선택 시 클릭 횟수를 계산하기 위한 변수
    int stageclick; //어떤 스테이지를 선택했는지 여부를 판단하기 위한 변수

    void Awake()
    {
        Screen.SetResolution(2560, 1440, true); //스크린 해상도를 2560, 1440으로 맞춤
        Application.targetFrameRate = 30; //30프레임 고정
        fadeImage.color = new Color32(0, 0, 0, 255);
    }

    void Start()
    {
        location = 2; //현재 위치는 2(마을 중앙)으로 설정
        LocationInfoUpdate(location); //현재 위치에 따라 정보 업데이트 진행
        pos = content.localPosition.x;
        movePos = content.rect.xMax - content.rect.xMax / 3;
        //pos, movePos 초기화
        StartCoroutine(EffectManager.Instance.FadeIn(fadeImage, 1f));
        //정보 처리가 끝난 후 페이드 인 효과로 정비 씬 화면을 나타냄
    }

    void Update()
    {
        if (openUI && Input.GetKeyDown(KeyCode.Escape))
        {
            GameObject actButton = uiPanel.transform.GetChild(2).gameObject;
            Text actButtonText = actButton.GetComponentInChildren<Text>();
            openUI = false;
            switch(location)
            {
                case 0:
                    combinationPage.SetActive(false);
                    actButtonText.text = "조합";
                    break;
                case 1:
                    maintainPage.SetActive(false);
                    actButtonText.text = "정비";
                    break;
                case 2:
                    combinationPage.SetActive(false);
                    actButtonText.text = "세이브";
                    break;
                case 3:
                    shopPage.SetActive(false);
                    actButtonText.text = "구매";
                    break;
                case 4:
                    dungeonPage.SetActive(false);
                    actButtonText.text = "던전 선택";
                    break;
            }            
        }
        else if (!openUI && Input.GetKeyDown(KeyCode.Escape) && location != 2)
        {
            ButtonMove(2);
        }
        else if (!openUI && Input.GetKeyDown(KeyCode.Escape) && location == 2)
        {
            EndButton();
        }
    }

    public void EndButton()
    {
        quitPanel.SetActive(true);
    }

    public void CancleButton()
    {
        quitPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    ///<summary>
    ///현재 마을 위치에서 우측으로 이동하는 함수
    ///</summary>
    public void MoveRight()
    {
        if (location >= maxlocation) return;

        if (content.rect.xMin + content.rect.xMax / 3 != movePos)
        {
            movePos = pos - content.rect.width / 3;
            pos = movePos;
            location++;
            if (!isScroll) StartCoroutine(Scroll());
        }
    }

    ///<summary>
    ///현재 마을 위치에서 좌측으로 이동하는 함수
    ///</summary>
    public void MoveLeft()
    {
        if (location <= minlocation) return;

        if (content.rect.xMin - content.rect.xMax / 3 != movePos)
        {
            movePos = pos + content.rect.width / 3;
            pos = movePos;
            location--;
            if (!isScroll) StartCoroutine(Scroll());
        }
    }

    ///<summary>
    ///버튼 입력으로 마을을 이동하는 함수
    ///</summary>
    public void ButtonMove(int getLocation)
    {
        int prevLoca = location;
        location = getLocation;

        if (prevLoca > location)
        {
            if (content.rect.xMin - content.rect.xMax / 3 != movePos)
            {
                movePos = pos + content.rect.width / 3;
                pos = movePos;
                if (!isScroll) StartCoroutine(Scroll());
            }
        }
        else
        {
            if (content.rect.xMin + content.rect.xMax / 3 != movePos)
            {
                movePos = pos - content.rect.width / 3;
                pos = movePos;
                if (!isScroll) StartCoroutine(Scroll());
            }
        }
    }

    ///<summary>
    ///마을 이동 시 스크롤 효과 적용 및 text, button의 내용을 업데이트하는 함수
    ///</summary>
    IEnumerator Scroll()
    {
        isScroll = true; //스크롤을 시작하였으므로 스크롤 여부를 판단하는 변수에 true

        StartCoroutine(EffectManager.Instance.FadeOut(fadeImage, 0.5f)); //페이드 아웃 효과 실행
        while (true)
        {
            background.transform.localScale += new Vector3(5f * Time.deltaTime, 5f * Time.deltaTime, 0);
            //배경화면의 크기를 증가시킴
            content.localPosition = Vector2.Lerp(content.localPosition, new Vector2(movePos, 0), Time.deltaTime * 2f);
            //이동하려는 위치로 배경화면을 스크롤
            if (Vector2.Distance(content.localPosition, new Vector2(movePos, 0)) < 100f) break;
            //스크롤 위치와 목표 위치의 거리가 100 이하면 while 탈출
            yield return null;
        }

        background.transform.localScale = new Vector3(1f, 1f, 1f);
        content.anchoredPosition = new Vector2(0, 0);
        pos = content.localPosition.x;
        movePos = content.rect.xMax - content.rect.xMax / 3;

        string backgroundPath = null;
        switch (location)
        {
            case 0:
                backgroundPath = "Sprite/TownImage/GemSmithy";
                break;
            case 1:
                backgroundPath = "Sprite/TownImage/Inn";
                break;
            case 2:
                backgroundPath = "Sprite/TownImage/Town";
                break;
            case 3:
                backgroundPath = "Sprite/TownImage/Shop";
                break;
            case 4:
                backgroundPath = "Sprite/TownImage/CastleFarView";
                break;
        }

        LocationInfoUpdate(location);
        background.sprite = Resources.Load<Sprite>(backgroundPath);
        GameObject movePanel = uiPanel.transform.GetChild(1).gameObject;
        movePanel.SetActive(false);
        locationInfoTouch = false;

        yield return StartCoroutine(EffectManager.Instance.FadeIn(fadeImage, 1f));
        isScroll = false;
    }

    public void SaveButton()
    {
        StartCoroutine(SaveProcessing());        
    }

    IEnumerator SaveProcessing()
    {
        yield return StartCoroutine(EffectManager.Instance.FadeOut(fadeImage, 1f));
        string saveToData = PlayerState.Instance.SaveStateData();
        GooglePlayServiceManager.Instance.SaveToCloud(saveToData);

        while (GooglePlayServiceManager.Instance.isProcessing) yield return null;

        StartCoroutine(EffectManager.Instance.FadeIn(fadeImage, 0.1f));
        StartCoroutine(EffectManager.Instance.Fade(saveText));
    }

    ///<summary>
    ///정비UI를 표시하거나 표시 해제를 하기 위한 함수
    ///</summary>
    public void SetupMaintainPage()
    {
        GameObject movePanel = uiPanel.transform.GetChild(1).gameObject;
        GameObject actButton = uiPanel.transform.GetChild(2).gameObject;
        Text actButtonText = actButton.GetComponentInChildren<Text>();

        movePanel.SetActive(false);
        locationInfoTouch = false;

        if (!openUI)
        {
            Maintain.Instance.ResetMaintainInfo();
            openUI = true;
            maintainPage.SetActive(true);
            actButtonText.text = "닫기";
        }
        else
        {
            openUI = false;
            maintainPage.SetActive(false);
            actButtonText.text = "정비";
        }
    }

    ///<summary>
    ///상점UI를 표시하거나 표시 해제를 하기 위한 함수
    ///</summary>
    public void SetupShopPage()
    {
        GameObject movePanel = uiPanel.transform.GetChild(1).gameObject;
        GameObject actButton = uiPanel.transform.GetChild(2).gameObject;
        Text actButtonText = actButton.GetComponentInChildren<Text>();

        movePanel.SetActive(false);
        locationInfoTouch = false;

        if (!openUI)
        {
            Shop.Instance.SettingTextValue();
            openUI = true;
            shopPage.SetActive(true);
            actButtonText.text = "닫기";
        }
        else
        {
            Shop.Instance.ResetInfo();
            openUI = false;
            shopPage.SetActive(false);
            actButtonText.text = "구매";
        }
    }

    ///<summary>
    ///조합UI를 표시하거나 표시 해제를 하기 위한 함수
    ///</summary>
    public void SetupCombinationPage()
    {
        GameObject movePanel = uiPanel.transform.GetChild(1).gameObject;
        GameObject actButton = uiPanel.transform.GetChild(2).gameObject;
        Text actButtonText = actButton.GetComponentInChildren<Text>();

        movePanel.SetActive(false);
        locationInfoTouch = false;

        if (!openUI)
        {
            Combination.Instance.ResetAllInfo();
            openUI = true;
            combinationPage.SetActive(true);
            actButtonText.text = "닫기";
        }
        else
        {
            openUI = false;
            combinationPage.SetActive(false);
            actButtonText.text = "조합";
        }
    }

    public void SetupDungeonPage() //던전 페이지 열기
    {
        dungeonPage.SetActive(true);
    }

    public void CloseDungeonPage()//던전 페이지 닫기
    {
        dungeonPage.SetActive(false);
        clickCount = 0;
    }

    public void TouchStage(int stage) //스테이지 버튼 클릭
    {
        if (clickCount == 0)
        {
            stageclick = stage;
            clickCount++;
            StartCoroutine(EnterStage(stage));
        }

        else if (clickCount == 1 && stageclick == stage)
            clickCount++;

        else if (clickCount == 1 && stageclick != stage)
        {
            for (int i = 0; i < 5; i++)
            {
                stagePoint[i].SetActive(false);
            }
            stagePoint[stage - 1].SetActive(true);
            stageclick = stage;
        }
    }
    IEnumerator EnterStage(int stage) //스테이지 버튼 클릭
    {
        if (clickCount >= 1)
            stagePoint[stage - 1].SetActive(true);

        while (true)
        {
            if (clickCount == 2)
            {
                admissionCheck.SetActive(true);
                DataPassing.stageNum = 1; //1스테이지 밖에 없기에 임시로 처리
                //DataPassing.stageNum = stage;
                break;
            }

            else if (clickCount == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    stagePoint[i].SetActive(false);
                }
                break;
            }

            yield return null;
        }

    }

    public void SelectNo() //던전 선택창에서 '아니오'를 눌렀을 때 실행되는 함수
    {
        admissionCheck.SetActive(false);

        for (int i = 0; i < 5; i++)
        {
            stagePoint[i].SetActive(false);
        }
        clickCount = 0;
    }

    public void SelectYes() //던전 선택창에서 '예'를 눌렀을 때 실행되는 함수
    {
        Debug.Log("던전 입장");
        StartCoroutine(StageEnter());
    }

    IEnumerator StageEnter()
    {
        fadeImage.raycastTarget = true;
        admissionCheck.SetActive(false);
        dungeonPage.SetActive(false);
        clickCount = 0;

        switch (DataPassing.stageNum)
        {
            case 1:
                DataPassing.stageName = "왕성 하수도";
                break;
            case 2:
                DataPassing.stageName = "로비";
                break;
            case 3:
                DataPassing.stageName = "하늘길";
                break;
            case 4:
                DataPassing.stageName = "그랜드 홀";
                break;
            case 5:
                DataPassing.stageName = "그레이트 체임버";
                break;
            default:
                DataPassing.stageName = "???";
                break;
        }
        yield return StartCoroutine(EffectManager.Instance.FadeOut(fadeImage, 1f));
        SceneManager.LoadScene("DungeonScene");
    }

    ///<summary>
    ///현재 마을 내 위치에 따라 위치 정보 및 버튼 이벤트 등을 업데이트하는 함수
    ///</summary>
    public void LocationInfoUpdate(int location)
    {
        GameObject locationInfo = uiPanel.transform.GetChild(0).gameObject;
        GameObject actButton = uiPanel.transform.GetChild(2).gameObject;
        GameObject movePanel = uiPanel.transform.GetChild(1).gameObject;
        GameObject[] movePanelButtons = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            movePanelButtons[i] = movePanel.transform.GetChild(i).gameObject;
        }

        Text locationInfoText = locationInfo.GetComponentInChildren<Text>();
        Text actButtonText = actButton.GetComponentInChildren<Text>();
        Text[] movePanelButtonTexts = new Text[4];
        for (int i = 0; i < 4; i++)
        {
            movePanelButtonTexts[i] = movePanelButtons[i].GetComponentInChildren<Text>();
        }

        EventTrigger[] movePanelButtonTriggers = new EventTrigger[4];
        for (int i = 0; i < 4; i++)
        {
            movePanelButtonTriggers[i] = movePanelButtons[i].GetComponent<EventTrigger>();
            movePanelButtonTriggers[i].triggers.Clear();
        }
        EventTrigger actButtonTrigger = actButton.GetComponent<EventTrigger>();
        actButtonTrigger.triggers.Clear();

        EventTrigger.Entry actButton_Entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };

        switch (location)
        {
            case 0:
                locationInfoText.text = "대장간";
                actButtonText.text = "젬 조합";
                movePanelButtonTexts[0].text = "정비";
                movePanelButtonTexts[1].text = "중앙";
                movePanelButtonTexts[2].text = "상점";
                movePanelButtonTexts[3].text = "던전";

                actButton_Entry.callback.AddListener((data) => { SetupCombinationPage(); });
                actButtonTrigger.triggers.Add(actButton_Entry);
                for (int i = 0; i < 4; i++)
                {
                    EventTrigger.Entry movePanelButton_Entry = new EventTrigger.Entry
                    {
                        eventID = EventTriggerType.PointerUp
                    };

                    switch (i)
                    {
                        case 0:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(1); });
                            break;
                        case 1:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(2); });
                            break;
                        case 2:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(3); });
                            break;
                        case 3:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(4); });
                            break;
                    }
                    movePanelButtonTriggers[i].triggers.Add(movePanelButton_Entry);
                }
                break;
            case 1:
                locationInfoText.text = "여관";
                actButtonText.text = "정비";
                movePanelButtonTexts[0].text = "조합";
                movePanelButtonTexts[1].text = "중앙";
                movePanelButtonTexts[2].text = "상점";
                movePanelButtonTexts[3].text = "던전";

                actButton_Entry.callback.AddListener((data) => { SetupMaintainPage(); });
                actButtonTrigger.triggers.Add(actButton_Entry);
                for (int i = 0; i < 4; i++)
                {
                    EventTrigger.Entry movePanelButton_Entry = new EventTrigger.Entry
                    {
                        eventID = EventTriggerType.PointerUp
                    };

                    switch (i)
                    {
                        case 0:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(0); });
                            break;
                        case 1:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(2); });
                            break;
                        case 2:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(3); });
                            break;
                        case 3:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(4); });
                            break;
                    }
                    movePanelButtonTriggers[i].triggers.Add(movePanelButton_Entry);
                }
                break;
            case 2:
                locationInfoText.text = "마을 중앙";
                actButtonText.text = "세이브";
                movePanelButtonTexts[0].text = "조합";
                movePanelButtonTexts[1].text = "정비";
                movePanelButtonTexts[2].text = "상점";
                movePanelButtonTexts[3].text = "던전";

                actButton_Entry.callback.AddListener((data) => { SaveButton(); });
                actButtonTrigger.triggers.Add(actButton_Entry);
                for (int i = 0; i < 4; i++)
                {
                    EventTrigger.Entry movePanelButton_Entry = new EventTrigger.Entry
                    {
                        eventID = EventTriggerType.PointerUp
                    };

                    switch (i)
                    {
                        case 0:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(0); });
                            break;
                        case 1:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(1); });
                            break;
                        case 2:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(3); });
                            break;
                        case 3:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(4); });
                            break;
                    }
                    movePanelButtonTriggers[i].triggers.Add(movePanelButton_Entry);
                }
                break;
            case 3:
                locationInfoText.text = "잡화 상점";
                actButtonText.text = "구매";
                movePanelButtonTexts[0].text = "조합";
                movePanelButtonTexts[1].text = "정비";
                movePanelButtonTexts[2].text = "중앙";
                movePanelButtonTexts[3].text = "던전";

                actButton_Entry.callback.AddListener((data) => { SetupShopPage(); });
                actButtonTrigger.triggers.Add(actButton_Entry);
                for (int i = 0; i < 4; i++)
                {
                    EventTrigger.Entry movePanelButton_Entry = new EventTrigger.Entry
                    {
                        eventID = EventTriggerType.PointerUp
                    };

                    switch (i)
                    {
                        case 0:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(0); });
                            break;
                        case 1:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(1); });
                            break;
                        case 2:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(2); });
                            break;
                        case 3:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(4); });
                            break;
                    }
                    movePanelButtonTriggers[i].triggers.Add(movePanelButton_Entry);
                }
                break;
            case 4:
                locationInfoText.text = "마성 입구";
                actButtonText.text = "던전 선택";
                movePanelButtonTexts[0].text = "조합";
                movePanelButtonTexts[1].text = "정비";
                movePanelButtonTexts[2].text = "중앙";
                movePanelButtonTexts[3].text = "상점";

                actButton_Entry.callback.AddListener((data) => { SetupDungeonPage(); });
                actButtonTrigger.triggers.Add(actButton_Entry);
                for (int i = 0; i < 4; i++)
                {
                    EventTrigger.Entry movePanelButton_Entry = new EventTrigger.Entry
                    {
                        eventID = EventTriggerType.PointerUp
                    };

                    switch (i)
                    {
                        case 0:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(0); });
                            break;
                        case 1:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(1); });
                            break;
                        case 2:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(2); });
                            break;
                        case 3:
                            movePanelButton_Entry.callback.AddListener((data) => { ButtonMove(3); });
                            break;
                    }
                    movePanelButtonTriggers[i].triggers.Add(movePanelButton_Entry);
                }
                break;
        }
    }

    ///<summary>
    ///위치 UI를 터치했을 때 발동하는 함수
    ///</summary>
    public void LocationInfoTouch()
    {
        if (openUI) return;

        GameObject movePanel = uiPanel.transform.GetChild(1).gameObject;

        if (!locationInfoTouch)
        {
            movePanel.SetActive(true);
            locationInfoTouch = true;
        }
        else
        {
            movePanel.SetActive(false);
            locationInfoTouch = false;
        }
    }
}
