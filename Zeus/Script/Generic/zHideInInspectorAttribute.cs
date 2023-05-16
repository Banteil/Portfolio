using System;
using UnityEngine;

namespace Zeus
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class zHideInInspectorAttribute : PropertyAttribute
    {       
        public bool HideProperty { get; set; }
        public string RefbooleanProperty;
        public bool InvertValue;
        public zHideInInspectorAttribute(string refbooleanProperty, bool invertValue = false)
        {
            this.RefbooleanProperty = refbooleanProperty;
            this.InvertValue = invertValue;
        }

    }
}
