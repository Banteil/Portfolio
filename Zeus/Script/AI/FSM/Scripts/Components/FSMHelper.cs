using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    using UnityEditor;
    public class GenericMenuItem
    {
        public GUIContent Content;
        public GenericMenu.MenuFunction Func;

        public GenericMenuItem(GUIContent content, GenericMenu.MenuFunction func)
        {
            this.Content = content;
            this.Func = func;
        }
    }
#endif

    public static class FSMHelper
    {
        public const float DragSnap = 5f;
        public static IEnumerable<Type> FindSubClasses(this Type type)
        {
            IEnumerable<Type> exporters = type
             .Assembly.GetTypes()
             .Where(t => t.IsSubclassOf(type) && !t.IsAbstract);
            return exporters;
        }
        public static float NearestRound(float x, float multiple)
        {
            if (multiple < 1)
            {
                float i = (float)Math.Floor(x);
                float x2 = i;
                while ((x2 += multiple) < x) ;
                float x1 = x2 - multiple;
                return (Math.Abs(x - x1) < Math.Abs(x - x2)) ? x1 : x2;
            }
            else
            {
                return (float)Math.Round(x / multiple, MidpointRounding.AwayFromZero) * multiple;
            }
        }
    }
}