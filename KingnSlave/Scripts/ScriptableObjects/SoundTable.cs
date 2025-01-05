using System;
using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.kingnslave
{
    [Serializable]
    public class SoundData
    {
        public string Name;
        public AudioClip Clip;
        [TextArea]
        public string Description;
    }

    [CreateAssetMenu(fileName = "SoundTable", menuName = "Table/SoundTable", order = 1)]
    public class SoundTable : ScriptableObject
    {
        public List<SoundData> BGMDatas;
        public List<SoundData> SFXDatas;
    }
}
