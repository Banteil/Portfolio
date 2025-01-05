using UnityEngine;
using DG.Tweening;

namespace starinc.io.kingnslave
{
    public class MultiOpponentFieldCard : FieldCard
    {
        protected override void Start()
        {
            base.Start();
            InGameManager.Instance.OpponentSubmitDone.AddPersistentListener(SetCard);
            InGameManager.Instance.BattlePhaseStart.AddPersistentListener(OpenCardDelay);
            InGameManager.Instance.EndPhaseStart.AddPersistentListener(Clear);
        }

        protected override void OpenCardAnim()
        {
            base.OpenCardAnim();
            transform.DORotate(new Vector3(0, transform.rotation.eulerAngles.y - 90, 0), Define.CARD_OPENNING_TIME / 2).SetEase(Ease.InBack).onComplete = () =>
            {
                if (IsBackSide) { Flip(); }
                transform.DORotate(new Vector3(0, transform.rotation.eulerAngles.y - 90, 0), Define.CARD_OPENNING_TIME / 2).SetEase(Ease.OutBack).onComplete = () =>
                {
                    //OpenCardComplete();
                };
            };
        }

        public override void CardAnimEndEventWithChangePhase()
        {
            animator.enabled = false;
            nameplateImage.gameObject.SetActive(true);
            characterRT.pivot = new Vector2(0.5f, 0.5f);
            characterRT.anchoredPosition = Vector2.zero;
            InGameManager.Instance.EndPhaseStart?.Invoke();
        }

        public override void CardAnimEndEvent()
        {
            animator.enabled = false;
            nameplateImage.gameObject.SetActive(true);
            characterRT.pivot = new Vector2(0.5f, 0.5f);
            characterRT.anchoredPosition = Vector2.zero;
        }
    }
}