namespace Zeus
{
    public interface IAttackReceiver
    {
        void OnReceiveAttack(Damage damage, ICharacter character);
    }
}
