using System.Collections.Generic;
using UnityEngine;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "IceCreamTable", menuName = "Scriptable Objects/Minigame/IceCreamTable")]
    public class IceCreamTable : ScriptableObject
    {
        [SerializeField]
        private List<IceCreamData> _iceCreamDatas;
        
        public IceCreamData GetRandomIceCream()
        {
            var index = Random.Range(0, _iceCreamDatas.Count);
            return _iceCreamDatas[index];
        }
    }
}
