using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "AccelerometerSensor", menuName = "Sensors/Accelerometer")]
    public class AccelerometerWrapperScriptable : SensorWrapperScriptable
    {
        public override string Name => "Accelerometer";
        public override bool IsAvailable => Accelerometer.current != null;
        public override bool IsEnabled => Accelerometer.current?.enabled ?? false;

        public override async UniTask InitializeAsync(float timeout = 5f)
        {
            var timer = 0f;
            while (Accelerometer.current == null)
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
            if (Accelerometer.current != null)
            {
                if (enable)
                    InputSystem.EnableDevice(Accelerometer.current);
                else
                    InputSystem.DisableDevice(Accelerometer.current);
            }
        }

        public Vector3 ReadValue()
        {
            if (Accelerometer.current == null)
            {
                Debug.LogWarning($"{Name} is not initialized.");
                return default;
            }
            return Accelerometer.current.acceleration.ReadValue();
        }
    }
}