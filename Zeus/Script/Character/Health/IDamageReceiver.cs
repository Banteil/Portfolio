using UnityEngine;
namespace Zeus
{
    public enum TypeDamageAngle { FRONT = 0, BACK = 1, LEFT = 2, RIGHT = 3 }

    [System.Serializable]
    public class OnReceiveDamage : UnityEngine.Events.UnityEvent<Damage> { }
    public interface IDamageReceiver
    {
        OnReceiveDamage onStartReceiveDamage { get; }
        OnReceiveDamage onReceiveDamage { get; }
        Transform transform { get; }
        GameObject gameObject { get; }
        void TakeDamage(Damage damage);
    }

    public static class DamageHelper
    {
        public static void ApplyDamage(this GameObject receiver, Damage damage)
        {
            var receivers = receiver.GetComponents<IDamageReceiver>();
            if (receivers != null)
            {
                for (int i = 0; i < receivers.Length; i++)
                {
                    receivers[i].TakeDamage(damage);
                }
            }
        }

        public static bool CanReceiveDamage(this GameObject receiver)
        {
            return receiver.GetComponent<IDamageReceiver>() != null;
        }

        public static TypeDamageAngle HitAngle(this Transform transform, Vector3 hitpoint)
        {
            var localTarget = transform.InverseTransformPoint(hitpoint);
            var _angle = (int)(Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg);

            if (_angle <= 45 && _angle >= -45)
                return TypeDamageAngle.FRONT;
            else if (_angle > 45 && _angle < 135)
                return TypeDamageAngle.RIGHT;
            else if (_angle >= 135 || _angle <= -135)
                return TypeDamageAngle.BACK;
            else
                return TypeDamageAngle.LEFT;
        }
    }
}
