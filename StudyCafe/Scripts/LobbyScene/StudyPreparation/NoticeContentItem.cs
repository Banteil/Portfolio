using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NoticeContentItem : MonoBehaviour
{
    const int body = 0;
    const int eyes = 1;
    const int top = 2;
    const int hair = 3;

    //공지 작성자 guid
    public string WriterGUID
    {
        set
        {
            string playerGUID = (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"];
            if (playerGUID.Equals(value))
            {
                modifyButtonObject.SetActive(true);
                deleteButtonObject.SetActive(true);
            }
            else
            {
                modifyButtonObject.SetActive(false);
                deleteButtonObject.SetActive(false);
            }
            StartCoroutine(SettingUserInfo(value));
        }
    }
    [HideInInspector]
    public string token;
    string fileName;
    public string FileName
    {
        set
        {
            fileName = value;
            fileInfoButtonObject.transform.GetChild(1).GetComponent<Text>().text = fileName;
        }
    }

    [Header("Notice Related Objects")]
    public Image[] faceImages;
    public Text writerNameText;
    public Text notificationDateText;
    public Text contentText;
    public GameObject attachmentImageObject;
    public GameObject fileInfoButtonObject;
    public GameObject noticeModifyInputObject;
    public GameObject modifyButtonObject;
    public GameObject deleteButtonObject;

    [Header("Comment Related Objects")]
    public GameObject expandCommentPageButtonObject;
    public GameObject commentItem;
    public InputField commentInput;
    public Transform commentContent;
    public List<NoticeCommentItem> commentItemList = new List<NoticeCommentItem>();

    bool commentPageMode;
    bool CommentPageMode
    {
        set
        {
            commentPageMode = value;
            if(commentPageMode)
            {
                for (int i = 0; i < commentItemList.Count; i++)
                {
                    commentItemList[i].gameObject.SetActive(true);
                }
            }
            else
            {
                for (int i = 0; i < commentItemList.Count - 1; i++)
                {
                    commentItemList[i].gameObject.SetActive(false);                    
                }
                commentItemList[commentItemList.Count - 1].gameObject.SetActive(true);
            }
        }
    }

    string info;

    #region 댓글 세팅
    public IEnumerator SettingCommentList()
    {
        yield return StartCoroutine(GetCommentInfo());
        if (info != null && !info.Equals(""))
        {
            string[] notices = info.Split('¶');
            for (int i = 0; i < notices.Length - 1; i++)
            {
                //token, user_guid, content, insert_time 순서
                string[] infos = notices[i].Split('☞');
                yield return StartCoroutine(CreateCommentItem(infos[0], infos[1], infos[2], infos[3]));
            }
        }
    }

    IEnumerator CreateCommentItem(string token, string guid, string content, string createTime)
    {
        GameObject item;
        if (commentItemList.Count.Equals(0))
            item = commentItem;
        else
            item = Instantiate(commentItem, commentContent, false);
        item.SetActive(true);

        NoticeCommentItem itemScript = item.GetComponent<NoticeCommentItem>();
        itemScript.token = token;
        itemScript.WriterGUID = guid;
        yield return StartCoroutine(itemScript.SettingUserInfo(guid));
        itemScript.commentText.text = content;

        DateTime insertTime = DateTime.Parse(createTime);
        string timeText = insertTime.Year + "년 " + insertTime.Month + "월 " + insertTime.Day + "일 " + insertTime.Hour + "시 " + insertTime.Minute + "분";
        itemScript.dateText.text = timeText;
        itemScript.delFuction = DeleteCommentProcess;
        commentItemList.Add(itemScript);
        expandCommentPageButtonObject.transform.GetChild(0).GetComponent<Text>().text = "공지사항 댓글 " + commentItemList.Count + "개";
        expandCommentPageButtonObject.SetActive(true);
        CommentPageMode = commentPageMode;
    }
    #endregion

    #region 공지 작성, 수정, 삭제 UI
    ///<summary>
    ///공지 수정 UI 띄우는 버튼
    ///</summary>
    public void ModifyNoticeButton()
    {
        if (!noticeModifyInputObject.activeSelf)
        {
            noticeModifyInputObject.GetComponent<InputField>().text = contentText.text;
            contentText.gameObject.SetActive(false);
            noticeModifyInputObject.SetActive(true);
        }
        else
        {
            noticeModifyInputObject.GetComponent<InputField>().text = "";
            contentText.gameObject.SetActive(true);
            noticeModifyInputObject.SetActive(false);
        }
    }

    ///<summary>
    ///공지 삭제 버튼
    ///</summary>
    public void DeleteNoticeButton()
    {
        CommonInteraction.Instance.ConfirmPanelUpdate("선택하신 공지사항을 삭제하시겠습니까?");
        CommonInteraction.Instance.confirmFunc = CheckDeleteNotice;
    }

    void CheckDeleteNotice(bool check)
    {
        if (check) StartCoroutine(DeleteNoticeInfo());
    }

    ///<summary>
    ///공지 수정 완료 버튼
    ///</summary>
    public void ContentModifyCompleteButton() => StartCoroutine(UpdateNoticeInfo());

    ///<summary>
    ///파일 다운로드 버튼
    ///</summary>
    public void DownloadAttachmentButton()
    {
        string fileDir = "/../storage/data/" + StudyPreparation.Instance.studyData.guid + "/noticefile/" + token + "/";
        StartCoroutine(DataManager.Instance.FileDownload(fileDir, fileName));
    }
    #endregion

    #region 댓글 관련 UI, 기능
    public void ExpandCommentPageButton()
    {
        if (commentPageMode)
            CommentPageMode = false;
        else
            CommentPageMode = true;
    }

    public void AddCommentButton()
    {
        StartCoroutine(AddCommentInfo());
    }

    public void DeleteCommentProcess(NoticeCommentItem commentItem)
    {
        for (int i = 0; i < commentItemList.Count; i++)
        {
            if(commentItemList[i].Equals(commentItem))
            {
                if (commentItemList[i].transform.GetSiblingIndex().Equals(0))
                    commentItemList[i].gameObject.SetActive(false);
                else
                    Destroy(commentItemList[i].gameObject);

                commentItemList.RemoveAt(i);
                break;
            }
        }

        expandCommentPageButtonObject.transform.GetChild(0).GetComponent<Text>().text = "공지사항 댓글 " + commentItemList.Count + "개";
        if (commentItemList.Count.Equals(0))
        {
            commentPageMode = false;
            expandCommentPageButtonObject.SetActive(false);
        }
    }

    #endregion

    #region 공지사항 updat, delete 관련 함수, 코루틴
    IEnumerator SettingUserInfo(string guid)
    {
        if (!guid.Equals("system"))
        {
            yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.NICKNAME, guid));
            string name = DataManager.Instance.info;
            yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.AVATAR, guid));
            string avatarInfo = DataManager.Instance.info;
            writerNameText.text = name;
            SetFaceImage(avatarInfo);
        }
    }

    void SetFaceImage(string avatarInfo)
    {
        string[] partsInfo = avatarInfo.Split(',');

        for (int i = 0; i < partsInfo.Length; i++)
        {
            Sprite[] sprites;
            string[] info = partsInfo[i].Split('_');
            string parts = info[0];
            int num = int.Parse(info[1]);

            switch (parts)
            {
                case "h":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Hair/hair_" + num);
                    if (!sprites.Length.Equals(0))
                        faceImages[hair].sprite = sprites[0];
                    break;
                case "e":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Eye/eye_" + num);
                    if (!sprites.Length.Equals(0))
                        faceImages[eyes].sprite = sprites[0];
                    break;
                case "t":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Top/top_" + num);
                    if (!sprites.Length.Equals(0))
                        faceImages[top].sprite = sprites[0];
                    break;
                case "sk":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Body/body_" + num);
                    if (!sprites.Length.Equals(0))
                        faceImages[body].sprite = sprites[0];
                    break;
                default:
                    continue;
            }
        }
    }

    public IEnumerator UpdateNoticeInfo()
    {
        string noticeInfoUrl = "https://stubugs.com/php/noticeinfo.php";
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.UPDATE);
        form.AddField("Token", token);
        form.AddField("Content", noticeModifyInputObject.GetComponent<InputField>().text);

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
                        CommonInteraction.Instance.InfoPanelUpdate("공지사항 내용을 수정하는데 실패했습니다.");
                        break;
                    case CheckSuccessed.SUCCESS:
                        CommonInteraction.Instance.InfoPanelUpdate("공지사항 내용을 수정하였습니다.");
                        contentText.text = noticeModifyInputObject.GetComponent<InputField>().text;
                        ModifyNoticeButton();
                        break;
                }
            }
        }
    }

    public IEnumerator DeleteNoticeInfo()
    {
        string noticeInfoUrl = "https://stubugs.com/php/noticeinfo.php";
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.DEL);
        form.AddField("Token", token);

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
                        CommonInteraction.Instance.InfoPanelUpdate("공지사항 내용을 삭제하는데 실패했습니다.");
                        break;
                    case CheckSuccessed.SUCCESS:
                        CommonInteraction.Instance.InfoPanelUpdate("공지사항 삭제가 완료되었습니다.");
                        Destroy(gameObject);
                        break;
                }
            }
        }
    }

    public IEnumerator GetCommentInfo()
    {
        string noticeCommentInfoUrl = "https://stubugs.com/php/noticecommentinfo.php";
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.GET);
        form.AddField("NoticeToken", token);        

        using (UnityWebRequest webRequest = UnityWebRequest.Post(noticeCommentInfoUrl, form))
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
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = check[1];
                        break;
                }
            }
        }
    }

    public IEnumerator AddCommentInfo()
    {
        Guid tokenGuid = Guid.NewGuid();
        string commentToken = tokenGuid.ToString();
        string guid = (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"];
        string noticeCommentInfoUrl = "https://stubugs.com/php/noticecommentinfo.php";
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.SET);
        form.AddField("NoticeToken", token);
        form.AddField("Token", commentToken);
        form.AddField("UserGUID", guid);
        form.AddField("Content", commentInput.text);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(noticeCommentInfoUrl, form))
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
                        CommonInteraction.Instance.InfoPanelUpdate("댓글 게시에 실패하였습니다.");
                        break;
                    case CheckSuccessed.SUCCESS:
                        yield return StartCoroutine(CreateCommentItem(commentToken, guid, commentInput.text, check[1]));
                        commentInput.text = "";
                        break;
                }
            }
        }
    }

    #endregion
}
