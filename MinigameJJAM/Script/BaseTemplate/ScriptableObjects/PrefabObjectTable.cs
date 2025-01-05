using System.Collections.Generic;
using UnityEngine;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "PrefabObjectTable", menuName = "Scriptable Objects/PrefabObjectTable")]
    public class PrefabObjectTable : ScriptableObject
    {
        public List<GameObject> Objects = new List<GameObject>();

        public GameObject GetPrefabObject(string name, Transform parent = null, bool instantiateInWorldSpace = false)
        {
            foreach (var _object in Objects)
            {
                if (_object != null && _object.name == name)
                {
                    var prefabObject = Instantiate(_object.gameObject, parent, instantiateInWorldSpace);
                    prefabObject.name = name;
                    return prefabObject;
                }
            }

            Debug.LogError($"Object with name '{name}' not found.");
            return null;
        }

        public GameObject GetPrefabObject(string name, Vector3 position, Quaternion rotation)
        {
            foreach (var _object in Objects)
            {
                if (_object != null && _object.name == name)
                {
                    var prefabObject = Instantiate(_object.gameObject, position, rotation);
                    prefabObject.name = name;
                    return prefabObject;
                }
            }

            Debug.LogError($"Object with name '{name}' not found.");
            return null;
        }
    }
}
