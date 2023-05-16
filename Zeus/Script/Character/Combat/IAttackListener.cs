namespace Zeus
{
    public interface IAttackListener
    {
        //���� �������� Ȯ�� �� IsAttacking�� true�� ��ȯ
        void OnEnableAttack();
        //IsAttacking�� false�� ��ȯ
        void OnDisableAttack();
        //���ݾִϸ��̼��� Trigger�� Reset
        void ResetAttackTriggers();
    }
}