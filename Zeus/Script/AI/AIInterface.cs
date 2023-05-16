using UnityEngine;

namespace Zeus
{
    using System.Collections;
    using System.Collections.Generic;
    public partial interface IControlAI : ICharacter
    {
        /// <summary>
        /// Used just to Create AI Editor
        /// </summary>
        void CreatePrimaryComponents();

        /// <summary>
        /// Used just to Create AI Editor
        /// </summary>
        void CreateSecondaryComponents();

        /// <summary>
        /// Check if <seealso cref="IControlAI"/> has a <seealso cref=" IAIComponent"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool HasComponent<T>() where T : IAIComponent;

        /// <summary>
        /// Get Specific <seealso cref="vIAIComponent"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetAIComponent<T>() where T : IAIComponent;

        Vector3 TargetDestination { get; }
        Collider SelfCollider { get; }
        AnimatorStateInfos AnimatorStateInfos { get; }
        AIReceivedDamegeInfo ReceivedDamage { get; }

        Vector3 LastTargetPosition { get; }
        Vector3 GetInput { get; }
        bool IsInDestination { get; }
        bool IsMoving { get; }
        bool IsStrafing { get; }
        bool TargetInLineOfSight { get; }
        bool IsStop { get; set; }
        AISightMethod SightMethod { get; set; }
        AIUpdateQuality UpdatePathQuality { get; set; }
        AIUpdateQuality FindTargetUpdateQuality { get; set; }
        AIUpdateQuality CanseeTargetUpdateQuality { get; set; }
        AIMovementSpeed MovementSpeed { get; }
        float TargetDistance { get; }
        Vector3 DesiredVelocity { get; }
        float RemainingDistance { get; }
        float StopingDistance { get; set; }
        float MinDistanceToDetect { get; set; }
        float MaxDistanceToDetect { get; set; }
        float FieldOfView { get; set; }
        bool DoAction { get; }
        void SetDetectionLayer(LayerMask mask);
        void SetDetectionTags(List<string> tags);
        void SetObstaclesLayer(LayerMask mask);
        void SetLineOfSight(float fov = -1, float minDistToDetect = -1, float maxDistToDetect = -1, float lostTargetDistance = -1);

        /// <summary>
        /// Move AI to a position in World Space
        /// </summary>
        /// <param name="destination">world space position</param>
        /// <param name="speed">movement speed</param>
        void MoveTo(Vector3 destination, AIMovementSpeed speed = AIMovementSpeed.Walking);
        /// <summary>
        /// Move AI to a position in World Space and rotate to a custom direction
        /// </summary>
        /// <param name="destination">world space position</param>
        /// <param name="forwardDirection">target rotation direction</param>
        /// <param name="speed">>movement speed</param>
        void StrafeMoveTo(Vector3 destination, Vector3 forwardDirection, AIMovementSpeed speed = AIMovementSpeed.Walking);

        /// <summary>
        /// Move AI to a position in World Space with out update the target rotation direction of the AI
        /// </summary>
        /// <param name="destination">world space position</param>      
        /// <param name="speed">>movement speed</param>
        void StrafeMoveTo(Vector3 destination, AIMovementSpeed speed = AIMovementSpeed.Walking);

        void RotateTo(Vector3 direction);
        void SetCurrentTarget(Transform target);
        void SetCurrentTarget(Transform target, bool overrideCanseeTarget);
        void RemoveCurrentTarget();
        AITarget CurrentTarget { get; }

        /// <summary>
        /// Return a list of targets resulted of <seealso cref="FindTarget"/> method
        /// </summary>
        /// <returns></returns>
        Collider[] GetTargetsInRange();
        /// <summary>
        /// Find target using Detection settings
        /// </summary>
        void FindTarget();
        /// <summary>
        /// Find target with ignoring obstacles option
        /// </summary>
        /// <param name="checkForObstacles"></param>
        void FindTarget(bool checkForObstacles);
        /// <summary>
        /// Try get a target 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        bool TryGetTarget(out AITarget target);
        /// <summary>
        /// Try get a target
        /// </summary>
        /// <param name="tag">possible target tag</param>
        /// <param name="target"></param>
        /// <returns></returns>
        bool TryGetTarget(string tag, out AITarget target);
        /// <summary>
        /// Try get a target 
        /// </summary>
        /// <param name="m_detectTags">list of possible target tags</param>
        /// <param name="target"></param>
        /// <returns></returns>
        bool TryGetTarget(List<string> m_detectTags, out AITarget target);
        /// <summary>
        /// Find target with specific detection settings
        /// </summary>
        /// <param name="m_detectTags">list of possible target tags</param>
        /// <param name="m_detectLayer">layer of possible target</param>
        /// <param name="checkForObstables"></param>
        void FindSpecificTarget(List<string> m_detectTags, LayerMask m_detectLayer, bool checkForObstables = true);
        void LookAround();
        void LookTo(Vector3 point, float stayLookTime = 1f, float offsetLookHeight = -1);
        void LookToTarget(Transform target, float stayLookTime = 1f, float offsetLookHeight = -1);
        void Stop();
        void ForceUpdatePath(float timeInUpdate = 1f);

        /// <summary>
        /// Check if AI is Trigger With some collider with specific tag
        /// </summary>
        /// <param name="targ"></param>
        /// <returns></returns>
        bool IsInTriggerWithTag(string tag);
        /// <summary>
        /// Check if AI is Trigger With some collider with specific name
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        bool IsInTriggerWithName(string name);

