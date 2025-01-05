using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.gallaryx
{
    [CreateAssetMenu(fileName = "Sprite Table", menuName = "Scriptable Object/Sprite Table", order = int.MaxValue)]
    public class SpriteTable : ScriptableObject
    {
        public List<SpriteData> Sprites = new List<SpriteData>();
    }
}
