using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "AttitudeSensor", menuName = "Sensors/Attitude")]
    public class AttitudeSensorWrapperScriptable : SensorWrapperScriptable
    {
        public override string Name => "AttitudeSensor";
        public override bool IsAvailable => AttitudeSensor.current != null;
        public override bool IsEnabled => AttitudeSensor.current?.enabled ?? false;

        public override async UniTask InitializeAsync(float timeout = 5f)
        {
            var timer = 0f;
            while (AttitudeSensor.current == null)
            {
                timer += Time.deltaTime;
                if (timer > timeout)
                {
                    Debug.LogWarning($"{Name} not available after timeout.");
                    return;
                }
                await UniTask.Yield();
            }
            Debug.Log($"{Name} initialized successfully!");
        }

        public override void Enable(bool enable)
        {
            if (AttitudeSensor.current != null)
            {
                if (enable)
                    InputSystem.EnableDevice(AttitudeSensor.current);
                else
                    InputSystem.DisableDevice(AttitudeSensor.current);
            }
        }

        public Quaternion ReadValue()
        {
            if (AttitudeSensor.current == null)
            {
                Debug.LogWarning($"{Name} is not initialized.");
                return default;
            }
            return AttitudeSensor.current.attitude.ReadValue();
        }
    }
}