        /// <summary>
        /// Check if AI is Trigger With some collider with specific name
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool IsInTriggerWithTag(string tag, out Collider result);
        /// <summary>
        /// Check if AI is Trigger With some collider with specific tag
        /// </summary>
        /// <param name="name"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool IsInTriggerWithName(string name, out Collider result);
    }

    public partial interface IControlAICombat : IControlAI, IAttackListener
    {
        int StrafeCombatSide { get; }
        float MinDistanceOfTheTarget { get; }
        float CombatRange { get; }
        bool IsInCombat { get; set; }
        bool StrafeCombatMovement { get; }

        int AttackCount { get; set; }
        float AttackDistance { get; }
        bool CanAttack { get; }
        bool CanMove { get; }
        bool IsAttacking { get; }
        void InitAttackTime(AIPattern pattern = null);
        void ResetAttackTime();
        void Attack(bool forceCanAttack = false);

        bool CanBlockInCombat { get; }
        void ResetBlockTime();
        void Blocking();
    }

    public partial interface IControlAIMelee : IControlAICombat
    {
        CombatManager CombatManager { get; set; }
        void SetMeleeHitTags(List<string> tags);
    }

    public partial interface IControlAIZeus : IControlAIMelee
    {
        string GroupID { get; set; }
        int MoveSetID { get; set; }
        int AttackSetID { get; set; }
        int RotateDirection { get; set; }
        List<AIPattern> AIPatterns { get; }
        int CurrentPatternIndex { get; set; }
        bool CanUsePattern { get; }
        bool ReactionPrevention { get; set; }
        void SetUsePatternIndex();
        void StartFSMCoroutine(IEnumerator coroutine);
    }


    [System.Serializable]
    public class AITarget
    {
        [SerializeField] protected Transform _transform;
        [SerializeField, HideInInspector] protected Collider _collider;
        public Transform Transform { get { return _transform; } protected set { _transform = value; } }
        public Collider Collider { get { return _collider; } protected set { _collider = value; } }

        public IHealthController HealthController;
        public IControlAICombat CombateController;
        public ICharacter Character;
        
        public bool IsFixedTarget = true;
        [HideInInspector]
        public bool IsLost;
        [HideInInspector]
        public bool HadHealthController;

        public bool HasCollider
        {
            get
            {                
                return _collider != null;
            }
        }
        public static implicit operator Transform(AITarget m)
        {
            try
            {
                return m._transform;
            }
            catch { return null; }
        }
        public bool HasHealthController
        {
            get
            {
                if (HadHealthController && HealthController == null)
                    _transform = null;
                return HealthController != null;
            }
        }

        public bool IsDead
        {
            get
            {
                var value = true;
                if (HasHealthController) value = HealthController.IsDead;
                else if (HadHealthController) value = true;
                else if (!_transform.gameObject.activeInHierarchy) value = true;
                else if (_collider) value = !_collider.enabled;
                return value;
            }
        }

        public bool IsAttacking
        {
            get
            {
                if (!IsFighter) return false;
                return CombateController != null ? CombateController.IsAttacking : false;
            }
        }

        public bool IsBlocking
        {
            get
            {
                if (!IsFighter) return false;
                return Character != null ? Character.IsBlocking : CombateController != null ? CombateController.IsBlocking : false;
            }
        }

        public bool IsFighter
        {
            get
            {
                return Character != null || CombateController != null;
            }
        }

        public bool IsCharacter
        {
            get
            {
                return Character != null;
            }
        }

        public float CurrentHealth
        {
            get
            {
                if (HasHealthController) return HealthController.CurrentHealth;
                return 0;
            }
        }

        public void InitTarget(Transform target)
        {
            if (target)
            {
                Transform = target;
                Collider = Transform.GetComponent<Collider>();
                HealthController = Transform.GetComponent<IHealthController>();
                HadHealthController = this.HealthController != null;
                Character = Transform.GetComponent<ICharacter>();
                CombateController = Transform.GetComponent<IControlAICombat>();
            }
        }

        public void ClearTarget()
        {
            Transform = null;
            Collider = null;
            HealthController = null;
            Character = null;
            CombateController = null;
        }
    }

    [System.Serializable]
    public class AIReceivedDamegeInfo
    {
        public AIReceivedDamegeInfo()
        {
            lasType = "unnamed";
        }
        [ReadOnly(false)] public bool isValid;
        [ReadOnly(false)] public float lastValue;
        [ReadOnly(false)] public string lasType = "unnamed";
        [ReadOnly(false)] public Transform lastSender;
        [ReadOnly(false)] public int massiveCount;
        [ReadOnly(false)] public float massiveValue;

        protected float lastValidDamage;
        float _massiveTime;
        public void Update()
        {
            _massiveTime -= Time.deltaTime;
            if (_massiveTime <= 0)
            {
                _massiveTime = 0;
                if (massiveValue > 0) massiveValue -= 1;
                if (massiveCount > 0) massiveCount -= 1;
            }
            isValid = lastValidDamage > Time.time;
        }

        public void UpdateDamage(Damage damage, float validDamageTime = 2f)
        {
            if (damage == null) return;
            lastValidDamage = Time.time + validDamageTime;
            _massiveTime += Time.deltaTime;
            massiveCount++;
            lastValue = damage.DamageValue;
            massiveValue += lastValue;
            lastSender = damage.Sender;
            lasType = string.IsNullOrEmpty(damage.DamageType) ? "unnamed" : damage.DamageType;
        }
    }

    public interface IStateAttackListener
    {
        void OnReceiveAttack(IControlAICombat combatController, ref Damage damage, Character attacker, ref bool canBlock);
    }
}