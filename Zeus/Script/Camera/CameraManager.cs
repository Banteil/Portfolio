using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public enum TypeCamera { DEFAULT, LOCKON, AIM, EXCUTION }
    public enum TypeCameraShake { Shake }
    public class CameraManager : BaseObject<CameraManager>
    {
        [SerializeField]
        private NoiseSettings ShakeNoiseSetting;

        [SerializeField]
        private List<VCam> _vCameras = new List<VCam>();

        private Coroutine _cameraShakeCoroutine;
        private VCam _prevActiveCam;

        private void Start()
        {
            foreach (var cam in _vCameras) 
            {
                cam.Initialized();
            }
        }

        public void RemoveCamera(TypeCamera cameraType)
        {
            var removeCamera = GetCamera(cameraType);
            _vCameras.Remove(removeCamera);
        }

        public void TurnOnCamera(TypeCamera cameraType)
        {
            if (cameraType == TypeCamera.DEFAULT)
            {
                if (_prevActiveCam != null && _prevActiveCam.CameraType == TypeCamera.LOCKON && _prevActiveCam.VCamera.enabled)
                {
                    var defaultCam = GetCamera(TypeCamera.DEFAULT);
                    defaultCam.VCamera.ForceCameraPosition(_prevActiveCam.VCamera.transform.position, _prevActiveCam.VCamera.transform.rotation);
                }
            }

            for (int i = 0; i < _vCameras.Count; i++)
            {
                _vCameras[i].VCamera.enabled = _vCameras[i].CameraType == cameraType;
                if (_vCameras[i].VCamera.enabled)
                    _prevActiveCam = _vCameras[i];
            }
        }

        internal VCam GetActiveCamera()
        {
            return _vCameras.Find(_ => _.VCamera.enabled);
        }

        public VCam GetCamera(TypeCamera cameraType)
        {
            return _vCameras.Find(_ => _.CameraType == cameraType);
        }

        public void CameraShake(float intensity, float during)
        {
            if (ShakeNoiseSetting == null)
            {
                Debug.LogError("ShakeNoiseSetting is Null");
                return;
            }

            var vcam = GetActiveCamera();
            if (vcam == null) 
            {
                Debug.LogError("CameraShake ReTURN Active Camera Not Found");
                return; 
            }

            if (vcam.Perlin == null)
            {
                Debug.LogError("CameraShake ReTURN vcam.Perlin is null");
                return;
            }

            if (_cameraShakeCoroutine != null)
            {
                StopCoroutine(_cameraShakeCoroutine);
            }
            _cameraShakeCoroutine = StartCoroutine(ShakeEndCo(vcam, during, intensity));
        }

        public IEnumerator ShakeEndCo(VCam vcam, float time, float intensity)
        {
            vcam.Perlin.m_NoiseProfile = ShakeNoiseSetting;
            vcam.Perlin.m_FrequencyGain = 3;

            if (intensity != 0f)
                vcam.Perlin.m_AmplitudeGain = intensity;

            var elapsedTime = 0f;
            while (elapsedTime < time)
            {
                elapsedTime += GameTimeManager.Instance.FixedDeltaTime;
                yield return new WaitForFixedUpdate();  
            }

            vcam.Perlin.m_NoiseProfile = vcam.OldNoiseSetting;
            vcam.Perlin.m_AmplitudeGain = vcam.AmplitudeGainOldValue;
            vcam.Perlin.m_FrequencyGain = vcam.FrequencyGainOldValue;

            _cameraShakeCoroutine = null;

            //Debug.Log("Shake Complete");
        }
    }
    [System.Serializable]
    public class VCam
    {
        public CinemachineVirtualCamera VCamera;
        public TypeCamera CameraType;
        internal CinemachineBasicMultiChannelPerlin Perlin;
        internal NoiseSettings OldNoiseSetting;
        internal float FrequencyGainOldValue;
        internal float AmplitudeGainOldValue;

        public VCam(CinemachineVirtualCamera VCamera, TypeCamera CameraType)
        {
            this.VCamera = VCamera;
            this.CameraType = CameraType;
            this.Perlin = VCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        internal void Initialized()
        {
            Perlin = VCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            if (Perlin != null)
            {
                OldNoiseSetting = Perlin.m_NoiseProfile;
                AmplitudeGainOldValue = Perlin.m_AmplitudeGain;
                FrequencyGainOldValue = Perlin.m_FrequencyGain;
            }
        }
    }
}
