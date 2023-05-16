using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileListPanel : MonoBehaviour
{
    public Transform fileListContent;
    public GameObject fileViewerPanel;
    GameObject fileInfoItem;

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
        yield return StartCoroutine(DataManager.Instance.GetFileList(studyGUID, "", "ALL"));
        string fileListInfo = DataManager.Instance.info;
        if (fileListInfo == null) yield break;

        //이름,이름,...
        string[] fileList = fileListInfo.Split('☞');
        for (int i = 0; i < fileList.Length - 1; i++)
        {
            string fileName = fileList[i];
            GameObject item = Instantiate(fileInfoItem, fileListContent, false);
            FileInfoItem fileItem = item.GetComponent<FileInfoItem>();
            fileItem.StudyGUID = studyGUID;
            fileItem.SetFileInfo(fileName);
            if(StudyPreparation.Instance.powerToEdit[4])
                fileItem.deleteButtonObj.SetActive(true);
            fileItem.downloadButtonObj.SetActive(true);
            fileItem.openViewerFunction = OpenFileViewer;
        }
    }

    ///<summary>
    ///자료에 파일을 업로드하는 함수 
    ///</summary>
    public void UploadFile()
    {
        DataManager.interactionData.studyGUID = StudyPreparation.Instance.studyData.guid;
        DataManager.Instance.OpenFileDialogButtonOnClickHandler();
        DataManager.Instance.interactFunc = SetUploadFileList;
    }

    ///<summary>
    ///업로드 파일 아이템을 생성, 세팅하는 함수
    ///</summary>
    public void SetUploadFileList()
    {
        //동일한 파일 업로드 = 덮어쓰기 했을 경우 아이템 추가 생성 없음
        if (DataManager.Instance.info.Equals("exist")) return;

        GameObject item = Instantiate(Resources.Load<GameObject>("Prefabs/UI/FileInfoItem"), fileListContent, false);
        FileInfoItem fileItem = item.GetComponent<FileInfoItem>();
        fileItem.StudyGUID = StudyPreparation.Instance.studyData.guid;
        fileItem.SetFileInfo(DataManager.interactionData.fileName);
        if (StudyPreparation.Instance.powerToEdit[4])
            fileItem.deleteButtonObj.SetActive(true);
        fileItem.downloadButtonObj.SetActive(true);
        fileItem.openViewerFunction = OpenFileViewer;
    }

    void OpenFileViewer(string fileName)
    {
        fileViewerPanel.SetActive(true);
        fileViewerPanel.GetComponent<FileViewer>().StudyGUID = StudyPreparation.Instance.studyData.guid;
        fileViewerPanel.GetComponent<FileViewer>().FileSetting(fileName);        
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
    }

    private void OnDisable()
    {
        Initialization();
    }
}
