using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public enum TypeFootStepSound
    {
        NONE = 0,
        GRASS,
        ROCK,
        METAL,
        GRAVEL = 3001,
    }

    public enum TypeUISound
    {
        NONE = 0,
        MENU_MOVE = 2001,
        MENU_PLAYSTART = 2002,
        MENU_SELECT = 2003,
        ROCK,
        METAL,
    }

    [System.Serializable]
    public class SoundTableData : AssetTableData
    {
        public bool BGM;
        public string[] RandomAssetName;
    }

    [CreateAssetMenu(fileName = "SoundTable", menuName = "Zeus/Table/SoundTable")]
    public class SoundTable : ScriptableObject
    {
        public List<SoundTableData> TableDatas;
        private Dictionary<int, SoundTableData> _soundTables;
        private void OnEnable()
        {
            Initialized();
        }

        private void Initialized()
        {
            if (_soundTables != null)
                return;

            _soundTables = new Dictionary<int, SoundTableData>();
            foreach (var item in TableDatas)
            {
                _soundTables.Add(item.ID, item);
            }
        }

        internal SoundTableData GetData(int id)
        {
            if (_soundTables.ContainsKey(id))
                return _soundTables[id];

            return null;
        }

        internal SoundTableData GetData(string name) 
        {
            return TableDatas.Find(_=>_.AssetName.Contains(name));  
        }
    }
}


