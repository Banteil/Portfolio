using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public class MultiYourHands : MonoBehaviour
    {
        [SerializeField]
        public GameObject cardPrefab;

        private int selectedCardNum = -1;
        [SerializeField]
        private List<Card> cardList = new List<Card>();
        private List<int> activatedCardNumList = new List<int>();

        private void Start()
        {
            for (int i = 0; i < Define.MAX_HANDS; i++)
            {
                cardList.Add(transform.GetChild(i).GetComponent<Card>());
            }
            InGameManager.Instance.NewRoundStart.AddPersistentListener(Refill);
            InGameManager.Instance.ClickCard.AddPersistentListener(SelectCard);
            InGameManager.Instance.YourSubmitStart.AddPersistentListener(SubmitCard);
            InGameManager.Instance.TimeOver.AddPersistentListener(SubmitAnyCard);
            InGameManager.Instance.NewRoundStart.AddPersistentListener(ResetSelectedCardNum);
        }

        public void Refill()
        {
            // 카드 재배정
            if (InGameManager.Instance.You.IsKingPlayer == true)
            {
                cardList[0].SetCard(Define.CardType.King);
            }
            else
            {
                cardList[0].SetCard(Define.CardType.Slave);
            }
            cardList[0].gameObject.SetActive(true);

            for (int i = 1; i < Define.MAX_HANDS; i++)
            {
                cardList[i].SetCard(Define.CardType.Citizen);
                cardList[i].gameObject.SetActive(true);
            }

            // 카드 위치 셔플
            int specialCardNum = Random.Range(0, Define.MAX_HANDS);
            Define.CardType tempCardType = cardList[0].Type;
            cardList[0].SetCard(cardList[specialCardNum].Type);
            cardList[specialCardNum].SetCard(tempCardType);
            InGameManager.Instance.MultiSetYourSpecialCardNumEvent?.Invoke(specialCardNum);

            //자신의 카드 앞면으로 뒤집기
            foreach(var card in cardList)
            {
                card.SetFrontSide();
            }
        }

        public void SelectCard(int cardNum)
        {
            if (cardNum < 0 || cardNum >= Define.MAX_HANDS) { return; }

            // 두 번 탭하여 카드 제출
            if (selectedCardNum == cardNum)
            {
                InGameManager.Instance.YourSubmitStart?.Invoke();
                return;
            }

            selectedCardNum = cardNum;

            // POST
            StartCoroutine(CallAPI.APIGameRoomInsertGameAction(UserDataManager.Instance.MyData, Define.APIActionCd.select.ToString(), cardList[selectedCardNum].Type, selectedCardNum));
        }

        public void SubmitCard()
        {
            if (InGameManager.Instance.GetYourPhase() != Define.InGamePhase.MainPhase
                || selectedCardNum < 0 || selectedCardNum >= Define.MAX_HANDS) { return; }
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.SubmitCard));
            cardList[selectedCardNum].gameObject.SetActive(false);
            InGameManager.Instance.MultiAddYourSubmitOrder(selectedCardNum);
            InGameManager.Instance.MultiOpponentSubmitStart?.Invoke(selectedCardNum);
            InGameManager.Instance.YourSubmitDone?.Invoke((int)cardList[selectedCardNum].Type);

            // POST
            StartCoroutine(CallAPI.APIGameRoomInsertGameAction(UserDataManager.Instance.MyData, Define.APIActionCd.submit.ToString(), cardList[selectedCardNum].Type, selectedCardNum));

            ResetSelectedCardNum();
        }

        public void SubmitAnyCard()
        {
            if (InGameManager.Instance.GetYourPhase() != Define.InGamePhase.MainPhase) { return; }

            // SelectAnyCard
            int randomlySelectedCardNum;
            if (selectedCardNum >= 0 && cardList[selectedCardNum].gameObject.activeInHierarchy)
            {
                randomlySelectedCardNum = selectedCardNum;
            }
            else
            {
                activatedCardNumList.Clear();
                for (int i = 0; i < Define.MAX_HANDS; i++)
                {
                    if (cardList[i].gameObject.activeSelf)
                    {
                        activatedCardNumList.Add(i);
                    }
                }
                if (activatedCardNumList.Count <= 0) { return; }
                int randomIndex = Random.Range(0, activatedCardNumList.Count);
                randomlySelectedCardNum = activatedCardNumList[randomIndex];
            }

            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.SubmitCard));
            cardList[randomlySelectedCardNum].gameObject.SetActive(false);
            InGameManager.Instance.MultiAddYourSubmitOrder(randomlySelectedCardNum);
            InGameManager.Instance.MultiOpponentSubmitStart?.Invoke(randomlySelectedCardNum);
            InGameManager.Instance.YourSubmitDone?.Invoke((int)cardList[randomlySelectedCardNum].Type);

            // POST
            StartCoroutine(CallAPI.APIGameRoomInsertGameAction(UserDataManager.Instance.MyData, Define.APIActionCd.submit.ToString(), cardList[randomlySelectedCardNum].Type, randomlySelectedCardNum));

            ResetSelectedCardNum();
        }

        private void ResetSelectedCardNum()
        {
            selectedCardNum = -1;
        }
    }
}