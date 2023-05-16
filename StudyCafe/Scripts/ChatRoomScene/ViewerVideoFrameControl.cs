using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ViewerVideoFrameControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public FileViewer viewer;
    Slider tracking;

    void Start()
    {
        tracking = GetComponent<Slider>();        
    }

    public void OnPointerDown(PointerEventData eventData) => viewer.isControl = true;

    public void OnPointerUp(PointerEventData eventData) => viewer.SetVideoFrame((float)tracking.value);

    void Update()
    {
        if (viewer.videoPlayer.enabled && !viewer.isControl)
        {
            tracking.value = (float)viewer.videoPlayer.frame / (float)viewer.videoPlayer.frameCount;
            if (tracking.value >= 1f)
                viewer.VideoControlButton(1);
        }
    }
}
