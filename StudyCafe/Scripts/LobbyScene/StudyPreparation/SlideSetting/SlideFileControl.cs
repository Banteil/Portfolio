using Newtonsoft.Json;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlideFileControl : MonoBehaviour
{
    public Transform slideSettingContent, fileListContent;
    public Button setButton, delButton, leftMoveButton, rightMoveButton;
    string fileDir;
    string studyGUID;
    public string StudyGUID
    {
        set
        {
            studyGUID = value;
            fileDir = "/../data/" + studyGUID + "/";
        }
    }
    string curriculumDate;
    public string CurriculumDate
    { 
        set
        {
            curriculumDate = value;
        }
    }

    GameObject slideItemObj, setSlideItemObj;
    SlideFileItem selectFileItem;
    int selectIndex;
    List<SetSlideItem> setSlideItemList = new List<SetSlideItem>();

    private void Awake()
    {
        setSlideItemObj = Resources.Load<GameObject>("Prefabs/UI/SetSlideItemPanel");
        slideItemObj = Resources.Load<GameObject>("Prefabs/UI/SlideFileItem");
    }

    private void OnEnable() => StartCoroutine(SettingList());

    IEnumerator SettingList()
    {        
        CommonInteraction.Instance.StartLoding();
        //발표 자료 리스트 아이템 세팅
        yield return StartCoroutine(DataManager.Instance.GetFileList(studyGUID, curriculumDate, "Presentation"));
        string fileListInfo = DataManager.Instance.info;
        if (fileListInfo != null)
        {
            //이름☞이름☞...
            string[] fileList = fileListInfo.Split('☞');
            for (int i = 0; i < fileList.Length - 1; i++)
            {
                string fileName = fileList[i];
                GameObject slideFileItem = Instantiate(slideItemObj, fileListContent, false);
                SlideFileItem slideFileScript = slideFileItem.GetComponent<SlideFileItem>();
                slideFileScript.SetFileInfo(fileName);
                slideFileScript.slideFileControl = this;
            }
        }

        //이미 저장된 발표 자료 정보 받아와서 세팅
        yield return StartCoroutine(DataManager.Instance.GetSlideData(studyGUID, curriculumDate));
        string jsonData = DataManager.Instance.info;        
        if (jsonData != null)
        {
            Debug.Log(jsonData);
            SlideSettingInfo slideSettingInfo = JsonConvert.DeserializeObject<SlideSettingInfo>(jsonData);
            int skipCount = 0; //슬라이드 아이템 생성 스킵 횟수
            for (int i = 0; i < slideSettingInfo.index.Count; i++)
            {
                //실제 파일 존재 여부 체크, 없어지면 리스트 추가 패스
                DataManager.interactionData.type = "Presentation";
                yield return StartCoroutine(DataManager.Instance.CheckFile(slideSettingInfo.fileNames[i], (string)PhotonNetwork.CurrentRoom.CustomProperties["StudyGUID"]));                
                if(DataManager.Instance.info.Equals("F"))
                {
                    skipCount++;
                    continue;
                }

                //슬라이드 아이템 생성 및 정보 세팅
                GameObject slideItem = Instantiate(setSlideItemObj, slideSettingContent);
                slideItem.transform.localScale = Vector3.one;
                SetSlideItem itemScript = slideItem.GetComponent<SetSlideItem>();
                itemScript.SetFileInfo(slideSettingInfo.fileNames[i]);
                itemScript.Index = slideSettingInfo.index[i] - skipCount;
                itemScript.slideFileControl = this;
                switch (itemScript.fileType)
                {
                    case FileType.IMAGE:
                        {
                            string fileName = slideSettingInfo.fileNames[i];
                            yield return StartCoroutine(DataManager.Instance.GetFileData(fileDir, fileName));
                            byte[] data = DataManager.Instance.DataInfo;
                            itemScript.thumbnail.texture = FileBrowserDialogLib.GetTexture2D(data);
                        }
                        break;
                    default:
                        {
                            string extension = FileBrowserDialogLib.GetFileResolution(slideSettingInfo.fileNames[i]);
                            Texture texture = Resources.Load<Texture>("UISprite/FileFormat/" + extension);
                            if (texture != null)
                                itemScript.thumbnail.texture = texture;
                            else
                                itemScript.thumbnail.texture = Resources.Load<Texture>("UISprite/FileFormat/blank");
                        }
                        break;
                }

                setSlideItemList.Add(itemScript);
            }            
        }
        CommonInteraction.Instance.isLoading = false;
    }

    /// <summary>
    /// 파일을 선택했을 때 해당 파일의 정보를 저장하고, 슬라이드 리스트에 세팅할 수 있게 준비하는 함수
    /// </summary>
    public void SelectFileButton(SlideFileItem item)
    {
        DeselectButton();
        selectFileItem = item;
        setButton.interactable = true;
    }

    /// <summary>
    /// 선택된 파일을 슬라이드 리스트에 추가하는 함수
    /// </summary>
    public void SetSlideButton()
    {
        StartCoroutine(SetSlideProcess());
    }

    IEnumerator SetSlideProcess()
    {
        GameObject slideItem = Instantiate(setSlideItemObj, slideSettingContent, false);
        SetSlideItem itemScript = slideItem.GetComponent<SetSlideItem>();
        itemScript.SetFileInfo(selectFileItem.fileNameText.text);
        itemScript.Index = setSlideItemList.Count;
        itemScript.slideFileControl = this;
        switch (itemScript.fileType)
        {
            case FileType.IMAGE:
                {
                    string fileName = selectFileItem.fileNameText.text;
                    yield return StartCoroutine(DataManager.Instance.GetFileData(fileDir, fileName));
                    byte[] data = DataManager.Instance.DataInfo;
                    itemScript.thumbnail.texture = FileBrowserDialogLib.GetTexture2D(data);
                }
                break;
            case FileType.DOCUMENT:
                {
                    string[] name = selectFileItem.fileNameText.text.Split('.');
                    string fileName = name[0] + "_1.jpg";
                    yield return StartCoroutine(DataManager.Instance.GetFileData(fileDir, fileName));
                    byte[] data = DataManager.Instance.DataInfo;
                    itemScript.thumbnail.texture = FileBrowserDialogLib.GetTexture2D(data);
                }
                break;
            default:
                {
                    string extension = FileBrowserDialogLib.GetFileResolution(selectFileItem.fileNameText.text);
                    Texture texture = Resources.Load<Texture>("UISprite/FileFormat/" + extension);
                    if (texture != null)
                        itemScript.thumbnail.texture = texture;
                    else
                        itemScript.thumbnail.texture = Resources.Load<Texture>("UISprite/FileFormat/blank");
                }
                break;
        }

        setSlideItemList.Add(itemScript);
        DeselectButton();
    }

    /// <summary>
    /// 버튼을 선택했을 때 index 정보를 저장하는 함수
    /// </summary>
    public void SelectSlideButton(int index)
    {
        DeselectButton();
        selectIndex = index;
        delButton.interactable = true;
        if(selectIndex > 0)
            leftMoveButton.interactable = true;
        if(selectIndex < setSlideItemList.Count - 1)
            rightMoveButton.interactable = true;
    }

    /// <summary>
    /// 버튼 선택 여부를 해제하는 함수
    /// </summary>
    void DeselectButton()
    {
        setButton.interactable = false;
        delButton.interactable = false;
        leftMoveButton.interactable = false;
        rightMoveButton.interactable = false;
        selectFileItem = null;
        selectIndex = 0;
    }

    /// <summary>
    /// 선택된 슬라이드 아이템을 목록에서 삭제하는 함수
    /// </summary>
    public void DeleteSlideButton()
    {
        Destroy(setSlideItemList[selectIndex].gameObject);
        setSlideItemList.RemoveAt(selectIndex);
        for (int i = selectIndex; i < setSlideItemList.Count; i++)
        {
            setSlideItemList[i].Index = i;
        }
        DeselectButton();
    }

    /// <summary>
    /// 선택된 슬라이드 아이템을 좌, 우로 이동시켜 재배치하는 함수
    /// </summary>
    public void MoveItemLR(int direction)
    {
        int selectItemSibling = setSlideItemList[selectIndex].transform.GetSiblingIndex();
        //0 == Left
        if (direction.Equals(0))
        {
            setSlideItemList[selectIndex].transform.SetSiblingIndex(--selectItemSibling);

            SetSlideItem tempItem = setSlideItemList[selectIndex - 1];
            setSlideItemList[selectIndex - 1] = setSlideItemList[selectIndex];
            setSlideItemList[selectIndex] = tempItem;            

            setSlideItemList[selectIndex - 1].Index--;
            setSlideItemList[selectIndex].Index++;
                        
            selectIndex--;
        }
        //1 == right
        else
        {
            setSlideItemList[selectIndex].transform.SetSiblingIndex(++selectItemSibling);

            SetSlideItem tempItem = setSlideItemList[selectIndex + 1];
            setSlideItemList[selectIndex + 1] = setSlideItemList[selectIndex];
            setSlideItemList[selectIndex] = tempItem;

            setSlideItemList[selectIndex + 1].Index++;
            setSlideItemList[selectIndex].Index--;

            selectIndex++;
        }

        if (selectIndex > 0)
            leftMoveButton.interactable = true;
        else
            leftMoveButton.interactable = false;

        if (selectIndex < setSlideItemList.Count - 1)
            rightMoveButton.interactable = true;
        else
            rightMoveButton.interactable = false;
    }

    /// <summary>
    /// 저장 후 패널을 닫는 함수
    /// </summary>
    public void SaveAndCloseSlidePanel() => StartCoroutine(SaveAndCloseProcess());

    IEnumerator SaveAndCloseProcess()
    {
        CommonInteraction.Instance.StartLoding();
        SlideSettingInfo slideData = new SlideSettingInfo();
        for (int i = 0; i < setSlideItemList.Count; i++)
        {
            slideData.index.Add(setSlideItemList[i].Index);
            slideData.fileNames.Add(setSlideItemList[i].fileNameText.text);
            slideData.fileTypes.Add(setSlideItemList[i].fileType);
        }
        string jsonData = JsonConvert.SerializeObject(slideData);
        yield return StartCoroutine(DataManager.Instance.SetSlideData(studyGUID, curriculumDate, jsonData));
        if(DataManager.Instance.info.Equals("SUCCESS"))
        {
            CommonInteraction.Instance.InfoPanelUpdate("슬라이드 정보 저장 성공");
            gameObject.SetActive(false);
        }
        else
        {
            CommonInteraction.Instance.InfoPanelUpdate("슬라이드 정보 저장 실패");
        }
        CommonInteraction.Instance.isLoading = false;        
    }

    /// <summary>
    /// 슬라이드 정보 초기화
    /// </summary>
    void Initialization()
    {
        DeselectButton();

        for (int i = 0; i < slideSettingContent.childCount; i++)
        {
            Destroy(slideSettingContent.GetChild(i).gameObject);
        }

        for (int i = 0; i < fileListContent.childCount; i++)
        {
            Destroy(fileListContent.GetChild(i).gameObject);
        }

        setSlideItemList = new List<SetSlideItem>();
    }

    private void OnDisable() => Initialization();
}
