using UnityEngine;
using DG.Tweening;

namespace starinc.io.kingnslave
{
    public class SingleOpponentFieldCard : FieldCard
    {
        [SerializeField]
        private SingleOpponentHands opponentHands;

        protected override void Start()
        {
            base.Start();
            InGameManager.Instance.AIChangeSubmitedCard.AddPersistentListener(ChangeCard);
            InGameManager.Instance.OpponentSubmitDone.AddPersistentListener(SetAnyCard);
            InGameManager.Instance.BattlePhaseStart.AddPersistentListener(OpenCardDelay);
            InGameManager.Instance.EndPhaseStart.AddPersistentListener(Clear);
        }

        /// <summary>
        /// 승리확률에 따라 카드 설정
        /// </summary>
        /// <param name="playerCard"></param>
        /// <param name="winProb"></param>
        private void ChangeCard(Define.CardType playerCard, float winProb)
        {
            float randomNum = Random.Range(float.Epsilon, 1f);
            Define.CardType newCard;

            if (opponentHands.HandsCount <= 0)
            {
                newCard = InGameManager.Instance.Opponent.IsKingPlayer ? Define.CardType.King : Define.CardType.Slave;
                InGameManager.Instance.Opponent.RPC_SetSubmitedCard((int)newCard);
                SetCard(newCard);
                return;
            }

            bool isAIWin = (winProb < randomNum);
            switch (playerCard)
            {
                case Define.CardType.Slave:
                    newCard = isAIWin ? Define.CardType.Citizen : Define.CardType.King;
                    break;
                case Define.CardType.King:
                    newCard = isAIWin ? Define.CardType.Slave : Define.CardType.Citizen;
                    break;
                case Define.CardType.Citizen:
                    newCard = InGameManager.Instance.Opponent.IsKingPlayer ?
                        (isAIWin ? Define.CardType.King : Define.CardType.Citizen) :
                        (isAIWin ? Define.CardType.Citizen : Define.CardType.Slave);
                    break;
                default:
                    newCard = Define.CardType.Citizen;
                    break;
            }

            InGameManager.Instance.Opponent.RPC_SetSubmitedCard((int)newCard);
            SetCard(newCard);
            Debug.Log($"---AI win?: [{isAIWin}], AI has king?: [{InGameManager.Instance.Opponent.IsKingPlayer}], winProb [{winProb}]---");
        }

        private void SetAnyCard(int cardType)
        {
            IsBackSide = true;
            characterRT.sizeDelta = characterOriginalSizeDelta;
            frontSide.SetActive(true);
            backSide.SetActive(true);
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