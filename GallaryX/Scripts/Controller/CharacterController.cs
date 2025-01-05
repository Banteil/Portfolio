using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace starinc.io.gallaryx
{
    public enum CharacterType
    { 
        Player,
        AI,
        Network,
    }

    public class CharacterController : BaseController
    {
        #region Cache
        [SerializeField] protected CharacterType _characterType;
        public CharacterType Type
        {
            get {  return _characterType; }
            set
            {
                if (Brain != null && _characterType == value) return;
                _characterType = value;
                if (_characterType == CharacterType.Player)
                    Brain = new PlayerBrain();
                else
                    Brain = new AIBrain();
                ChangeCharacterTypeCallback?.Invoke(_characterType);
            }
        }
        public event Action<CharacterType> ChangeCharacterTypeCallback;

        public bool IsControlCharacter { get { return this == MainSceneManager.Instance.Player; } }

        [SerializeField] protected Transform _abilityParent;
        protected Animator _animator;
        protected Rigidbody _rigidBody;
        protected CapsuleCollider _bodyCollider;
        protected List<BaseAbility> _characterAbilities = new List<BaseAbility>();
        protected BaseBrain _brain;
        #endregion

        protected bool _isInteraction;
        public bool IsInteraction
        {
            get { return _isInteraction; }
            set 
            { 
                _isInteraction = value;
                CheckInteractionsCallback?.Invoke(_isInteraction);
            }
        }
        public Animator AnimatorComponent { get { return _animator; } }
        public Rigidbody RigidBodyComponent { get { return _rigidBody; } }
        public CapsuleCollider BodyCollider { get { return _bodyCollider; } }
        public List<BaseAbility> CharacterAbilities { get { return _characterAbilities; } }
        public BaseBrain Brain 
        { 
            get { return _brain; }
            set
            {
                _brain?.OnDisable();
                _brain = value;
                _brain.OnEnable(this);
            }
        }

        public event Action InputStateCallback, InputActionCallback;
        public event Action<bool> CheckInteractionsCallback;

        protected override void OnAwake()
        {
            _animator = Util.GetOrAddComponent<Animator>(gameObject);
            _rigidBody = Util.GetOrAddComponent<Rigidbody>(gameObject);
            _bodyCollider = Util.GetOrAddComponent<CapsuleCollider>(gameObject);
            _abilityParent = Util.FindOrAddChild(transform, "AbilityParent");
            _characterAbilities = _abilityParent.GetComponentsInChildren<BaseAbility>().ToList();
            Type = _characterType;
        }

        protected override void InputStateFunction() => InputStateCallback?.Invoke();

        protected override void InputActionFunction() => InputActionCallback?.Invoke();

        /// <summary>
        /// 캐릭터에 어빌리티를 추가하는 함수
        /// </summary>
        /// <param name="componentName"></param>
        public void AddAbility<T>() where T : BaseAbility
        {
            if (HasAbility<T>())
            {
                Debug.Log("이미 동일한 어빌리티가 있습니다.");
                return;
            }
            var abilityObject = new GameObject(typeof(T).Name);
            abilityObject.transform.parent = _abilityParent;
            abilityObject.AddComponent<T>();
        }

        /// <summary>
        /// 추가된 어빌리티를 삭제하는 함수
        /// </summary>
        /// <param name="type"></param>
        public void RemoveAbility<T>() where T : BaseAbility
        {
            var ability = GetAbility<T>();
            if (ability != null)
                ability.RemoveAbility();
        }

        /// <summary>
        /// 해당 캐릭터에 매개변수 타입의 어빌리티가 있으면 리턴
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public T GetAbility<T>() where T : BaseAbility
        {
            foreach (var ability in _characterAbilities)
            {
                if (ability is T findAbility)
                    return findAbility;
            }
            return null;
        }

        /// <summary>
        /// 어빌리티가 있는지 여부를 체크하는 함수
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool HasAbility<T>() where T : BaseAbility
        {
            foreach (var ability in _characterAbilities)
            {
                if (ability is T)
                    return true;
            }
            return false;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            _brain.OnUpdate();
        }
    }
}
