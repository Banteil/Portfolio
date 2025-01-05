using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "LocalizationDatabase", menuName = "Scriptable Objects/Localization/LocalizationDatabase")]
    public class LocalizationDatabase : ScriptableObject
    {
        [SerializeField]
        private List<LocalizationTable> _tables;

        public LocalizationTable SelectedTable { get; private set; }

        public void OnLocaleChanged(Locale newLocale)
        {
            var localeCode = newLocale.Identifier.Code;
            SelectedTable = _tables.Find(table => table.Code == localeCode);
            if (SelectedTable == null)
            {
                Debug.LogWarning($"Locale '{localeCode}'�� �ش��ϴ� LocalizationTable�� ã�� �� �����ϴ�.");
            }
        }

        public string GetLocalizedString(string key) => SelectedTable.GetLocalizedString(key);
    }
}
