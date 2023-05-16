using System;

namespace Zeus
{
    public class ExcutionManager : BaseObject<ExcutionManager>
    {
        /// <param name="owner">처형 실행자</param>
        /// <param name="target">처형 대상자</param>
        public void Excute(Character owner, Character target, ExcutionData excutionData, Action onFinish = null)
        {
            var excutionBehaviour = TableManager.GetExcutionBehaviour(excutionData.BehaviourPath);
            if (excutionBehaviour == null) { onFinish?.Invoke(); return; }
            excutionBehaviour.transform.SetParent(this.transform);
            excutionBehaviour.Play(owner, target, excutionData, onFinish);
        }
    }
}
