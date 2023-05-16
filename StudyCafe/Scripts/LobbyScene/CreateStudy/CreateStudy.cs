using Newtonsoft.Json;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = System.Random;

public class CreateStudy : Singleton<CreateStudy>
{
    [Header("LearningInfoPanelUI")]
    public GameObject learningInfoPanel;
    public InputField studyNameInput;
    public InputField studySubjectInput;
    public InputField objectiveStartDateInput, objectiveEndDateInput, objectiveTimeInput, objectiveCountInput;
    public Dropdown objectiveTimeSelectDropdown;

    [Header("RuleInfoPanelUI")]
    public GameObject ruleInfoPanel;
    public InputField tardyCountInput;
    public InputField absentCountInput;
    public InputField assignmentCountInput;
    public Dropdown tardyPenaltyDropdown, absentPenaltyDropdown, assignmentPenaltyDropdown;

    [Header("CurriculumPanelUI")]
    public GameObject curriculumInfoPanel;
    public GameObject addCurriculumInfoPanel;
    public InputField curriculumStartDateInput;
    public InputField curriculumStartHourInput;
    public InputField curriculumStartMinuteInput;
    public InputField curriculumEndDateInput, curriculumEndHourInput, curriculumEndMinuteInput;
    public InputField curriculumInfoInput;
    public Transform content;

    [Header("CurriculumItem")]
    List<CurriculumItem> curriculumItemList = new List<CurriculumItem>();
    int selectItemIndex;

    [Header("AnnouncementPanelUI")]
    public GameObject announcementPanel;
    public Toggle[] announcementToggles;

    [Header("InviteMemberUI")]
    public GameObject inviteMemberPanel;

    [Header("Base UI and Object")]
    public Toggle[] taps;
    public Button prevButton;
    public Text completeButtonText;
    int currentPage;
    public int CurrentPage
    {
        get { return currentPage; }
        set
        {
            currentPage = value;
            taps[currentPage].isOn = true;
            switch (currentPage)
            {
                case 0:
                    {
                        learningInfoPanel.SetActive(true);
                        ruleInfoPanel.SetActive(false);
                        curriculumInfoPanel.SetActive(false);                        
                        prevButton.interactable = false;
                        completeButtonText.text = "다음";
                    }
                    break;
                case 1:
                    {
                        learningInfoPanel.SetActive(false);
                        ruleInfoPanel.SetActive(true);
                        curriculumInfoPanel.SetActive(false);                        
                        prevButton.interactable = true;
                        completeButtonText.text = "다음";
                    }
                    break;
                case 2:
                    {
                        learningInfoPanel.SetActive(false);
                        ruleInfoPanel.SetActive(false);
                        curriculumInfoPanel.SetActive(true);
                        prevButton.interactable = true;
                        completeButtonText.text = "개설";
                    }
                    break;
            }
            CloseDateInfoPanelButton();
        }
    }
    string studyGUID;

