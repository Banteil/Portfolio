using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class FileViewer : MonoBehaviour
{
    public FileType fileType;

    [Header("StatusBar")]
    public Text fileNameText;
    public RectTransform uiCanvasRect;
    Vector2 distance;

    [Header("ViewerPanel")]
    public Image imageViewer;
    public RawImage videoViewer;
    public VideoPlayer videoPlayer;
    public AudioSource audioViewer;
    bool isPlaying;

    [Header("ControlBar")]
    public Slider videoFrameSlider;
    public Button prevButton, nextButton, playButton, stopButton;
    public Sprite playImage, pauseImage;
    public Texture2D cursorImage;
    [HideInInspector]
    public bool isControl;
    string studyGUID;
    public string StudyGUID
    {
        set
        {
            studyGUID = value;
            fileDir = "/../storage/data/" + studyGUID + "/";
            videoUrl = "https://stubugs.com/data/" + studyGUID + "/";
        }
    }
    public RectTransform lockPos;
    Vector2 prevChangePos; //직전 프레임의 좌표 저장용 벡터
    bool isDrag; //현재 드래그 여부 체크용 bool
    bool isLockXRight, isLockXLeft, isLockYUp, isLockYDown; //상하좌우 크기 조절 락 체크용 bool

    string fileDir; //파일 경로
    string videoUrl; //비디오 경로
    string fileName; //파일명    
    List<byte[]> pdfImageList = new List<byte[]>(); //PDF 전용 이미지 리스트
    int currentPage = 0; //PDF 뷰어용 현재 페이지
    int maxPage; //PDF 뷰어용 최대 페이지

    const float minXSize = 650f;
    const float minYSize = 120f;

    #region 뷰어 정보 세팅
    /// <summary>
    /// 뷰어 정보 초기화
    /// </summary>
    void Initialized()
    {
        RectTransform rT = gameObject.GetComponent<RectTransform>();
        rT.anchoredPosition = new Vector2(-635f, -215f);
        rT.sizeDelta = new Vector2(650f, 650f);
        currentPage = 0;
        videoViewer.gameObject.SetActive(false);
        imageViewer.gameObject.SetActive(false);
        prevButton.interactable = false;
        nextButton.interactable = false;
        playButton.interactable = false;
        stopButton.interactable = false;
        videoFrameSlider.value = 0;
        imageViewer.sprite = null;
        videoPlayer.enabled = false;
        fileNameText.text = null;
        isPlaying = false;
        VideoControlButtonImageSwap(false);
        pdfImageList = new List<byte[]>();
    }

    /// <summary>
    /// 교재 아이템에 파일 정보를 세팅하는 함수
    /// </summary>
    public void FileSetting(string name)
    {
        gameObject.SetActive(true);
        fileName = name;
        fileNameText.text = name;
        string[] fileInfo = name.Split('.');
        string extension = fileInfo[1];
        fileType = DataManager.Instance.CheckFileType(extension);
        Debug.Log(fileType);
        CheckFileType(fileType);
    }

    /// <summary>
    /// 파일의 타입을 체크하여 타입에 맞게 뷰어를 세팅하는 함수
    /// </summary>
    void CheckFileType(FileType type)
    {
        FileTypeUIInteract(type);
        switch (type)
        {
            case FileType.IMAGE:
                videoViewer.gameObject.SetActive(false);
                imageViewer.gameObject.SetActive(true);
                audioViewer.gameObject.SetActive(false);
                StartCoroutine(ImageSetting());
                break;
            case FileType.VIDEO:
                imageViewer.sprite = null;
                videoViewer.gameObject.SetActive(true);
                imageViewer.gameObject.SetActive(false);
                audioViewer.gameObject.SetActive(false);
                VideoSetting();
                break;
            case FileType.DOCUMENT:
                videoViewer.gameObject.SetActive(false);
                imageViewer.gameObject.SetActive(true);
                audioViewer.gameObject.SetActive(false);
                StartCoroutine(DocumentSetting());
                break;
            case FileType.AUDIO:
                imageViewer.sprite = null;
                videoViewer.gameObject.SetActive(false);
                imageViewer.gameObject.SetActive(false);
                audioViewer.gameObject.SetActive(true);
                StartCoroutine(AuidoSetting());
                break;
            default:
                videoViewer.gameObject.SetActive(false);
                imageViewer.gameObject.SetActive(true);
                audioViewer.gameObject.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// 파일 타입에 맞게 UI 상호작용 요소를 변경하는 함수
    /// </summary>
    public void FileTypeUIInteract(FileType fileType)
    {
        switch (fileType)
        {
            case FileType.VIDEO:
                prevButton.interactable = false;
                nextButton.interactable = false;
                playButton.interactable = true;
                stopButton.interactable = true;
                videoFrameSlider.interactable = true;
                break;
            default:
                prevButton.interactable = false;
                nextButton.interactable = false;
                playButton.interactable = false;
                stopButton.interactable = false;
                videoFrameSlider.interactable = false;
                break;
        }
    }

    /// <summary>
    /// 이미지 정보를 받아와 sprite에 세팅하는 함수
    /// </summary>
    IEnumerator ImageSetting()
    {
        yield return StartCoroutine(DataManager.Instance.GetFileData(fileDir, fileName));
        byte[] data = DataManager.Instance.DataInfo;
        imageViewer.sprite = FileBrowserDialogLib.GetSprite(data);
    }

    /// <summary>
    /// 동영상 정보를 받아와 뷰어에 세팅하는 함수
    /// </summary>
    void VideoSetting()
    {
        if (videoPlayer.playOnAwake)
        {
            isPlaying = true;
            VideoControlButtonImageSwap(true);
        }
        else
        {
            isPlaying = false;
            VideoControlButtonImageSwap(false);
        }

        videoPlayer.url = videoUrl + fileName;
        videoPlayer.enabled = true;
    }

    /// <summary>
    /// 문서(pdf) 파일의 정보를 뷰어에 세팅하는 함수
    /// </summary>
    IEnumerator DocumentSetting()
    {
        string[] extension = fileName.Split('.');
        if (extension[1].Equals("pdf"))
        {
            int count = 1;
            while (true)
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
            nextButton.interactable = true;
        }
        imageViewer.sprite = FileBrowserDialogLib.GetSprite(pdfImageList[0]);
    }

    /// <summary>
    /// 동영상 정보를 받아와 뷰어에 세팅하는 함수
    /// </summary>
    IEnumerator AuidoSetting()
    {
        WWW www = new WWW(videoUrl + fileName);
        yield return www;
        audioViewer.clip = www.GetAudioClip(false);
        audioViewer.Play();
    }

    #endregion

    #region StatusBar UI 조작
    /// <summary>
    /// 뷰어를 닫는 버튼 함수
    /// </summary>
    public void CloseButton()
    {
        Initialized();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 뷰어 위치 조절 드래그를 시작할 때 판정
    /// </summary>
    public void StartDrag()
    {
        InputControl.Instance.isValid = false;
        RectTransform rT = gameObject.GetComponent<RectTransform>();
        distance = rT.anchoredPosition - (Vector2)Input.mousePosition;
    }

    /// <summary>
    /// 뷰어 위치 조절 중 판정
    /// </summary>
    public void Drag()
    {
        RectTransform rT = gameObject.GetComponent<RectTransform>();
        rT.anchoredPosition = (Vector2)Input.mousePosition + distance;

        float xSize = uiCanvasRect.rect.width - rT.rect.width;
        if (rT.anchoredPosition.x > 0f)
            rT.anchoredPosition = new Vector2(0f, rT.anchoredPosition.y);
        else if (rT.anchoredPosition.x < -xSize)
            rT.anchoredPosition = new Vector2(-xSize, rT.anchoredPosition.y);

        float ySize = uiCanvasRect.rect.height - 60f;
        if (rT.anchoredPosition.y > 0f)
            rT.anchoredPosition = new Vector2(rT.anchoredPosition.x, 0f);
        else if (rT.anchoredPosition.y < -ySize)
            rT.anchoredPosition = new Vector2(rT.anchoredPosition.x, -ySize);
    }

    public void EndDrag()
    {
        InputControl.Instance.isValid = true;
    }

    #endregion

    #region ControlBar UI 조작
    /// <summary>
    /// 페이지 변경 버튼 함수
    /// </summary>
    public void PageSwapButton(int type)
    {
        if (type.Equals(0))
            currentPage--;
        else
            currentPage++;

        imageViewer.sprite = FileBrowserDialogLib.GetSprite(pdfImageList[currentPage]);
        imageViewer.preserveAspect = true;
        PageSwapButtonInteract(type, currentPage, maxPage);
    }

    /// <summary>
    /// 현재, 최대 페이지 상태에 따라 상호작용 여부를 변경하는 함수 
    /// </summary>
    public void PageSwapButtonInteract(int type, int currentPage, int maxPage)
    {
        if (type.Equals(0))
        {
            nextButton.interactable = true;
            if (currentPage <= 0)
                prevButton.interactable = false;
        }
        else
        {
            prevButton.interactable = true;
            if (currentPage >= maxPage - 1)
                nextButton.interactable = false;
        }
    }

    /// <summary>
    /// 일시정지, 재생 버튼의 기능을 처리하는 버튼 함수
    /// </summary>
    public void VideoControlButton(int type)
    {
        switch (type)
        {
            case 0:
                if (!isPlaying)
                {
                    isPlaying = true;
                    videoPlayer.Play();
                }
                else
                {
                    isPlaying = false;
                    videoPlayer.Pause();
                }
                VideoControlButtonImageSwap(isPlaying);
                break;
            case 1:
                isPlaying = false;
                videoPlayer.Stop();
                VideoControlButtonImageSwap(isPlaying);
                break;
        }
    }

    /// <summary>
    /// 일시정지, 재생 버튼 이미지를 교체하는 함수
    /// </summary>
    public void VideoControlButtonImageSwap(bool isPlaying)
    {
        Image playButtonImage = playButton.gameObject.GetComponent<Image>();
        if (isPlaying)
            playButtonImage.sprite = pauseImage;
        else
            playButtonImage.sprite = playImage;
    }

    /// <summary>
    /// 프레임 UI 클릭 시 해당 프레임 위치에 맞게 영상을 재생시키는 함수
    /// </summary>
    public void SetVideoFrame(float trackingValue)
    {
        float frame = trackingValue * (float)videoPlayer.frameCount;
        videoPlayer.frame = (long)frame;
        isControl = false;
    }
    #endregion

    #region 뷰어 사이즈 조절
    /// <summary>
    /// 뷰어 크기 조절 UI에 마우스를 올려놓았을 때 처리 함수
    /// </summary>
    public void MouseEnter()
    {
        Vector2 cursorHotspot = new Vector2(cursorImage.width / 2, cursorImage.height / 2);
        Cursor.SetCursor(cursorImage, cursorHotspot, CursorMode.Auto);
    }

    /// <summary>
    /// 뷰어 크기 조절 UI에서 마우스를 벗어나게 했을 때 처리 함수
    /// </summary>
    public void MouseExit()
    {
        if (isDrag) return;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    /// <summary>
    /// 뷰어 크기 조절을 시작할 때 처리 함수
    /// </summary>
    public void StartChangeSize()
    {
        InputControl.Instance.isValid = false;
        isDrag = true;
        prevChangePos = Input.mousePosition;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        if(isDrag)
        {
            ChangeSize();
        }
    }

    /// <summary>
    /// 뷰어 크기 조절을 진행 중일 때 처리 함수
    /// </summary>
    public void ChangeSize()
    {
        RectTransform rT = gameObject.GetComponent<RectTransform>();
        //이전 좌표와 비교해서 얼마만큼 움직였는지 changePos로 판단 (현재 마우스 좌표 - 이전 마우스 좌표)
        Vector2 changePos = (Vector2)Input.mousePosition - prevChangePos;
        Vector2 posX = new Vector2(changePos.x, 0f);
        Vector2 posY = new Vector2(0f, changePos.y);
        Vector2 lockPos = Camera.main.WorldToScreenPoint(this.lockPos.position);

        if (isLockXRight && Input.mousePosition.x < lockPos.x)
            isLockXRight = false;
        if (isLockXLeft && Input.mousePosition.x >= lockPos.x)
            isLockXLeft = false;
        if (isLockYUp && Input.mousePosition.y < lockPos.y)
            isLockYUp = false;
        if (isLockYDown && Input.mousePosition.y >= lockPos.y)
            isLockYDown = false;

        //이동 거리만큼 x좌표 사이즈 조절
        if (!isLockXRight && !isLockXLeft)
        {
            rT.sizeDelta -= posX;
            //x좌표 사이즈 조절 락
            if (rT.rect.width <= minXSize) //최소 크기
            {
                rT.sizeDelta = new Vector2(minXSize, rT.rect.height);
                isLockXRight = true;
            }
            else if (rT.rect.width > uiCanvasRect.rect.width) //최대 크기
            {
                rT.sizeDelta = new Vector2(uiCanvasRect.rect.width, rT.rect.height);
                isLockXLeft = true;
            }
        }

        //이동 거리만큼 y좌표 사이즈 조절
        if (!isLockYUp && !isLockYDown)
        {
            rT.sizeDelta -= posY;
            //y좌표 사이즈 조절 락
            if (rT.rect.height <= minYSize) //최소 크기
            {
                rT.sizeDelta = new Vector2(rT.rect.width, minYSize);
                isLockYUp = true;
            }
            else if (rT.rect.height > uiCanvasRect.rect.height) //최대 크기
            {
                rT.sizeDelta = new Vector2(rT.rect.width, uiCanvasRect.rect.height);
                isLockYDown = true;
            }
        }

        //사이즈 조절 처리 후 현재 마우스 위치를 이전 마우스 좌표로 저장
        prevChangePos = Input.mousePosition;
    }

    /// <summary>
    /// 뷰어 크기 조절을 완료했을 때 처리 함수
    /// </summary>
    public void EndChange()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.lockState = CursorLockMode.None;
        isDrag = false;
        isLockXRight = false;
        isLockXLeft = false;
        isLockYUp = false;
        isLockYDown = false;
        InputControl.Instance.isValid = true;
    }
    #endregion
}
