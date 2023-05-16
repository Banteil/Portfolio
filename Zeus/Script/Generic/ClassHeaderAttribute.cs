using System;

namespace Zeus
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class ClassHeaderAttribute : Attribute
    {
        public string Header;
        public bool OpenClose;
        public string IconName;
        public bool UseHelpBox;
        public string HelpBoxText;

        public ClassHeaderAttribute(string header, bool openClose = true, string iconName = "icon_v2", bool useHelpBox = false, string helpBoxText = "")
        {
            this.Header = header.ToUpper();
            this.OpenClose = openClose;
            this.IconName = iconName;
            this.UseHelpBox = useHelpBox;
            this.HelpBoxText = helpBoxText;
        }

        public ClassHeaderAttribute(string header, string helpBoxText)
        {
            this.Header = header.ToUpper();
            this.OpenClose = true;
            this.IconName = "icon_v2";
            this.UseHelpBox = true;
            this.HelpBoxText = helpBoxText;
        }
    }
}