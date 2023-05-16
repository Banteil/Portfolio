using UnityEngine;

namespace Zeus
{
    public class DestroyTimer : ZeusTimer
    {
        public GameObject Object;

        public DestroyTimer(int id, float duration) : base(id, duration) {}

        internal override void Done()
        {
            GameObject.Destroy(Object);
        }

        internal override void Tick()
        {
 
        }
    }

    public static partial class GameObjectExtension
    {
        public static void DestroyTimer(this GameObject _ob, float lifeTime)
        {
            var destory = new Zeus.DestroyTimer(_ob.GetInstanceID(), lifeTime);
            destory.Object = _ob;
        }
    }
}
