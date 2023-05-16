using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankingInfo
{
    public string name;
    public int score;
}

public enum QuestionType { MULTIPLE, SHORTANSWER, KEYWORD };

public class MiniGameManager : Singleton<MiniGameManager>
{
    const int multiple = 0;
    const int shortAnswer = 1;
    const int keyword = 2;

    public QuestionType type;
    PhotonView pV;
    //특정 스터디에서 그루퍼에게 지급된 점수를 저장하는 리스트 (마스터가 관리)
    public List<RankingInfo> rankingList = new List<RankingInfo>();

    [Header("MiniGameBaseUI")]
    public GameObject miniGameCanvas;
    public GameObject typeTextUI;
    public Text infoText, typeText;
    public Text[] rankingNameText;
    public Text[] rankingScoreText;

    [Header("MiniGameMasterUI")]
    public GameObject masterPanel;
    public GameObject gameSettingPanel;
    public InputField limitTimeInputField, scoreInputField;
    public Toggle randomCheck;
    public Button startButton;
    public Button multipleButton, shortAnswerButton, keywordButton;
    public Transform content;
    public RectTransform keywordItemPanel;
    //문제 표시용 아이템 리스트
    List<QuestionItem> itemList = new List<QuestionItem>();

    [Header("InGameUI")]
    public GameObject InGameCanvas;
    public Text limitTimeText;
    public GameObject multiplePanel, shortAnswerPanel, keywordPanel;
    public Text multipleQuestionText, shortAnswerQuestionText, keywordQuestionText;
    public Text[] exampleText;

    [Header("PrefabObject")]
    GameObject questionItem, keywordItem;

    [Header("MiniGameVariable")]
    List<string> multipleQuestionInfo = new List<string>();
    //Question_Examples(split(',')_Answer
    List<string> answerQuestionInfo = new List<string>();
    //Question_Answer
    List<string> keywordQuestionInfo = new List<string>();
    //Question_Keywords(split(',')_Answer

    public delegate void InteractActive();
    public InteractActive keywordDestroy;
    //게임 총괄적인 플레이를 관리하는 코루틴 
    Coroutine playGameRoutine, keywordProcess;

    //현재 게임 플레이 중인지 여부를 체크
    public bool isPlaying = false;
    //현재 선택한 문제 번호
    public int currentIndex;
    //랜덤 문제 제출 여부 체크
    bool isRandom;
    //문제 해결 여부 체크
    bool problemSolve;
    //제한 시간
    int limitTime = 30;
    //추가 점수
    int scoreValue = 1;

    #region 데이터 세팅
    void Start()
    {
        pV = GetComponent<PhotonView>();
        string multipleQuestions = (string)PhotonNetwork.CurrentRoom.CustomProperties["MultipleQuestions"];
        SplitInfo(multipleQuestions, "M");
        string answerQuestions = (string)PhotonNetwork.CurrentRoom.CustomProperties["AnswerQuestions"];
        SplitInfo(answerQuestions, "A");
        string keywordQuestions = (string)PhotonNetwork.CurrentRoom.CustomProperties["KeywordQuestions"];
        SplitInfo(keywordQuestions, "K");

        questionItem = Resources.Load<GameObject>("Prefabs/UI/QuestionItem");
        keywordItem = Resources.Load<GameObject>("Prefabs/UI/KeywordItem");
    }

    public IEnumerator InfoTextPresentation()
    {
        int max = 0;
        while (RoomManager.Instance.chatRoom.playingMiniGame)
        {
            if (!isPlaying)
            {
                string info = "게임 준비 중";
                for (int i = 0; i < max; i++)
                {
                    info += ".";
                }
                infoText.text = info;
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                infoText.text = "";
            }

            max++;
            if (max > 3) max = 0;
            yield return null;
        }
    }

    void SplitInfo(string question, string type)
    {
        string[] splitQuestions = question.Split('|');
        for (int i = 0; i < splitQuestions.Length - 1; i++)
        {
            switch (type)
            {
                case "M":
                    multipleQuestionInfo.Add(splitQuestions[i]);
                    break;
                case "A":
                    answerQuestionInfo.Add(splitQuestions[i]);
                    break;
                case "K":
                    keywordQuestionInfo.Add(splitQuestions[i]);
                    break;
            }

        }
    }

