using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    [ClassHeader("AI HEADTRACK",
        HelpBoxText =
            "If the bone hips don't have the same orientation of the character,\n you can add a custom hips to override the original (Transforms)",
        UseHelpBox = true)]
    public class AIHeadtrack : zMonoBehaviour
    {
        #region Public Variables

        [EditorToolbar("Settings")] public bool canLook = true;
        public bool FreezeLookPoint = false;

        [vHelpBox("Check this option to continue to look and ignore the limitAngle. ex: something is pursuing you")]
        public bool KeepLookingOutAngle = true;

        [Range(0, 1)] public float StrafeHeadWeight = 0.8f;
        [Range(0, 1)] public float StrafeBodyWeight = 0.8f;
        [Range(0, 1)] public float FreeHeadWeight = 1f;
        [Range(0, 1)] public float FreeBodyWeight = 0.4f;

        [vMinMax(minLimit = -180, maxLimit = 180)]
        public Vector2 LimitAngleX = new Vector2(-90, 90);

        [vMinMax(minLimit = -90, maxLimit = 90)]
        public Vector2 LimitAngleY = new Vector2(-90, 90);

        [Tooltip("Apply offset Y to look point")]
        public float defaultOffSetLookHeight = 1.5f;

        [SerializeField, ReadOnly] protected float _currentOffSetLookHeight;
        public float Smooth = 12f;

        [vHelpBox("Add a AnimatorTag here to ignore the Headtrack and play the animation instead")]
        public List<string> animatorTags = new List<string>()
            { "Attack", "LockMovement", "CustomAction", "IgnoreHeadtrack" };

        public Vector2 offsetSpine, offsetHead;
        public Transform mainLookTarget;
        public Transform eyes;
        public float timeToExitLookPoint = 1;
        public float timeToExitLookTarget = 1;

        [vHelpBox("Use it with the FSM Action LookAround to simulate an look around animation")] [SerializeField]
        protected float lookAroundAngle = 60f;

        [SerializeField] protected AnimationCurve lookAroundCurve;
        [SerializeField] protected float lookAroundSpeed = 0.1f;

        [EditorToolbar("Transforms")]
        [Tooltip(
            "If the bone hips don't have the same orientation of the character, you can add a custom hips to override the original (Transforms)")]
        public Transform hips;

        [Header("Just for Debug")] public Transform head;
        public List<Transform> spine;
        public Vector3 currentLookPoint { get; set; }
        public Vector3 currentLookDirection { get; set; }

        #endregion

        #region Private Variables

        private IControlAI character;
        private Animator animator;
        private Transform temporaryLookTarget;
        private Vector3 temporaryLookPoint;
        private Vector3 targetLookPoint;
        private bool inLockPoint;
        private bool inLockTarget;
        private bool isInSmoothValues;
        private bool updateIK;
        private float targetOffsetHeight;
        private float exitLookPointTime;
        private float exitLookTargetTime;
        private float headHeight;
        private float yAngle, xAngle;
        private float _yAngle, _xAngle;
        private float yRotation, xRotation;
        private float _currentHeadWeight, _currentBodyWeight;
        private float lookAroundProgress;
        private IAnimatorStateInfoController animatorStateInfos;

        #endregion

        [EditorToolbar("Events")] public UnityEngine.Events.UnityEvent onPreUpdateSpineIK, onPosUpdateSpineIK;

        #region PROTECTED VIRTUAL METHODS

        #region UNITY METHODS

        protected virtual void Start()
        {
            character = GetComponent<IControlAI>();
            animator = GetComponent<Animator>();
            animatorStateInfos = GetComponent<IAnimatorStateInfoController>();
            if (animator.isHuman)
            {
                head = animator.GetBoneTransform(HumanBodyBones.Head);
                var spine1 = animator.GetBoneTransform(HumanBodyBones.Spine);
                var spine2 = animator.GetBoneTransform(HumanBodyBones.Chest);
                spine = new List<Transform>();
                if (spine1)
                    spine.Add(spine1);
                if (spine2)
                    spine.Add(spine2);
                var neck = animator.GetBoneTransform(HumanBodyBones.Neck);
                if (!hips)
                    hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                if (neck && spine2 && neck.parent && neck.parent != spine2)
                    spine.Add(neck.parent);
            }

            if (head)
                headHeight = Vector3.Distance(transform.position, head.position);

            ResetOffseLookHeight();
            CurrentLookPosition = GetLookPoint();
            _lastLocalLookPosition = _currentLocalLookPosition;
            lookAroundProgress = 0.5f;
        }

        protected virtual void FixedUpdate()
        {
            updateIK = true;
        }

        private Vector3 _currentLocalLookPosition;
        private Vector3 _lastLocalLookPosition;

        public virtual Vector3 CurrentLookPosition
        {
            get => FreezeLookPoint
                ? transform.TransformPoint(_lastLocalLookPosition)
                : transform.TransformPoint(_currentLocalLookPosition);
            protected set
            {
                _currentLocalLookPosition = transform.InverseTransformPoint(value);
                if (!FreezeLookPoint) _lastLocalLookPosition = _currentLocalLookPosition;
            }
        }

        protected virtual void LateUpdate()
        {
            if (animator == null || (character != null && (character.CurrentHealth <= 0 || character.IsDead)) ||
                !animator.enabled || (!updateIK && animator.updateMode == AnimatorUpdateMode.AnimatePhysics)) return;

            updateIK = false;
            // call pre Update Event
            if (onPreUpdateSpineIK != null) onPreUpdateSpineIK.Invoke();
            // update SpineIK
            if (!FreezeLookPoint) currentLookPoint = GetLookPoint();
            LookAtIK(currentLookPoint, _currentHeadWeight, _currentBodyWeight);
            // call pos Update Event
            if (onPosUpdateSpineIK != null && !IgnoreHeadTrackFromAnimator()) onPosUpdateSpineIK.Invoke();
        }

        #endregion

        #region SPINE IK BEHAVIOUR

        protected virtual void LookAtIK(Vector3 point, float headWeight, float spineWeight)
        {
            var lookRotation = Quaternion.LookRotation(point);
            var euler = lookRotation.eulerAngles - transform.rotation.eulerAngles;

            var y = NormalizeAngle(euler.y);
            var x = NormalizeAngle(euler.x);

            xAngle = Mathf.Clamp(Mathf.Lerp(xAngle, (x), Smooth * Time.deltaTime), LimitAngleX.x, LimitAngleX.y);
            yAngle = Mathf.Clamp(Mathf.Lerp(yAngle, (y), Smooth * Time.deltaTime), LimitAngleY.x, LimitAngleY.y);

            foreach (var segment in spine)
            {
                var _y = NormalizeAngle(yAngle + Quaternion.Euler(offsetSpine).eulerAngles.y);
                var _x = NormalizeAngle(xAngle + Quaternion.Euler(offsetSpine).eulerAngles.x);

                var rotX = Quaternion.AngleAxis((_x * spineWeight) / spine.Count,
                    segment.InverseTransformDirection(transform.right));
                var rotY = Quaternion.AngleAxis((_y * spineWeight) / spine.Count,
                    segment.InverseTransformDirection(transform.up));
                segment.rotation *= rotX * rotY;
            }

            var eulerHeadOffset = Quaternion.Euler(offsetHead).eulerAngles.NormalizeAngle();

            _yAngle = Mathf.Lerp(_yAngle, (yAngle - (yAngle * spineWeight)) + eulerHeadOffset.y,
                Smooth * Time.deltaTime);
            _xAngle = Mathf.Lerp(_xAngle, (xAngle - (xAngle * spineWeight)) + eulerHeadOffset.x,
                Smooth * Time.deltaTime);
            var _rotX = Quaternion.AngleAxis(_xAngle * headWeight, head.InverseTransformDirection(transform.right));
            var _rotY = Quaternion.AngleAxis(_yAngle * headWeight, head.InverseTransformDirection(transform.up));
            head.rotation *= _rotX * _rotY;
        }

        protected virtual void SmoothValues(float _headWeight = 0, float _bodyWeight = 0, float _x = 0, float _y = 0)
        {
            _currentHeadWeight = Mathf.Lerp(_currentHeadWeight, _headWeight, Smooth * Time.deltaTime);
            _currentBodyWeight = Mathf.Lerp(_currentBodyWeight, _bodyWeight, Smooth * Time.deltaTime);
            yRotation = Mathf.Lerp(yRotation, _y, Smooth * Time.deltaTime);
            xRotation = Mathf.Lerp(xRotation, _x, Smooth * Time.deltaTime);
            yRotation = Mathf.Clamp(yRotation, LimitAngleY.x, LimitAngleY.y);
            xRotation = Mathf.Clamp(xRotation, LimitAngleX.x, LimitAngleX.y);

            var completeY = Mathf.Abs(yRotation - Mathf.Clamp(_y, LimitAngleY.x, LimitAngleY.y)) < 0.01f;
            var completeX = Mathf.Abs(yRotation - Mathf.Clamp(_x, LimitAngleX.x, LimitAngleX.y)) < 0.01f;
            isInSmoothValues = !(completeY && completeX);
        }

        protected virtual Vector3 HeadPoint => transform.position + (transform.up * headHeight);

        protected virtual Vector3 GetLookPoint()
        {
            if (!IgnoreHeadTrackFromAnimator() && canLook)
            {
                // default look Point
                var _defaultLookPoint = mainLookTarget
                    ? mainLookTarget.position + Vector3.up * OffsetHeightResult
                    : defaultLookPoint;
                targetLookPoint = _defaultLookPoint;
                // temporary look Target
                if (exitLookTargetTime > 0 || inLockTarget)
                {
                    if (temporaryLookTarget)
                        targetLookPoint = temporaryLookTarget.position + Vector3.up * OffsetHeightResult;
                    else exitLookTargetTime = 0;
                    if (!inLockTarget) exitLookTargetTime -= Time.deltaTime;
                }

                // temporary look point
                if (exitLookPointTime > 0 || inLockPoint)
                {
                    targetLookPoint = temporaryLookPoint + Vector3.up * OffsetHeightResult;
                    if (!inLockPoint) exitLookPointTime -= Time.deltaTime;
                }

                // calc look direction         
                var currentDir = defaultLookPoint - HeadPoint;
                var desiredDir = targetLookPoint - HeadPoint;

                currentDir = desiredDir;
                // apply limit angles
                var angle = GetTargetAngle(currentDir);

                //check if is out angle
                if (!KeepLookingOutAngle)
                {
                    if (LookDirectionIsOnRange(currentDir))
                    {
                        if (character != null && character.IsStrafing)
                            SmoothValues(StrafeHeadWeight, StrafeBodyWeight, angle.x, angle.y);
                        else SmoothValues(FreeHeadWeight, FreeBodyWeight, angle.x, angle.y);
                    }
                    else SmoothValues();
                }
                else
                {
                    if (character is { IsStrafing: true })
                        SmoothValues(StrafeHeadWeight, StrafeBodyWeight, angle.x, angle.y);

                    else SmoothValues(FreeHeadWeight, FreeBodyWeight, angle.x, angle.y);
                }
            }
            else SmoothValues();

            // finish look point calc
            var rotA = Quaternion.AngleAxis(yRotation, transform.up);
            var rotB = Quaternion.AngleAxis(xRotation, transform.right);
            var finalRotation = (rotA * rotB);
            var lookDirection = finalRotation * transform.forward;
            currentLookPoint = HeadPoint + (lookDirection);

            return lookDirection;
        }

        private Vector3 defaultLookPoint => HeadPoint + (transform.forward * 100);

        protected virtual Vector2 GetTargetAngle(Vector3 direction)
        {
            var lookRotation = Quaternion.LookRotation(direction, transform.up); //rotation from head to camera point
            var angleResult =
                lookRotation.eulerAngles -
                transform.eulerAngles; // diference between transform rotation and desiredRotation
            Quaternion desiredRotation = Quaternion.Euler(angleResult); // convert angleResult to Rotation
            var x = (float)System.Math.Round(NormalizeAngle(desiredRotation.eulerAngles.x), 2);
            var y = (float)System.Math.Round(NormalizeAngle(desiredRotation.eulerAngles.y), 2);
            return new Vector2(x, y);
        }

        protected virtual bool IgnoreHeadTrackFromAnimator()
        {
            return animatorTags.Exists(tag => IsAnimatorTag(tag));
        }

        public virtual bool IsAnimatorTag(string tag)
        {
            if (animator == null) return false;
            if (animatorStateInfos.IsValid())
            {
                if (animatorStateInfos.AnimatorStateInfos.HasTag(tag))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual float OffsetHeightResult
        {
            get
            {
                return _currentOffSetLookHeight = Mathf.Lerp(_currentOffSetLookHeight, targetOffsetHeight, Smooth * 2f);
            }
        }

        protected virtual float NormalizeAngle(float angle)
        {
            if (angle < -180)
                return angle + 360;
            else if (angle > 180)
                return angle - 360;
            else
                return angle;
        }

        protected virtual bool LookDirectionIsOnRange(Vector3 direction)
        {
            var angle = GetTargetAngle(direction);
            return (angle.x >= LimitAngleX.x && angle.x <= LimitAngleX.y && angle.y >= LimitAngleY.x &&
                    angle.y <= LimitAngleY.y);
        }

        #endregion

        #endregion

        #region PUBLIC VIRTUAL METHODS. LOOK POINT AND TARGET BEHAVIOUR

        /// <summary>
        /// Set the definitive look Target
        /// </summary>
        /// <param name="target"></param>
        public virtual void SetMainLookTarget(Transform target)
        {
            mainLookTarget = target;
        }

        /// <summary>
        /// Remove the definitive look Target
        /// </summary>
        /// <param name="target"></param>
        public virtual void RemoveMainLookTarget()
        {
            mainLookTarget = null;
        }

        /// <summary>
        /// Simulate a Look Around Animation using the Headtrack
        /// </summary>
        public virtual void LookAround()
        {
            lookAroundProgress += Time.deltaTime * lookAroundSpeed;
            var pp = Mathf.PingPong(lookAroundProgress, 1f);
            var l = Quaternion.AngleAxis(Mathf.Lerp(-lookAroundAngle, lookAroundAngle, lookAroundCurve.Evaluate(pp)),
                transform.up) * transform.forward;
            var eyesPoint = eyes ? eyes.position : transform.position + Vector3.up * headHeight;
            var lookp = eyesPoint + l * 100f;
            LookAtPoint(lookp, 0.1f);
        }

        /// <summary>
        /// Look at point. Set a point to follow for default time (override lookTarget).<seealso cref="vAIHeadtrack.timeToExitLookPoint"/> 
        /// if you need to follow point, call this in update or use <seealso cref="vAIHeadtrack.LookAtTarget(Transform)"/>
        /// </summary>
        /// <param name="point">Point to look</param>
        public virtual void LookAtPoint(Vector3 point, float offsetLookHeight = -1)
        {
            if (inLockPoint) return;
            if (offsetLookHeight != -1) SetOffsetLookHeight(offsetLookHeight);
            else ResetOffseLookHeight();
            temporaryLookPoint = point;
            exitLookPointTime = timeToExitLookPoint;
        }

        /// <summary>
        /// Look at point. Set a point to follow for a especific time (override lookTarget).
        /// if you need to follow point, call this in update or use <seealso cref="vAIHeadtrack.LookAtTarget(Transform, float)"/>
        /// </summary>
        /// <param name="point">Point to look</param>
        /// <param name="timeToExitLookPoint">Time that will to be looking</param>
        public virtual void LookAtPoint(Vector3 point, float timeToExitLookPoint, float offsetLookHeight = -1)
        {
            if (inLockPoint) return;
            if (offsetLookHeight != -1) SetOffsetLookHeight(offsetLookHeight);
            else ResetOffseLookHeight();
            temporaryLookPoint = point;
            exitLookPointTime = timeToExitLookPoint;
        }

        /// <summary>
        /// Look at target.
        /// Set a target to follow for default time <seealso cref="vAIHeadtrack.timeToExitLookTarget"/> 
        /// if you need to follow target always, call <seealso cref="vAIHeadtrack.LookAtPoint(Vector3)"/> in update
        /// </summary>
        /// <param name="target"> Target to look</param>
        public virtual void LookAtTarget(Transform target, float offsetLookHeight = -1)
        {
            if (inLockTarget) return;
            if (offsetLookHeight != -1) SetOffsetLookHeight(offsetLookHeight);
            else ResetOffseLookHeight();
            temporaryLookTarget = target;
            exitLookTargetTime = timeToExitLookPoint;
        }

        /// <summary>
        /// Look at target.
        /// Set a target to follow for a especific time
        /// if you need to follow target always, call <seealso cref="vAIHeadtrack.LookAtPoint(Vector3,float)"/> in update or Call <seealso cref="vAIHeadtrack.LockLookAt"/>
        /// </summary>
        /// <param name="target">Target to look</param>
        /// <param name="timeToExitLookTarget"> Time that will to be looking</param>
        public virtual void LookAtTarget(Transform target, float timeToExitLookTarget, float offsetLookHeight = -1)
        {
            if (inLockTarget) return;
            if (offsetLookHeight != -1) SetOffsetLookHeight(offsetLookHeight);
            else ResetOffseLookHeight();

            temporaryLookTarget = target;
            exitLookTargetTime = timeToExitLookTarget;
        }

        /// <summary>
        /// Lock the current temporary look point
        /// </summary>
        public virtual void LockLookAtPoint()
        {
            inLockPoint = true;
            inLockTarget = false;
        }

        /// <summary>
        /// Unlock the current temporary look point
        /// </summary>
        public virtual void UnlockLookAtPoint()
        {
            inLockPoint = false;
        }

        /// <summary>
        /// Lock the current temporary look target
        /// </summary>
        public virtual void LockLookAtTarget()
        {
            inLockTarget = true;
            inLockPoint = false;
        }

        /// <summary>
        /// Unlock the current temporary look target
        /// </summary>
        public virtual void UnlockLookAtTarget()
        {
            inLockTarget = false;
        }

        /// <summary>
        /// Remove the temporary look point (Ignore timeToExit) <seealso cref="vAIHeadtrack.timeToExitLookPoint"/>
        /// </summary>
        public virtual void ResetLookPoint()
        {
            exitLookPointTime = 0;
            inLockPoint = false;
        }

        /// <summary>
        /// Remove the temporary look target (Ignore timeToExit) <seealso cref="vAIHeadtrack.timeToExitLookTarget"/>
        /// </summary>
        public virtual void ResetLookTarget()
        {
            exitLookTargetTime = 0;
            temporaryLookTarget = null;
            inLockTarget = false;
        }

        /// <summary>
        /// Remove all temporary look (Ignore timeToExit)<seealso cref="vAIHeadtrack.timeToExitLookPoint"/>, <seealso cref="vAIHeadtrack.timeToExitLookTarget"/>
        /// </summary>
        public virtual void ResetLook()
        {
            ResetLookPoint();
            ResetLookTarget();
        }

        /// <summary>
        /// Check if has a look point or look target
        /// </summary>
        public virtual bool isLookingForSomething
        {
            get { return ((defaultLookPoint != targetLookPoint && canLook) || isInSmoothValues); }
        }

        /// <summary>
        /// Set off set look height
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetOffsetLookHeight(float value)
        {
            targetOffsetHeight = value;
        }

        /// <summary>
        /// Set offset look Height to default <see cref="vAIHeadtrack.defaultOffSetLookHeight"/>
        /// </summary>
        public virtual void ResetOffseLookHeight()
        {
            if (targetOffsetHeight != defaultOffSetLookHeight)
                targetOffsetHeight = defaultOffSetLookHeight;
        }

        #endregion
    }
}