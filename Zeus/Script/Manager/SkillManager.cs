using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class SkillManager : BaseObject<SkillManager>
    {
        public List<SkillController> ControllerList;

        internal void FireSkill(int skillID, Vector3 position, Vector3 dir, CombatManager combatManager, Transform parent = null)
        {
            ControllerList ??= new();

            var skillTableData = TableManager.GetSkillTableData(skillID);
            if (skillTableData == null)
                return;

            var tr = skillTableData.FireType == TypeSkillFire.WORLD ? null : parent;
            var skillData = new SkillController(skillTableData, position, dir, combatManager, tr);
            ControllerList.Add(skillData);

            foreach (var item in skillTableData.SequenceSkillDatas)
            {
                var sequenceData = new SkillController(item, position, dir, combatManager, tr);
                ControllerList.Add(sequenceData);
            }
        }

        private void Update()
        {
            if (ControllerList == null)
                return;

            for (int i = ControllerList.Count - 1; i >= 0; --i)
            {
                var item = ControllerList[i];
                if (item == null)
                    ControllerList.RemoveAt(i);

                item.ElapsedTime += GameTimeManager.Instance.DeltaTime;

                if (item.RemainTime <= 0)
                {
                    item.SkillFire();
                    ControllerList.RemoveAt(i);
                }
            }
        }
    }
}