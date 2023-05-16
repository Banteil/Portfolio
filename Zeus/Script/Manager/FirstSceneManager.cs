using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

namespace Zeus
{
    public class FirstSceneManager : BaseObject<FirstSceneManager>
    {
        public VisualEffect FX;

        private void Awake()
        {
            FX.Stop();
        }

        // Start is called before the first frame update
        IEnumerator Start()
        {
            var input = FindObjectOfType<ThirdPersonInput>();
            input.enabled = false;

            yield return new WaitForSeconds(5f);

            input.enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            FX.Play();

            var postProcessingVolume = GetComponent<Volume>(); ;
            postProcessingVolume.profile.TryGet(out VolumeGrayScale volumeGrayScale);
            if (volumeGrayScale != null)
            {
                if (_grayVolumeUpdate != null)
                    StopCoroutine(_grayVolumeUpdate);

                _grayVolumeUpdate = GrayVolumeValueUpdate(volumeGrayScale, true);
                StartCoroutine(_grayVolumeUpdate);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            FX.Stop();

            var postProcessingVolume = GetComponent<Volume>(); ;
            postProcessingVolume.profile.TryGet(out VolumeGrayScale volumeGrayScale);
            if (volumeGrayScale != null)
            {
                if (_grayVolumeUpdate != null)
                    StopCoroutine(_grayVolumeUpdate);

                _grayVolumeUpdate = GrayVolumeValueUpdate(volumeGrayScale, false);
                StartCoroutine(_grayVolumeUpdate);
            }
        }

        public void OnClickPortal()
        {
            SceneLoadManager.Instance.LoadScene("BossMap");
        }

        private IEnumerator _grayVolumeUpdate;
        IEnumerator GrayVolumeValueUpdate(VolumeGrayScale volumeGrayScale, bool add)
        {
            var startTime = Time.realtimeSinceStartup;
            var offset = 0.5f;

            while (true)
            {
                yield return new WaitForEndOfFrame();
                var elapsedTime = Time.realtimeSinceStartup - startTime;
                startTime = Time.realtimeSinceStartup;
                var value = volumeGrayScale.intensity.value;
                if (add)
                {
                    value += offset * elapsedTime;
                }
                else
                {
                    value -= offset * elapsedTime;
                }
                volumeGrayScale.intensity.value = Mathf.Clamp(value, 0f, 1f);

                if (volumeGrayScale.intensity.value == 1f || volumeGrayScale.intensity.value == 0f)
                    break;
            }

            _grayVolumeUpdate = null;
        }
    }
}