    public void Initialization(bool isEnd)
    {
        if (playGameRoutine != null)
        {
            StopCoroutine(playGameRoutine);
            playGameRoutine = null;
        }

        if (keywordProcess != null)
        {
            StopCoroutine(keywordProcess);
            keywordProcess = null;
        }

        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        pV.RPC("ResetItemList", RpcTarget.All);
        startButton.interactable = false;
        limitTimeInputField.text = "";
        scoreInputField.text = "";
        randomCheck.isOn = false;
        isRandom = false;
        scoreValue = 1;
        multipleButton.interactable = true;
        shortAnswerButton.interactable = true;
        keywordButton.interactable = true;
        gameSettingPanel.SetActive(false);
        multiplePanel.SetActive(false);
        shortAnswerPanel.SetActive(false);
        keywordPanel.SetActive(false);
        InGameCanvas.SetActive(false);
        limitTime = 30;
        scoreValue = 1;

        if (isEnd)
        {
            isPlaying = false;
            StopAllCoroutines();
        }
    }

    public int GetRankerReward(string name)
    {
        int value = 0;

        for (int i = 0; i < rankingList.Count; i++)
        {
            if(rankingList[i].name.Equals(name))
            {
                switch (i)
                {
                    case 0:
                        value = 3;
                        break;
                    case 1:
                        value = 2;
                        break;
                    case 2:
                        value = 1;
                        break;
                }
            }
        }
        
        return value;
    }
    #endregion

    #region UI 조작
    /// <summary>
    /// 미니게임 진행 중단 버튼
    /// </summary>
    public void StopButton()
    {
        if (keywordProcess != null) keywordProcess = null;
        pV.RPC("GameStopProcess", RpcTarget.AllViaServer);
        Initialization(false);        
    }

    /// <summary>
    /// 게임 타입을 선택하는 버튼
    /// </summary>
    public void SelectGameTypeButton(int type)
    {
        if (gameSettingPanel.activeSelf) return;
        SelectGameType(type);
    }

    /// <summary>
    /// 선택된 게임 타입에 따라 정보 및 UI를 세팅하는 함수
    /// </summary>
    void SelectGameType(int typeValue)
    {
        switch (typeValue)
        {
            case multiple:
                typeText.text = "4지선다";
                type = QuestionType.MULTIPLE;
                shortAnswerButton.interactable = false;
                keywordButton.interactable = false;

                for (int i = 0; i < multipleQuestionInfo.Count; i++)
                {
                    GameObject item = Instantiate(questionItem, content);
                    item.transform.localScale = Vector3.one;
                    QuestionItem itemScript = item.GetComponent<QuestionItem>();
                    itemScript.Index = i;
                    string[] split = multipleQuestionInfo[i].Split('_');
                    itemScript.question = split[0];
                    itemScript.questionText.text = split[0];
                    itemScript.options = split[1];
                    itemScript.answer = split[2];
                    itemList.Add(itemScript);
                }
                break;
            case shortAnswer:
                typeText.text = "주관식";
                type = QuestionType.SHORTANSWER;
                multipleButton.interactable = false;
                keywordButton.interactable = false;

                for (int i = 0; i < answerQuestionInfo.Count; i++)
                {
                    GameObject item = Instantiate(questionItem, content);
                    item.transform.localScale = Vector3.one;
                    QuestionItem itemScript = item.GetComponent<QuestionItem>();
                    itemScript.Index = i;
                    string[] split = answerQuestionInfo[i].Split('_');
                    itemScript.question = split[0];
                    itemScript.questionText.text = split[0];
                    itemScript.answer = split[1];
                    itemList.Add(itemScript);
                }
                break;
            case keyword:
                typeText.text = "키워드";
                type = QuestionType.KEYWORD;
                multipleButton.interactable = false;
                shortAnswerButton.interactable = false;

                for (int i = 0; i < keywordQuestionInfo.Count; i++)
                {
                    GameObject item = Instantiate(questionItem, content);
                    item.transform.localScale = Vector3.one;
                    QuestionItem itemScript = item.GetComponent<QuestionItem>();
                    itemScript.Index = i;
                    string[] split = keywordQuestionInfo[i].Split('_');
                    itemScript.question = split[0];
                    itemScript.questionText.text = split[0];
                    itemScript.options = split[1];
                    itemScript.answer = split[2];
                    itemList.Add(itemScript);
                }
                break;
        }
  
        gameSettingPanel.SetActive(true);
        pV.RPC("SettingSelectInfo", RpcTarget.Others, (int)type);
    }
    #endregion

