using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SlideManager : Singleton<SlideManager>
{
    [Header("ETC")]
    PhotonView pV;

    [Header("SlideCanvas")]
    public GameObject slideUICanvas;
    public VideoPlayer slideVideoPlayer;
    public Camera slideCamera;
    public RenderTexture slideRenderTexture;

    [Header("SlideControl")]
    public SlideController mainController;
    public SlideController slideScreenController;
    public bool isControl = false;
    public int currentPage = 0;
    int maxPage;
    bool isVideoPlaying;
    List<byte[]> pdfImageList = new List<byte[]>();

    [Header("SlideObject")]
    public Image slideImage;
    public RawImage slideRawImage;
    public RenderTexture videoRenderTexture;
    public SpotlightEffect imageSpotlight, videoSpotlight;
    public bool onSlide = false; //전체 화면 슬라이드 여부

    [Header("SlideChatObject")]
    public GameObject myChatBox;
    public GameObject otherChatBox;
    public Transform chatContent;

    [Header("MasterUI")]
    public GameObject masterSlidePanel;
    public GameObject drawOptionUI;
    public GameObject endLectureButton;

    string videoUrl;
    string fileDir;
    bool isDisplay;

    [Header("PrefabObject")]
    Sprite rightArrow, leftArrow;

    //슬라이드 최대 인덱스
    int maxIndex;
    //현재 슬라이드 인덱스
    //값을 넣으면 바로 text 값 변경 및 버튼 interactable 설정, 슬라이드 정보 스왑 진행
    int currentIndex;
    public int CurrentIndex
    {   
        get { return currentIndex; }
        set
        {
            if (value < 0 || value >= maxIndex) return;

            currentIndex = value;
            mainController.SetSliderControllerInfo(slideSettingInfo, currentIndex, maxIndex);
            slideScreenController.SetSliderControllerInfo(slideSettingInfo, currentIndex, maxIndex);
            CheckFileType(slideSettingInfo.fileTypes[currentIndex]);
        }
    }

    [Header("SlideInfo")]
    bool isSlideNull = true;
    SlideSettingInfo slideSettingInfo = new SlideSettingInfo();
    public SlideSettingInfo SlideSetting
    {
        get { return slideSettingInfo; }
        set
        {
            isSlideNull = false;
            slideSettingInfo = value;
            maxIndex = slideSettingInfo.index.Count;            
            CurrentIndex = 0;
        }
    }

    private void Awake()
    {
        pV = GetComponent<PhotonView>();
        if(slideVideoPlayer.playOnAwake)
        {
            isVideoPlaying = true;
            mainController.VideoControlButtonImageSwap(true);
            slideScreenController.VideoControlButtonImageSwap(true);
        }
        else
        {
            isVideoPlaying = false;
            mainController.VideoControlButtonImageSwap(false);
            slideScreenController.VideoControlButtonImageSwap(false);
        }

        rightArrow = Resources.Load<Sprite>("UISprite/month_right_arrow");
        leftArrow = Resources.Load<Sprite>("UISprite/month_left_arrow");
        fileDir = "/../storage/data/" + DataManager.Instance.currentStudyData.guid + "/";
        videoUrl = "https://stubugs.com/data/" + DataManager.Instance.currentStudyData.guid + "/";
    }

    private void Update()
    {
        if (DataManager.isMaster && slideVideoPlayer.isPlaying)
        {
            if (onSlide)
                RoomManager.Instance.chatRoom.syncObject.videoFrame = slideScreenController.videoFrameSlider.value;
            else
                RoomManager.Instance.chatRoom.syncObject.videoFrame = mainController.videoFrameSlider.value;
        }
        
    }

    #region 슬라이드 UI 조작, RPC
    /// <summary>
    /// 슬라이드 스크린 모드로 전환하는 함수
    /// </summary>
    public void OnSlideScreen()
    {
        //미니게임 플레이 시엔 슬라이드 전체화면 보기 불가능
        if (RoomManager.Instance.chatRoom.playingMiniGame) return;

        onSlide = true;
        InputControl.Instance.isValid = false;
        InputControl.Instance.enterKey = RoomManager.Instance.chatRoom.SlideSend;
        InputControl.Instance.cancel = OffSlideScreen;
        slideCamera.targetTexture = null;
        RoomManager.Instance.uiCanvas.SetActive(false);
        RoomManager.Instance.roomObj.slideScreen.SetActive(false);
        slideUICanvas.SetActive(true);
        if (DataManager.isMaster)
        {
            RoomManager.Instance.masterCanvas.SetActive(false);
            masterSlidePanel.SetActive(true);
        }
        else
            RoomManager.Instance.grouperCanvas.SetActive(false);

    }

    /// <summary>
    /// 슬라이드 스크린 모드를 종료하는 함수
    /// </summary>
    public void OffSlideScreen()
    {
        //강의 모드일 경우 자의로 종료할 수 없음
        if (RoomManager.Instance.chatRoom.isLecture)
        {
            CommonInteraction.Instance.InfoPanelUpdate("강의 중에는 기존 화면으로 전환할 수 없습니다.");
            return;
        }

        onSlide = false;
        InputControl.Instance.isValid = true;
        InputControl.Instance.enterKey = RoomManager.Instance.Send;
        InputControl.Instance.cancel = RoomManager.Instance.LeaveRoomButton;

        if (!isSlideNull)
        {
            if (slideSettingInfo.fileTypes[currentIndex].Equals(FileType.VIDEO))
                videoSpotlight.IsHighlightSpot = false;
            else
                imageSpotlight.IsHighlightSpot = false;            
        }

        slideCamera.targetTexture = slideRenderTexture;
        RoomManager.Instance.uiCanvas.SetActive(true);
        RoomManager.Instance.roomObj.slideScreen.SetActive(true);
        slideUICanvas.SetActive(false);
        for (int i = 0; i < chatContent.childCount; i++)
        {
            Destroy(chatContent.GetChild(i).gameObject);
        }        

        if (DataManager.isMaster)
        {
            RoomManager.Instance.masterCanvas.SetActive(true);
            masterSlidePanel.SetActive(false);
        }
        else
        {
            RoomManager.Instance.grouperCanvas.SetActive(true);
        }
    }

    /// <summary>
    /// 강의 모드 시 모드를 종료하는 함수
    /// </summary>
    public void EndLectureButton() => pV.RPC("EndLectureProcess", RpcTarget.AllViaServer);

    [PunRPC]
    void EndLectureProcess()
    {
        RoomManager.Instance.chatRoom.isLecture = false;
        endLectureButton.SetActive(false);
        OffSlideScreen();
    }

    /// <summary>
    /// 슬라이더 조작으로 비디오 프레임을 설정하는 함수
    /// </summary>
    public void SetVideoFrame(float trackingValue)
    {
        pV.RPC("VideoFrameProcess", RpcTarget.AllViaServer, trackingValue);
    }

    /// <summary>
    /// [RPC]슬라이더 현재 value 값을 토대로 비디오의 프레임을 설정하는 함수
    /// </summary>
    [PunRPC]
    void VideoFrameProcess(float trackingValue)
    {
        float frame = trackingValue * (float)slideVideoPlayer.frameCount;
        slideVideoPlayer.frame = (long)frame;
        isControl = false;
    }

    /// <summary>
    /// 슬라이드 페이지를 넘기는 버튼에 할당되는 함수
    /// </summary>
    public void SlideSwapButton(int value) => pV.RPC("SlideSwap", RpcTarget.AllViaServer, value);

    /// <summary>
    /// [RPC]매개변수 value 값에 따라 이전 or 다음 슬라이드 페이지를 표시하는 함수
    /// (0 - 이전, 1 - 다음)
    /// </summary>
    [PunRPC]
    void SlideSwap(int value)
    {
        slideVideoPlayer.enabled = false;
        mainController.videoFrameSlider.value = 0;
        slideScreenController.videoFrameSlider.value = 0;
        pdfImageList = new List<byte[]>();
        currentPage = 0;

        if (slideSettingInfo.fileTypes[currentIndex].Equals(FileType.VIDEO))
            videoSpotlight.IsHighlightSpot = false;
        else
            imageSpotlight.IsHighlightSpot = false;

        if (value.Equals(0))
            CurrentIndex--;
        else
            CurrentIndex++;

        if (DataManager.isMaster)
        {
            RoomManager.Instance.chatRoom.syncObject.slideIndex = CurrentIndex;
            RoomManager.Instance.chatRoom.syncObject.slidePage = currentPage;
        }
    }

    public void VideoControlButton(int type) => pV.RPC("VideoControl", RpcTarget.AllViaServer, type);

    /// <summary>
    /// [RPC]슬라이더 현재 value 값을 토대로 비디오의 프레임을 설정하는 함수
    /// </summary>
    [PunRPC]
    public void VideoControl(int type)
    {
        switch (type)
        {            
            case 0:
                if (!isVideoPlaying)
                {
                    isVideoPlaying = true;
                    slideVideoPlayer.Play();
                    if (DataManager.isMaster)
                        RoomManager.Instance.chatRoom.syncObject.isPause = false;
                }
                else
                {
                    isVideoPlaying = false;
                    slideVideoPlayer.Pause();
                    if (DataManager.isMaster)
                        RoomManager.Instance.chatRoom.syncObject.isPause = true;
                }
                mainController.VideoControlButtonImageSwap(isVideoPlaying);
                slideScreenController.VideoControlButtonImageSwap(isVideoPlaying);
                break;
            case 1:
                isVideoPlaying = false;
                slideVideoPlayer.Stop();
                if (DataManager.isMaster)
                    RoomManager.Instance.chatRoom.syncObject.isPause = false;
                mainController.VideoControlButtonImageSwap(isVideoPlaying);
                slideScreenController.VideoControlButtonImageSwap(isVideoPlaying);
                break;
        }
    }

    public void PageSwapButton(int type) => pV.RPC("PageSwap", RpcTarget.AllViaServer, type);    

    [PunRPC]
    public void PageSwap(int type)
    {
        if (type.Equals(0))
            currentPage--;
        else
            currentPage++;

        slideImage.sprite = FileBrowserDialogLib.GetSprite(pdfImageList[currentPage]);
        slideImage.preserveAspect = true;
        slideScreenController.PageSwapButtonInteract(type, currentPage, maxPage);
        mainController.PageSwapButtonInteract(type, currentPage, maxPage);

        if (DataManager.isMaster)
            RoomManager.Instance.chatRoom.syncObject.slidePage = currentPage;
    }

    #endregion

    void CheckFileType(FileType type)
    {
        mainController.FileTypeUIInteract(type);
        slideScreenController.FileTypeUIInteract(type);

        switch (type)
        {
            case FileType.IMAGE:
                slideRawImage.gameObject.SetActive(false);
                slideImage.gameObject.SetActive(true);
                StartCoroutine(ImageSetting());
                break;
            case FileType.VIDEO:
                slideImage.sprite = null;
                slideRawImage.gameObject.SetActive(true);
                slideImage.gameObject.SetActive(false);                
                VideoSetting();
                break;
            case FileType.DOCUMENT:
                slideRawImage.gameObject.SetActive(false);
                slideImage.gameObject.SetActive(true);
                StartCoroutine(DocumentSetting());
                break;
            default:
                slideRawImage.gameObject.SetActive(false);
                slideImage.gameObject.SetActive(true);
                break;
        }
    }

    IEnumerator ImageSetting()
    {
        string fileName = SlideSetting.fileNames[CurrentIndex];
        yield return StartCoroutine(DataManager.Instance.GetFileData(fileDir, fileName));
        byte[] data = DataManager.Instance.DataInfo;
        slideImage.sprite = FileBrowserDialogLib.GetSprite(data);
    }

    void VideoSetting()
    {
        string fileName = SlideSetting.fileNames[CurrentIndex];
        slideVideoPlayer.url = videoUrl + fileName;
        slideRawImage.texture = videoRenderTexture;
        slideVideoPlayer.enabled = true;
    }

    IEnumerator DocumentSetting()
    {
        string fileName = SlideSetting.fileNames[CurrentIndex];
        string[] extension = fileName.Split('.');
        if(extension[1].Equals("pdf"))
        {
            int count = 1;
            while(true)
            {                
                string pdfImageName = extension[0] + "_" + count + ".jpg";
                yield return StartCoroutine(DataManager.Instance.GetFileData(fileDir, pdfImageName));
                byte[] pdfData = DataManager.Instance.DataInfo;                
                if (pdfData == null) break;
                else
                {                    
                    pdfImageList.Add(pdfData);
                    count++;
                }

                yield return null;
            }           
        }

        maxPage = pdfImageList.Count;
        if (maxPage > 1)
        {
            slideScreenController.pageNextButton.interactable = true;
            mainController.pageNextButton.interactable = true;
        }
        slideImage.sprite = FileBrowserDialogLib.GetSprite(pdfImageList[0]);
    }

    public void DisplayMasterPanel()
    {
        if (!isDisplay)
        {
            drawOptionUI.transform.localPosition -= new Vector3(270f, 0f, 0f);
            isDisplay = true;
            drawOptionUI.transform.GetChild(0).GetComponent<Image>().sprite = rightArrow;
        }
        else
        {
            drawOptionUI.transform.localPosition += new Vector3(270f, 0, 0);
            isDisplay = false;
            drawOptionUI.transform.GetChild(0).GetComponent<Image>().sprite = leftArrow;
        }
    }

    /// <summary>
    /// 슬라이드 전체 화면에 표시되는 채팅 메세지를 생성, 세팅하는 함수
    /// </summary>
    public void SettingSlideChat(bool isMine, string playerName, string msg)
    {
        GameObject chatBox = Instantiate(isMine ? myChatBox : otherChatBox, chatContent);
        SlideChatBox boxScript = chatBox.GetComponent<SlideChatBox>();
        boxScript.boxRect.sizeDelta = new Vector2(350, boxScript.boxRect.sizeDelta.y);
        boxScript.msg.text = msg;
        if (!isMine)
            boxScript.userName.text = playerName;
        LayoutRebuilder.ForceRebuildLayoutImmediate(boxScript.boxRect);

        float X = boxScript.msgRect.sizeDelta.x + 20;
        float Y = boxScript.msgRect.sizeDelta.y;
        if (Y > 49)
        {
            for (int i = 0; i < 200; i++)
            {
                boxScript.boxRect.sizeDelta = new Vector2(X - i * 2, boxScript.boxRect.sizeDelta.y);
                LayoutRebuilder.ForceRebuildLayoutImmediate(boxScript.boxRect);

                if (Y != boxScript.msgRect.sizeDelta.y)
                {
                    boxScript.boxRect.sizeDelta = new Vector2(X - (i * 2) + 2, Y);
                    break;
                }
            }
        }
        else
            boxScript.boxRect.sizeDelta = new Vector2(X, Y);
    }
    
    public void HighlightButton()
    {
        if(slideSettingInfo.fileTypes[currentIndex].Equals(FileType.VIDEO))
        {
            if (!videoSpotlight.IsHighlightSpot)
                videoSpotlight.IsHighlightSpot = true;
            else
                videoSpotlight.IsHighlightSpot = false;
        }
        else
        {
            if (!imageSpotlight.IsHighlightSpot)
                imageSpotlight.IsHighlightSpot = true;
            else
                imageSpotlight.IsHighlightSpot = false;
        }
    }
}
