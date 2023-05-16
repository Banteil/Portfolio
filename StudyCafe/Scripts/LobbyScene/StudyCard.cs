using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StudyCard : MonoBehaviour
{
    [Header("Card UI")]
    public Text studyNameText;
    public Text studySubjectText;
    public Text curriculumInfoText;
    public Dropdown dateDropdown;
    public GameObject ribbonImage;
    public GameObject deleteButtonObject;
    public GameObject modifyButtonObject;
    public GameObject unsubscribeButtonObject;
    public GameObject assignmentButtonObject;

    [Header("Card Data")]
    //커리큘럼 설명 정보 리스트
    List<string> curriculumInfoList = new List<string>();
    StudyInfoData studyData;    
    /// <summary>
    /// 카드에 저장될 스터디 정보, 저장과 동시에 정보 세팅
    /// </summary>
    public StudyInfoData StudyData
    {
        get { return studyData; }
        set
        {
            studyData = value;
            //표기용 텍스트와 필터 정보에 이름, 주제 정보 삽입
            studyNameText.text = studyData.studyName;
            studyNameFilter = studyData.studyName;
            studySubjectText.text = studyData.subject;
            studySubjectFilter = studyData.subject;
            dateDropdown.ClearOptions();
            //스터디의 커리큘럼 리스트 전체 체크
            for (int i = 0; i < studyData.curriculumInfoList.Count; i++)
            {
                //커리큘럼 드롭다운 초기화
                string curriculumStr = studyData.curriculumInfoList[i].startDate + "\n~\n" + studyData.curriculumInfoList[i].endDate;
                Dropdown.OptionData option = new Dropdown.OptionData();
                option.text = curriculumStr;
                dateDropdown.options.Add(option);
                //커리큘럼 설명 정보 리스트에 추가
                curriculumInfoList.Add(studyData.curriculumInfoList[i].description);
                //필터 정보 추가 프로세스
                string[] startDateToTime = studyData.curriculumInfoList[i].startDate.Split(' ');
                string[] endDateToTime = studyData.curriculumInfoList[i].endDate.Split(' ');
                string[] startDate = startDateToTime[0].Split('-');
                string[] endDate = endDateToTime[0].Split('-');
                for (int j = 0; j < startDate.Length; j++)
                {
                    AddFilterList(j, startDate[j]);
                    AddFilterList(j, endDate[j]);
                }                
            }            

            //최근 날짜에 가장 가까운 커리큘럼 표시
            for (int i = 0; i < dateDropdown.options.Count; i++)
            {
                string[] splitDate = dateDropdown.options[i].text.Split('~');
                DateTime endDate = DateTime.Parse(splitDate[1]);
                if (endDate > DateTime.Now)                    
                {
                    //커리큘럼 설명 추가
                    curriculumInfoText.text = curriculumInfoList[i];
                    dateDropdown.captionText.text = dateDropdown.options[i].text;
                    dateDropdown.value = i;
                    break;
                }
            }            
        }
    }

    [Header("Filter information")]
    [HideInInspector]
    public string studyNameFilter;
    [HideInInspector]
    public string studySubjectFilter;
    [HideInInspector]
    public List<string> yearFilterList = new List<string>();
    [HideInInspector]
    public List<string> monthFilterList = new List<string>();
    [HideInInspector]
    public List<string> dayFilterList = new List<string>();
    bool isMine;
    /// <summary>
    /// 내가 만든 스터디인지 여부 체크용 bool, true로 체크 시 카드에 리본이 표시됨
    /// </summary>
    public bool IsMine
    {
        get { return isMine; }
        set
        {
            isMine = value;
            if (isMine) 
                ribbonImage.SetActive(true);
            else
            {
                deleteButtonObject.SetActive(false);
                unsubscribeButtonObject.SetActive(true);
            }
        }
    }

    //공고문 용 스터디 카드인지 여부 판단
    bool isAnnouncement;
    public bool IsAnnouncement
    {
        get { return isAnnouncement; }
        set
        {
            isAnnouncement = value;
            if (isAnnouncement)
            {
                modifyButtonObject.SetActive(false);
                deleteButtonObject.SetActive(false);
                curriculumInfoText.gameObject.SetActive(false);
                dateDropdown.gameObject.SetActive(false);
            }
            else
            {
                modifyButtonObject.SetActive(true);
                deleteButtonObject.SetActive(true);
                curriculumInfoText.gameObject.SetActive(true);
                dateDropdown.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 특정 커리큘럼 선택 시 해당 커리큘럼 설명이 표시되도록 하는 함수
    /// </summary>
    public void SetCurriculumDropdownValue(Dropdown dropdown)
    {
        curriculumInfoText.text = curriculumInfoList[dropdown.value];
    }

    /// <summary>
    /// 필터 리스트에 중복된 데이터 없이 정보를 삽입하는 함수
    /// </summary>
    void AddFilterList(int type, string contents)
    {
        switch(type)
        {
            case 0:
                {
                    if (yearFilterList.Count.Equals(0))
                        yearFilterList.Add(contents);
                    else
                    {
                        bool overlap = false;
                        for (int i = 0; i < yearFilterList.Count; i++)
                        {
                            if(yearFilterList[i].Equals(contents))
                            {
                                overlap = true;
                                break;
                            }
                        }
                        if (!overlap)
                            yearFilterList.Add(contents);
                    }
                }
                break;
            case 1:
                {
                    if (monthFilterList.Count.Equals(0))
                        monthFilterList.Add(contents);
                    else
                    {
                        bool overlap = false;
                        for (int i = 0; i < monthFilterList.Count; i++)
                        {
                            if (monthFilterList[i].Equals(contents))
                            {
                                overlap = true;
                                break;
                            }
                        }
                        if (!overlap)
                            monthFilterList.Add(contents);
                    }
                }
                break;
            case 2:
                {
                    if (dayFilterList.Count.Equals(0))
                        dayFilterList.Add(contents);
                    else
                    {
                        bool overlap = false;
                        for (int i = 0; i < dayFilterList.Count; i++)
                        {
                            if (dayFilterList[i].Equals(contents))
                            {
                                overlap = true;
                                break;
                            }
                        }
                        if (!overlap)
                            dayFilterList.Add(contents);
                    }
                }
                break;
        }
    }

    #region 스터디 카드 UI 조작
    /// <summary>
    /// 준비된 스터디 실행<br /> 
    /// 카드에 저장된 StudyInfoData 정보를 토대로 Photon Room Option 설정<br /> 
    /// 룸 커스텀 프로퍼티 정보 hash = Master, GUID, Token, Type, Code, DisplayName, EMail, CurriculumDate, IsReady
    /// </summary>
    public void StudyOpenButton()
    {
        if (!isAnnouncement)
        {
            //string[] dateStr = dateDropdown.options[dateDropdown.value].text.Split('~');
            //해당 스터디 커리큘럼의 티처일 때 스터디 실행 여부를 물어봄, 이외에는 입장 여부만 물어봄
            CommonInteraction.Instance.ConfirmPanelUpdate("'" + studyNameText.text + "' 스터디 룸에 입장하시겠습니까?");
            CommonInteraction.Instance.confirmFunc = StudyOpenCheck;
        }
        else
        {
            StudyRecruitment.Instance.OpenAnnouncementInfoPanel(studyData);
        }
    }

    void StudyOpenCheck(bool check)
    {
        if (check) StartCoroutine(StudyOpenProcess());
    }

    IEnumerator StudyOpenProcess()
    {
        ///////////////////////////////////////////필요 정보 세팅
        DataManager.interactionData.studyGUID = studyData.guid;
        string curriculumDate = studyData.curriculumInfoList[dateDropdown.value].startDate + "~" + studyData.curriculumInfoList[dateDropdown.value].endDate;
        DataManager.interactionData.curriculumDate = curriculumDate;
        Guid token = Guid.NewGuid();

        ///////////////////////////////////////////////////////////////
        //////////////////////////////////////룸 커스텀 프로퍼티 정보 대입
        Hashtable hash = new Hashtable();
        hash.Add("Teacher", studyData.curriculumInfoList[dateDropdown.value].teacherGUID);
        hash.Add("StudyGUID", studyData.guid);
        hash.Add("Token", token.ToString());
        hash.Add("Code", studyData.code);
        hash.Add("DisplayName", studyData.studyName);
        hash.Add("CurriculumDate", curriculumDate);
        hash.Add("IsReady", false);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.CustomRoomPropertiesForLobby = new string[]
        {
            "Teacher", "StudyGUID", "Code", "DisplayName", "CurriculumDate", "IsReady"
        };
        roomOptions.CustomRoomProperties = hash;
        ///////////////////////////////////////////////////////////        
        yield return StartCoroutine(DataManager.Instance.SetAttendance(curriculumDate));
        //JoinOrCreateRoom을 위해 룸 옵션, 코드명을 loadData에 저장
        DataManager.loadData.roomOptions = roomOptions;
        DataManager.loadData.code = studyData.code;
        //오픈,참가하는 스터디 정보를 DataManager에 저장
        DataManager.Instance.currentStudyData = studyData;

        yield return StartCoroutine(CommonInteraction.Instance.FadeOut(1f));
        LobbyManager.Instance.myStudyListCanvas.SetActive(false);   
        LobbyManager.Instance.purpose = PurposeLeavingLounge.STUDYSTART;        
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// 스터디 삭제 버튼
    /// </summary>
    public void DeleteStudyButton()
    {
        CommonInteraction.Instance.ConfirmPanelUpdate("'" + studyNameText.text + "' 스터디를 정말로 삭제하시겠습니까?\n(삭제 시 스터디와 관련된 모든 정보, 자료는 삭제되며 복구가 불가능합니다.)");
        CommonInteraction.Instance.confirmFunc = DeleteCheckFunction;
    }

    void DeleteCheckFunction(bool check)
    {
        if (check) StartCoroutine(DeleteStudyProcess());        
    }

    IEnumerator DeleteStudyProcess()
    {
        yield return StartCoroutine(DataManager.Instance.StudyRegistration(studyData.guid));
        for (int i = 0; i < MyStudyManagement.Instance.studyCardList.Count; i++)
        {
            if (MyStudyManagement.Instance.studyCardList[i].Equals(this))
            {
                MyStudyManagement.Instance.studyCardList.RemoveAt(i);
                break;
            }
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// 스터디 정보 조회 버튼
    /// </summary>
    public void StudyInfoButton()
    {
        MyStudyManagement.Instance.StartStudyInfoView(this);
    }

    /// <summary>
    /// 스터디 멤버 탈퇴 버튼
    /// </summary>
    public void UnsubscribeButton()
    {
        CommonInteraction.Instance.ConfirmPanelUpdate("'" + studyNameText.text + "' 스터디에서 탈퇴하시겠습니까?\n(탈퇴 시 해당 스터디의 멤버가 모두 동의하기 전까지 재가입은 불가능합니다.)");
        CommonInteraction.Instance.confirmFunc = UnsubscribeCheck;
    }

    void UnsubscribeCheck(bool check)
    {
        if(check)
        {
            StartCoroutine(UnsubscribeProcess());
        }
    }

    IEnumerator UnsubscribeProcess()
    {
        string memberGUID = (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"];
        yield return StartCoroutine(DataManager.Instance.DeleteMemberInfo(studyData.guid, memberGUID));
        if(DataManager.Instance.info.Equals("SUCCESS"))
        {
            CommonInteraction.Instance.InfoPanelUpdate("'" + studyData.studyName + "' 스터디 멤버에서 탈퇴하였습니다.");
            Destroy(gameObject);
        }
        else
            CommonInteraction.Instance.InfoPanelUpdate("멤버 탈퇴에 실패하셨습니다.\n다시 시도해 주세요.");
    }

    /// <summary>
    /// 스터디 과제 제출 버튼
    /// </summary>
    public void AssignmentButton()
    {
        MyStudyManagement.Instance.StartAssignmentUpload(this);
    }

    #endregion
}
