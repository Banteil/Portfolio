using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스터디 준비 시 파일 업로드, 다운로드, 삭제 등 관리를 진행하는 역할을 담당하는 클래스
/// </summary>
public class FileManagementPanel : MonoBehaviour
{
    public GameObject fileTypePanel;
    public GameObject fileListView;
    public Transform fileListContent, curriculumListContent;
    public Button fileUploadButton;
    public bool isWaitingRoom;

    ///<summary>
    ///DB에서 파일 리스트를 획득하여 세팅하는 함수
    ///</summary>
    void SettingCurriculumList()
    {
        //List<Dictionary<string, string>> curriculumInfo = myStudy.GetComponent<StudyPreparation>().curriculum.GetCurriculumInfoList();
        //int count = 1;
        //for (int i = 0; i < curriculumInfo.Count; i++)
        //{
        //    string date = curriculumInfo[i]["Date"];
        //    string[] times = curriculumInfo[i]["Time"].Split('^');
        //    for (int j = 0; j < times.Length; j++)
        //    {
        //        string curriculum = date + " " + times[j];
        //        GameObject item = Instantiate(Resources.Load<GameObject>("Prefabs/UI/CurriculumItem"), curriculumListContent, false);
        //        CurriculumItem itemScript = item.GetComponent<CurriculumItem>();
        //        itemScript.indexText.text = count.ToString();
        //        itemScript.dateText.text = curriculum;
        //        Destroy(itemScript);
        //        Button itemButton = item.GetComponent<Button>();
        //        itemButton.onClick.RemoveAllListeners();
        //        itemButton.onClick.AddListener(() => SelectCurriculum(curriculum));
        //        ColorBlock cb = itemButton.colors;
        //        Color tC = Color.red;
        //        cb.selectedColor = tC;
        //        itemButton.colors = cb;
        //        count++;
        //    }
        //}
    }

    ///<summary>
    ///DB에서 파일 리스트를 획득하여 세팅하는 함수
    ///</summary>
    IEnumerator SettingFileList()
    {
        //string guid = (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"];
        //yield return StartCoroutine(DataManager.Instance.GetFileList());
        string fileListInfo = DataManager.Instance.info;

        if (fileListInfo == null) yield break;

        //이름|타입,이름|타입,...
        string[] fileList = fileListInfo.Split(',');
        for (int i = 0; i < fileList.Length - 1; i++)
        {
            string fileName = fileList[i];
            GameObject item = Instantiate(Resources.Load<GameObject>("Prefabs/UI/FileInfoItem"), fileListContent, false);
            FileInfoItem fileItem = item.GetComponent<FileInfoItem>();
            fileItem.SetFileInfo(fileName);
        }
    }

    ///<summary>
    ///파일 리스트 뷰를 활성화하는 버튼 함수
    ///</summary>
    public void OpenFileListViewButton(string type)
    {
        DataManager.interactionData.type = type;
        fileTypePanel.SetActive(false);
        fileListView.SetActive(true);

        if (!isWaitingRoom)
            SettingCurriculumList();
        else
        {
            string curriculum = (string)PhotonNetwork.CurrentRoom.CustomProperties["CurriculumDate"];
            SelectCurriculum(curriculum);
        }
    }

    public void SelectCurriculum(string curriculumDate)
    {
        if (!DataManager.interactionData.type.Equals("Assignment"))
            fileUploadButton.interactable = true;

        for (int i = 0; i < fileListContent.childCount; i++)
        {
            Destroy(fileListContent.GetChild(i).gameObject);
        }

        DataManager.interactionData.curriculumDate = curriculumDate;
        StartCoroutine(SettingFileList());
    }

    ///<summary>
    ///자료에 파일을 업로드하는 함수 
    ///</summary>
    public void UploadFile()
    {
        DataManager.Instance.OpenFileDialogButtonOnClickHandler();
        DataManager.Instance.interactFunc = SetUploadFileList;
    }

    ///<summary>
    ///파일 리스트 뷰를 비활성화하는 함수
    ///</summary>
    public void CloseView()
    {
        Initialization();
        fileListView.SetActive(false);
        fileTypePanel.SetActive(true);
    }

    ///<summary>
    ///업로드 파일 아이템을 생성, 세팅하는 함수
    ///</summary>
    public void SetUploadFileList()
    {
        if (DataManager.Instance.info.Equals("exist")) return;

        GameObject item = Instantiate(Resources.Load<GameObject>("Prefabs/UI/FileInfoItem"), fileListContent, false);
        FileInfoItem fileItem = item.GetComponent<FileInfoItem>();
        fileItem.SetFileInfo(DataManager.interactionData.fileName);
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

        for (int i = 0; i < curriculumListContent.childCount; i++)
        {
            Destroy(curriculumListContent.GetChild(i).gameObject);
        }

        fileUploadButton.interactable = false;
    }

    public void CloseCabinet() => gameObject.SetActive(false);

    void OnDisable()
    {
        Initialization();
        fileListView.SetActive(false);
        fileTypePanel.SetActive(true);        
    }
}
