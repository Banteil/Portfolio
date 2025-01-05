using UnityEngine;

namespace starinc.io.kingnslave
{
    public class FieldCard : Card
    {
        protected Animator animator;

        protected override void Awake()
        {
            base.Awake();
            animator = GetComponent<Animator>();
            animator.enabled = false;
        }

        protected override void Start()
        {
            Clear();
        }

        public virtual void OpenCardDelay()
        {
            Invoke("OpenCardAnim", Define.CARD_OPENNING_TIME);
        }

        protected void Clear()
        {
            frontSide.SetActive(false);
            backSide.SetActive(false);
            IsBackSide = true;
            Vector3 currentRot = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(currentRot + new Vector3(0, 180, 0));
        }

        protected virtual void OpenCardAnim()
        {
            //Disable일 때 Invoke 실패
            if (gameObject.activeInHierarchy == false) { return; }
        }

        protected virtual void OpenCardComplete() { }
    }
}