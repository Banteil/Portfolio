using System;
using System.Collections.Generic;
using UnityEngine;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "LocalizationTable", menuName = "Scriptable Objects/Localization/LocalizationTable")]
    public class LocalizationTable : ScriptableObject
    {
        [SerializeField]
        private string _code;
        public string Code { get { return _code; } }

        [SerializeField]
        private List<LocalizationEntry> _entries = new List<LocalizationEntry>();

        public string GetLocalizedString(string key)
        {
            LocalizationEntry entry = _entries.Find(e => e.key == key);
            return entry != null ? entry.value : $"[{key}]";
        }
    }

    [Serializable]
    public class LocalizationEntry
    {
        public string key;
        [TextArea] public string value;
    }
}