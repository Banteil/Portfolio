using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class Card : MonoBehaviour
    {
        private const string PLAY_CARD_EFFECT_PARAM = "OnPlayEffect";

        public Define.CardType Type { get; private set; }

        [field: SerializeField]
        public bool IsOpponentCard { get; protected set; } = false;

        [field: SerializeField]
        public bool IsBackSide { get; protected set; } = true;

        protected GameObject frontSide;
        protected GameObject backSide;

        protected Image characterImage;
        protected Image backgroundImage;
        protected Image frameImage;
        protected Image nameplateImage;
        protected TMP_Text nameText;
        protected Image backSideImage;
        protected Animator backSideEffect;
        protected RawImage adImage;

        protected RectTransform characterRT;
        protected Vector2 characterOriginalSizeDelta;

        protected virtual void Awake()
        {
            frontSide = transform.GetChild(0).gameObject;
            backSide = transform.GetChild(1).gameObject;

            characterImage = Util.FindChild<Image>(gameObject, "Character", true);
            backgroundImage = Util.FindChild<Image>(gameObject, "Background", true);
            frameImage = Util.FindChild<Image>(gameObject, "Frame", true);
            nameplateImage = Util.FindChild<Image>(gameObject, "Nameplate", true);
            nameText = Util.FindChild<TMP_Text>(gameObject, "Name", true);
            backSideImage = backSide.GetComponent<Image>();
            adImage = Util.FindChild<RawImage>(backSide, "ADImage", true);

            frameImage.sprite = ResourceManager.Instance.GetCardFrameSprite(0);
            nameplateImage.sprite = ResourceManager.Instance.GetCardNameplateSprite(0);

            backSideEffect = backSide.transform.GetChild(0).GetComponent<Animator>();
            backSideEffect.writeDefaultValuesOnDisable = true;
            if (IsOpponentCard && UserDataManager.Instance.OpponentCardSkinImageList.Count > 0 && UserDataManager.Instance.OpponentDataList.Count > 0)
            {
                backSideImage.sprite = UserDataManager.Instance.OpponentCardSkinImageList[0];
                if (UserDataManager.Instance.OpponentDataList[0].item_seq_card_skin != (int)Define.ItemCardSkin.Basic)
                {
                    backSideEffect.enabled = false;
                }
            }
            else if (!IsOpponentCard)
            {
                backSideImage.sprite = UserDataManager.Instance.MyCardSkinImage;
                if (UserDataManager.Instance.MyData.item_seq_card_skin != (int)Define.ItemCardSkin.Basic)
                {
                    backSideEffect.enabled = false;
                }
            }

            if (backSideImage.sprite == null)
            {
                backSideImage.sprite = ResourceManager.Instance.GetCardBackSideSprite(0);
                backSideEffect.enabled = true;
            }

            characterRT = characterImage.GetComponent<RectTransform>();
            characterOriginalSizeDelta = characterRT.sizeDelta;

            InGameManager.Instance.EffectController.CardEffectPlayEvent.AddPersistentListener(PlayCardEffect);
            InGameManager.Instance.ADCallback += DisplayADs;

            frontSide.SetActive(true);
            backSide.SetActive(true);
        }

        protected virtual void Start() { }

        public virtual void SetCard(int cardType)
        {
            IsBackSide = true;
            Type = (Define.CardType)cardType;
            characterImage.sprite = ResourceManager.Instance.GetCardCharacterSprite((Define.CardType)cardType);
            backgroundImage.sprite = ResourceManager.Instance.GetCardBackgroundSprite((Define.CardType)cardType);
            nameText.text = ((Define.CardType)cardType).ToString().ToUpper();
            characterRT.sizeDelta = characterOriginalSizeDelta;
            frontSide.SetActive(true);
            backSide.SetActive(true);
        }

        public virtual void SetCard(Define.CardType cardType)
        {
            IsBackSide = true;
            Type = (Define.CardType)cardType;
            characterImage.sprite = ResourceManager.Instance.GetCardCharacterSprite((Define.CardType)cardType);
            backgroundImage.sprite = ResourceManager.Instance.GetCardBackgroundSprite((Define.CardType)cardType);
            nameText.text = ((Define.CardType)cardType).ToString().ToUpper();
            characterRT.sizeDelta = characterOriginalSizeDelta;
            frontSide.SetActive(true);
            backSide.SetActive(true);
        }

        public virtual void SetFrontSide()
        {
            IsBackSide = false;
            backSide.SetActive(false);
        }

        public void Flip()
        {
            IsBackSide = !IsBackSide;
            backSide.SetActive(IsBackSide);
        }

        public void PlayCardEffect()
        {
            if (!IsBackSide || backSide.activeInHierarchy == false) { return; }
            backSideEffect.SetTrigger(PLAY_CARD_EFFECT_PARAM);
        }

        private void DisplayADs(bool active)
        {
            if (adImage != null && adImage.texture == null && InGameManager.Instance.AdTexture != null)
                adImage.texture = InGameManager.Instance.AdTexture;

            adImage.gameObject.SetActive(active);
        }

        public virtual void CardAnimEndEventWithChangePhase() { }
        public virtual void CardAnimEndEvent() { }

        private void OnDestroy()
        {
            if (InGameManager.HasInstance)
                InGameManager.Instance.ADCallback -= DisplayADs;
        }
    }
}