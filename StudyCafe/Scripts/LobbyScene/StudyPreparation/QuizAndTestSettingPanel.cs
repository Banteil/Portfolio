using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuizAndTestSettingPanel : MonoBehaviour
{
    GameObject quizAndTestItem;

    [Header("ItemListPanel")]
    public GameObject itemListPanel;
    public Text infoText;
    public Transform itemListContent;
    public Text addButtonText;
    public Toggle firstTap;

    [Header("InputInfomationPanel")]
    public GameObject inputInfomationPanel;

    int panelType;
    string curriculumDate;
    string type;
    List<QuizAndTestItem> multipleChoiceItemList = new List<QuizAndTestItem>();
    List<QuizAndTestItem> shortAnswerItemList = new List<QuizAndTestItem>();
    List<QuizAndTestItem> oxQuizItemList = new List<QuizAndTestItem>();

    private void Awake()
    {
        quizAndTestItem = Resources.Load<GameObject>("Prefabs/UI/QuizAndTestItem");
    }

    private void OnEnable()
    {
        StartCoroutine(SetPanelTypeInfoProcess());
    }

    public void SetPanelType(int panelType, string curriculumDate)
    {
        this.panelType = panelType;
        this.curriculumDate = curriculumDate;
    }

    IEnumerator SetPanelTypeInfoProcess()
    {
        CommonInteraction.Instance.StartLoding();
        string studyGUID = StudyPreparation.Instance.studyData.guid;
        //퀴즈 타입
        if (panelType.Equals(0))
            infoText.text = "퀴즈 정보";
        //쪽지 시험 타입
        else
            infoText.text = "쪽지 시험 정보";

        yield return StartCoroutine(DataManager.Instance.GetQuizOrTestInfo(panelType, studyGUID, curriculumDate, "MultipleChoice"));
        CreateQuizAndTestItem(DataManager.Instance.info, "MultipleChoice");
        yield return StartCoroutine(DataManager.Instance.GetQuizOrTestInfo(panelType, studyGUID, curriculumDate, "ShortAnswer"));
        CreateQuizAndTestItem(DataManager.Instance.info, "ShortAnswer");
        yield return StartCoroutine(DataManager.Instance.GetQuizOrTestInfo(panelType, studyGUID, curriculumDate, "OX"));
        CreateQuizAndTestItem(DataManager.Instance.info, "OX");

        SelectTypeTap("MultipleChoice");
        CommonInteraction.Instance.isLoading = false;
    }

    #region 아이템 리스트 패널 관련
    void CreateQuizAndTestItem(string info, string type)
    {
        if (info == null)
        {
            CommonInteraction.Instance.InfoPanelUpdate("퀴즈 정보 획득 중 문제가 발생했습니다.\n다시 시도해 주세요.");
            CommonInteraction.Instance.isLoading = false;
            StopAllCoroutines();
            gameObject.SetActive(false);
            return;
        }
        else if(info.Equals("null")) return;
        else
        {
            string[] data = info.Split('¶');
            for (int i = 0; i < data.Length - 1; i++)
            {
                string[] infoData = data[i].Split('☞');
                string itemGUID = infoData[0];
                string jsonData = infoData[1];

                QuizAndTestInfo quizInfo = JsonConvert.DeserializeObject<QuizAndTestInfo>(jsonData);
                GameObject item = Instantiate(quizAndTestItem, itemListContent, false);
                QuizAndTestItem itemScript = item.GetComponent<QuizAndTestItem>();
                itemScript.Index = i;
                itemScript.guid = itemGUID;
                itemScript.Info = quizInfo;
                itemScript.type = type;
                switch(type)
                {
                    case "MultipleChoice":
                        multipleChoiceItemList.Add(itemScript);
                        break;
                    case "ShortAnswer":
                        shortAnswerItemList.Add(itemScript);
                        break;
                    case "OX":
                        oxQuizItemList.Add(itemScript);
                        break;
                }
                itemScript.updateFunc = UpdateItem;
                itemScript.deleteFunc = DeleteItem;
            }
        }
    }

    public void CreateQuizAndTestItem(QuizAndTestInfo info, string type, string guid)
    {
        GameObject item = Instantiate(quizAndTestItem, itemListContent, false);
        QuizAndTestItem itemScript = item.GetComponent<QuizAndTestItem>();
        itemScript.Info = info;
        itemScript.type = type;
        itemScript.guid = guid;
        switch (type)
        {
            case "MultipleChoice":
                itemScript.Index = multipleChoiceItemList.Count;
                multipleChoiceItemList.Add(itemScript);
                break;
            case "ShortAnswer":
                itemScript.Index = shortAnswerItemList.Count;
                shortAnswerItemList.Add(itemScript);
                break;
            case "OX":
                itemScript.Index = oxQuizItemList.Count;
                oxQuizItemList.Add(itemScript);
                break;
        }
        itemScript.updateFunc = UpdateItem;
        itemScript.deleteFunc = DeleteItem;
    }

    public void ModifyItemInfo(QuizAndTestInfo info, string type, int index)
    {
        switch (type)
        {
            case "MultipleChoice":                
                multipleChoiceItemList[index].Info = info;
                break;
            case "ShortAnswer":
                shortAnswerItemList[index].Info = info;
                break;
            case "OX":
                oxQuizItemList[index].Info = info;
                break;
        }
    }

    public void SelectTypeTap(string type)
    {
        this.type = type;
        switch (type)
        {
            case "MultipleChoice":
                {
                    addButtonText.text = "객관식 문제 추가";
                    for (int i = 0; i < multipleChoiceItemList.Count; i++)
                    {
                        multipleChoiceItemList[i].gameObject.SetActive(true);
                    }

                    for (int i = 0; i < shortAnswerItemList.Count; i++)
                    {
                        shortAnswerItemList[i].gameObject.SetActive(false);
                    }

                    for (int i = 0; i < oxQuizItemList.Count; i++)
                    {
                        oxQuizItemList[i].gameObject.SetActive(false);
                    }
                }
                break;
            case "ShortAnswer":
                {
                    addButtonText.text = "주관식 문제 추가";
                    for (int i = 0; i < multipleChoiceItemList.Count; i++)
                    {
                        multipleChoiceItemList[i].gameObject.SetActive(false);
                    }

                    for (int i = 0; i < shortAnswerItemList.Count; i++)
                    {
                        shortAnswerItemList[i].gameObject.SetActive(true);
                    }

                    for (int i = 0; i < oxQuizItemList.Count; i++)
                    {
                        oxQuizItemList[i].gameObject.SetActive(false);
                    }
                }
                break;
            case "OX":
                {
                    addButtonText.text = "OX퀴즈 문제 추가";
                    for (int i = 0; i < multipleChoiceItemList.Count; i++)
                    {
                        multipleChoiceItemList[i].gameObject.SetActive(false);
                    }

                    for (int i = 0; i < shortAnswerItemList.Count; i++)
                    {
                        shortAnswerItemList[i].gameObject.SetActive(false);
                    }

                    for (int i = 0; i < oxQuizItemList.Count; i++)
                    {
                        oxQuizItemList[i].gameObject.SetActive(true);
                    }
                }
                break;
        }
    }

    public void CloseButton() => gameObject.SetActive(false);

    public void AddButton()
    {
        itemListPanel.SetActive(false);
        InputInfomationPanel infoPanel = inputInfomationPanel.GetComponent<InputInfomationPanel>();
        infoPanel.SetInputType(panelType, type, curriculumDate);
        inputInfomationPanel.SetActive(true);        
    }

    void UpdateItem(string type, int index)
    {
        string guid = "";
        QuizAndTestInfo info = new QuizAndTestInfo();
        switch (type)
        {
            case "MultipleChoice":
                {
                    guid = multipleChoiceItemList[index].guid;
                    info = multipleChoiceItemList[index].Info;
                }
                break;
            case "ShortAnswer":
                {
                    guid = shortAnswerItemList[index].guid;
                    info = shortAnswerItemList[index].Info;
                }
                break;
            case "OX":
                {
                    guid = oxQuizItemList[index].guid;
                    info = oxQuizItemList[index].Info;
                }
                break;
        }
        itemListPanel.SetActive(false);
        InputInfomationPanel infoPanel = inputInfomationPanel.GetComponent<InputInfomationPanel>();
        infoPanel.ModifyInputType(panelType, type, curriculumDate, guid, index, info);
        inputInfomationPanel.SetActive(true);
    }

    void DeleteItem(string type, int index)
    {
        switch (type)
        {
            case "MultipleChoice":
                {
                    StartCoroutine(DataManager.Instance.DeleteQuizOrTestInfo(panelType, multipleChoiceItemList[index].guid));
                    multipleChoiceItemList.RemoveAt(index);
                }
                break;
            case "ShortAnswer":
                {
                    StartCoroutine(DataManager.Instance.DeleteQuizOrTestInfo(panelType, shortAnswerItemList[index].guid));
                    shortAnswerItemList.RemoveAt(index);
                }
                break;
            case "OX":
                {
                    StartCoroutine(DataManager.Instance.DeleteQuizOrTestInfo(panelType, oxQuizItemList[index].guid));
                    oxQuizItemList.RemoveAt(index);
                }
                break;
        }
    }
    #endregion

    void Initialization()
    {
        infoText.text = "";
        addButtonText.text = "추가";
        for (int i = 0; i < itemListContent.childCount; i++)
        {
            Destroy(itemListContent.GetChild(i).gameObject);
        }
        multipleChoiceItemList = new List<QuizAndTestItem>();
        shortAnswerItemList = new List<QuizAndTestItem>();
        oxQuizItemList = new List<QuizAndTestItem>();
        itemListPanel.SetActive(true);
        inputInfomationPanel.SetActive(false);
        firstTap.isOn = true;
    }

    private void OnDisable()
    {
        Initialization();
    }
}
