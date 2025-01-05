using UnityEngine;

namespace starinc.io.kingnslave
{
    public class SafeArea : MonoBehaviour
    {
        void Start()
        {
            SafeAreaSetting();
        }

        private void SafeAreaSetting()
        {
            var safeArea = Screen.safeArea;
            var minAnchor = safeArea.position;
            var maxAnchor = minAnchor + safeArea.size;

            minAnchor.x /= Screen.width;
            minAnchor.y /= Screen.height;
            maxAnchor.x /= Screen.width;
            maxAnchor.y /= Screen.height;

            var _rectTransform = (RectTransform)transform;
            _rectTransform.anchorMin = minAnchor;
            _rectTransform.anchorMax = maxAnchor;
        }

        private void OnRectTransformDimensionsChange()
        {
            SafeAreaSetting();
        }
    }
}