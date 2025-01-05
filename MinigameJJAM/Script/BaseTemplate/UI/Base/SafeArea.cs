using UnityEngine;

namespace starinc.io
{
    public class SafeArea : MonoBehaviour
    {
        private void Awake()
        {
            SetToFullScreen();
        }

        private void Start()
        {
            SafeAreaSetting();
        }

        private void SetToFullScreen()
        {
            var rectTransform = (RectTransform)transform;
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.rotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;

            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
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

            var rectTransform = (RectTransform)transform;
            rectTransform.anchorMin = minAnchor;
            rectTransform.anchorMax = maxAnchor;
        }

        private void OnRectTransformDimensionsChange()
        {
            SafeAreaSetting();
        }
    }
}