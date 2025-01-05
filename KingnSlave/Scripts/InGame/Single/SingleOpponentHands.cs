using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public class SingleOpponentHands : MonoBehaviour
    {
        [field: SerializeField]
        public int HandsCount { get; private set; }
        [SerializeField]
        private GameObject cardPrefab;

        private int selectedCardNum = -1;
        private List<Card> cardList = new List<Card>();
        private List<int> activatedCardNumList = new List<int>();

        private void Start()
        {
            for (int i = 0; i < Define.MAX_HANDS; i++)
            {
                cardList.Add(transform.GetChild(i).GetComponent<Card>());
            }

            InGameManager.Instance.NewRoundStart.AddPersistentListener(Refill);
            InGameManager.Instance.AISelectAndSubmitCard.AddPersistentListener(SelectAndSubmitAnyCard);
            InGameManager.Instance.TimeOver.AddPersistentListener(SelectAndSubmitAnyCard);
        }

        public void Refill()
        {
            // 카드 재배정
            for (int i = 0; i < Define.MAX_HANDS; i++)
            {
                cardList[i].gameObject.SetActive(true);
            }

            HandsCount = Define.MAX_HANDS;
        }

        public void SelectAndSubmitAnyCard()
        {
            if (InGameManager.Instance.GetOpponentPhase() != Define.InGamePhase.MainPhase)
            {
                return;
            }    

            activatedCardNumList.Clear();
            for (int i = 0; i < Define.MAX_HANDS; i++)
            {
                if (cardList[i].gameObject.activeSelf)
                {
                    activatedCardNumList.Add(i);
                }
            }
            if (activatedCardNumList.Count <= 0)
            {
                return;
            }

            int randomIndex = Random.Range(0, activatedCardNumList.Count);
            selectedCardNum = activatedCardNumList[randomIndex];
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.SubmitCard));
            cardList[selectedCardNum].gameObject.SetActive(false);
            InGameManager.Instance.OpponentSubmitDone?.Invoke((int)Define.CardType.Citizen);
            ResetSelectedCardNum();

            --HandsCount;
        }

        public void ResetSelectedCardNum()
        {
            selectedCardNum = -1;
        }
    }
}