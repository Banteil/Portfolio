using System;

namespace Zeus
{
    public class ExcutionManager : BaseObject<ExcutionManager>
    {
        /// <param name="owner">ó�� ������</param>
        /// <param name="target">ó�� �����</param>
        public void Excute(Character owner, Character target, ExcutionData excutionData, Action onFinish = null)
        {
            var excutionBehaviour = TableManager.GetExcutionBehaviour(excutionData.BehaviourPath);
            if (excutionBehaviour == null) { onFinish?.Invoke(); return; }
            excutionBehaviour.transform.SetParent(this.transform);
            excutionBehaviour.Play(owner, target, excutionData, onFinish);
        }
    }
}
