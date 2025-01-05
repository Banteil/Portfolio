using UnityEngine;

namespace starinc.io.gallaryx
{
    public class JumpAbility : BaseAbility
    {
        #region Cache
        private readonly int _hashJump = Animator.StringToHash("Jump");
        private readonly int _hashIsGround = Animator.StringToHash("IsGround");

        private int _obstaclesLayerMask;
        private int _groundLayerMask;
        private LayerMask _checkJumpLayerMask = -1;
        #endregion

        [SerializeField] protected float _groundCheckDistance = 0.3f;
        [SerializeField] private float _jumpPower = 300f;
        [SerializeField] private float _jumpMoveSpeed = 10f;
        private Vector3 _boxCenter = new Vector3(0f, 0f, 0.1f);
        private Vector3 _boxSize = new Vector3(0.4f, 0.1f, 0.55f);

        private InputState _jumpInput;
        private int _collidedWithGroundCount;

        public bool IsGround
        {
            get { return CharacterAnimator.GetBool(_hashIsGround); }
            set { CharacterAnimator.SetBool(_hashIsGround, value); }
        }

        public bool JumpStart { get { return _jumpInput == InputState.Up; } }

        public bool IsJumping { get; private set; }

        public LayerMask CheckJumpLayerMask { get { return _checkJumpLayerMask; } }

        public float JumpMoveSpeed { get { return _jumpMoveSpeed; } }

        protected override void OnAwake()
        {
            base.OnAwake();
            GroundCheckolliderSetting();

            _obstaclesLayerMask = LayerMask.NameToLayer("Obstacles");
            _groundLayerMask = LayerMask.NameToLayer("Ground");
            _checkJumpLayerMask = (1 << _groundLayerMask) | (1 << _obstaclesLayerMask);
        }

        private void GroundCheckolliderSetting()
        {
            var boxCollider = Util.GetOrAddComponent<BoxCollider>(gameObject);
            boxCollider.isTrigger = true;
            boxCollider.center = _boxCenter;
            boxCollider.size = _boxSize;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void AbilityInputState()
        {
            GroundCheck();
            _jumpInput = AbilityInputEvent.InputJumpKey?.Invoke() ?? InputState.None;
        }

        private void GroundCheck()
        {
            if (IsJumping && _characterController.RigidBodyComponent.linearVelocity.y > 0)
            {
                IsGround = false;
                return;
            }

            var groundRayHit = Physics.RaycastAll(CharacterTransform.position + new Vector3(0f, 0.025f, 0f), -CharacterTransform.up, _groundCheckDistance, _checkJumpLayerMask);
            IsGround = groundRayHit.Length > 0 && _collidedWithGroundCount > 0;
            if (IsGround && IsJumping) IsJumping = false;
        }

        protected override void AbilityInputAction()
        {
            Jump();
        }

        private void Jump()
        {
            if (Util.IsUIFocusing) return;

            if (JumpStart && IsGround)
            {
                _characterController.AnimatorComponent.SetTrigger(_hashJump);
                _characterController.RigidBodyComponent.linearVelocity = Vector3.zero;
                _characterController.RigidBodyComponent.AddForce(transform.up * _jumpPower);
                IsJumping = true;
            }
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == _obstaclesLayerMask || other.gameObject.layer == _groundLayerMask)
                _collidedWithGroundCount++;
        }

        protected void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == _obstaclesLayerMask || other.gameObject.layer == _groundLayerMask)
                _collidedWithGroundCount--;
        }

    }
}
