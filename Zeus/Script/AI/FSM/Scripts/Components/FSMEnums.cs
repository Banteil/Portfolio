
namespace Zeus
{
    [System.Flags]
    public enum FSMComponentExecutionType
    {
        OnStateUpdate = 1,
        OnStateEnter =2,       
        OnStateExit = 4,

    }
    public enum TransitionOutputType
    {
        Default,  TrueFalse
    }
}