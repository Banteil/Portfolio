using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnterCurriculumInfoPanel : MonoBehaviour
{
    const int body = 0;
    const int eyes = 1;
    const int top = 2;
    const int hair = 3;
    CurriculumInfo curriculumInfo = new CurriculumInfo();

    public StudyCurriculumPanel studyCurriculumPanel;
    public GameObject teacherInfoPanel;
    public Image[] teacherFaceImage;
    public Text teacherName;
    public InputField startDateInput, startHourInput, startMinuteInput;
    public InputField endDateInput, endHourInput, endMinuteInput;
    public InputField curriculumInfoInput;
    public GameObject moreInfomationPanel;
    public Text addButtonText;

    public GameObject setCurriculumFilePanel;
    public GameObject quizAndTestSettingPanel;

    string teacherGUID;
    int itemIndex;
    bool isModify;

    private void OnEnable()
    {
        if(isModify) StartCoroutine(SettingTeacherInfo());
    }

    public void SetCurriculumInfo(CurriculumInfo info, int index)
    {
        curriculumInfo = info;
        teacherGUID = info.teacherGUID;
        string[] startTime = info.startDate.Split(' ');
        startDateInput.text = startTime[0];
        startHourInput.text = startTime[1].Split(':')[0];
        startMinuteInput.text = startTime[1].Split(':')[1];

        string[] endTime = info.endDate.Split(' ');
        endDateInput.text = endTime[0];
        endHourInput.text = endTime[1].Split(':')[0];
        endMinuteInput.text = endTime[1].Split(':')[1];

        curriculumInfoInput.text = info.description;
        moreInfomationPanel.SetActive(true);
        addButtonText.text = "커리큘럼 수정";
        isModify = true;
        itemIndex = index;
    }

    IEnumerator SettingTeacherInfo()
    {
        teacherInfoPanel.SetActive(true);
        yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.NICKNAME, teacherGUID));
        string name = DataManager.Instance.info;
        yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.AVATAR, teacherGUID));
        string avatarInfo = DataManager.Instance.info;
        teacherName.text = name;
        SetFaceImage(avatarInfo);
    }

    void SetFaceImage(string avatarInfo)
    {
        string[] partsInfo = avatarInfo.Split(',');

        for (int i = 0; i < partsInfo.Length; i++)
        {
            Sprite[] sprites;
            string[] info = partsInfo[i].Split('_');
            string parts = info[0];
            int num = int.Parse(info[1]);

            switch (parts)
            {
                case "h":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Hair/hair_" + num);
                    if (!sprites.Length.Equals(0))
                        teacherFaceImage[hair].sprite = sprites[0];
                    break;
                case "e":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Eye/eye_" + num);
                    if (!sprites.Length.Equals(0))
                        teacherFaceImage[eyes].sprite = sprites[0];
                    break;
                case "t":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Top/top_" + num);
                    if (!sprites.Length.Equals(0))
                        teacherFaceImage[top].sprite = sprites[0];
                    break;
                case "sk":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Body/body_" + num);
                    if (!sprites.Length.Equals(0))
                        teacherFaceImage[body].sprite = sprites[0];
                    break;
                default:
                    continue;
            }
        }
    }

    #region 기본 정보 입력 관련 UI, 기능
    ///<summary>
    ///커리큘럼 아이템 생성 및 리스트 추가
    ///</summary>
    public void AddCurriculumButton()
    {
        if (!CheckExceptionsCurriculum()) return;

        string startDateStr = startDateInput.text + " " + DataManager.Instance.ChangeNumberCharacters(startHourInput.text, 2) +
                ":" + DataManager.Instance.ChangeNumberCharacters(startMinuteInput.text, 2);
        string endDateStr = endDateInput.text + " " + DataManager.Instance.ChangeNumberCharacters(endHourInput.text, 2) +
        ":" + DataManager.Instance.ChangeNumberCharacters(endMinuteInput.text, 2);

        //커리큘럼 아이템 생성 및 정보 세팅
        if (!isModify)
        {
            CurriculumInfo info = new CurriculumInfo();
            //시작 날짜 정보 삽입
            info.startDate = startDateStr;
            //종료 날짜 정보 삽입
            info.endDate = endDateStr;
            //커리큘럼 설명 정보 삽입
            info.description = curriculumInfoInput.text;
            info.teacherGUID = (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"];
            studyCurriculumPanel.CreateCurriculumAndSortList(info);
        }
        else
        {
            curriculumInfo.startDate = startDateStr;
            curriculumInfo.endDate = endDateStr;
            //커리큘럼 설명 정보 삽입
            curriculumInfo.description = curriculumInfoInput.text;
            studyCurriculumPanel.curriculumItemList[itemIndex].CurriculumInfomation = curriculumInfo;
        }

        CloseButton();
    }

    public void CloseButton() => gameObject.SetActive(false);

    ///<summary>
    ///달력 오픈 버튼
    ///</summary>
    public void OpenCalendarButton(InputField selectedInputField)
    {
        CommonInteraction.Instance.calendarPanel.SetActive(true);
        Calendar calendar = CommonInteraction.Instance.calendarPanel.GetComponent<Calendar>();
        calendar._selectedInputField = selectedInputField;
        calendar._dateTime = DateTime.Now;
        calendar.CreateCalendar();
    }      

    #endregion

    public void FileInfoButton(string fileType)
    {
        SetCurriculumFilePanel filePanelScript = setCurriculumFilePanel.GetComponent<SetCurriculumFilePanel>();
        filePanelScript.fileType = fileType;
        filePanelScript.curriculumDate = curriculumInfo.startDate + "~" + curriculumInfo.endDate;
        setCurriculumFilePanel.SetActive(true);
    }

    public void QuizAndNoteTestInfoButton(int type)
    {        
        QuizAndTestSettingPanel quizAndTestScript = quizAndTestSettingPanel.GetComponent<QuizAndTestSettingPanel>();
        string curriculumDate = curriculumInfo.startDate + "~" + curriculumInfo.endDate;
        quizAndTestScript.SetPanelType(type, curriculumDate);
        quizAndTestSettingPanel.SetActive(true);
    }


    void Initialization()
    {
        for (int i = 0; i < teacherFaceImage.Length; i++)
        {
            teacherFaceImage[i].sprite = null;
        }
        teacherName.text = "";
        startDateInput.text = "";
        startHourInput.text = "";
        startMinuteInput.text = "";
        endDateInput.text = "";
        endHourInput.text = "";
        endMinuteInput.text = "";
        curriculumInfoInput.text = "";
        isModify = false;
        teacherInfoPanel.SetActive(false);
        moreInfomationPanel.SetActive(false);
        addButtonText.text = "커리큘럼 추가";        
    }

    #region 유효성 검사
    public bool CheckExceptionsCurriculum()
    {
        DateTime dummy = new DateTime();
        if (startDateInput.text.Equals(""))
        {
            startDateInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("수업 자료의 시작 날짜를 입력해 주세요.\n예시) 2021-01-01");
            return false;
        }
        else
        {
            if (!DateTime.TryParse(startDateInput.text, out dummy))
            {
                startDateInput.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("수업 자료의 시작 날짜를 정확히 입력해 주세요.\n예시) 2021-01-01");
                return false;
            }
            else
            {
                dummy = DateTime.Parse(startDateInput.text);
                if (dummy < DateTime.Today)
                {
                    startDateInput.ActivateInputField();
                    CommonInteraction.Instance.InfoPanelUpdate("지난 날짜를 입력하실 수 없습니다.");
                    return false;
                }
            }
        }

        if (endDateInput.text.Equals(""))
        {
            endDateInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("수업 자료의 종료 날짜를 입력해 주세요.\n예시) 2021-01-01");
            return false;
        }
        else
        {
            if (!DateTime.TryParse(endDateInput.text, out dummy))
            {
                endDateInput.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("수업 자료의 종료 날짜를 정확히 입력해 주세요.\n예시) 2021-01-01");
                return false;
            }
            else
            {
                dummy = DateTime.Parse(endDateInput.text);
                if (dummy < DateTime.Today)
                {
                    endDateInput.ActivateInputField();
                    CommonInteraction.Instance.InfoPanelUpdate("지난 날짜를 입력하실 수 없습니다.");
                    return false;
                }

                DateTime startDummy = DateTime.Parse(startDateInput.text);
                if (dummy < startDummy)
                {
                    endDateInput.ActivateInputField();
                    CommonInteraction.Instance.InfoPanelUpdate("종료일을 시작일보다 이전의 날짜로 입력하실 수 없습니다.");
                    return false;
                }
            }
        }

        if (startHourInput.text.Equals(""))
        {
            startHourInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("수업자료의 시작 시간을 입력해 주세요.");
            return false;
        }

        if (endHourInput.text.Equals(""))
        {
            endHourInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("수업자료의 종료 시간을 입력해 주세요.");
            return false;
        }

        if (startMinuteInput.text.Equals(""))
        {
            startMinuteInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("수업자료의 시작 시간(분)을 입력해 주세요.");
            return false;
        }

        if (endMinuteInput.text.Equals(""))
        {
            endMinuteInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("수업자료의 종료 시간(분)을 입력해 주세요.");
            return false;
        }
        else
        {
            string tempStartDateStr = startDateInput.text + " " + startHourInput.text + ":" + startMinuteInput.text;
            string tempEndDateStr = endDateInput.text + " " + endHourInput.text + ":" + endMinuteInput.text;
            DateTime startTime = DateTime.Parse(tempStartDateStr);
            DateTime endTime = DateTime.Parse(tempEndDateStr);
            if (endTime <= startTime)
            {
                endHourInput.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("시작 시간과 같거나 이전의 시간을 입력할 수 없습니다.");
                return false;
            }
        }

        for (int i = 0; i < studyCurriculumPanel.curriculumItemList.Count; i++)
        {
            if (isModify && i.Equals(itemIndex)) continue;

            string inputStartDateStr = startDateInput.text + " " + DataManager.Instance.ChangeNumberCharacters(startHourInput.text, 2) +
                ":" + DataManager.Instance.ChangeNumberCharacters(startMinuteInput.text, 2);
            string inputEndDateStr = endDateInput.text + " " + DataManager.Instance.ChangeNumberCharacters(endHourInput.text, 2) +
        ":" + DataManager.Instance.ChangeNumberCharacters(endMinuteInput.text, 2);
            DateTime startDate = DateTime.Parse(studyCurriculumPanel.curriculumItemList[i].CurriculumInfomation.startDate);
            DateTime endDate = DateTime.Parse(studyCurriculumPanel.curriculumItemList[i].CurriculumInfomation.endDate);
            DateTime inputStartDate = DateTime.Parse(inputStartDateStr);
            DateTime inputEndDate = DateTime.Parse(inputEndDateStr);
            if (inputStartDate.Equals(startDate) && inputEndDate.Equals(endDate))
            {
                startDateInput.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("이미 동일한 시간의 수업 자료가 있습니다.");
                return false;
            }
            else if (inputStartDate <= startDate && inputEndDate >= startDate)
            {
                startDateInput.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("시간이 겹치는 수업 자료가 있습니다.");
                return false;
            }
            else if (inputStartDate >= startDate && inputEndDate <= endDate)
            {
                startDateInput.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("시간이 겹치는 수업 자료가 있습니다.");
                return false;
            }
            else if (inputStartDate <= endDate && inputEndDate >= endDate)
            {
                startDateInput.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("시간이 겹치는 수업 자료가 있습니다.");
                return false;
            }
        }

        if (curriculumInfoInput.text.Equals(""))
        {
            curriculumInfoInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("커리큘럼 정보의 내용을 입력해 주세요.");
            return false;
        }

        return true;
    }


    ///<summary>
    ///커리큘럼 시 입력란의 숫자를 체크하여 유효한 숫자만 입력되도록 변경
    ///</summary>
    public void CheckExceptionsHour(string type)
    {
        if (type.Equals("START"))
        {
            if (startHourInput.text.Equals("")) return;
            int dummy = int.Parse(startHourInput.text);
            if (dummy < 0) startHourInput.text = "0";
            else if (dummy >= 24) startHourInput.text = "23";
        }
        else
        {
            if (endHourInput.text.Equals("")) return;
            int dummy = int.Parse(endHourInput.text);
            if (dummy < 0) endHourInput.text = "0";
            else if (dummy >= 24) endHourInput.text = "23";
        }
    }

    ///<summary>
    ///커리큘럼 분 입력란의 숫자를 체크하여 유효한 숫자만 입력되도록 변경
    ///</summary>
    public void CheckExceptionsMinute(string type)
    {
        if (type.Equals("START"))
        {
            if (startMinuteInput.text.Equals("")) return;
            int dummy = int.Parse(startMinuteInput.text);
            if (dummy < 0) startMinuteInput.text = "0";
            else if (dummy >= 60) startMinuteInput.text = "59";
        }
        else
        {
            if (endMinuteInput.text.Equals("")) return;
            int dummy = int.Parse(endMinuteInput.text);
            if (dummy < 0) endMinuteInput.text = "0";
            else if (dummy >= 60) endMinuteInput.text = "59";
        }
    }
    #endregion

    private void OnDisable()
    {
        Initialization();
    }
}
