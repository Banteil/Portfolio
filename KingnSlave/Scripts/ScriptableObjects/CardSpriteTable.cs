using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.kingnslave
{
    [CreateAssetMenu(fileName = "CardSpriteTable", menuName = "Table/CardSpriteTable", order = 1)]
    public class CardSpriteTable : ScriptableObject
    {
        public List<SpriteData> CharacterSpriteDataList;
        public List<SpriteData> BackgroundSpriteDataList;
        public List<SpriteData> FrameSpriteDataList;
        public List<SpriteData> NameplateSpriteDataList;
        public List<SpriteData> BackSideSpriteDataList;
    }
}
