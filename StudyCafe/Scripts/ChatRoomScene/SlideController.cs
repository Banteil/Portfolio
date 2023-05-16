using UnityEngine;
using UnityEngine.UI;

public class SlideController : MonoBehaviour
{
    public Text slideIndex, fileName;
    public Button slidePrevButton, slideNextButton, pagePrevButton, pageNextButton;
    public Button videoPlayButton, videoStopButton;
    public Sprite playImage, pauseImage;
    public Slider videoFrameSlider;

    public void SetSliderControllerInfo(SlideSettingInfo info, int currentIndex, int maxIndex)
    {
        slideIndex.text = "[" + (info.index[currentIndex] + 1).ToString("D2") + "/" + maxIndex.ToString("D2") + "]";
        fileName.text = info.fileNames[currentIndex];
        if (currentIndex.Equals(0))
        {
            slidePrevButton.interactable = false;
            slideNextButton.interactable = true;
        }
        else if (currentIndex.Equals(maxIndex - 1))
        {
            slidePrevButton.interactable = true;
            slideNextButton.interactable = false;
        }
        else
        {
            slidePrevButton.interactable = true;
            slideNextButton.interactable = true;
        }
    }

    public void PageSwapButtonInteract(int type, int currentPage, int maxPage)
    {
        if (type.Equals(0))
        {
            pageNextButton.interactable = true;
            if (currentPage <= 0)
                pagePrevButton.interactable = false;
        }
        else
        {
            pagePrevButton.interactable = true;
            if (currentPage >= maxPage - 1)
                pageNextButton.interactable = false;
        }
    }

    public void FileTypeUIInteract(FileType fileType)
    {
        switch (fileType)
        {
            case FileType.VIDEO:
                pagePrevButton.interactable = false;
                pageNextButton.interactable = false;
                videoPlayButton.interactable = true;
                videoStopButton.interactable = true;
                videoFrameSlider.interactable = true;
                break;
            default:
                pagePrevButton.interactable = false;
                pageNextButton.interactable = false;
                videoPlayButton.interactable = false;
                videoStopButton.interactable = false;
                videoFrameSlider.interactable = false;
                break;
        }
    }

    public void VideoControlButtonImageSwap(bool isPlaying)
    {
        Image playButtonImage = videoPlayButton.gameObject.GetComponent<Image>();
        if (isPlaying)
            playButtonImage.sprite = pauseImage;
        else
            playButtonImage.sprite = playImage;
    }
}
