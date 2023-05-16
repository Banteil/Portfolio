using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QualificationRequirementsPanel : MonoBehaviour
{
    GameObject questionItem;
    [Header("Question")]
    public Transform questionContent;
    public InputField questionInput;
    public Button addQuestionButton;
    List<QualificationQuestionInputField> questionInputList = new List<QualificationQuestionInputField>();
    [Header("Evidence")]
    public InputField evidenceInput;
    [Header("Gender")]
    public Dropdown genderInfoDropdown;
    [Header("InfoControl")]
    public GameObject saveButtonObject;
    bool isFirstEnable = true;

    private void Awake()
    {
        questionItem = Resources.Load<GameObject>("Prefabs/UI/QualificationQuestionInputField");
    }

    private void OnEnable()
    {
        saveButtonObject.GetComponent<Button>().onClick.RemoveAllListeners();
        saveButtonObject.GetComponent<Button>().onClick.AddListener(ModifyCompleteButton);
        if (isFirstEnable)
        {
            StudyPreparation.Instance.interactFunc += Initialization;
            SettingRequirementsInfo();
            isFirstEnable = false;
        }
    }

    void SettingRequirementsInfo()
    {
        CommonInteraction.Instance.StartLoding();
        if (!StudyPreparation.Instance.powerToEdit[1])
            NonEditableProcess();

        //자격요건 관련 정보 세팅
        if (!StudyPreparation.Instance.studyData.eligibilityRequirements["Question"].Equals(""))
        {
            string[] questions = StudyPreparation.Instance.studyData.eligibilityRequirements["Question"].Split(',');
            for (int i = 0; i < questions.Length; i++)
            {
                CreateQuestionInputItem(questions[i]);
            }
        }
        evidenceInput.text = StudyPreparation.Instance.studyData.eligibilityRequirements["Evidence"];
        string genderInfo = StudyPreparation.Instance.studyData.eligibilityRequirements["Gender"];
        switch(genderInfo)
        {
            case "남성":
                genderInfoDropdown.value = 1;
                break;
            case "여성":
                genderInfoDropdown.value = 2;
                break;
            default:
                genderInfoDropdown.value = 0;
                break;
        }
        
        CommonInteraction.Instance.isLoading = false;
    }

    void NonEditableProcess()
    {
        questionInput.interactable = false;
        evidenceInput.interactable = false;
        genderInfoDropdown.interactable = false;
        addQuestionButton.interactable = false;

        saveButtonObject.GetComponent<Button>().interactable = false;
        saveButtonObject.transform.GetChild(0).GetComponent<Text>().text = "학습 내용의 수정 권한이 없습니다.";
    }

    void CreateQuestionInputItem(string content)
    {
        GameObject item = Instantiate(questionItem, questionContent, false);
        QualificationQuestionInputField itemScript = item.GetComponent<QualificationQuestionInputField>();
        itemScript.Index = questionInputList.Count;
        itemScript.contentText.text = content;
        itemScript.deleteFunction = DeleteQuestionList;
        if (!StudyPreparation.Instance.powerToEdit[1])
            itemScript.contentText.interactable = false;

        questionInputList.Add(itemScript);
    }

    #region 자격 요건 관련 기능 및 UI
    public void AddQuestionButton()
    {
        if (!CheckExceptions()) return;

        CreateQuestionInputItem(questionInput.text);
        questionInput.text = "";
    }

    void DeleteQuestionList(int index)
    {
        questionInputList.RemoveAt(index);
        for (int i = index; i < questionInputList.Count; i++)
        {
            questionInputList[i].Index = i;
        }
    }
    #endregion

    public void ModifyCompleteButton() => StartCoroutine(ModifyCompleteProcess());

    IEnumerator ModifyCompleteProcess()
    {
        StudyInfoData tempData = StudyPreparation.Instance.studyData;

        string questions = "";
        for (int i = 0; i < questionInputList.Count; i++)
        {
            questions += questionInputList[i].contentText.text;
            if (i < questionInputList.Count - 1)
                questions += ",";
        }
        tempData.eligibilityRequirements["Question"] = questions;
        tempData.eligibilityRequirements["Evidence"] = evidenceInput.text;
        switch (genderInfoDropdown.value)
        {
            case 0:
                tempData.eligibilityRequirements["Gender"] = "성별 무관";
                break;
            case 1:
                tempData.eligibilityRequirements["Gender"] = "남성";
                break;
            case 2:
                tempData.eligibilityRequirements["Gender"] = "여성";
                break;
        }

        yield return StartCoroutine(DataManager.Instance.UpdateStudyRegistration(tempData));
        if (DataManager.Instance.info.Equals("SUCCESS"))
        {
            CommonInteraction.Instance.InfoPanelUpdate("수정된 자격 요건이 저장되었습니다.");
            StudyPreparation.Instance.studyData = tempData;
        }
        else
        {
            CommonInteraction.Instance.InfoPanelUpdate("자격 요건 수정 과정에 문제가 발생하였습니다.\n다시 시도해 주세요.");
        }
    }

    void Initialization()
    {
        questionInput.interactable = true;
        questionInput.text = "";
        evidenceInput.interactable = true;
        evidenceInput.text = "";
        genderInfoDropdown.interactable = true;
        genderInfoDropdown.value = 0;
        addQuestionButton.interactable = true;
        questionInputList = new List<QualificationQuestionInputField>();

        for (int i = 0; i < questionContent.childCount; i++)
        {
            Destroy(questionContent.GetChild(i).gameObject);
        }

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
        if (questionInput.text.Equals(""))
        {
            CommonInteraction.Instance.InfoPanelUpdate("질문 내용을 입력해 주셔야 합니다.");
            return false;
        }

        return true;
    }

    #endregion
}
