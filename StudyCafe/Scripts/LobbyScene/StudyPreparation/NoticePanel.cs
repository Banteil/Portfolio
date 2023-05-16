using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NoticePanel : MonoBehaviour
{
    GameObject noticeItemObject;
    public Transform noticeContent;
    public GameObject openPanelButtonObject;
    public GameObject contentInputPanel;
    public Text fileNameText;
    public InputField contentInput;
    public Button postingButton;

    string studyGUID;

    string fileName = "";
    string tempToken = "";
    string info;

    private void Awake()
    {
        noticeItemObject = Resources.Load<GameObject>("Prefabs/UI/NoticeContentItem");
    }

    private void OnEnable()
    {
        StartCoroutine(SettingNoticeList());
    }

    IEnumerator SettingNoticeList()
    {        
        CommonInteraction.Instance.StartLoding();
        if (!StudyPreparation.Instance.powerToEdit[0])
            NonEditableProcess();

        studyGUID = StudyPreparation.Instance.studyData.guid;
        yield return StartCoroutine(GetNoticeInfo());
        if(info != null && !info.Equals("null"))
        {
            string[] notices = info.Split('¶');
            for (int i = notices.Length - 2; i >= 0; i--)
            {
                //token, user_guid, content, file_name, insert_time 순서
                string[] infos = notices[i].Split('☞');
                yield return StartCoroutine(CreateNoticeItem(infos[0], infos[1], infos[2], infos[3], infos[4], false));
            }
        }        

        CommonInteraction.Instance.isLoading = false;
    }

    void NonEditableProcess()
    {
        openPanelButtonObject.GetComponent<Button>().interactable = false;
        openPanelButtonObject.transform.GetChild(0).GetComponent<Text>().text = "공지 게시 권한이 없습니다.";
    }

    void Initialization()
    {
        for (int i = 0; i < noticeContent.childCount; i++)
        {
            Destroy(noticeContent.GetChild(i).gameObject);
        }
        CloseContentInputPanelButton();
        openPanelButtonObject.GetComponent<Button>().interactable = true;
        openPanelButtonObject.transform.GetChild(0).GetComponent<Text>().text = "멤버들에게 공지할 내용을 입력해 주세요.";
        info = null;
    }

    IEnumerator CreateNoticeItem(string token, string guid, string content, string fileName, string createTime, bool isAdd)
    {
        GameObject item = Instantiate(noticeItemObject, noticeContent, false);
        NoticeContentItem itemScript = item.GetComponent<NoticeContentItem>();
        itemScript.token = token;
        itemScript.WriterGUID = guid;
        itemScript.contentText.text = content;
        if (fileName.Equals(""))
        {
            itemScript.attachmentImageObject.SetActive(false);
            itemScript.fileInfoButtonObject.SetActive(false);
        }
        else
        {
            string[] fileInfo = fileName.Split('.');
            FileType type = DataManager.Instance.CheckFileType(fileInfo[1]);
            if (type.Equals(FileType.IMAGE))
            {
                //이미지 파일일 경우 표시되도록 처리                    
                string fileDir = "/../storage/data/" + studyGUID + "/noticefile/" + token + "/";
                itemScript.attachmentImageObject.SetActive(true);
                yield return StartCoroutine(DataManager.Instance.GetFileData(fileDir, fileName));
                byte[] data = DataManager.Instance.DataInfo;
                itemScript.attachmentImageObject.GetComponent<Image>().sprite = FileBrowserDialogLib.GetSprite(data);
            }
            //파일 다운로드 버튼 활성화
            itemScript.fileInfoButtonObject.SetActive(true);
            itemScript.FileName = fileName;
        }
        DateTime insertTime = DateTime.Parse(createTime);
        string timeText = insertTime.Year + "년 " + insertTime.Month + "월 " + insertTime.Day + "일 " + insertTime.Hour + "시 " + insertTime.Minute + "분";
        itemScript.notificationDateText.text = timeText;
        StartCoroutine(itemScript.SettingCommentList());

        if (isAdd)
            item.transform.SetAsFirstSibling();
    }

    #region 공지 인풋 패널 관련 기능

    public void OpenContentInputPanelButton()
    {
        Guid guid = Guid.NewGuid();
        tempToken = guid.ToString();
        contentInputPanel.SetActive(true);
    }

    public void CloseContentInputPanelButton()
    {
        if(DataManager.Instance != null)
            DataManager.Instance.DeleteTempFileProcess(tempToken);
        tempToken = "";
        contentInput.text = "";
        fileNameText.text = "";
        postingButton.interactable = false;
        contentInputPanel.SetActive(false);
    }

    public void CheckActivePostingButton()
    {
        if (contentInput.text.Equals("")) postingButton.interactable = false;
        else postingButton.interactable = true;
    }

    public void PostingButton() => StartCoroutine(PostingProcess());

    IEnumerator PostingProcess()
    {
        string guid = (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"];
        yield return StartCoroutine(DataManager.Instance.AddNotice(studyGUID, tempToken, guid, contentInput.text, fileName));
        if(DataManager.Instance.info == null)
        {
            CommonInteraction.Instance.InfoPanelUpdate("공지사항 게시에 실패하였습니다.");
        }
        else
        {
            string createTime = DataManager.Instance.info;
            yield return StartCoroutine(CreateNoticeItem(tempToken, guid, contentInput.text, fileName, createTime, true));
            CloseContentInputPanelButton();
            CommonInteraction.Instance.InfoPanelUpdate("공지사항 게시 완료!");            
        }
        
    }

    public void UploadAttachmentButton()
    {
        DataManager.isTempFileUpload = true;
        DataManager.interactionData.type = tempToken;
        DataManager.Instance.OpenFileDialogButtonOnClickHandler();
        DataManager.Instance.stringTransfer = SetFileInfo;
    }

    void SetFileInfo(string info)
    {
        fileName = info;
        fileNameText.text = info;
    }

    #endregion

    #region 공지사항 get, set 통신 코루틴
    public IEnumerator GetNoticeInfo()
    {
        string noticeInfoUrl = "https://stubugs.com/php/noticeinfo.php";
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.GET);
        form.AddField("StudyGUID", studyGUID);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(noticeInfoUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        CommonInteraction.Instance.InfoPanelUpdate("공지사항 정보를 불러오는데 실패했습니다.");
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = check[1];
                        break;
                }
            }
        }
    }

    #endregion

    private void OnDisable()
    {
        Initialization();
    }
}
