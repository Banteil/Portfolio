using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class TapMenu : ToggleGroup
    {
        public float MinWidth = 100f;
        public float MaxWidth
        {
            get
            {
                var rectTr = (RectTransform)transform;
                var max = rectTr.sizeDelta.x - (MinWidth * (m_Toggles.Count - 1));
                return max;
            }
        }

        public void ResetLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }

        public Toggle GetTapIndex(int index) => m_Toggles[index];

        public Toggle GetActiveTap()
        {
            var activeToggle = m_Toggles.Find((toggle) => toggle.isOn == true);
            return activeToggle;
        }
    }
}
