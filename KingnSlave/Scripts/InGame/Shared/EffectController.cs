using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EffectController : MonoBehaviour
{
	private const float CARD_BACK_SIDE_EFFECT_DELAY = 5f;
	private const float SLAVE_ATTACK_X_POS = -50f;
	private const string BATTLE_EFFECT_PARAM_ATTACKED = "OnAttacked";
	private const string BATTLE_EFFECT_PARAM_DEFENSED = "OnDefensed";
	private const string BATTLE_EFFECT_PARAM_ON_SLAVE_ATTACK = "OnSlaveAttack";
	private const string SCREEN_EFFECT_PARAM_ON_FLASHING = "OnFlashing";

	[SerializeField]
	private RectTransform yourFieldRect;
	[SerializeField]
	private RectTransform opponentFieldRect;

	[SerializeField]
	private Animator yourFieldEffect;
	[SerializeField]
	private Animator opponentFieldEffect;
	[SerializeField]
	private Animator massiveFieldEffect;
	[SerializeField]
	private Animator screenEffect;

	private RectTransform yourFieldEffectRect;
	private RectTransform opponentFieldEffectRect;

	public UnityEvent CardEffectPlayEvent = new UnityEvent();

    private void Start()
    {
		yourFieldEffectRect = yourFieldEffect.GetComponent<RectTransform>();
		opponentFieldEffectRect = opponentFieldEffect.GetComponent<RectTransform>();
		StartCoroutine(PlayCardEffect());
    }

	private void ResetYourFieldPos()
	{
        yourFieldEffectRect.localPosition = yourFieldRect.localPosition;
    }

	private void ResetOpponentFieldPos()
	{
        opponentFieldEffectRect.localPosition = opponentFieldRect.localPosition;
    }

	private void SetYourFieldPos(float x, float y)
	{
        yourFieldEffectRect.localPosition = yourFieldRect.localPosition + new Vector3(x, y);
    }

	private void SetOpponentFieldPos(float x, float y)
	{
        opponentFieldEffectRect.localPosition = opponentFieldRect.localPosition + new Vector3(x, y);
    }

	private IEnumerator PlayCardEffect()
	{
		WaitForSecondsRealtime effectWaitingTime = new WaitForSecondsRealtime(CARD_BACK_SIDE_EFFECT_DELAY);
		while (true)
		{
			CardEffectPlayEvent?.Invoke();
			yield return effectWaitingTime;
		}
	}

    public void PlayYourFieldAttacked()
	{
        ResetYourFieldPos();
        yourFieldEffect.SetTrigger(BATTLE_EFFECT_PARAM_ATTACKED);
    }

	public void PlayYourFieldDefensed()
	{
		ResetYourFieldPos();
        yourFieldEffect.SetTrigger(BATTLE_EFFECT_PARAM_DEFENSED);
	}

	public void PlayOpponentFieldAttacked()
	{
        ResetOpponentFieldPos();
        opponentFieldEffect.SetTrigger(BATTLE_EFFECT_PARAM_ATTACKED);
    }

	public void PlayOpponentFieldDefensed()
	{
        ResetOpponentFieldPos();
        opponentFieldEffect.SetTrigger(BATTLE_EFFECT_PARAM_DEFENSED);
	}

    public void PlayYourFieldKingAttacked()
    {
		SetYourFieldPos(SLAVE_ATTACK_X_POS, 0);
		screenEffect.SetTrigger(SCREEN_EFFECT_PARAM_ON_FLASHING);
        yourFieldEffect.SetTrigger(BATTLE_EFFECT_PARAM_ON_SLAVE_ATTACK);
    }

    public void PlayOpponentFieldKingAttacked()
    {
        SetOpponentFieldPos(SLAVE_ATTACK_X_POS, 0);
        screenEffect.SetTrigger(SCREEN_EFFECT_PARAM_ON_FLASHING);
        opponentFieldEffect.SetTrigger(BATTLE_EFFECT_PARAM_ON_SLAVE_ATTACK);
    }

    //public void PlayMassiveFieldSlaveAttack()
    //{
    //    massiveFieldEffect.SetTrigger(BATTLE_EFFECT_PARAM_ON_SLAVE_ATTACK);
    //}
}