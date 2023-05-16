using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetCurriculumFilePanel : MonoBehaviour
{
    GameObject fileInfoItem;
    public Transform fileListContent;
    [HideInInspector]
    public string fileType;
    [HideInInspector]
    public string curriculumDate;

    List<FileInfoItem> fileInfoItemList = new List<FileInfoItem>();

    private void Awake()
    {
        fileInfoItem = Resources.Load<GameObject>("Prefabs/UI/FileInfoItem");
    }

    private void OnEnable()
    {
        StartCoroutine(SettingFileList());
    }

    ///<summary>
    ///DB에서 파일 리스트를 획득하여 세팅하는 함수
    ///</summary>
    IEnumerator SettingFileList()
    {
        string studyGUID = StudyPreparation.Instance.studyData.guid;
        yield return StartCoroutine(DataManager.Instance.GetFileList(studyGUID, curriculumDate, fileType));
        string setTypefileInfo = DataManager.Instance.info;
        if(setTypefileInfo != null)
        {
            //이름☞이름☞...
            string[] setFileList = setTypefileInfo.Split('☞');
            for (int i = 0; i < setFileList.Length - 1; i++)
            {
                string fileName = setFileList[i];
                GameObject item = Instantiate(fileInfoItem, fileListContent, false);
                FileInfoItem fileItem = item.GetComponent<FileInfoItem>();
                fileItem.StudyGUID = studyGUID;
                fileItem.SetFileInfo(fileName);
                fileItem.selectFileToggleObj.SetActive(true);
                fileItem.selectFileToggleObj.GetComponent<Toggle>().isOn = true;
                fileInfoItemList.Add(fileItem);
            }
        }

        yield return StartCoroutine(DataManager.Instance.GetFileList(studyGUID, "", "ALL"));
        string fileListInfo = DataManager.Instance.info;
        if (fileListInfo == null) yield break;

        //이름☞이름☞...
        string[] fileList = fileListInfo.Split('☞');
        for (int i = 0; i < fileList.Length - 1; i++)
        {
            string fileName = fileList[i];
            if (CheckSameName(fileName)) continue;
            GameObject item = Instantiate(fileInfoItem, fileListContent, false);
            FileInfoItem fileItem = item.GetComponent<FileInfoItem>();
            fileItem.StudyGUID = studyGUID;
            fileItem.SetFileInfo(fileName);
            fileItem.selectFileToggleObj.SetActive(true);            
            fileInfoItemList.Add(fileItem);
        }
    }

    bool CheckSameName(string fileName)
    {
        for (int i = 0; i < fileInfoItemList.Count; i++)
        {
            if (fileInfoItemList[i].fileNameText.text.Equals(fileName))
                return true;
        }
        return false;
    }

    public void AddTypeButton()
    {
        StartCoroutine(AddFileTypeProcess());
    }

    IEnumerator AddFileTypeProcess()
    {
        string studyGUID = StudyPreparation.Instance.studyData.guid;
        List<string> selectFileNameList = new List<string>();
        for (int i = 0; i < fileInfoItemList.Count; i++)
        {
            if (fileInfoItemList[i].IsSelectToggleOn)
                selectFileNameList.Add(fileInfoItemList[i].fileNameText.text);
        }

        if (selectFileNameList.Count > 0)
        {
            CommonInteraction.Instance.StartLoding();
            for (int i = 0; i < selectFileNameList.Count; i++)
            {
                Debug.Log(fileType);
                yield return StartCoroutine(DataManager.Instance.SetFileType(fileType, studyGUID, curriculumDate, selectFileNameList[i]));
            }
            CommonInteraction.Instance.isLoading = false;
            CommonInteraction.Instance.InfoPanelUpdate("파일 등록을 완료하였습니다!");
            CloseButton();
        }
    }

    public void CloseButton()
    {
        gameObject.SetActive(false);
    }

    ///<summary>
    ///파일 리스트 객체 초기화 함수
    ///</summary>
    public void Initialization()
    {
        for (int i = 0; i < fileListContent.childCount; i++)
        {
            Destroy(fileListContent.GetChild(i).gameObject);
        }

        fileInfoItemList = new List<FileInfoItem>();
    }

    private void OnDisable()
    {
        Initialization();
    }

}
