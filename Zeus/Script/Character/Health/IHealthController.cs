using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    [System.Serializable]
    public class OnDead : UnityEngine.Events.UnityEvent<GameObject> { }
    public interface IHealthController : IDamageReceiver
    {
        OnDead OnDeadEvent { get; }
        float CurrentHealth { get; }
        int MaxHealth { get; }
        bool IsDead { get; set; }
        void AddHealth(int value);
        void ChangeHealth(int value);
        void ChangeMaxHealth(int value);
        void ResetHealth(float health);
        void ResetHealth();
    }

    public static class zHealthControllerHelper
    {
        static IHealthController GetHealthController(this GameObject gameObject)
        {
            return gameObject.GetComponent<IHealthController>();
        }

        public static void AddHealth(this GameObject receiver, int health)
        {
            var healthController = receiver.GetHealthController();
            if (healthController != null)
            {
                healthController.AddHealth(health);
            }
        }

        public static void ChangeHealth(this GameObject receiver, int health)
        {
            var healthController = receiver.GetHealthController();
            if (healthController != null)
            {
                healthController.ChangeHealth(health);
            }
        }

        public static void ChangeMaxHealth(this GameObject receiver, int health)
        {
            var healthController = receiver.GetHealthController();
            if (healthController != null)
            {
                healthController.ChangeMaxHealth(health);
            }
        }

        public static bool HasHealth(this GameObject gameObject)
        {
            return gameObject.GetHealthController() != null;
        }

        public static bool IsDead(this GameObject gameObject)
        {
            var health = gameObject.GetHealthController();
            return health == null || health.IsDead;
        }

        public static void ResetHealth(this GameObject receiver, float health)
        {
            var healthController = receiver.GetHealthController();
            if (healthController != null)
            {
                healthController.ResetHealth(health);
            }
        }

        public static void ResetHealth(this GameObject receiver)
        {
            var healthController = receiver.GetHealthController();
            if (healthController != null)
            {
                healthController.ResetHealth();
            }
        }
    }
}