    void OnEnable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);        
    }

    void OnDisable() => Initialization();

    string CreateCode(int _nLength = 8)
    {
        Random random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
        string strPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";  //문자 생성 풀
        char[] chRandom = new char[_nLength];

        for (int i = 0; i < _nLength; i++)
        {
            chRandom[i] = strPool[random.Next(strPool.Length)];
        }
        string code = new string(chRandom);

        return code;
    }

    ///<summary>
    ///스터디 준비 UI 정보를 초기화 하는 함수
    ///</summary>
    void Initialization()
    {
        //학습 관련 정보 초기화
        studyNameInput.text = "";
        studySubjectInput.text = "";
        objectiveStartDateInput.text = "";
        objectiveEndDateInput.text = "";
        objectiveTimeInput.text = "";
        objectiveCountInput.text = "";
        objectiveTimeSelectDropdown.value = 0;
        //규칙 관련 정보 초기화
        tardyCountInput.text = "";
        absentCountInput.text = "";
        assignmentCountInput.text = "";
        tardyPenaltyDropdown.value = 0;
        absentPenaltyDropdown.value = 0;
        assignmentPenaltyDropdown.value = 0;
        //커리큘럼 관련 정보 초기화
        curriculumStartDateInput.text = "";
        curriculumStartHourInput.text = "";
        curriculumStartMinuteInput.text = "";
        curriculumEndDateInput.text = "";
        curriculumEndHourInput.text = "";
        curriculumEndMinuteInput.text = "";
        curriculumInfoInput.text = "";
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        curriculumItemList = new List<CurriculumItem>();
        CurrentPage = 0;
        CommonInteraction.Instance.calendarPanel.SetActive(false);
    }

    #region 유효성 검사 함수
    ///<summary>
    ///스터디 생성 시도 시 유효성 검사 함수
    ///</summary>
    bool CheckExceptions()
    {
        DateTime dummy = new DateTime();

        if (studyNameInput.text.Equals(""))
        {
            CurrentPage = 0;
            studyNameInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("스터디 이름을 입력해 주세요.");
            return false;
        }

        if (studySubjectInput.text.Equals(""))
        {
            CurrentPage = 0;
            studySubjectInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("스터디 주제를 입력해 주세요.");
            return false;
        }

        if (objectiveStartDateInput.text.Equals(""))
        {
            CurrentPage = 0;
            objectiveStartDateInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("스터디 목표의 시작 날짜를 입력해 주세요.\n예시) 2021-01-01");
            return false;
        }
        else
        {
            if (!DateTime.TryParse(objectiveStartDateInput.text, out dummy))
            {
                CurrentPage = 0;
                objectiveStartDateInput.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("스터디 목표의 시작 날짜를 정확히 입력해 주세요.\n예시) 2021-01-01");
                return false;
            }
            else
            {
                dummy = DateTime.Parse(objectiveStartDateInput.text);
                if (dummy < DateTime.Today)
                {
                    CurrentPage = 0;
                    objectiveStartDateInput.ActivateInputField();
                    CommonInteraction.Instance.InfoPanelUpdate("지난 날짜를 입력하실 수 없습니다.");
                    return false;
                }
            }
        }

        if (objectiveEndDateInput.text.Equals(""))
        {
            CurrentPage = 0;
            objectiveEndDateInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("스터디 목표의 종료 날짜를 입력해 주세요.\n예시) 2021-01-02");
            return false;
        }
        else
        {
            if (!DateTime.TryParse(objectiveEndDateInput.text, out dummy))
            {
                CurrentPage = 0;
                objectiveEndDateInput.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("스터디 목표의 종료 날짜를 정확히 입력해 주세요.\n예시) 2021-01-02");
                return false;
            }
            else
            {
                dummy = DateTime.Parse(objectiveEndDateInput.text);
                if (dummy < DateTime.Today)
                {
                    CurrentPage = 0;
                    objectiveEndDateInput.ActivateInputField();
                    CommonInteraction.Instance.InfoPanelUpdate("지난 날짜를 입력하실 수 없습니다.");
                    return false;
                }

                DateTime startDummy = DateTime.Parse(objectiveStartDateInput.text);
                if (dummy < startDummy)
                {
                    CurrentPage = 0;
                    objectiveEndDateInput.ActivateInputField();
                    CommonInteraction.Instance.InfoPanelUpdate("종료일을 시작일보다 이전의 날짜로 입력하실 수 없습니다.");
                    return false;
                }
            }
        }

        if (objectiveTimeInput.text.Equals(""))
        {
            CurrentPage = 0;
            objectiveTimeInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("스터디 목표의 1회 평균 스터디 시간을 입력해 주세요.");
            return false;
        }

        if (objectiveCountInput.text.Equals(""))
        {
            CurrentPage = 0;
            objectiveCountInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("스터디 목표의 총 스터디 횟수를 입력해 주세요.");
            return false;
        }

        if (tardyCountInput.text.Equals(""))
        {
            CurrentPage = 1;
            tardyCountInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("지각 규칙의 횟수를 입력해 주세요.");
            return false;
        }

        if (absentCountInput.text.Equals(""))
        {
            CurrentPage = 1;
            absentCountInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("결석 규칙의 횟수를 입력해 주세요.");
            return false;
        }

        if (assignmentCountInput.text.Equals(""))
        {
            CurrentPage = 1;
            assignmentCountInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("과제 규칙의 횟수를 입력해 주세요.");
            return false;
        }

        if (content.childCount <= 0)
        {
            CurrentPage = 2;
            CommonInteraction.Instance.InfoPanelUpdate("수업 자료는 최소 하나 이상 등록해 주셔야 합니다.");
            return false;
        }

        return true;
    }

    public bool CheckExceptionsCurriculum()
    {
        DateTime dummy = new DateTime();
        if (curriculumStartDateInput.text.Equals(""))
        {
            curriculumStartDateInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("수업 자료의 시작 날짜를 입력해 주세요.\n예시) 2021-01-01");
            return false;
        }
        else
        {
            if (!DateTime.TryParse(curriculumStartDateInput.text, out dummy))
            {
                curriculumStartDateInput.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("수업 자료의 시작 날짜를 정확히 입력해 주세요.\n예시) 2021-01-01");
                return false;
            }
            else
            {
                dummy = DateTime.Parse(curriculumStartDateInput.text);
                if (dummy < DateTime.Today)
                {
                    curriculumStartDateInput.ActivateInputField();
                    CommonInteraction.Instance.InfoPanelUpdate("지난 날짜를 입력하실 수 없습니다.");
                    return false;
                }
            }
        }

        if (curriculumEndDateInput.text.Equals(""))
        {
            curriculumEndDateInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("수업 자료의 종료 날짜를 입력해 주세요.\n예시) 2021-01-01");
            return false;
        }
        else
        {
            if (!DateTime.TryParse(curriculumEndDateInput.text, out dummy))
            {
                curriculumEndDateInput.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("수업 자료의 종료 날짜를 정확히 입력해 주세요.\n예시) 2021-01-01");
                return false;
            }
            else
            {
                dummy = DateTime.Parse(curriculumEndDateInput.text);
                if (dummy < DateTime.Today)
                {
                    curriculumEndDateInput.ActivateInputField();
                    CommonInteraction.Instance.InfoPanelUpdate("지난 날짜를 입력하실 수 없습니다.");
                    return false;
                }
                
                DateTime startDummy = DateTime.Parse(curriculumStartDateInput.text);
                if (dummy < startDummy)
                {
                    curriculumEndDateInput.ActivateInputField();
                    CommonInteraction.Instance.InfoPanelUpdate("종료일을 시작일보다 이전의 날짜로 입력하실 수 없습니다.");
                    return false;
                }
            }
        }

        if (curriculumStartHourInput.text.Equals(""))
        {
            curriculumStartHourInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("수업자료의 시작 시간을 입력해 주세요.");
            return false;
        }

        if (curriculumEndHourInput.text.Equals(""))
        {
            curriculumEndHourInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("수업자료의 종료 시간을 입력해 주세요.");
            return false;
        }

        if (curriculumStartMinuteInput.text.Equals(""))
        {
            curriculumStartMinuteInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("수업자료의 시작 시간(분)을 입력해 주세요.");
            return false;
        }

        if (curriculumEndMinuteInput.text.Equals(""))
        {
            curriculumEndMinuteInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("수업자료의 종료 시간(분)을 입력해 주세요.");
            return false;
        }
        else
        {
            string tempStartDateStr = curriculumStartDateInput.text + " " + curriculumStartHourInput.text + ":" + curriculumStartMinuteInput.text;
            string tempEndDateStr = curriculumEndDateInput.text + " " + curriculumEndHourInput.text + ":" + curriculumEndMinuteInput.text;
            DateTime startTime = DateTime.Parse(tempStartDateStr);
            DateTime endTime = DateTime.Parse(tempEndDateStr);
            if (endTime <= startTime)
            {
                curriculumEndHourInput.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("시작 시간과 같거나 이전의 시간을 입력할 수 없습니다.");
                return false;
            }
        }

        for (int i = 0; i < curriculumItemList.Count; i++)
        {
            string inputStartDateStr = curriculumStartDateInput.text + " " + DataManager.Instance.ChangeNumberCharacters(curriculumStartHourInput.text, 2) +
                ":" + DataManager.Instance.ChangeNumberCharacters(curriculumStartMinuteInput.text, 2);
            string inputEndDateStr = curriculumEndDateInput.text + " " + DataManager.Instance.ChangeNumberCharacters(curriculumEndHourInput.text, 2) +
        ":" + DataManager.Instance.ChangeNumberCharacters(curriculumEndMinuteInput.text, 2);
            DateTime startDate = DateTime.Parse(curriculumItemList[i].CurriculumInfomation.startDate);
            DateTime endDate = DateTime.Parse(curriculumItemList[i].CurriculumInfomation.endDate);
            DateTime inputStartDate = DateTime.Parse(inputStartDateStr);
            DateTime inputEndDate = DateTime.Parse(inputEndDateStr);
            if(inputStartDate.Equals(startDate) && inputEndDate.Equals(endDate))
            {
                curriculumStartDateInput.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("이미 동일한 시간의 수업 자료가 있습니다.");
                return false;
            }
            else if(inputStartDate <= startDate && inputEndDate >= startDate)
            {
                curriculumStartDateInput.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("시간이 겹치는 수업 자료가 있습니다.");
                return false;
            }
            else if (inputStartDate >= startDate && inputEndDate <= endDate)
            {
                curriculumStartDateInput.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("시간이 겹치는 수업 자료가 있습니다.");
                return false;
            }
            else if (inputStartDate <= endDate && inputEndDate >= endDate)
            {
                curriculumStartDateInput.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("시간이 겹치는 수업 자료가 있습니다.");
                return false;
            }
        }

        if(curriculumInfoInput.text.Equals(""))
        {
            curriculumInfoInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("커리큘럼 정보의 내용을 입력해 주세요.");
            return false;
        }

        return true;
    }

    ///<summary>
    ///목표 1회 평균 시간 입력란의 숫자를 체크하여 유효한 숫자만 입력되도록 변경
    ///</summary>
    public void CheckExceptionsObjectiveTime()
    {
        if (objectiveTimeInput.text.Equals("")) return;
        int dummy = int.Parse(objectiveTimeInput.text);
        //시간 단위로 체크할 경우
        if (objectiveTimeSelectDropdown.value.Equals(0))
        {
            if (dummy < 0) objectiveTimeInput.text = "0";
            else if (dummy >= 24) objectiveTimeInput.text = "23";
        }
        //분 단위로 체크할 경우
        else
        {
            if (dummy < 0) objectiveTimeInput.text = "0";
            else if (dummy >= 60) objectiveTimeInput.text = "59";
        }
    }

    ///<summary>
    ///커리큘럼 시 입력란의 숫자를 체크하여 유효한 숫자만 입력되도록 변경
    ///</summary>
    public void CheckExceptionsHour(string type)
    {
        if (type.Equals("START"))
        {
            if (curriculumStartHourInput.text.Equals("")) return;
            int dummy = int.Parse(curriculumStartHourInput.text);
            if (dummy < 0) curriculumStartHourInput.text = "0";
            else if (dummy >= 24) curriculumStartHourInput.text = "23";
        }
        else
        {
            if (curriculumEndHourInput.text.Equals("")) return;
            int dummy = int.Parse(curriculumEndHourInput.text);
            if (dummy < 0) curriculumEndHourInput.text = "0";
            else if (dummy >= 24) curriculumEndHourInput.text = "23";
        }
    }

    ///<summary>
    ///커리큘럼 분 입력란의 숫자를 체크하여 유효한 숫자만 입력되도록 변경
    ///</summary>
    public void CheckExceptionsMinute(string type)
    {
        if (type.Equals("START"))
        {
            if (curriculumStartMinuteInput.text.Equals("")) return;
            int dummy = int.Parse(curriculumStartMinuteInput.text);
            if (dummy < 0) curriculumStartMinuteInput.text = "0";
            else if (dummy >= 60) curriculumStartMinuteInput.text = "59";
        }
        else
        {
            if (curriculumEndMinuteInput.text.Equals("")) return;
            int dummy = int.Parse(curriculumEndMinuteInput.text);
            if (dummy < 0) curriculumEndMinuteInput.text = "0";
            else if (dummy >= 60) curriculumEndMinuteInput.text = "59";
        }
    }

    ///<summary>
    ///0 이하의 숫자 입력을 방지하여 유효한 숫자만 입력되도록 변경
    ///</summary>
    public void CheckExceptionsCount(string type)
    {
        switch (type)
        {
            case "OBJECTIVE":
                {
                    if (objectiveCountInput.text.Equals("")) return;
                    int dummy = int.Parse(objectiveCountInput.text);
                    if (dummy <= 0) objectiveCountInput.text = "1";
                }
                break;
            case "TARDY":
                {
                    if (tardyCountInput.text.Equals("")) return;
                    int dummy = int.Parse(tardyCountInput.text);
                    if (dummy <= 0) tardyCountInput.text = "1";
                }
                break;
            case "ABSENT":
                {
                    if (absentCountInput.text.Equals("")) return;
                    int dummy = int.Parse(absentCountInput.text);
                    if (dummy <= 0) absentCountInput.text = "1";
                }
                break;
            case "ASSIGNMENT":
                {
                    if (assignmentCountInput.text.Equals("")) return;
                    int dummy = int.Parse(assignmentCountInput.text);
                    if (dummy <= 0) assignmentCountInput.text = "1";
                }
                break;
        }
    }

    #endregion

    #region 스터디 만들기 기본 UI 조작
    public void PrevButton()
    { 
        if(currentPage > 0) 
            CurrentPage--;
    }

    ///<summary>
    ///스터디 만들기를 끝내고 개설을 진행하는 함수
    ///</summary>
    public void CompleteButton()
    {
        if (currentPage.Equals(2))
        {
            if (!CheckExceptions())
                return;

            CommonInteraction.Instance.ConfirmPanelUpdate("입력하신 정보로 스터디를 개설하시겠습니까?");
            CommonInteraction.Instance.confirmFunc = CompleteProcess;
        }
        else
        {
            CurrentPage++;
        }
    }

    void CompleteProcess(bool check)
    {
        if(check)
            StartCoroutine(CreateStudyProcess());
    }

    public void SelectTab(int index) => CurrentPage = index;

    ///<summary>
    ///스터디 만들기창을 종료하는 함수
    ///</summary>
    public void CloseButton() => gameObject.SetActive(false);

    public void AnnouncementSetting(int index)
    {
        selectItemIndex = index;
        string[] splitInfo = curriculumItemList[index].CurriculumInfomation.announcementInfo.Split(',');
        for (int i = 0; i < splitInfo.Length; i++)
        {
            if (splitInfo[i].Equals("T"))
                announcementToggles[i].isOn = true;
            else
                announcementToggles[i].isOn = false;
        }
        announcementPanel.SetActive(true);
    }

    public void ConfirmAnnouncementInfo()
    {
        string info = "";
        for (int i = 0; i < announcementToggles.Length; i++)
        {
            if (announcementToggles[i].isOn)
                info += "T";
            else
                info += "F";

            if (i < announcementToggles.Length - 1)
                info += ",";
        }

        curriculumItemList[selectItemIndex].CurriculumInfomation.announcementInfo = info;
        CancelAnnouncementInfo();
    }

    public void CancelAnnouncementInfo()
    {
        for (int i = 0; i < announcementToggles.Length; i++)
        {
            announcementToggles[i].isOn = true;
        }
        announcementPanel.SetActive(false);
    }

    ///<summary>
    ///커리큘럼 아이템 리스트에서 삭제
    ///</summary>
    public void DeleteCurriculumItem(int index)
    {
        curriculumItemList.RemoveAt(index);
        CurricululItemIndexReset();
    }

    ///<summary>
    ///커리큘럼 아이템 인덱스 재설정
    ///</summary>
    void CurricululItemIndexReset()
    {
        //인덱스 다시 세팅
        for (int i = 0; i < curriculumItemList.Count; i++)
        {
            curriculumItemList[i].ListIndex = i;
        }
    }
    #endregion

    #region 스터디 만들기 프로세스
    ///<summary>
    ///스터디 준비를 통해 얻은 정보를 StudyInfoData 클래스에 값 대입, json으로 변환하여 DB에 저장 
    ///</summary>
    IEnumerator CreateStudyProcess()
    {
        StudyInfoData infoData = new StudyInfoData();
        //////////////////////////////////////GUID 및 코드 삽입
        Guid guid = Guid.NewGuid();
        studyGUID = guid.ToString();
        infoData.guid = guid.ToString();
        infoData.code = CreateCode();
        DataManager.loadData.code = infoData.code;
        //////////////////////////////////////

        //////////////////////////////////////스터디명, 스터디 주제, 스터디목표 정보 삽입
        infoData.studyName = studyNameInput.text;
        DataManager.loadData.roomName = infoData.studyName;
        infoData.subject = studySubjectInput.text;
        //스터디 목표
        //ex) 2010-01-01,2010-01-02,10,0,5
        string objectiveInfo = objectiveStartDateInput.text + "," + objectiveEndDateInput.text + "," + objectiveTimeInput.text + 
            "," + objectiveTimeSelectDropdown.value + "," + objectiveCountInput.text;
        infoData.objectives = objectiveInfo;
        /////////////////////////////////////////

        ///////////////////////////////////////////규칙 정보 등록
        Dictionary<string, string> rulesInfo = new Dictionary<string, string>();
        //ex) 10,0 (카운트 횟수, 페널티 타입 정보)
        rulesInfo.Add("Tardy", tardyCountInput.text + "," + tardyPenaltyDropdown.value);
        rulesInfo.Add("Absent", absentCountInput.text + "," + absentPenaltyDropdown.value);
        rulesInfo.Add("Assignment", assignmentCountInput.text + "," + assignmentPenaltyDropdown.value);
        infoData.rules = rulesInfo;
        //////////////////////////////////////////////////////////////

        ////////////////////////////////////////커리큘럼 정보 등록
        if (curriculumItemList != null)
        {
            for (int i = 0; i < curriculumItemList.Count; i++)
            {
                infoData.curriculumInfoList.Add(curriculumItemList[i].CurriculumInfomation);
            }
        }
        //////////////////////////////////////

        //////////////////////////////////////교육장 정보 등록
        infoData.studyRoomType = 0;        
        //////////////////////////////////////////////////////////////

        ///////////////////////////infoData 클래스 -> json 파싱
        yield return StartCoroutine(DataManager.Instance.SetStudyRegistration(infoData));
        ///////////////////////////////////////////////////////////////////////////////

        if(DataManager.Instance.info.Equals("SUCCESS"))
        {
            CommonInteraction.Instance.ConfirmPanelUpdate("스터디 개설 완료!\n스터디 모집 공고를 등록하시겠습니까?");
            CommonInteraction.Instance.confirmFunc = DiscloseAnnouncementCheck;
        }
        else
            CommonInteraction.Instance.InfoPanelUpdate("스터디 개설 실패");
    }

    ///<summary>
    ///모집공고 등록 여부 체크
    ///</summary>
    void DiscloseAnnouncementCheck(bool check) => StartCoroutine(AnnouncementCheckProcess(check));

    IEnumerator AnnouncementCheckProcess(bool check)
    {
        if(check)
        {
            yield return StartCoroutine(DataManager.Instance.SetStudyCompositionInfo(StudyComposition.ANNOUNCEMENT, "T", studyGUID));
            if(DataManager.Instance.info.Equals("SUCCESS"))
            {
                CommonInteraction.Instance.InfoPanelUpdate("공고 등록을 완료했습니다!");
                Initialization();
            }
            else
            {
                CommonInteraction.Instance.ConfirmPanelUpdate("공고 등록에 실패하였습니다.\n다시 시도하시겠습니까?");
                CommonInteraction.Instance.confirmFunc = DiscloseAnnouncementCheck;
            }
        }
        else
        {
            CommonInteraction.Instance.ConfirmPanelUpdate("친구들에게 스터디 초대 코드를 발송할까요?");
            CommonInteraction.Instance.confirmFunc = OpenEmailInputPanel;
        }
    }

    void OpenEmailInputPanel(bool check)
    {
        if(check)
        {
            inviteMemberPanel.SetActive(true);
        }
        else
        {
            CommonInteraction.Instance.InfoPanelUpdate("내 스터디룸에서 다시 친구를 초대하실 수 있습니다.");            
        }
        Initialization();
    }

    ///<summary>
    ///날짜 입력 인풋 필드에 정보를 입력하는 캘린더 표시
    ///</summary>
    public void OpenCalendarButton(InputField selectedInputField)
    {
        CommonInteraction.Instance.calendarPanel.SetActive(true);
        Calendar calendar = CommonInteraction.Instance.calendarPanel.GetComponent<Calendar>();
        calendar._selectedInputField = selectedInputField;
        calendar._dateTime = DateTime.Now;
        calendar.CreateCalendar();
    }

    public void OpenDateInfoPanelButton()
    {
        addCurriculumInfoPanel.SetActive(true);
    }

    public void CloseDateInfoPanelButton()
    {
        curriculumStartDateInput.text = "";
        curriculumStartHourInput.text = "";
        curriculumStartMinuteInput.text = "";
        curriculumEndDateInput.text = "";
        curriculumEndHourInput.text = "";
        curriculumEndMinuteInput.text = "";
        curriculumInfoInput.text = "";
        addCurriculumInfoPanel.SetActive(false);
    }

    ///<summary>
    ///세팅된 정보로 커리큘럼을 추가하는 함수
    ///</summary>
    public void AddCurriculumButton()
    {
        if (!CheckExceptionsCurriculum())
            return;

        CurriculumInfo info = new CurriculumInfo();
        //시작 날짜 정보 삽입
        info.startDate = curriculumStartDateInput.text + " " + DataManager.Instance.ChangeNumberCharacters(curriculumStartHourInput.text, 2) +
            ":" + DataManager.Instance.ChangeNumberCharacters(curriculumStartMinuteInput.text, 2);
        //종료 날짜 정보 삽입
        info.endDate = curriculumEndDateInput.text + " " + DataManager.Instance.ChangeNumberCharacters(curriculumEndHourInput.text, 2) +
    ":" + DataManager.Instance.ChangeNumberCharacters(curriculumEndMinuteInput.text, 2);
        //커리큘럼 설명 정보 삽입
        info.description = curriculumInfoInput.text;
        info.teacherGUID = (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"];

        //커리큘럼 아이템 생성 및 정보 세팅
        GameObject curriculumItem = Instantiate(Resources.Load<GameObject>("Prefabs/UI/CurriculumItem"), content, false);
        CurriculumItem itemScript = curriculumItem.GetComponent<CurriculumItem>();
        itemScript.CurriculumInfomation = info;
        itemScript.ListIndex = curriculumItemList.Count;
        itemScript.itemInfoFunction = AnnouncementSetting;
        itemScript.deleteFunction = DeleteCurriculumItem;
        curriculumItemList.Add(itemScript);
        //날짜 순서로 오브젝트 순서 변경
        for (int i = 0; i < curriculumItemList.Count - 1; i++)
        {
            DateTime left = DateTime.Parse(curriculumItemList[i].CurriculumInfomation.startDate);
            DateTime right = DateTime.Parse(curriculumItemList[curriculumItemList.Count - 1].CurriculumInfomation.startDate);            
            if (left > right)
            {
                curriculumItem.transform.SetSiblingIndex(i);
                break;
            }
        }

        //날짜 순서로 리스트 재정렬
        for (int i = 0; i < curriculumItemList.Count - 1; i++)
        {
            for (int j = i + 1; j < curriculumItemList.Count; j++)
            {
                DateTime left = DateTime.Parse(curriculumItemList[i].CurriculumInfomation.startDate);
                DateTime right = DateTime.Parse(curriculumItemList[j].CurriculumInfomation.startDate);
                if (left > right)
                {
                    CurriculumItem temp = curriculumItemList[i];
                    curriculumItemList[i] = curriculumItemList[j];
                    curriculumItemList[j] = temp;
                }
            }
        }

        CurricululItemIndexReset();
        CloseDateInfoPanelButton();
    }
    #endregion
}