    #region 미니게임 프로세스
    /// <summary>
    /// 미니게임 실행 시 설정된 세팅에 따라 준비, 동기화 및 게임을 실행하는 프로세스
    /// </summary>
    IEnumerator PlayGameProcess()
    {
        int maxCount = 1; //최대 연속 플레이 횟수
        if (isRandom)
            maxCount = itemList.Count;
        //랜덤 게임일 경우 문제 리스트 전체를 실행하므로 itemList.Count 대입

        List<int> indexList = new List<int>();
        for (int i = 0; i < itemList.Count; i++)
        {
            indexList.Add(i);
        }
        //랜덤 인덱스 추출을 위한 indexList

        for(int i = 0; i < maxCount; i++)
        {
            if (isRandom && DataManager.isMaster)
            {
                int rand = Random.Range(0, indexList.Count);
                SetIndex(indexList[rand]);
                indexList.RemoveAt(rand);
            }

            pV.RPC("DisplayQuestionPanel", RpcTarget.AllViaServer, true);

            float time = limitTime;
            while (!problemSolve)
            {
                time -= Time.deltaTime;
                int intTime = (int)time;                         
                if (time < 0)
                {
                    pV.RPC("TimeSynchronization", RpcTarget.AllViaServer, "00");
                    break;
                }
                else
                    pV.RPC("TimeSynchronization", RpcTarget.AllViaServer, intTime.ToString("D2"));

                yield return null;
            }
            if (type.Equals(QuestionType.KEYWORD))
                pV.RPC("DeleteAllKeywordItem", RpcTarget.AllViaServer);

            problemSolve = false;
            pV.RPC("DisplayQuestionPanel", RpcTarget.AllViaServer, false);

            yield return new WaitForSeconds(1f);
        }
            
        StopButton();
    }

    [PunRPC]
    void ResetItemList() => itemList = new List<QuestionItem>();

