//using System;
//using System.Collections.Generic;
//using Cysharp.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.Video;

//namespace starinc.io.instarverse
//{
//    public class ExhibitsManager : Singleton<ExhibitsManager>
//    {
//        [SerializeField]
//        private List<ExhibitionSeqData> _exhibitionSeqDataList;

//        private ExhibitionHall _hall;

//        private VideoPlayer _videoPlayer;

//        protected override void OnAwake()
//        {
//            _videoPlayer = transform.GetChild(0).GetComponent<VideoPlayer>();
//        }

//        public void SetHall(ExhibitionHall hall) => _hall = hall;

//        public void SetInfoVideo(Exhibits exhibits)
//        {
//            if (exhibits.ExhibitsType != Define.FileType.VIDEO) return;
//            _videoPlayer.time = 0;
//            _videoPlayer.Play();
//            _videoPlayer.SetDirectAudioMute(0, false);
//        }

//        public void Call_MuteVideo(Exhibits exhibits)
//        {
//            if (exhibits.ExhibitsType != Define.FileType.VIDEO) return;
//            _videoPlayer.time = 1;
//            _videoPlayer.Pause();
//            _videoPlayer.SetDirectAudioMute(0, true);
//        }

//        public async UniTask SetExhibitsData(List<ExhibitionSeqData> exhibitionSeqDatas)
//        {
//            _exhibitionSeqDataList = exhibitionSeqDatas;
//            for (int i = 0; i < _hall.ExhibitsList.Count; i++)
//            {
//                _hall.ExhibitsList[i].Reset();
//                if (i >= _exhibitionSeqDataList.Count)
//                {
//                    _hall.ExhibitsList[i].DisableRenderer();
//                    continue;
//                }
//                await _hall.ExhibitsList[i].SetDataAsync(_exhibitionSeqDataList[i]);
//            }
//            //await UniTask.WaitUntil(() => !_exhibits.TrueForAll(exhibit => exhibit.DataSettingComplete));
//            await Resources.UnloadUnusedAssets();
//        }

//        public void ShowExhibitsInfoPopup(Exhibits exhibits)
//        {
//            var texture = GetExhibitsTexture(exhibits);
//            if (texture == null) return;

//            MainSceneManager.Instance.Player.IsInteraction = true;
//            SetInfoVideo(exhibits);
//            var popupUI = UIManager.Instance.ShowPopupUI<UIExhibitsInfoPopup>("ExhibitsInfoUI", () => Call_MuteVideo(exhibits));
//            popupUI.SetInfo(texture, exhibits.Title, exhibits.Description);
//        }

//        private Texture GetExhibitsTexture(Exhibits exhibits)
//        {
//            switch (exhibits.ExhibitsType)
//            {
//                case Define.FileType.IMAGE:
//                    return exhibits.MainRenderer.material.mainTexture;
//                case Define.FileType.VIDEO:
//                    return _videoPlayer.texture;
//                default:
//                    return null;
//            }
//        }

//        private async UniTask VideoPreparedProcess()
//        {
//            try
//            {
//                _videoPlayer.Prepare();
//                await UniTask.WaitUntil(() => _videoPlayer.isPrepared);

//                _videoPlayer.Play();
//                await UniTask.WaitUntil(() => _videoPlayer.time > 0f);
//                _videoPlayer.time = 0f;
//                _videoPlayer.Pause();
//                transform.localScale = Util.GetRatioProportionalToTextureSize(transform, _videoPlayer.texture.width, _videoPlayer.texture.height);
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError($"Video Prepare Error! : {ex}");
//            }
//        }
//    }
//}
