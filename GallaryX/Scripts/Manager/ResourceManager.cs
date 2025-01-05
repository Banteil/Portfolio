using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace starinc.io.gallaryx
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        [SerializeField]
        private SpriteTable _spriteTable;
        private AsyncOperationHandle _handle;

        public T Load<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }

        public GameObject Instantiate(string path, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            GameObject prefab = Load<GameObject>($"Prefabs/{path}");
            if (prefab == null)
            {
                Debug.Log($"프리팹 로드에 실패하였습니다 : {path}");
                return null;
            }

            GameObject obj = Object.Instantiate(prefab, parent, instantiateInWorldSpace);
            Util.ExcludingCloneName(obj);
            return obj;
        }

        public void Destroy(GameObject obj)
        {
            if (obj == null) return;

            Object.Destroy(obj);
        }

        public Sprite GetSprite(string name)
        {
            var data = _spriteTable.Sprites.Find((x) => x.Name.Contains(name));
            if (data == null)
                return null;
            else
                return data.Sprite;
        }

        #region Addressable Asset
        public async UniTask<GameObject> LoadExhibition(string url)
        {
            GameObject result = null;
            Addressables.LoadAssetAsync<GameObject>(url).Completed += (AsyncOperationHandle<GameObject> obj) =>
            {
                _handle = obj;
                result = obj.Result;
            };
            await Addressables.LoadAssetAsync<GameObject>(url).Task;
            return result;
        }

        public void UnloadExhibition()
        {
            Addressables.Release(_handle);
        }
        #endregion
    }
}
