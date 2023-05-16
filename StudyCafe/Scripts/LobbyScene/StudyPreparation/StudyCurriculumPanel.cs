using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StudyCurriculumPanel : MonoBehaviour
{
    GameObject curriculumItem;

    public GameObject enterCurriculumInfoPanel;
    public Button addButton;
    public Transform curriculumContent;    
    public GameObject saveButtonObject;
    [HideInInspector]
    public List<CurriculumItem> curriculumItemList = new List<CurriculumItem>();

    bool isFirstEnable = true;

    private void Awake()
    {
        curriculumItem = Resources.Load<GameObject>("Prefabs/UI/CurriculumItem");
    }

    private void OnEnable()
    {
        saveButtonObject.GetComponent<Button>().onClick.RemoveAllListeners();
        saveButtonObject.GetComponent<Button>().onClick.AddListener(ModifyCompleteButton);
        if (isFirstEnable)
        {
            StudyPreparation.Instance.interactFunc += Initialization;
            SettingCurriculumInfo();
            isFirstEnable = false;
        }
    }

    void SettingCurriculumInfo()
    {
        CommonInteraction.Instance.StartLoding();
        if (!StudyPreparation.Instance.powerToEdit[1])
            NonEditableProcess();

        for (int i = 0; i < StudyPreparation.Instance.studyData.curriculumInfoList.Count; i++)
        {
            CreateCurriculumItem(StudyPreparation.Instance.studyData.curriculumInfoList[i]);
        }
        
        CommonInteraction.Instance.isLoading = false;
    }

    ///<summary>
    ///수정 권한 없을 때 처리
    ///</summary>
    void NonEditableProcess()
    {
        addButton.interactable = false;
        saveButtonObject.GetComponent<Button>().interactable = false;
        saveButtonObject.transform.GetChild(0).GetComponent<Text>().text = "수업 자료의 수정 권한이 없습니다.";
    }

    ///<summary>
    ///커리큘럼 아이템 생성 함수
    ///</summary>
    public void CreateCurriculumItem(CurriculumInfo info)
    {
        GameObject item = Instantiate(curriculumItem, curriculumContent, false);
        CurriculumItem itemScript = item.GetComponent<CurriculumItem>();
        itemScript.ListIndex = curriculumItemList.Count;
        itemScript.CurriculumInfomation = info;
        itemScript.itemInfoFunction = OpenCurriculumSubMenu;
        itemScript.deleteFunction = DeleteCurriculumItem;
        curriculumItemList.Add(itemScript);
    }

    ///<summary>
    ///커리큘럼 아이템 생성 및 리스트 정렬 함수
    ///</summary>
    public void CreateCurriculumAndSortList(CurriculumInfo info)
    {
        CreateCurriculumItem(info);
        //날짜 순서로 오브젝트 순서 변경
        for (int i = 0; i < curriculumItemList.Count - 1; i++)
        {
            DateTime left = DateTime.Parse(curriculumItemList[i].CurriculumInfomation.startDate);
            DateTime right = DateTime.Parse(curriculumItemList[curriculumItemList.Count - 1].CurriculumInfomation.startDate);
            if (left > right)
            {
                curriculumItemList[curriculumItemList.Count - 1].transform.SetSiblingIndex(i);
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
    }

    public void OpenEnterCurriculumInfoPanelButton() => enterCurriculumInfoPanel.SetActive(true);

    ///<summary>
    ///커리큘럼 아이템 선택 시 실행되는 함수<br/>
    ///커리큘럼 정보 입력창에 정보 세팅 진행
    ///</summary>
    void OpenCurriculumSubMenu(int index)
    {
        enterCurriculumInfoPanel.GetComponent<EnterCurriculumInfoPanel>().SetCurriculumInfo(curriculumItemList[index].CurriculumInfomation, index);
        enterCurriculumInfoPanel.SetActive(true);
    }

    ///<summary>
    ///커리큘럼 아이템 리스트에서 삭제
    ///</summary>
    void DeleteCurriculumItem(int index)
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

    public void ModifyCompleteButton() => StartCoroutine(ModifyCompleteProcess());

    IEnumerator ModifyCompleteProcess()
    {
        if (!CheckException()) yield break;

        List<CurriculumInfo> tempCurriculumInfoList = new List<CurriculumInfo>();
        for (int i = 0; i < curriculumItemList.Count; i++)
        {
            tempCurriculumInfoList.Add(curriculumItemList[i].CurriculumInfomation);
        }

        StudyInfoData tempData = StudyPreparation.Instance.studyData;
        tempData.curriculumInfoList = tempCurriculumInfoList;

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
        for (int i = 0; i < curriculumContent.childCount; i++)
        {
            Destroy(curriculumContent.GetChild(i).gameObject);
        }

        addButton.interactable = true;
        curriculumItemList = new List<CurriculumItem>();

        saveButtonObject.GetComponent<Button>().interactable = true;
        saveButtonObject.transform.GetChild(0).GetComponent<Text>().text = "수정한 내용 저장";
        isFirstEnable = true;        
    }

    #region 유효성 검사
    public bool CheckException()
    {
        if (curriculumContent.childCount <= 0)
        {
            CommonInteraction.Instance.InfoPanelUpdate("수업 자료는 최소 하나 이상 등록되어 있어야 합니다.");
            return false;
        }

        return true;
    }
    
    #endregion

    private void OnDisable()
    {
        enterCurriculumInfoPanel.SetActive(false);
    }
}