    /// <summary>
    /// 게임 실행 중 문제 내용 표시 여부를 판단, 진행하는 함수
    /// </summary>
    [PunRPC]
    void DisplayQuestionPanel(bool isDisplay)
    {
        if (isDisplay)
        {
            DataManager.interactionData.studyGUID = (string)PhotonNetwork.CurrentRoom.CustomProperties["StudyGUID"];
            DataManager.interactionData.curriculumDate = (string)PhotonNetwork.CurrentRoom.CustomProperties["CurriculumDate"];
            switch (type)
            {
                case QuestionType.MULTIPLE:                    
                    multipleButton.interactable = false;
                    multipleQuestionText.text = "Q. " + itemList[currentIndex].question;
                    string[] examples = itemList[currentIndex].options.Split(',');
                    for (int j = 0; j < examples.Length; j++)
                    {
                        exampleText[j].text = examples[j];
                    }
                    multiplePanel.SetActive(true);
                    if (DataManager.isMaster)
                        StartCoroutine(DataManager.Instance.SetReviewQuiz("M", itemList[currentIndex].question, itemList[currentIndex].answer, itemList[currentIndex].options));
                    break;
                case QuestionType.SHORTANSWER:                   
                    shortAnswerButton.interactable = false;
                    shortAnswerQuestionText.text = "Q. " + itemList[currentIndex].question;
                    shortAnswerPanel.SetActive(true);
                    if (DataManager.isMaster)
                        StartCoroutine(DataManager.Instance.SetReviewQuiz("S", itemList[currentIndex].question, itemList[currentIndex].answer, itemList[currentIndex].options));
                    break;
                case QuestionType.KEYWORD:
                    keywordButton.interactable = false;
                    keywordQuestionText.text = "Q. " + itemList[currentIndex].question;
                    keywordPanel.SetActive(true);
                    if (DataManager.isMaster)
                    {
                        StartCoroutine(DataManager.Instance.SetReviewQuiz("K", itemList[currentIndex].question, itemList[currentIndex].answer, itemList[currentIndex].options));
                        if(keywordProcess == null)
                            keywordProcess = StartCoroutine(KeywordProcess());
                    }
                    break;
            }
        }
        else
        {
            multiplePanel.SetActive(false);
            shortAnswerPanel.SetActive(false);
            keywordPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 키워드 객체 하강 연출을 진행하는 코루틴
    /// </summary>
    IEnumerator KeywordProcess()
    {
        float coolTime = 0f;
        while(isPlaying)
        {            
            coolTime += Time.deltaTime;
            if (coolTime >= 1.5f)
            {
                float itemWidth = keywordItem.GetComponent<RectTransform>().rect.width / 2;
                float startX = -(keywordItemPanel.rect.width / 2f) + itemWidth;
                float endX = (keywordItemPanel.rect.width / 2f) - itemWidth;
                float positionX = Random.Range(startX, endX);
                if (itemList.Count.Equals(0)) break;
                string[] keywords = itemList[currentIndex].options.Split(',');
                string keyword = keywords[Random.Range(0, keywords.Length)];

                pV.RPC("CreateKeywordItem", RpcTarget.AllViaServer, positionX, keyword);
                coolTime = 0f;
            }
            yield return null;
        }

        pV.RPC("DeleteAllKeywordItem", RpcTarget.AllViaServer);
    }

    /// <summary>
    /// 키워드 아이템을 생성하는 함수
    /// </summary>
    [PunRPC]
    void CreateKeywordItem(float positionX, string keyword)
    {        
        GameObject keywordItem = Instantiate(this.keywordItem, keywordItemPanel);
        float height = keywordItemPanel.rect.height / 2f + keywordItem.GetComponent<RectTransform>().rect.height;
        keywordItem.GetComponent<RectTransform>().localPosition = new Vector3(positionX, height);

        KeywordItem keywordScript = keywordItem.GetComponent<KeywordItem>();
        keywordScript.keyword.text = keyword;
        keywordScript.max = -height;
        keywordScript.panel = null;
        keywordDestroy += keywordScript.DestroyItem;
    }

    /// <summary>
    /// 키워드 아이템을 삭제하는 함수
    /// </summary>
    [PunRPC]
    void DeleteAllKeywordItem() => keywordDestroy?.Invoke();

    /// <summary>
    /// 시간을 동기화 하여 표시하는 함수
    /// </summary>
    [PunRPC]
    void TimeSynchronization(string time) => limitTimeText.text = time;

    /// <summary>
    /// 매개변수 chat 정보와 답을 비교하여 정답 여부를 체크하는 함수
    /// </summary>
    public bool ConfirmCorrectAnswer(string chat)
    {
        if (type.Equals(QuestionType.MULTIPLE)) //사지선다일 경우
        {
            /////////////////채팅이 문제 보기 내용일 경우 체크
            string[] answers = itemList[currentIndex].options.Split(',');
            for (int i = 0; i < answers.Length; i++)
            {
                if (chat.Equals(answers[i]))
                {
                    pV.RPC("AddRankingList", RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer.NickName, scoreValue);
                    pV.RPC("ConfirmCorrectAnswerRPC", RpcTarget.MasterClient);
                    return true;
                }
            }
            //////////////////채팅이 문제 보기 번호일 경우 체크
            string answerIndex = itemList[currentIndex].answer;
            //ex. 1, 1번 식으로 입력해도 판정 
            if (chat.Equals(answerIndex) || chat.Equals(answerIndex + "번"))
            {
                pV.RPC("AddRankingList", RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer.NickName, scoreValue);
                pV.RPC("ConfirmCorrectAnswerRPC", RpcTarget.MasterClient);
                return true;
            }
        }
        else //그 외의 경우
        {
            if (chat.Equals(itemList[currentIndex].answer))
            {
                pV.RPC("AddRankingList", RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer.NickName, scoreValue);
                pV.RPC("ConfirmCorrectAnswerRPC", RpcTarget.MasterClient);                
                return true;
            }
        }

        return false;
    }

    [PunRPC]
    void AddRankingList(string name, int scoreValue)
    {
        RankingInfo info = new RankingInfo();
        info.name = name;
        info.score = scoreValue;

        for (int i = 0; i < rankingList.Count; i++)
        {
            if(rankingList[i].name.Equals(info.name))
            {
                rankingList[i].score += info.score;
                SortRanking(rankingList);
                RankingSetting();
                return;
            }
        }

        rankingList.Add(info);
        SortRanking(rankingList);
        RankingSetting();
    }

    void SortRanking(List<RankingInfo> rankings)
    {
        for (int i = 0; i < rankings.Count; i++)
        {
            for (int j = i + 1; j < rankings.Count; j++)
            {
                if (rankings[i].score < rankings[j].score)
                {
                    RankingInfo tempInfo = rankings[i];
                    rankings[i] = rankings[j];
                    rankings[j] = tempInfo;
                }
            }
        }
    }

    /// <summary>
    /// 부여된 점수에 따라 랭킹을 표시하는 함수<br /> 
    /// 동일한 룸에 있는 모든 플레이어에게 동일하게 보임(동기화)
    /// </summary>
    void RankingSetting()
    {
        for (int i = 0; i < rankingList.Count; i++)
        {
            if (i >= rankingNameText.Length) return;

            rankingNameText[i].text = rankingList[i].name;
            rankingScoreText[i].text = rankingList[i].score.ToString();
        }
    }
    #endregion

    #region GameSettingUI 관련 조작    

    public void CancleButton()
    {
        Initialization(false);
    }

    public void StartGameButton() => pV.RPC("StartGameProcess", RpcTarget.AllViaServer);

    public void RandomCheck()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            itemList[i].ButtonActive(randomCheck.isOn);
        }

        if (randomCheck.isOn)
        {
            startButton.interactable = true;
            pV.RPC("RandomCheckRPC", RpcTarget.All, true);
        }
        else
        {
            startButton.interactable = false;
            pV.RPC("RandomCheckRPC", RpcTarget.All, false);
        }
    }

