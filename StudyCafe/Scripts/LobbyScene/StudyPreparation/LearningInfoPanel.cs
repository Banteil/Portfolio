using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LearningInfoPanel : MonoBehaviour
{
    public InputField studyName;
    public InputField studySubject;
    public InputField startObjectiveDate, endObjectiveDate, objectiveTimeInput, objectiveCountInput;
    public Dropdown objectiveTimeSelectDropdown;    

    public GameObject saveButtonObject;
    bool isFirstEnable = true;

    private void OnEnable()
    {
        saveButtonObject.GetComponent<Button>().onClick.RemoveAllListeners();
        saveButtonObject.GetComponent<Button>().onClick.AddListener(ModifyCompleteButton);
        if (isFirstEnable)
        {
            StudyPreparation.Instance.interactFunc += Initialization;
            SettingLearningInfo();
            isFirstEnable = false;
        }
    }

    void SettingLearningInfo()
    {
        if (!StudyPreparation.Instance.powerToEdit[1])
            NonEditableProcess();
        //학습 관련 정보 세팅
        studyName.text = StudyPreparation.Instance.studyData.studyName;
        studySubject.text = StudyPreparation.Instance.studyData.subject;
        string[] splitObjective = StudyPreparation.Instance.studyData.objectives.Split(',');
        startObjectiveDate.text = splitObjective[0];
        endObjectiveDate.text = splitObjective[1];
        objectiveTimeInput.text = splitObjective[2];
        objectiveCountInput.text = splitObjective[4];
        objectiveTimeSelectDropdown.value = int.Parse(splitObjective[3]);
    }

    void NonEditableProcess()
    {
        studyName.interactable = false;
        studySubject.interactable = false;
        startObjectiveDate.interactable = false;
        endObjectiveDate.interactable = false;
        objectiveTimeInput.interactable = false;
        objectiveCountInput.interactable = false;
        objectiveTimeSelectDropdown.interactable = false;

        saveButtonObject.GetComponent<Button>().interactable = false;
        saveButtonObject.transform.GetChild(0).GetComponent<Text>().text = "학습 내용의 수정 권한이 없습니다.";
    }

    public void OpenCalendarButton(InputField selectedInputField)
    {
        CommonInteraction.Instance.calendarPanel.SetActive(true);
        Calendar calendar = CommonInteraction.Instance.calendarPanel.GetComponent<Calendar>();
        calendar._selectedInputField = selectedInputField;
        calendar._dateTime = DateTime.Now;
        calendar.CreateCalendar();
    }

    public void ModifyCompleteButton() => StartCoroutine(ModifyCompleteProcess());

    IEnumerator ModifyCompleteProcess()
    {
        if (!CheckExceptions()) yield break;

        StudyInfoData tempData = StudyPreparation.Instance.studyData;
        tempData.studyName = studyName.text;
        tempData.subject = studySubject.text;
        string objectiveStr = startObjectiveDate.text + "," + endObjectiveDate.text + "," + objectiveTimeInput.text + "," + objectiveTimeSelectDropdown.value.ToString()
            + "," + objectiveCountInput.text;
        tempData.objectives = objectiveStr;

        yield return StartCoroutine(DataManager.Instance.UpdateStudyRegistration(tempData));
        if (DataManager.Instance.info.Equals("SUCCESS"))
        {
            CommonInteraction.Instance.InfoPanelUpdate("수정된 스터디 정보가 저장되었습니다.");
            StudyPreparation.Instance.studyData = tempData;
        }
        else
        {
            CommonInteraction.Instance.InfoPanelUpdate("스터디 정보 수정 과정에 문제가 발생하였습니다.\n다시 시도해 주세요.");
        }
    }

    void Initialization()
    {
        studyName.interactable = true;
        studyName.text = "";
        studySubject.interactable = true;
        studySubject.text = "";
        startObjectiveDate.interactable = true;
        startObjectiveDate.text = "";
        endObjectiveDate.interactable = true;
        endObjectiveDate.text = "";
        objectiveTimeInput.interactable = true;
        objectiveTimeInput.text = "";
        objectiveCountInput.interactable = true;
        objectiveCountInput.text = "";
        objectiveTimeSelectDropdown.interactable = true;
        objectiveTimeSelectDropdown.value = 0;

        saveButtonObject.GetComponent<Button>().interactable = true;
        saveButtonObject.transform.GetChild(0).GetComponent<Text>().text = "수정한 내용 저장";
        isFirstEnable = true;        
    }

    #region 유효성 검사 함수
    ///<summary>
    ///스터디 생성 시도 시 유효성 검사 함수
    ///</summary>
    bool CheckExceptions()
    {
        DateTime dummy = new DateTime();

        if (studyName.text.Equals(""))
        {
            studyName.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("스터디 이름을 입력해 주세요.");
            return false;
        }

        if (studySubject.text.Equals(""))
        {
            studySubject.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("스터디 주제를 입력해 주세요.");
            return false;
        }

        if (startObjectiveDate.text.Equals(""))
        {
            startObjectiveDate.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("스터디 목표의 시작 날짜를 입력해 주세요.\n예시) 2021-01-01");
            return false;
        }
        else
        {
            if (!DateTime.TryParse(startObjectiveDate.text, out dummy))
            {
                startObjectiveDate.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("스터디 목표의 시작 날짜를 정확히 입력해 주세요.\n예시) 2021-01-01");
                return false;
            }
            else
            {
                dummy = DateTime.Parse(startObjectiveDate.text);
                if (dummy < DateTime.Today)
                {
                    startObjectiveDate.ActivateInputField();
                    CommonInteraction.Instance.InfoPanelUpdate("지난 날짜를 입력하실 수 없습니다.");
                    return false;
                }
            }
        }

        if (endObjectiveDate.text.Equals(""))
        {
            endObjectiveDate.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("스터디 목표의 종료 날짜를 입력해 주세요.\n예시) 2021-01-02");
            return false;
        }
        else
        {
            if (!DateTime.TryParse(endObjectiveDate.text, out dummy))
            {
                endObjectiveDate.ActivateInputField();
                CommonInteraction.Instance.InfoPanelUpdate("스터디 목표의 종료 날짜를 정확히 입력해 주세요.\n예시) 2021-01-02");
                return false;
            }
            else
            {
                dummy = DateTime.Parse(endObjectiveDate.text);
                if (dummy < DateTime.Today)
                {
                    endObjectiveDate.ActivateInputField();
                    CommonInteraction.Instance.InfoPanelUpdate("지난 날짜를 입력하실 수 없습니다.");
                    return false;
                }

                DateTime startDummy = DateTime.Parse(startObjectiveDate.text);
                if (dummy < startDummy)
                {
                    endObjectiveDate.ActivateInputField();
                    CommonInteraction.Instance.InfoPanelUpdate("종료일을 시작일보다 이전의 날짜로 입력하실 수 없습니다.");
                    return false;
                }
            }
        }

        if (objectiveTimeInput.text.Equals(""))
        {
            objectiveTimeInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("스터디 목표의 1회 평균 스터디 시간을 입력해 주세요.");
            return false;
        }

        if (objectiveCountInput.text.Equals(""))
        {
            objectiveCountInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("스터디 목표의 총 스터디 횟수를 입력해 주세요.");
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
    ///0 이하의 숫자 입력을 방지하여 유효한 숫자만 입력되도록 변경
    ///</summary>
    public void CheckExceptionsCount()
    {
        if (objectiveCountInput.text.Equals("")) return;
        int dummy = int.Parse(objectiveCountInput.text);
        if (dummy <= 0) objectiveCountInput.text = "1";
    }

    #endregion
}
