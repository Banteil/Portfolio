using UnityEngine;

namespace starinc.io.gallaryx
{
    public class PlayerBrain : BaseBrain
    {
        #region Cache
        private CameraController _cameraController;

        public override Vector3 GetMoveDirection
        {
            get
            {
                Vector3 keyDir = new Vector3(BrainInputEvent.InputHorizontal(), 0, BrainInputEvent.InputVertical()).normalized;
                Quaternion keyRot = Quaternion.identity;
                keyRot.eulerAngles = new Vector3(0, Mathf.Atan2(keyDir.x, keyDir.z) * Mathf.Rad2Deg, 0);

                var cameraDir = Util.GetDirectionWithoutYAxis(_character.transform.position, Camera.main.transform.position).normalized;
                var moveDir = keyRot * cameraDir;
                return moveDir;
            }
        }

        public override Vector3 TargetRotationValue
        {
            get
            {
                if (_cameraController.Perspective == CameraPerspective.FirstPerson)
                {
                    var cameraDir = Util.GetDirectionWithoutYAxis(_character.transform.position, Camera.main.transform.position).normalized;
                    return cameraDir;
                }
                else
                    return _character.transform.forward;
            }
            set
            {
                _character.transform.forward = value;
            }
        }
        #endregion

        public override void OnEnable(CharacterController characterController)
        {
            base.OnEnable(characterController);
            _cameraController = GameObject.FindFirstObjectByType<CameraController>();
        }


        protected override void SetInputEvent() => BrainInputEvent = InputManager.Instance.KeyInputEvent;
    }
}
