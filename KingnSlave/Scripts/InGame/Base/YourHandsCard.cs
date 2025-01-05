using UnityEngine;

namespace starinc.io.kingnslave
{
    public class YourHandsCard : Card
    {
        private const string KING_IDLE_TRIGGER_NAME = "OnKingIdle";
        private const string SLAVE_IDLE_TRIGGER_NAME = "OnSlaveIdle";
        private const string CITIZEN_IDLE_TRIGGER_NAME = "OnCitizenIdle";

        protected static int selectedCardNum = -1;

        protected int cardNumInHands = default;
        protected Vector3 originLocalPos = Vector3.zero;
        protected Vector3 increasedLocalPos = Vector3.zero;

        [SerializeField]
        protected RuntimeAnimatorController handsCardAnimationController;
        protected Animator animator;

        protected override void Awake()
        {
            base.Awake();
            animator = GetComponent<Animator>();
            animator.runtimeAnimatorController = handsCardAnimationController;
        }

        protected override void Start()
        {
            originLocalPos = transform.localPosition;
            cardNumInHands = transform.GetSiblingIndex();
        }

        public override void SetCard(Define.CardType cardType)
        {
            base.SetCard(cardType);
            ResetPosition();
        }

        public void ResetPosition()
        {
            transform.localPosition = originLocalPos;
        }

        protected void IncreasePosition()
        {
            if (selectedCardNum != cardNumInHands)
            {
                return;
            }
            transform.localPosition = increasedLocalPos;
        }

        public override void SetFrontSide()
        {
            base.SetFrontSide();
            //animator.enabled = true;
            //if (Type == Define.CardType.King)
            //{
            //    animator.SetTrigger(KING_IDLE_TRIGGER_NAME);
            //}
            //else if (Type == Define.CardType.Slave)
            //{
            //    animator.SetTrigger(SLAVE_IDLE_TRIGGER_NAME);
            //}
            //else
            //{
            //    animator.SetTrigger(CITIZEN_IDLE_TRIGGER_NAME);
            //}
        }
    }
}