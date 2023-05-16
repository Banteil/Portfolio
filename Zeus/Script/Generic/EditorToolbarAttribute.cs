using UnityEngine;

namespace Zeus
{
    public class EditorToolbarAttribute : PropertyAttribute
    {
        public readonly string Title;
        public readonly string Icon;
        public readonly bool UseIcon;
        public readonly bool OverrideChildOrder;
        public readonly bool OverrideIcon;

        public EditorToolbarAttribute(string title, bool useIcon = false, string iconName = "", bool overrideIcon = false, bool overrideChildOrder = false)
        {
            this.Title = title;
            this.Icon = iconName;
            this.UseIcon = useIcon;
            this.OverrideChildOrder = overrideChildOrder;
            this.OverrideIcon = overrideIcon;
        }
    }
}
