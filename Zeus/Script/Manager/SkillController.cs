using UnityEngine;

namespace Zeus
{
    public class SkillController
    {
        public SkillController(SequenceSkillData data, Vector3 position, Vector3 dir, CombatManager manager, Transform transform = null)
        {
            skillData = data;
            combatManager = manager;
            skillStartPosition = position;
            skillDirection = dir;
            parent = transform;
            ElapsedTime = 0f;
        }

        internal float RemainTime => skillData.DelayActiveTime - ElapsedTime;
        internal float ElapsedTime { get; set; }

        private SequenceSkillData skillData;
        private CombatManager combatManager;
        private Vector3 skillStartPosition;
        private Vector3 skillDirection;
        private Transform parent;

        internal void SkillFire()
        {
            TableManager.Instance.GetGameObjectAsync(skillData.AssetName, result =>
            {
                if (result == null)
                    return;

                var position = skillStartPosition;
                if (skillData.TargetType == TypeSkillTarget.AUTO)
                {
                    if (combatManager != null)
                    {
                        var component = combatManager.gameObject.GetComponent<LockOnBehaviour>();
                        if (component != null)
                        {
                            if (component.CurrentTarget != null)
                            {
                                position = component.CurrentTarget.position;
                            }
                            else
                            {
                                var target = component.GetTarget();
                                if (target == null)
                                {
                                    GameObject.Destroy(result);
                                    return;
                                }
                                position = target.position;
                            }
                        }
                    }
                }

                SoundManager.Instance.Play(skillData.SoundTableID, position);

                var rot = Quaternion.LookRotation(skillDirection, Vector3.up);
                result.transform.SetPositionAndRotation(position, rot);
                if (parent != null)
                    result.transform.SetParent(parent, false);
                result.SetActive(true);
                var attackOB = result.GetComponentInChildren<AttackObject>();
                if (attackOB != null)
                {
                    if (attackOB.DamageInfo.AttackerHitProperties == null)
                    {
                        attackOB.DamageInfo.AttackerHitProperties = new HitProperties(attackOB.HitProperties);
                        attackOB.DamageInfo.AttackerHitProperties.SenderInfo.SkillID = skillData.ID;
                        attackOB.DamageInfo.AttackerHitProperties.DefaultDamageVaule = skillData.Damage;
                    }
                }
                result.DestroyTimer(skillData.LifeTime);
            });
        }
    }
}