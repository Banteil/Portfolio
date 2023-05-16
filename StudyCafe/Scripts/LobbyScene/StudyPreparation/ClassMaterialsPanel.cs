using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassMaterialsPanel : MonoBehaviour
{
    GameObject curriculumItem;

    public Transform curriculumContent;
    public GameObject assignmentFileListPanel;
    [HideInInspector]
    public List<CurriculumItem> curriculumItemList = new List<CurriculumItem>();

    private void Awake()
    {
        curriculumItem = Resources.Load<GameObject>("Prefabs/UI/CurriculumItem");
    }

    private void OnEnable()
    {
        SettingCurriculumInfo();
    }

    void SettingCurriculumInfo()
    {
        CommonInteraction.Instance.StartLoding();
        for (int i = 0; i < StudyPreparation.Instance.studyData.curriculumInfoList.Count; i++)
        {
            CreateCurriculumItem(StudyPreparation.Instance.studyData.curriculumInfoList[i], i);
        }
        CommonInteraction.Instance.isLoading = false;
    }

    ///<summary>
    ///커리큘럼 아이템 생성 함수
    ///</summary>
    public void CreateCurriculumItem(CurriculumInfo info, int index)
    {
        GameObject item = Instantiate(curriculumItem, curriculumContent, false);
        CurriculumItem itemScript = item.GetComponent<CurriculumItem>();
        itemScript.ListIndex = index;
        itemScript.CurriculumInfomation = info;
        itemScript.deleteButtonObject.SetActive(false);
        itemScript.itemInfoFunction = OpenCurriculumSubMenu;
        curriculumItemList.Add(itemScript);
    }

    ///<summary>
    ///커리큘럼 아이템 선택 시 실행되는 함수<br/>
    ///커리큘럼 정보 입력창에 정보 세팅 진행
    ///</summary>
    void OpenCurriculumSubMenu(int index)
    {
        AssignmentFileListPanel listScript = assignmentFileListPanel.GetComponent<AssignmentFileListPanel>();
        listScript.studyGUID = StudyPreparation.Instance.studyData.guid;
        listScript.CurriculumDate = curriculumItemList[index].CurriculumInfomation.startDate + "~" + curriculumItemList[index].CurriculumInfomation.endDate;
        listScript.IsClassMaterial = true;
        assignmentFileListPanel.SetActive(true);
    }

    void Initialization()
    {
        for (int i = 0; i < curriculumContent.childCount; i++)
        {
            Destroy(curriculumContent.GetChild(i).gameObject);
        }
        curriculumItemList = new List<CurriculumItem>();
    }

    private void OnDisable()
    {
        Initialization();
    }
}
