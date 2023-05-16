using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Zeus
{
    [System.Serializable]
    public class StringTableData
    {
        public int ID;
        public string KR;
        public string EN;
    }

    [CreateAssetMenu(fileName = "StringTable", menuName = "Zeus/Table/StringTable")]
    public class StringTable : ScriptableObject
    {
        public StringTableData[] stringTableDatas;

        private Dictionary<int, StringTableData> _stringTable;

        private void OnEnable()
        {
            Initialized();
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            var idList = stringTableDatas.Select(x => x.ID).ToList();
            var duplicatedList = idList.GroupBy(x => x).Where(group => group.Count() > 1).Select(x => x.Key).ToList();
            StringBuilder duplicatedString = new StringBuilder();
            if (duplicatedList.Count > 0)
            {
                duplicatedString.Append("ID ม฿บน\n");
                for (int i = 0; i < duplicatedList.Count; i++)
                {
                    var duplicatedID = duplicatedList[i];
                    duplicatedString.Append($"ID : {duplicatedID} Index : ");
                    for (int j = 0; j < idList.Count; j++)
                    {
                        var id = idList[j];
                        if (id == duplicatedID)
                        {
                            duplicatedString.Append($"{j}, ");
                        }
                    }
                    duplicatedString.Remove(duplicatedString.Length - 2, 2);
                    duplicatedString.Append("\n");
                }
                Debug.LogWarning(duplicatedString.ToString());
            }
        }
#endif

        private void Initialized()
        {
            if (_stringTable != null)
                return;

            _stringTable = new Dictionary<int, StringTableData>();
            foreach (var item in stringTableDatas)
            {
                _stringTable.Upsert(item.ID, item);
            }
        }

        internal string GetString(int id)
        {
            if (!_stringTable.ContainsKey(id))
                return string.Empty;

            var str = string.Empty;
            if (Application.systemLanguage == SystemLanguage.Korean)
            {
                str = _stringTable[id].KR;
            }
            else
            {
                str = _stringTable[id].EN;
            }

            return str;
        }
    }
}