using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Zeus
{
    [RequireComponent(typeof(BoxCollider))]
    public class MapDataObject : MonoBehaviour
    {
        public bool GridIn { get; set; }
        public TypeMapCheck LoadLevel;
        public AssetLabelReference HighObjecReference;
        public AssetLabelReference[] LowObjectReference;
    }
}