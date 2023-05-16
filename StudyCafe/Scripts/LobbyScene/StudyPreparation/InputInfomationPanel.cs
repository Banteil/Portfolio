using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputInfomationPanel : MonoBehaviour
{
    [Header("InputInfomationPanel")]
    public Text selectedTypeText;
    public InputField questionInputField;

    public GameObject examplePanel;
    public InputField[] exampleItems;
    public Dropdown exampleValueDropdown;

    public InputField answerInputField;
    public Dropdown answerDropdown;

    public InputField hintInputField;

    [Header("ItemListPanel")]
    public QuizAndTestSettingPanel settingPanel;
    public GameObject itemListPanel;

    int panelType; //퀴즈인지, 쪽지시험인지
    string type; //객,주,O 구분
    string curriculumDate;

    //수정 시 필요 정보
    public bool isModify;
    string guid;
    int index;

    public void SetInputType(int panelType, string type, string curriculumDate)
    {
        this.panelType = panelType;
        this.curriculumDate = curriculumDate;
        if (panelType.Equals(0)) hintInputField.gameObject.SetActive(true);
        this.type = type;
        switch (type)
        {
            case "MultipleChoice":
                {
                    selectedTypeText.text = "객관식";
                    examplePanel.SetActive(true);
                    answerDropdown.ClearOptions();
                    for (int i = 0; i < exampleItems.Length; i++)
                    {
                        if (i <= 1) exampleItems[i].gameObject.SetActive(true);
                        else exampleItems[i].gameObject.SetActive(false);
                    }

                    for (int i = 0; i < exampleValueDropdown.value + 2; i++)
                    {
                        Dropdown.OptionData option = new Dropdown.OptionData();
                        option.text = (i + 1).ToString() + "번";
                        answerDropdown.options.Add(option);
                        exampleItems[i].gameObject.SetActive(true);
                    }
                    answerDropdown.value = 0;
                    answerDropdown.gameObject.SetActive(true);
                }
                break;
            case "ShortAnswer":
                {
                    selectedTypeText.text = "주관식";
                    answerInputField.gameObject.SetActive(true);
                }
                break;
            case "OX":
                {
                    selectedTypeText.text = "OX 퀴즈";
                    answerDropdown.ClearOptions();
                    for (int i = 0; i < 2; i++)
                    {
                        Dropdown.OptionData option = new Dropdown.OptionData();
                        if(i.Equals(0))                            
                            option.text = "O";
                        else
                            option.text = "X";
                        answerDropdown.options.Add(option);
                    }
                    answerDropdown.value = 0;
                    answerDropdown.gameObject.SetActive(true);
                }
                break;
        }
    }

    public void ModifyInputType(int panelType, string type, string curriculumDate, string guid, int index, QuizAndTestInfo info)
    {
        isModify = true;
        this.panelType = panelType;
        if (panelType.Equals(0))
        {
            hintInputField.text = info.hint;
            hintInputField.gameObject.SetActive(true);
        }
        this.curriculumDate = curriculumDate;
        this.guid = guid;
        this.type = type;
        this.index = index;

        questionInputField.text = info.question;
        switch (type)
        {
            case "MultipleChoice":
                {
                    selectedTypeText.text = "객관식";
                    examplePanel.SetActive(true);
                    answerDropdown.ClearOptions();
                    string[] options = info.options.Split(',');
                    for (int i = 0; i < options.Length; i++)
                    {
                        exampleItems[i].text = options[i];
                        Dropdown.OptionData option = new Dropdown.OptionData();
                        option.text = (i + 1).ToString() + "번";
                        if (option.text.Equals(info.answer))
                        {
                            answerDropdown.captionText.text = info.answer;
                            answerDropdown.value = i;
                        }
                        answerDropdown.options.Add(option);
                        exampleItems[i].gameObject.SetActive(true);                        
                    }

                    answerDropdown.gameObject.SetActive(true);
                }
                break;
            case "ShortAnswer":
                {
                    selectedTypeText.text = "주관식";
                    answerInputField.text = info.answer;
                    answerInputField.gameObject.SetActive(true);
                }
                break;
            case "OX":
                {
                    selectedTypeText.text = "OX 퀴즈";
                    answerDropdown.ClearOptions();
                    for (int i = 0; i < 2; i++)
                    {
                        Dropdown.OptionData option = new Dropdown.OptionData();
                        if (i.Equals(0))
                            option.text = "O";
                        else
                            option.text = "X";
                        if (option.text.Equals(info.answer))
                        {
                            answerDropdown.captionText.text = info.answer;
                            answerDropdown.value = i;
                        }
                        answerDropdown.options.Add(option);
                    }
                    answerDropdown.gameObject.SetActive(true);
                }
                break;
        }
    }

    #region 정보 기입 패널 관련
    public void CancelButton()
    {
        itemListPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    public void SaveButton()
    {
        StartCoroutine(SetQuizOrTestInfoProcess(SettingQuizAndTestInfo()));
    }

    QuizAndTestInfo SettingQuizAndTestInfo()
    {
        QuizAndTestInfo info = new QuizAndTestInfo();
        info.question = questionInputField.text;
        switch (type)
        {
            case "MultipleChoice":
                {
                    string options = "";
                    for (int i = 0; i < exampleItems.Length; i++)
                    {
                        if (!exampleItems[i].gameObject.activeSelf)
                        {
                            options = options.Substring(0, options.Length - 1);
                            break;
                        }
                        options += exampleItems[i].text;
                        if (i < exampleItems.Length - 1)
                            options += ",";
                    }
                    info.options = options;
                    info.answer = answerDropdown.options[answerDropdown.value].text;
                }
                break;
            case "ShortAnswer":
                {
                    info.answer = answerInputField.text;
                }
                break;
            case "OX":
                {
                    info.answer = answerDropdown.options[answerDropdown.value].text;
                }
                break;
        }
        info.hint = hintInputField.text;
        return info;
    }

    IEnumerator SetQuizOrTestInfoProcess(QuizAndTestInfo info)
    {
        string jsonData = JsonConvert.SerializeObject(info);
        string categoryName = "";
        if (panelType.Equals(0))
            categoryName = "퀴즈";
        else
            categoryName = "쪽지 시험";
        string studyGUID = StudyPreparation.Instance.studyData.guid;
        //새로 생성일 시
        if (!isModify)
        {
            Guid guid = Guid.NewGuid();
            yield return StartCoroutine(DataManager.Instance.SetQuizOrTestInfo(panelType, guid.ToString(), studyGUID, curriculumDate, type, jsonData));
            if (DataManager.Instance.info.Equals("SUCCESS"))
            {
                settingPanel.CreateQuizAndTestItem(info, type, guid.ToString());
                CommonInteraction.Instance.InfoPanelUpdate(categoryName + " 추가 완료!");
                CancelButton();
            }
            else
            {
                CommonInteraction.Instance.InfoPanelUpdate(categoryName + " 추가에 실패하였습니다.\n다시 시도해 주세요.");
                yield break;
            }
        }
        //수정일 시
        else
        {
            yield return StartCoroutine(DataManager.Instance.UpdateQuizOrTestInfo(panelType, guid, jsonData));
            if (DataManager.Instance.info.Equals("SUCCESS"))
            {
                settingPanel.ModifyItemInfo(info, type, index);
                CommonInteraction.Instance.InfoPanelUpdate(categoryName + " 수정 완료!");
                CancelButton();
            }
            else
            {
                CommonInteraction.Instance.InfoPanelUpdate(categoryName + " 수정에 실패하였습니다.\n다시 시도해 주세요.");
                yield break;
            }
        }
    }

    ///<summary>
    ///보기 개수 드롭다운 조절 시 값에 맞게 인풋 필드를 활성화시키는 함수
    ///</summary>
    public void ExampleValueChanged()
    {
        int maxValue = exampleValueDropdown.value + 2;
        for (int i = 0; i < exampleItems.Length; i++)
        {
            if(i >= maxValue) exampleItems[i].text = "";
            exampleItems[i].gameObject.SetActive(false);
        }

        answerDropdown.ClearOptions();
        for (int i = 0; i < maxValue; i++)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = (i + 1).ToString() + "번";
            answerDropdown.options.Add(option);
            exampleItems[i].gameObject.SetActive(true);
        }

        answerDropdown.value = 0;
    }

    void Initialization()
    {
        selectedTypeText.text = "";
        questionInputField.text = "";
        examplePanel.SetActive(false);
        for (int i = 0; i < exampleItems.Length; i++)
        {
            exampleItems[i].text = "";
            if (i <= 1) exampleItems[i].gameObject.SetActive(true);
            else exampleItems[i].gameObject.SetActive(false);
        }
        exampleValueDropdown.value = 0;
        answerInputField.text = "";
        answerInputField.gameObject.SetActive(false);
        answerDropdown.ClearOptions();
        answerDropdown.value = 0;
        answerDropdown.gameObject.SetActive(false);
        hintInputField.text = "";
        hintInputField.gameObject.SetActive(false);
        isModify = false;
    }

    #endregion

    private void OnDisable()
    {
        Initialization();
    }
}
