using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

namespace starinc.io.gallaryx
{
    public class Exhibits : MonoBehaviour
    {
        public Define.FileType ExhibitsType { get; set; } = Define.FileType.EMPTY;
        [SerializeField] private Renderer _mainRenderer;
        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private float _sizeScale = 1f;

        private GalleryXMedia _exhibitionData;
        public GalleryXMedia ExhibitionData { get { return _exhibitionData; } }

        private bool _dataSettingComplete;
        public bool DataSettingComplete { get { return _dataSettingComplete; } }

        private void Awake()
        {
            _mainRenderer = GetComponent<Renderer>();
            _videoPlayer = GetComponent<VideoPlayer>();
        }

        public void Reset()
        {
            _mainRenderer.enabled = true;
            ExhibitsType = Define.FileType.EMPTY;
            _mainRenderer.material.mainTexture = null;
            _videoPlayer.enabled = true;
            _videoPlayer.url = "";
            _videoPlayer.Stop();
        }

        public void DisableRenderer() => _mainRenderer.enabled = false;

        public string GetVideoURL() => _videoPlayer.url;

        public Texture GetTexture()
        {
            switch (ExhibitsType)
            {
                case Define.FileType.IMAGE:
                    return _mainRenderer.material.mainTexture;
                case Define.FileType.VIDEO:
                    return _videoPlayer.texture;
                case Define.FileType.AUDIO:
                    return _mainRenderer.material.mainTexture;
                default:
                    return null;
            }
        }

        public void SetTexture(Texture texture)
        {
            _mainRenderer.material.mainTexture = texture;
            transform.localScale = Util.GetRatioProportionalToTextureSize(transform, _mainRenderer.material.mainTexture.width, _mainRenderer.material.mainTexture.height, _sizeScale);
        }

        public async UniTask SetDataAsync(GalleryXMedia exhibitionData)
        {           
            //var stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();
            try
            {
                _dataSettingComplete = false;
                ExhibitsType = Util.GetFileType(exhibitionData.type);
                if (ExhibitsType == Define.FileType.EMPTY)
                {
                    Debug.LogError($"{exhibitionData.title} 파일의 타입을 확인하지 못하였습니다.");
                    _dataSettingComplete = true;
                    return;
                }

                _exhibitionData = exhibitionData;
                switch (ExhibitsType)
                {
                    case Define.FileType.IMAGE:
                        {
                            var texture = await CallAPI.GetTextureData(exhibitionData.thumbnail_url);
                            if (texture == null) break;                            
                            _mainRenderer.material.mainTexture = texture;                            
                            _mainRenderer.material.mainTextureScale = new Vector2(1.06f, 1.06f);
                            _mainRenderer.material.mainTextureOffset = new Vector2(0, -0.06f);
                            transform.localScale = Util.GetRatioProportionalToTextureSize(transform, _mainRenderer.material.mainTexture.width, _mainRenderer.material.mainTexture.height, _sizeScale);
                            _videoPlayer.enabled = false;

                        }
                        break;
                    case Define.FileType.VIDEO:
                        _videoPlayer.url = exhibitionData.url;
                        await VideoPreparedProcess();
                        break;
                }
                _dataSettingComplete = true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                _videoPlayer.enabled = false;
                DisableRenderer();
                ExhibitsType = Define.FileType.EMPTY;
            }
            //stopwatch.Stop();
            //Debug.Log($"{exhibitionData.thumbnail_url} 함수 실행 시간: {stopwatch.ElapsedMilliseconds}ms");
        }

        private async UniTask VideoPreparedProcess()
        {
            try
            {
                _videoPlayer.Play();
                await UniTask.WaitForSeconds(1f);
                if (_videoPlayer.isPlaying)
                {
                    _videoPlayer.time = 0f;
                    _videoPlayer.Pause();
                    transform.localScale = Util.GetRatioProportionalToTextureSize(transform, _videoPlayer.texture.width, _videoPlayer.texture.height);
                }
                else
                {
                    _videoPlayer.enabled = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Video Prepare Fail.. : {ex}");
                _dataSettingComplete = true;
            }
        }

        public void SetInfoVideo()
        {
            if (ExhibitsType != Define.FileType.VIDEO) return;
            _videoPlayer.time = 0;
            _videoPlayer.Play();
            _videoPlayer.SetDirectAudioMute(0, false);
        }

        public void Call_MuteVideo()
        {
            if (ExhibitsType != Define.FileType.VIDEO) return;
            _videoPlayer.time = 1;
            _videoPlayer.Pause();
            _videoPlayer.SetDirectAudioMute(0, true);
        }

        private void OnMouseEnter()
        {
            if (Util.IsPointerOverUI() || ExhibitsType != Define.FileType.VIDEO) return;
            _videoPlayer.Play();
        }

        private void OnMouseOver()
        {
            if (Util.IsPointerOverUI()) return;
            var activeUI = _mainRenderer.enabled && InputManager.Instance.KeyInputEvent.InputMouseLeftKey() == InputState.None && !UIManager.Instance.InteractUI;
            UIManager.Instance.ActiveZoomInUI(activeUI, transform);
        }

        private void OnMouseExit()
        {
            UIManager.Instance.ActiveZoomInUI(false);
            if (Util.IsPointerOverUI() || ExhibitsType != Define.FileType.VIDEO) return;
            _videoPlayer.time = 1f;
            _videoPlayer.Pause();
        }
    }
}