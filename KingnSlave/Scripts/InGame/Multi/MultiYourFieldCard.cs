using UnityEngine;
using DG.Tweening;

namespace starinc.io.kingnslave
{
    public class MultiYourFieldCard : FieldCard
    {
        protected override void Start()
        {
            base.Start();
            InGameManager.Instance.YourSubmitDone.AddPersistentListener(SetCard);
            InGameManager.Instance.BattlePhaseStart.AddPersistentListener(OpenCardDelay);
            InGameManager.Instance.EndPhaseStart.AddPersistentListener(Clear);
        }

        protected override void OpenCardAnim()
        {
            base.OpenCardAnim();
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.OpenCard));
            transform.DORotate(new Vector3(0, transform.rotation.eulerAngles.y + 90, 0), Define.CARD_OPENNING_TIME / 2).SetEase(Ease.InBack).onComplete = () =>
            {
                if (IsBackSide) { Flip(); }
                transform.DORotate(new Vector3(0, transform.rotation.eulerAngles.y + 90, 0), Define.CARD_OPENNING_TIME / 2).SetEase(Ease.OutBack).onComplete = () =>
                {
                    OpenCardComplete();
                };
            };
        }

        protected override void OpenCardComplete()
        {
			float animationTime = Define.BATTLE_ANIMATION_TIME;
            const string ANIM_END_METHOD_NAME = "CardAnimEndEventWithChangePhase";

            switch (InGameManager.Instance.GetYourSubmitCardType(), InGameManager.Instance.GetOpponentSubmitCardType())
			{
				case (Define.CardType.Citizen, Define.CardType.Citizen):
					AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.CitizenVsCitizen));
					InGameManager.Instance.EffectController.PlayYourFieldDefensed();
					InGameManager.Instance.EffectController.PlayOpponentFieldDefensed();
					animationTime = Define.BATTLE_DRAW_ANIMATION_TIME;

                    Invoke(ANIM_END_METHOD_NAME, animationTime);
                    break;
				case (Define.CardType.Citizen, Define.CardType.Slave):
					AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.CitizenVsSlave));
					InGameManager.Instance.EffectController.PlayOpponentFieldAttacked();

                    InGameManager.Instance.CardAnimController.PlayYourFieldCitizenAttack();
                    InGameManager.Instance.CardAnimController.PlayOpponentFieldSlaveAttacked();
                    break;
				case (Define.CardType.Slave, Define.CardType.Citizen):
					AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.CitizenVsSlave));
					InGameManager.Instance.EffectController.PlayYourFieldAttacked();

                    InGameManager.Instance.CardAnimController.PlayYourFieldSlaveAttacked();
                    InGameManager.Instance.CardAnimController.PlayOpponentFieldCitizenAttack();
                    break;
				case (Define.CardType.King, Define.CardType.Citizen):
					AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.KingVsCitizen));
					InGameManager.Instance.EffectController.PlayOpponentFieldAttacked();

					InGameManager.Instance.CardAnimController.PlayYourFieldKingAttack();
                    InGameManager.Instance.CardAnimController.PlayOpponentFieldCitizenAttacked();
                    break;
				case (Define.CardType.Citizen, Define.CardType.King):
					AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.KingVsCitizen));
					InGameManager.Instance.EffectController.PlayYourFieldAttacked();

                    InGameManager.Instance.CardAnimController.PlayYourFieldCitizenAttacked();
                    InGameManager.Instance.CardAnimController.PlayOpponentFieldKingAttack();
					break;
				case (Define.CardType.Slave, Define.CardType.King):
					AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.SlaveVsKing));
                    InGameManager.Instance.EffectController.PlayOpponentFieldKingAttacked();
                    //InGameManager.Instance.EffectController.PlayMassiveFieldSlaveAttack();

                    InGameManager.Instance.CardAnimController.PlayYourFieldSlaveAttack();
                    InGameManager.Instance.CardAnimController.PlayOpponentFieldKingAttacked();
                    break;
				case (Define.CardType.King, Define.CardType.Slave):
					AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.SlaveVsKing));
                    InGameManager.Instance.EffectController.PlayYourFieldKingAttacked();
                    //InGameManager.Instance.EffectController.PlayMassiveFieldSlaveAttack();

                    InGameManager.Instance.CardAnimController.PlayYourFieldKingAttacked();
                    InGameManager.Instance.CardAnimController.PlayOpponentFieldSlaveAttack();
                    break;
				default:
					Debug.Log("Card Compare Error");
					AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.CitizenVsCitizen));

                    Invoke(ANIM_END_METHOD_NAME, animationTime);
                    break;
			}

			//Invoke("BattleDone", animationTime);
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