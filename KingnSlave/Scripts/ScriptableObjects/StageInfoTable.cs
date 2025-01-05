using System;
using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.kingnslave
{
    [Serializable]
    public class StageInfoData
    {
        public string Name;
        public int MaxScore;
        public string CharacterNameTextKey;
        public Sprite Character;
        public Texture CharacterProfile;
        public Sprite InGameBackground;
        public Sprite LoadingBackground;
        public Sprite LoadingBackgroundBottom;
        public Sprite LoadingUI;
    }

    [CreateAssetMenu(fileName = "StageInfoTable", menuName = "Table/StageInfoTable", order = 2)]
    [Serializable]
    public class StageInfoTable : ScriptableObject
    {
        public List<StageInfoData> StageInfos;
    }
}