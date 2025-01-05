using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace starinc.io
{
    public abstract class AttackPattern : ScriptableObject
    {
        public CreatureController Creature { get; set; }
        [SerializeField] protected float _minDelayNextPattern = 1f;
        [SerializeField] protected float _maxDelayNextPattern = 2f;

        public abstract void ObjectPoolInitialize<T>(IObjectPool<T> pool) where T : MonoBehaviour;

        public abstract UniTask PlayPattern();
    }
}
