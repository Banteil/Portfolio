using System;
using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.kingnslave
{
    [Serializable]
    public class SpriteData
    {
        public string Name;        
        public Sprite Sprite;
        [TextArea]
        public string Description;
    }

    [CreateAssetMenu(fileName = "SpriteTable", menuName = "Table/SpriteTable", order = 1)]
    public class SpriteTable : ScriptableObject
    {
        public List<SpriteData> SpriteDatas;
        public List<SpriteData> TierDatas;
        public List<SpriteData> InGameBackgroundDatas;
        public List<Sprite> CountryFlagSprites;
    }
}