    public void InputValueCheck(int type)
    {
        if (type.Equals(0))
        {
            if (limitTimeInputField.text.Equals("")) return;
            pV.RPC("InputValueCheckRPC", RpcTarget.All, type, limitTimeInputField.text);
        }
        else
        {
            if (scoreInputField.text.Equals("")) return;
            pV.RPC("InputValueCheckRPC", RpcTarget.All, type, scoreInputField.text);
        }
    }

    #endregion

    #region 정보 동기화(RPC)    

    [PunRPC]
    void SettingSelectInfo(int typeInfo)
    {
        type = (QuestionType)typeInfo;
        switch (type)
        {
            case QuestionType.MULTIPLE:
                typeText.text = "4지선다";
                for (int i = 0; i < multipleQuestionInfo.Count; i++)
                {
                    QuestionItem itemScript = new QuestionItem();
                    string[] split = multipleQuestionInfo[i].Split('_');
                    itemScript.question = split[0];
                    itemScript.options = split[1];
                    itemScript.answer = split[2];
                    itemList.Add(itemScript);
                }
                break;
            case QuestionType.SHORTANSWER:
                typeText.text = "주관식";
                for (int i = 0; i < answerQuestionInfo.Count; i++)
                {
                    QuestionItem itemScript = new QuestionItem();
                    string[] split = answerQuestionInfo[i].Split('_');
                    itemScript.question = split[0];
                    itemScript.answer = split[1];
                    itemList.Add(itemScript);
                }
                break;
            case QuestionType.KEYWORD:
                typeText.text = "키워드";
                for (int i = 0; i < keywordQuestionInfo.Count; i++)
                {
                    QuestionItem itemScript = new QuestionItem();
                    string[] split = keywordQuestionInfo[i].Split('_');
                    itemScript.question = split[0];
                    itemScript.options = split[1];
                    itemScript.answer = split[2];
                    itemList.Add(itemScript);
                }
                break;
        }
    }

    [PunRPC]
    void GameStopProcess()
    {
        isPlaying = false;
        typeTextUI.SetActive(false);
        multiplePanel.SetActive(false);
        shortAnswerPanel.SetActive(false);
        keywordPanel.SetActive(false);
        limitTimeText.text = "00";
    }

    [PunRPC]
    void RandomCheckRPC(bool value) => isRandom = value;

    [PunRPC]
    void InputValueCheckRPC(int type, string strValue)
    {
        if (type.Equals(0))
        {
            int value = int.Parse(strValue);
            if (value <= 0)
            {
                if (DataManager.isMaster)
                    limitTimeInputField.text = "";
            }
            else
                limitTime = value;                   
        }
        else
        {
            int value = int.Parse(strValue);
            if (value <= 0)
            {
                if (DataManager.isMaster)
                    scoreInputField.text = "";
            }
            else
                scoreValue = value;
        }
    }

    [PunRPC]
    void ConfirmCorrectAnswerRPC()
    {
        problemSolve = true;
    }    

    /// <summary>
    /// 마스터가 선택한 문제의 인덱스를 세팅하는 함수<br /> 
    /// PunRPC를 통해 모든 인원들에게 동일한 인덱스를 부여(동기화)
    /// </summary>
    public void SetIndex(int index) => pV.RPC("SetIndexRPC", RpcTarget.AllViaServer, index);

    [PunRPC]
    void SetIndexRPC(int index) => currentIndex = index;

    /// <summary>
    /// 선택한 타입, 아이템에 따라 미니게임을 실행하는 함수<br /> 
    /// PlayGameProcess 코루틴으로 미니게임 타입에 따른 UI 표시 및 동작 진행(동기화)
    /// </summary>
    [PunRPC]
    void StartGameProcess()
    {
        typeTextUI.SetActive(true);
        InGameCanvas.SetActive(true);
        isPlaying = true;
        if (DataManager.isMaster)
        {
            gameSettingPanel.SetActive(false);            
            playGameRoutine = StartCoroutine(PlayGameProcess());
        }
    }
    #endregion
}
