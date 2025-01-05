using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io.kingnslave
{
    public class SingleYourHandsCard : YourHandsCard, IPointerClickHandler
    {
        private const int INCREASING_Y_POS = 100;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            increasedLocalPos = new Vector3(originLocalPos.x, originLocalPos.y + INCREASING_Y_POS, originLocalPos.z);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (selectedCardNum < transform.parent.childCount && selectedCardNum >= 0)
            {
                transform.parent.GetChild(selectedCardNum).GetComponent<YourHandsCard>().ResetPosition();
            }
            selectedCardNum = cardNumInHands;
            IncreasePosition();
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.SelectCard));
            InGameManager.Instance.ClickCard?.Invoke(cardNumInHands);

        }
    }
}