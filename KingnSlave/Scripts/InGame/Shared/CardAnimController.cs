using UnityEngine;

public class CardAnimController : MonoBehaviour
{
    private const string CARD_ANIM_PARAM_ON_KING_ATTACK = "OnKingAttack";
    private const string CARD_ANIM_PARAM_ON_SLAVE_ATTACK = "OnSlaveAttack";
    private const string CARD_ANIM_PARAM_ON_CITIZEN_ATTACK = "OnCitizenAttack";
    private const string CARD_ANIM_PARAM_ON_KING_ATTACKED = "OnKingAttacked";
    private const string CARD_ANIM_PARAM_ON_SLAVE_ATTACKED = "OnSlaveAttacked";
    private const string CARD_ANIM_PARAM_ON_CITIZEN_ATTACKED = "OnCitizenAttacked";

    [SerializeField]
    private Animator yourFieldCardAnim;
    [SerializeField]
    private Animator opponentFieldCardAnim;

    public void PlayYourFieldKingAttack()
    {
        yourFieldCardAnim.enabled = true;
        yourFieldCardAnim.SetTrigger(CARD_ANIM_PARAM_ON_KING_ATTACK);
    }

    public void PlayOpponentFieldKingAttack()
    {
        opponentFieldCardAnim.enabled = true;
        opponentFieldCardAnim.SetTrigger(CARD_ANIM_PARAM_ON_KING_ATTACK);
    }

    public void PlayYourFieldKingAttacked()
    {
        yourFieldCardAnim.enabled = true;
        yourFieldCardAnim.SetTrigger(CARD_ANIM_PARAM_ON_KING_ATTACKED);
    }

    public void PlayOpponentFieldKingAttacked()
    {
        opponentFieldCardAnim.enabled = true;
        opponentFieldCardAnim.SetTrigger(CARD_ANIM_PARAM_ON_KING_ATTACKED);
    }

    public void PlayYourFieldSlaveAttack()
    {
        yourFieldCardAnim.enabled = true;
        yourFieldCardAnim.SetTrigger(CARD_ANIM_PARAM_ON_SLAVE_ATTACK);
    }

    public void PlayOpponentFieldSlaveAttack()
    {
        opponentFieldCardAnim.enabled = true;
        opponentFieldCardAnim.SetTrigger(CARD_ANIM_PARAM_ON_SLAVE_ATTACK);
    }

    public void PlayYourFieldSlaveAttacked()
    {
        yourFieldCardAnim.enabled = true;
        yourFieldCardAnim.SetTrigger(CARD_ANIM_PARAM_ON_SLAVE_ATTACKED);
    }

    public void PlayOpponentFieldSlaveAttacked()
    {
        opponentFieldCardAnim.enabled = true;
        opponentFieldCardAnim.SetTrigger(CARD_ANIM_PARAM_ON_SLAVE_ATTACKED);
    }

    public void PlayYourFieldCitizenAttack()
    {
        yourFieldCardAnim.enabled = true;
        yourFieldCardAnim.SetTrigger(CARD_ANIM_PARAM_ON_CITIZEN_ATTACK);
    }

    public void PlayOpponentFieldCitizenAttack()
    {
        opponentFieldCardAnim.enabled = true;
        opponentFieldCardAnim.SetTrigger(CARD_ANIM_PARAM_ON_CITIZEN_ATTACK);
    }

    public void PlayYourFieldCitizenAttacked()
    {
        yourFieldCardAnim.enabled = true;
        yourFieldCardAnim.SetTrigger(CARD_ANIM_PARAM_ON_CITIZEN_ATTACKED);
    }

    public void PlayOpponentFieldCitizenAttacked()
    {
        opponentFieldCardAnim.enabled = true;
        opponentFieldCardAnim.SetTrigger(CARD_ANIM_PARAM_ON_CITIZEN_ATTACKED);
    }
}
