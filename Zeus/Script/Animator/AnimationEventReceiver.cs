using UnityEngine;

namespace Zeus
{
    //call Animation Event
    public class AnimationEventReceiver : MonoBehaviour
    {
        internal CombatManager CombatManager { get; set; }
        private void SoundEffect(int soundTableID)
        {
            //Debug.Log("Sound Effect : " + soundTableID);
            var soundData = TableManager.GetSoundTable().GetData(soundTableID);
            if (soundData == null)
            {
                Debug.LogError($"Not Found SoundTableData ID : {soundTableID}");
                return;
            }

            if (soundData.RandomAssetName.Length == 0)
            {
                SoundManager.Instance.Play(soundTableID, transform.position);
            }
            else
            {
                var randomSound = soundData.RandomAssetName[UnityEngine.Random.Range(0, soundData.RandomAssetName.Length)];
                SoundManager.Instance.Play(randomSound, transform.position);
            }
        }

        private void SoundStringEffect(string assetName)
        {
            //Debug.Log("Sound Effect assetName : " + assetName);
            SoundManager.Instance.Play(assetName, transform.position);
        }

        private void SkillEvent(int tableID)
        {
            SkillManager.Get().FireSkill(tableID, transform.position, transform.forward, CombatManager);
        }

        private void WeaponSkillEvent(int index)
        {
            var weaponId = EquipManager.Get().EquipedWeaponID;
            var skillID = TableManager.GetWeaponSkillID(weaponId, index);
            SkillManager.Get().FireSkill(skillID, transform.position, transform.forward, CombatManager);
        }

        private void RuneSkillEvent()
        {
            var runeID = TableManager.CurrentPlayerData.GetEquipRuneID();
            var runeData = TableManager.GetRuneTableData(runeID);
            SkillManager.Get().FireSkill(runeData.SkillID, transform.position, transform.forward, CombatManager);
        }

        private void FootStepEffect(AnimationEvent aniEvent)
        {
            if (aniEvent.animatorClipInfo.weight > 0.5f)
            {
                var tableID = SoundManager.Instance.ConvertHitMaterialToSoundTableID(TypeHitMaterial.STONE);
                if (CombatManager.ZoneInfo != null)
                    tableID = SoundManager.Instance.ConvertHitMaterialToSoundTableID(CombatManager.ZoneInfo.GroundMaterial);

                SoundManager.Instance.Play(tableID, transform.position);
            };
        }

        private void FootStepEffectToID(AnimationEvent aniEvent)
        {
            if (aniEvent.animatorClipInfo.weight > 0.5f)
            {
                SoundManager.Instance.Play(aniEvent.intParameter, transform.position);
            };
        }
    }
}