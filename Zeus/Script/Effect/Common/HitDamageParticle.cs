using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    [ClassHeader("HITDAMAGE PARTICLE", "Default hit Particle to instantiate every time you receive damage and Custom hit Particle to instantiate based on a custom DamageType that comes from the MeleeControl Behaviour (AnimatorController)")]
    public class HitDamageParticle : zMonoBehaviour
    {
        public List<GameObject> DefaultDamageEffects = new List<GameObject>();
        public List<DamageEffect> CustomDamageEffects = new List<DamageEffect>();

        private FisherYatesRandom _random;

        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            var healthController = GetComponent<HealthController>();
            if (healthController != null)
            {
                healthController.onReceiveDamage.AddListener(OnReceiveDamage);
            }
        }

        public void OnReceiveDamage(Damage damage)
        {
            // instantiate the hitDamage particle - check if your character has a HitDamageParticle component
            var damageDirection = damage.HitPosition - new Vector3(transform.position.x, damage.HitPosition.y, transform.position.z);
            var hitrotation = damageDirection != Vector3.zero ? Quaternion.LookRotation(damageDirection) : transform.rotation;

            if (damage.DamageValue > 0)
            {
                TriggerEffect(new DamageEffectInfo(damage.HitPosition, hitrotation, damage.DamageType, damage.Receiver));
            }
        }

        /// <summary>
        /// Raises the hit event.
        /// </summary>
        /// <param name="damageEffectInfo">Hit effect info.</param>
        void TriggerEffect(DamageEffectInfo damageEffectInfo)
        {            
            if (_random == null)
            {
                _random = new FisherYatesRandom();
            }
            var damageEffect = CustomDamageEffects.Find(effect => effect.DamageType.Equals(damageEffectInfo.DamageType));

            if (damageEffect != null)
            {                
                damageEffect.OnTriggerEffect.Invoke();
                if (damageEffect.CustomDamageEffect != null && damageEffect.CustomDamageEffect.Count > 0)
                {
                    var randomCustomEffect = damageEffect.CustomDamageEffect[_random.Next(damageEffect.CustomDamageEffect.Count)];

                    Instantiate(randomCustomEffect, damageEffectInfo.Position,
                        damageEffect.RotateToHitDirection ? damageEffectInfo.Rotation : randomCustomEffect.transform.rotation,
                        damageEffect.AttachInReceiver && damageEffectInfo.Receiver ? damageEffectInfo.Receiver : ObjectContainer.Root);
                }
            }
            else if (DefaultDamageEffects.Count > 0 && damageEffectInfo != null)
            {                
                var randomDefaultEffect = DefaultDamageEffects[_random.Next(DefaultDamageEffects.Count)];
                Instantiate(randomDefaultEffect, damageEffectInfo.Position, damageEffectInfo.Rotation, ObjectContainer.Root);
            }
        }

        private void Reset()
        {
            DefaultDamageEffects = new List<GameObject>();
            var defaultEffect = Resources.Load("defaultDamageEffect");

            if (defaultEffect != null)
            {
                DefaultDamageEffects.Add(defaultEffect as GameObject);
            }
        }
    }



    public class DamageEffectInfo
    {
        public Transform Receiver;
        public Vector3 Position;
        public Quaternion Rotation;
        public string DamageType;

        public DamageEffectInfo(Vector3 position, Quaternion rotation, string damageType = "", Transform receiver = null)
        {
            this.Receiver = receiver;
            this.Position = position;
            this.Rotation = rotation;
            this.DamageType = damageType;
        }
    }

    [System.Serializable]
    public class DamageEffect
    {
        public string DamageType = "";
        public List<GameObject> CustomDamageEffect;
        public bool RotateToHitDirection = true;
        [Tooltip("Attach prefab in Damage Receiver transform")]
        public bool AttachInReceiver = false;
        public UnityEvent OnTriggerEffect;
    }
}
