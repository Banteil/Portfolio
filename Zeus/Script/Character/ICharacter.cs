using UnityEngine;

namespace Zeus
{
    public class OnActiveRagdoll : UnityEngine.Events.UnityEvent<Damage> { }
    public interface ICharacter : IHealthController, IDamageReceiver
    {
        string GUID { get; }
        OnActiveRagdoll OnActiveRagdollEvent { get; }
        Animator Animator { get; }
        AnimationStateTable AnimationStateTable { get; }
        void OnRecoil(int recoilID);
        bool IsBlocking { get; }
        bool GhostMode { set; }

        Transform CharacterTransform { get; }
        GameObject CharacterGameObject { get; }
        T GetAbility<T>() where T : CharacterAbility;
        CharacterAbility GetAbility(string abilityName);
    }
}
