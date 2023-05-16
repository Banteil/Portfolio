using UnityEngine;

namespace Zeus
{
    public class ObjectContainer : MonoBehaviour
    {

        static ObjectContainer instance;
        public static Transform Root
        {
            get
            {
                if (!instance)
                {
                    instance = new GameObject("Object Container", typeof(ObjectContainer)).GetComponent<ObjectContainer>();
                }
                return instance.transform;
            }
        }
    }
}
