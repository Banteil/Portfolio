using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RulePanel : MonoBehaviour
{
    public InputField tardyCountInput, absentCountInput, assignmentCountInput;
    public Dropdown tardyPenaltyDropdown, absentPenaltyDropdown, assignmentPenaltyDropdown;
    public GameObject saveButtonObject;

    bool isFirstEnable = true;

    private void OnEnable()
    {
        if(isFirstEnable)
        {
            StudyPreparation.Instance.interactFunc += Initialization;
            SettingRulesInfo();
            isFirstEnable = false;
        }
    }

    void SettingRulesInfo()
    {
        CommonInteraction.Instance.StartLoding();
        if (!StudyPreparation.Instance.powerToEdit[2])
            NonEditableProcess();

        string[] tardyRules = StudyPreparation.Instance.studyData.rules["Tardy"].Split(',');
        tardyCountInput.text = tardyRules[0];
        tardyPenaltyDropdown.value = int.Parse(tardyRules[1]);

        string[] absentRules = StudyPreparation.Instance.studyData.rules["Absent"].Split(',');
        absentCountInput.text = absentRules[0];
        absentPenaltyDropdown.value = int.Parse(absentRules[1]);

        string[] assignmentRules = StudyPreparation.Instance.studyData.rules["Assignment"].Split(',');
        assignmentCountInput.text = assignmentRules[0];
        assignmentPenaltyDropdown.value = int.Parse(assignmentRules[1]);

        CommonInteraction.Instance.isLoading = false;
    }

    ///<summary>
    ///수정 권한 없을 때 처리
    ///</summary>
    void NonEditableProcess()
    {
        tardyCountInput.interactable = false;
        absentCountInput.interactable = false;
        assignmentCountInput.interactable = false;
        tardyPenaltyDropdown.interactable = false;
        absentPenaltyDropdown.interactable = false;
        assignmentPenaltyDropdown.interactable = false;

        saveButtonObject.GetComponent<Button>().interactable = false;
        saveButtonObject.transform.GetChild(0).GetComponent<Text>().text = "수업 자료의 수정 권한이 없습니다.";
    }

    public void ModifyCompleteButton() => StartCoroutine(ModifyCompleteProcess());

    IEnumerator ModifyCompleteProcess()
    {
        if (!CheckException()) yield break;

        StudyInfoData tempData = StudyPreparation.Instance.studyData;
        string tardy = tardyCountInput.text + "," + tardyPenaltyDropdown.value;
        tempData.rules["Tardy"] = tardy;
        string absent = absentCountInput.text + "," + absentPenaltyDropdown.value;
        tempData.rules["Absent"] = absent;
        string assignment = assignmentCountInput.text + "," + assignmentPenaltyDropdown.value;
        tempData.rules["Assignment"] = assignment;

        yield return StartCoroutine(DataManager.Instance.UpdateStudyRegistration(tempData));
        if (DataManager.Instance.info.Equals("SUCCESS"))
        {
            CommonInteraction.Instance.InfoPanelUpdate("수정된 학습 내용이 저장되었습니다.");
            StudyPreparation.Instance.studyData = tempData;
        }
        else
        {
            CommonInteraction.Instance.InfoPanelUpdate("학습 내용 수정 과정에 문제가 발생하였습니다.\n다시 시도해 주세요.");
        }
    }

    void Initialization()
    {
        tardyCountInput.text = "";
        absentCountInput.text = "";
        assignmentCountInput.text = "";
        tardyPenaltyDropdown.value = 0;
        absentPenaltyDropdown.value = 0;
        assignmentPenaltyDropdown.value = 0;
        tardyCountInput.interactable = true;
        absentCountInput.interactable = true;
        assignmentCountInput.interactable = true;
        tardyPenaltyDropdown.interactable = true;
        absentPenaltyDropdown.interactable = true;
        assignmentPenaltyDropdown.interactable = true;

        saveButtonObject.GetComponent<Button>().interactable = true;
        saveButtonObject.transform.GetChild(0).GetComponent<Text>().text = "수정한 내용 저장";
        isFirstEnable = true;        
    }

    #region 유효성 검사
    public bool CheckException()
    {
        if (tardyCountInput.text.Equals(""))
        {
            tardyCountInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("지각 규칙의 횟수를 입력해 주세요.");
            return false;
        }

        if (absentCountInput.text.Equals(""))
        {
            absentCountInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("결석 규칙의 횟수를 입력해 주세요.");
            return false;
        }

        if (assignmentCountInput.text.Equals(""))
        {
            assignmentCountInput.ActivateInputField();
            CommonInteraction.Instance.InfoPanelUpdate("과제 규칙의 횟수를 입력해 주세요.");
            return false;
        }

        return true;
    }

    ///<summary>
    ///0 이하의 숫자 입력을 방지하여 유효한 숫자만 입력되도록 변경
    ///</summary>
    public void CheckExceptionsCount(InputField input)
    {
        if (input.text.Equals("")) return;
        int dummy = int.Parse(input.text);
        if (dummy <= 0) input.text = "1";
    }
    #endregion
}
