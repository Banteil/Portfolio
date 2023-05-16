using UnityEngine;

namespace Zeus
{
    [ClassHeader("Melee Weapon", OpenClose = false)]
    public class MeleeWeapon : AttackObject
    {
        [Header("Melee Weapon Settings")]
        public MeleeType Type = MeleeType.OnlyAttack;
        [Header("Attack Settings")]
        public bool UseStrongAttack = true;
        [Tooltip("Simple AI Only")]
        public float DistanceToAttack = 1;
        [Tooltip("Trigger a Attack Animation")]
        public int AttackID;
        [Tooltip("Change the MoveSet when using this Weapon")]
        public int MovesetID;
        [Header("* Third Person Controller Only *")]
        [Tooltip("How much stamina will be consumed when attack")]
        public float StaminaCost;
        [Tooltip("How much time the stamina will wait to start recover")]
        public float StaminaRecoveryDelay;
        [Header("Defense Settings")]
        [Range(0, 100)]
        public int DefenseRate = 100;
        [Range(0, 180)]
        public float DefenseRange = 90;
        [Tooltip("Trigger a Defense Animation")]
        public int DefenseID;
        [Tooltip("What recoil animatil will trigger")]
        public int RecoilID;
        [Tooltip("Can break the oponent attack, will trigger a recoil animation")]
        public bool BreakAttack;

        [HideInInspector]
        public UnityEngine.Events.UnityEvent OnDefenseEvent;

        public void OnDefense()
        {
            OnDefenseEvent.Invoke();
        }
    }

    public enum MeleeType
    {
        OnlyDefense, OnlyAttack, AttackAndDefense
    }
}