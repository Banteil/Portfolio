using UnityEngine;

namespace starinc.io
{
    public class BreadController : CreatureController
    {
        #region Cache
        private const int MAX_HP = 3;

        public override int HP
        {
            get { return ControllerAnimator.GetInteger("HP"); }
            set
            {
                if (value > MAX_HP)
                {
                    if (HP == MAX_HP) return;
                    else
                        value = MAX_HP;
                }
                ControllerAnimator.SetInteger("HP", value);
                ChangedHP(value);
                if (value <= 0)
                    DisableController();
            }
        }

        private Vector2 _moveVec
        {
            get
            {
                var vector = new Vector2();
                vector.x = ControllerAnimator.GetFloat("MoveX");
                vector.y = ControllerAnimator.GetFloat("MoveY");
                return vector;
            }
            set
            {
                ControllerAnimator.SetFloat("MoveX", value.x);
                ControllerAnimator.SetFloat("MoveY", value.y);
            }
        }

        private Vector2 _lastMoveVec
        {
            get
            {
                var vector = new Vector2();
                vector.x = ControllerAnimator.GetFloat("LastMoveX");
                vector.y = ControllerAnimator.GetFloat("LastMoveY");
                return vector;
            }
            set
            {
                ControllerAnimator.SetFloat("LastMoveX", value.x);
                ControllerAnimator.SetFloat("LastMoveY", value.y);
                Renderer.flipX = value.x > 0;
            }
        }

        [SerializeField]
        private float _speed = 10;
        public override float Speed { get { return _speed; } set { _speed = value; } }
        #endregion

        protected override void ChangedHP(int hp)
        {
            ControllerAnimator.SetTrigger("ChangedHP");
            base.ChangedHP(hp);
        }

        public override void InputAction(Vector2 input)
        {
            _moveVec = input;
            var isMove = input != Vector2.zero;
            ControllerAnimator.SetBool("IsMove", isMove);
            if (isMove)
            {
                _lastMoveVec = input;
                var direction = input.normalized;
                var movePos = Rigid2D.position + (direction * Speed * Time.deltaTime);
                Rigid2D.MovePosition(movePos);
            }
        }

        protected override void DisableController()
        {
            Collider.enabled = false;
            base.DisableController();
        }
    }
}
