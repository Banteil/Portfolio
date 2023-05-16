using UnityEngine;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VideoFrameControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public VideoPlayer video;
    Slider tracking;

    void Awake()
    {        
        tracking = GetComponent<Slider>();        
    }

    public void OnPointerDown(PointerEventData eventData) => SlideManager.Instance.isControl = true;

    public void OnPointerUp(PointerEventData eventData) => SlideManager.Instance.SetVideoFrame((float)tracking.value);

    void Update()
    {
        if (video.enabled && !SlideManager.Instance.isControl)
        {
            tracking.value = (float)video.frame / (float)video.frameCount;
            if (tracking.value >= 1f)
                SlideManager.Instance.VideoControl(1);
        }
    }
}
