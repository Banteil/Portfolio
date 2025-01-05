using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "SpriteTable", menuName = "Scriptable Objects/SpriteTable")]
    public class SpriteTable : ScriptableObject
    {
        public List<SpriteAtlas> SpriteAtlasList = new List<SpriteAtlas>();
        public List<Sprite> SpriteList = new List<Sprite>();        

        public Sprite GetSprite(string name)
        {
            var spriteByAtlas = SpriteAtlasList
                .Select(atlas => atlas.GetSprite(name))
                .FirstOrDefault(s => s != null);
            var sprite = SpriteList.FirstOrDefault(s => s.name == name);
            if (spriteByAtlas == null && spriteByAtlas == null)
            {
                Debug.LogWarning($"Sprite with name {name} not found in SpriteList.");
            }

            return spriteByAtlas ?? sprite;
        }
    }
}
