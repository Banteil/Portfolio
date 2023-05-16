
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Zeus
{
    public class MeshSaveTool : MonoBehaviour
    {
        public KeyCode saveKey = KeyCode.F12;
        public string saveName = "SavedMesh";
        public Transform selectedGameObject;

        private void Start()
        {

        }

        public void SaveAsset()
        {
            var mfs = selectedGameObject.GetComponentsInChildren<MeshFilter>(true);

            foreach (var item in mfs)
            {
                var savePath = "Assets/" + item.gameObject.name + ".asset";
                Debug.Log("Saved Mesh to:" + savePath);
                AssetDatabase.CreateAsset(item.mesh, savePath);
            }

            AssetDatabase.SaveAssets();
        }
    }
}
#endif