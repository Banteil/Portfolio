using UnityEngine;

namespace starinc.io.kingnslave
{
    public class MultiOpponentHands : MonoBehaviour
    {
        private const float IncreaseYPos = 100f;
        private float originYPos;

        private void Start()
        {
            originYPos = transform.GetChild(0).localPosition.y;
            InGameManager.Instance.NewRoundStart.AddPersistentListener(ResetCard);
            InGameManager.Instance.MultiOpponentClickCard.AddPersistentListener(UpdateCardPosition);
            InGameManager.Instance.MultiOpponentSubmitStartEvent.AddPersistentListener(DeleteSubmitedCard);
        }

        public void UpdateCardPosition(int cardNum)
        {
            if (cardNum < 0 || cardNum >= transform.childCount || !transform.GetChild(cardNum).gameObject.activeSelf) { return; }
            ResetCardPosition();
            transform.GetChild(cardNum).localPosition -= new Vector3(0, IncreaseYPos, 0);
        }

        public void ResetCard()
        {
            ResetCardPosition();
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }

        public void DeleteSubmitedCard(int cardNum)
        {
            transform.GetChild(cardNum).gameObject.SetActive(false);
			AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.SubmitCard));
		}

        private void ResetCardPosition()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).localPosition = new Vector3(transform.GetChild(i).localPosition.x, originYPos, transform.GetChild(i).localPosition.z);
            }
        }
    }
}