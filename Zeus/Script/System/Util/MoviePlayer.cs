using Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Zeus
{
    public class MoviePlayer : MonoBehaviour
    {
        public UnityEvent StartEvent, EndEvent;
        public string FileName;
        public bool PlayOnAwake;
        public bool SkipAble;
        public RenderTexture MovieRenderTexture;
        public RawImage MovieRenderer;
        private VideoPlayer _moviePlayer;

        void Start()
        {
            _moviePlayer = gameObject.AddComponent<VideoPlayer>();
            _moviePlayer.playOnAwake = false;
            _moviePlayer.renderMode = VideoRenderMode.RenderTexture;
            _moviePlayer.targetTexture = MovieRenderTexture;
            _moviePlayer.targetCameraAlpha = 1F;
            _moviePlayer.url = Zeus.Define.GetStreamingAssetsPath() + FileName;
            _moviePlayer.isLooping = false;
            _moviePlayer.loopPointReached += EndReached;
            _moviePlayer.started += StartProcess;

            if (PlayOnAwake)
            {
                Play();
            }
        }

        private void OnSkip()
        {
            EndReached(_moviePlayer);
        }

        void EndReached(VideoPlayer vp)
        {
            Debug.Log("~~~ EndReached");

            vp.Stop();
            FadeManager.Instance.DoFade(true, 1f, 0, () => 
            {
                Debug.Log("~~~ EndReached 2");
                EndEvent?.Invoke();
            });
            if (SkipAble && InputReader.HasInstance)
            {
                InputReader.Instance.CallCancel -= OnSkip;
            }
        }

        internal void Play()
        {
            if (SkipAble)
                InputReader.Instance.CallCancel += OnSkip;

            if (_moviePlayer != null)
            {                
                MovieRenderTexture.Release();
                MovieRenderer.enabled = true;
                _moviePlayer.Play();                              
            }
        }

        void StartProcess(UnityEngine.Video.VideoPlayer vp) => StartEvent?.Invoke();
    }
}
