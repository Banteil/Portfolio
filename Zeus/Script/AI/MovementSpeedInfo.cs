using UnityEngine;

namespace Zeus
{
    [System.Serializable]
    public class MovementSpeedInfo
    {
        [Tooltip("Rotation speed of the character")]
        public float RotationSpeed = 10f;

        [Tooltip("Speed to Walk using rigibody force or extra speed if you're using RootMotion")]
        public float WalkSpeed = 2f;

        [Tooltip("Speed to Run using rigibody force or extra speed if you're using RootMotion")]
        public float RunningSpeed = 3f;

        [Tooltip("Speed to Sprint using rigibody force or extra speed if you're using RootMotion")]
        public float SprintSpeed = 4f;
    }
}
