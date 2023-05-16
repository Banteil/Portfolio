namespace Zeus
{
    public interface IAttackListener
    {
        //공격 가능한지 확인 후 IsAttacking을 true로 전환
        void OnEnableAttack();
        //IsAttacking을 false로 전환
        void OnDisableAttack();
        //공격애니메이션의 Trigger를 Reset
        void ResetAttackTriggers();
    }
}