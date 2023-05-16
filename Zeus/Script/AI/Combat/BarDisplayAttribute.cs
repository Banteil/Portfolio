using UnityEngine;

namespace Zeus
{
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class BarDisplayAttribute : PropertyAttribute
    {
        public readonly string maxValueProperty;
        public readonly bool showJuntInPlayMode;
        public BarDisplayAttribute(string maxValueProperty, bool showJuntInPlayMode = false)
        {
            this.maxValueProperty = maxValueProperty;
            this.showJuntInPlayMode = showJuntInPlayMode;
        }
    }
}