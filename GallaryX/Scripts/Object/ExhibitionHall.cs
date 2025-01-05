using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public class ExhibitionHall : MonoBehaviour
    {
        private GalleryXExhibitionData _exhibitionData;
        public GalleryXExhibitionData ExhibitionData { get { return _exhibitionData; } }
        private List<GalleryXMedia> _exhibitionSeqDataList;
        private List<Exhibits> _exhibits;

        [SerializeField]
        private Transform _startPositionTr;

        [SerializeField]
        private TextMeshProUGUI _titleText, _descriptionText;
        [SerializeField]
        private RawImage _posterImage;

        public List<GalleryXMedia> SeqDataList { get { return _exhibitionSeqDataList; } }
        public List<Exhibits> ExhibitsList { get { return _exhibits; } }

        private List<Vector3> _indexPositionList = new List<Vector3>();
        private List<Quaternion> _indexRotationList = new List<Quaternion>();

        private void Awake()
        {
            if (_startPositionTr == null)
                _startPositionTr = transform.Find("StartPosition");

            if (_exhibits == null)
            {
                _exhibits = transform.GetComponentsInChildren<Exhibits>().ToList();
                foreach (var exhibit in _exhibits)
                {
                    _indexPositionList.Add(exhibit.transform.position);
                    _indexRotationList.Add(exhibit.transform.rotation);
                }
            }

            _titleText.font = Resources.Load<TMP_FontAsset>("TMPFont/BaseFont");
            _descriptionText.font = Resources.Load<TMP_FontAsset>("TMPFont/BaseFont");
        }

        public Vector3 GetStartPos() => _startPositionTr.position;
        public Quaternion GetStartRot() => _startPositionTr.rotation;

        public async UniTask SetExhibitsData(List<GalleryXMedia> exhibitionSeqDatas)
        {            
            if (_exhibits.Count == 0) return;
            _exhibitionSeqDataList = exhibitionSeqDatas != null ? exhibitionSeqDatas : new List<GalleryXMedia>();
            Debug.Log($"SetExhibitsData : {_exhibitionSeqDataList.Count}");
            if (_exhibitionSeqDataList.Count < _exhibits.Count)
            {
                
                var actualData = _exhibitionSeqDataList
                    .Where(data => !data.type.Equals(Define.FileType.EMPTY.ToString(), StringComparison.OrdinalIgnoreCase))
                    .OrderBy(data => data.order_no)
                    .ToList();
                var sortedList = new List<GalleryXMedia>(_exhibits.Count);

                var emptyName = Define.FileType.EMPTY.ToString();
                for (int i = 0; i < _exhibits.Count; i++)
                {
                    sortedList.Add(new GalleryXMedia
                    {                        
                        type = Define.FileType.EMPTY.ToString().ToLower(),
                        order_no = i + 1,
                    });
                }

                for (int i = 0; i < actualData.Count; i++)
                {
                    int index = actualData[i].order_no;
                    if (index >= _exhibits.Count) break;
                    else if (index < 0 || sortedList[index] != null)
                        index = i;
                    sortedList[index] = actualData[i];
                }
                _exhibitionSeqDataList = sortedList;
            }

            for (int i = 0; i < _exhibitionSeqDataList.Count; i++)
            {
                if (i >= _exhibits.Count) break;
                _exhibits[i].Reset();
                if (_exhibitionSeqDataList[i].type.Equals(Define.FileType.EMPTY.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    _exhibits[i].DisableRenderer();
                    continue;
                }
                await _exhibits[i].SetDataAsync(_exhibitionSeqDataList[i]);
            }
            //await UniTask.WaitUntil(() => !_exhibits.TrueForAll(exhibit => exhibit.DataSettingComplete));
            await Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        public async void SetExhibitionData(GalleryXExhibitionData data)
        {
            Debug.Log("SetExhibitionData");
            if (data == null) return;
            _exhibitionData = data;
            if (_titleText != null)
                _titleText.text = data.title;
            if (_descriptionText != null)
            {
                var description = Util.FilterUnsupportedTags(data.description);
                _descriptionText.text = description;
            }
            if (_posterImage != null && !string.IsNullOrEmpty(data.postar_url))
            {
                try
                {
                    _posterImage.texture = await CallAPI.GetTextureData(data.postar_url);
                    var rectImageUI = _posterImage.GetComponent<RectTransform>();
                    var width = _posterImage.texture.width / (float)_posterImage.texture.height * 2;
                    var height = _posterImage.texture.height / (float)_posterImage.texture.width * 2;

                    if (height > width)
                        rectImageUI.sizeDelta = new Vector2(width, 2);
                    else
                        rectImageUI.sizeDelta = new Vector2(2, height);
                    
                    _posterImage.enabled = true;
                }
                catch(Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public void SwapSeqDataInfo(int oldIndex, int newIndex)
        {
            var seqItem = _exhibitionSeqDataList[oldIndex];
            _exhibitionSeqDataList.RemoveAt(oldIndex);
            _exhibitionSeqDataList.Insert(newIndex, seqItem);
            for (int i = 0; i < _exhibitionSeqDataList.Count; i++)
            {
                _exhibitionSeqDataList[i].order_no = i + 1;
            }


        }

        public void SwapExhibitIndices(int oldIndex, int newIndex)
        {
            var exhibitsItem = _exhibits[oldIndex];
            _exhibits.RemoveAt(oldIndex);
            _exhibits.Insert(newIndex, exhibitsItem);

            for (int i = 0; i < _exhibits.Count; i++)
            {
                _exhibits[i].transform.position = _indexPositionList[i];
                _exhibits[i].transform.rotation = _indexRotationList[i];
            }
        }
    }
}
