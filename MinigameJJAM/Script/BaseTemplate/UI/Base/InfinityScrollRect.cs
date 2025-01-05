using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io
{
    // 무한 스크롤 뷰를 구현한 클래스, ScrollRect를 상속받아 확장
    public class InfinityScrollRect : ScrollRect
    {
        #region Cache
        // 부모 UI 및 리스트 UI를 캐싱하기 위한 필드
        [SerializeField]
        private BaseUI _parentUI;
        [SerializeField]
        private ListUI _baseListUI; // 리스트 아이템 프리팹
        private IListUI _parentInterface; // 부모 UI에서 IListUI 인터페이스로 접근

        // 최대 풀링 사이즈, 그리드 크기, 패딩 및 최대 카운트를 설정하는 필드
        public int MaxPoolingSize = 10; // 최대 생성할 풀링된 아이템 수
        public int GridSize = 1; // 그리드 형태의 열 개수
        public int TopPadding = 0;
        public int BottomPadding = 0;
        public int HorizontalSpacing = 0; // 그리드의 수평 패딩
        public int VerticalSpacing = 0; // 그리드의 수직 패딩
        public int MaxCount = 1; // 아이템의 총 개수

        // 아이템 풀링 및 관리에 필요한 계산 필드
        protected int LimitCount // 최대 그리드 수를 계산하는 속성
        {
            get
            {
                var maxCount = MaxCount - _poolingObjectList.Count; // 전체 개수에서 현재 풀링된 리스트 수를 뺀 값
                var limitCount = maxCount / GridSize; // 그리드 사이즈에 따른 제한된 카운트 계산
                if (maxCount != 0 && MaxCount % GridSize != 0)
                    limitCount++;
                return limitCount;
            }
        }

        protected int _currentIndex = 0; // 현재 스크롤 위치에 따른 아이템 인덱스
        protected float _elementWidth = -1; // 리스트 요소의 너비
        protected float _elementHeight = -1; // 리스트 요소의 높이
        protected bool _completeCreatePool = false; // 풀링이 완료되었는지 여부

        // 풀링된 ListUI 객체들을 저장하는 리스트
        protected List<ListUI> _poolingObjectList = new List<ListUI>();
        public List<ListUI> PoolingObjectList { get { return _poolingObjectList; } }
        #endregion

        #region Callback
        public event Action OnCreatePoolingList;
        #endregion

        // 초기화 및 설정
        protected override void Awake()
        {
            // 부모 UI 및 인터페이스 찾기
            if (_parentUI == null)
                _parentUI = Util.FindComponentInParents<BaseUI>(transform);
            _parentInterface = _parentUI.gameObject.GetComponent<IListUI>();

            // 스크롤 변경 시 동작할 이벤트 추가
            onValueChanged.AddListener(CheckScroll);
        }

        // 아이템 풀링 및 생성
        public void CreatePoolingList<T>() where T : ListUI
        {
            // 이미 풀링이 완료된 경우 리턴
            if (_completeCreatePool) return;

            // 최대 풀 사이즈 또는 전체 카운트 중 작은 값을 선택
            var maxSize = MaxPoolingSize > MaxCount ? MaxCount : MaxPoolingSize;

            // 리스트 아이템을 풀링하여 생성
            for (int i = 0; i < maxSize; i++)
            {
                var listUI = AddListUI<T>(content, i);
                if (listUI != null)
                    _poolingObjectList.Add(listUI); // 생성된 아이템을 풀에 추가
                SetListData(i, listUI); // 아이템의 데이터를 설정                
            }

            if (LimitCount == 0)
                content.sizeDelta += new Vector2(0f, BottomPadding);
            OnCreatePoolingList?.Invoke();
            _completeCreatePool = true; // 풀링 완료 플래그 설정
        }

        // 그리드 레이아웃에서 각 요소의 수평 위치를 계산하는 함수
        protected float GridWidth(int index, float widthSize)
        {
            var gridIndex = (index % GridSize) + 1; // 그리드 인덱스 계산
            var spacing = HorizontalSpacing;

            // 수평 딩을 그리드 인덱스에 맞춰 조정
            if (spacing != 0)
            {
                if (GridSize % 2 == 0)
                    spacing *= gridIndex <= (GridSize / 2) ? -1 : 1;
                else
                {
                    var middle = (GridSize / 2) + 1;
                    if (gridIndex < middle) spacing *= -1;
                    else if (gridIndex > middle) spacing *= 1;
                    else spacing = 0;
                }
            }

            // 콘텐츠 크기를 기준으로 아이템 위치를 계산
            var percent = 1f / (GridSize + 1);
            var contentWidth = content.rect.width * (percent * gridIndex) - (widthSize * 0.5f) + spacing;
            return contentWidth;
        }

        // 그리드 레이아웃에서 각 요소의 수직 위치를 계산하는 함수
        protected float GridHeight(int index)
        {
            var gridIndex = index % GridSize; // 그리드 인덱스 계산
            var result = 0f;

            if (GridSize > 1)
            {
                var contentHeight = content.rect.height / GridSize; // 그리드에 따른 콘텐츠 높이 계산
                result = contentHeight * gridIndex;
            }
            return result;
        }

        // 리스트 데이터를 업데이트하는 함수
        protected void SetListData(int index, ListUI listUI)
        {
            // 활성화할 범위 안에 있으면 활성화
            var active = index >= 0 && index < MaxCount;
            if (active)
                listUI.gameObject.SetActive(true);
            else
            {
                listUI.gameObject.SetActive(false);
                return;
            }

            listUI.SetIndex(index); // 리스트 아이템의 인덱스를 설정
            _parentInterface.SetListData(listUI); // 부모 인터페이스를 통해 데이터 설정
        }

        // 풀링된 리스트 데이터를 초기화
        public void ResetListData()
        {
            for (int i = 0; i < _poolingObjectList.Count; i++)
            {
                _parentInterface.SetListData(_poolingObjectList[i]);
            }
        }

        // 풀링된 리스트 아이템을 파괴하고 초기화
        public void DestroyAllListData()
        {
            for (int i = 0; i < _poolingObjectList.Count; i++)
            {
                Destroy(_poolingObjectList[i].gameObject);
            }
            _poolingObjectList.Clear();
        }

        // 스크롤 위치 변경 시 호출되는 함수
        private void CheckScroll(Vector2 vec)
        {
            if (vertical) CheckVertical(); // 수직 스크롤일 경우 수직 확인
        }

        // 수직 스크롤 위치에 따라 리스트 아이템을 재배치
        private void CheckVertical()
        {
            // 높이나 풀링된 아이템이 없는 경우 리턴
            if (_elementHeight <= 0 || _poolingObjectList.Count <= 0) return;

            var scrollY = content.localPosition.y; // 스크롤 위치 계산
            var newIndex = Mathf.FloorToInt((scrollY - TopPadding) / (_elementHeight + VerticalSpacing)); // 현재 스크롤 위치에 따른 인덱스 계산
            if (newIndex < 0) newIndex = 0;

            // 스크롤이 실제로 변경되었을 때만 처리
            if (newIndex != _currentIndex)
            {
                if (newIndex > _currentIndex)
                    ScrollDown(newIndex);
                else
                    ScrollUp(newIndex);
            }
        }

        // 스크롤 다운 시 처리
        private void ScrollDown(int newIndex)
        {
            int prevIndex = _currentIndex + 1;
            if (prevIndex > LimitCount) return;

            _currentIndex = Mathf.Min(newIndex, LimitCount);

            for (int i = prevIndex; i <= _currentIndex; i++)
            {
                var gridIndex = (i - 1) * GridSize;

                for (int j = 0; j < GridSize; j++)
                {
                    var maxIndex = (gridIndex + j) + _poolingObjectList.Count;
                    var list = _poolingObjectList[0];
                    //list.Reset();
                    SetListData(maxIndex, list);
                    list.transform.SetAsLastSibling();

                    _poolingObjectList = ReorderList(_poolingObjectList, true);

                    var rectTr = (RectTransform)list.transform;
                    var gridMaxIndex = maxIndex / GridSize;
                    rectTr.anchoredPosition = new Vector2(GridWidth(j, rectTr.sizeDelta.x),
                        -(gridMaxIndex * rectTr.sizeDelta.y) - (VerticalSpacing * gridMaxIndex) - TopPadding);
                }
                content.sizeDelta += new Vector2(0f, _elementHeight + VerticalSpacing);
            }

            if (_currentIndex == LimitCount)
                content.sizeDelta += new Vector2(0f, BottomPadding);
        }

        // 스크롤 업 시 처리
        private void ScrollUp(int newIndex)
        {
            int prevIndex = _currentIndex - 1;
            if (prevIndex < LimitCount && _currentIndex == LimitCount)
                content.sizeDelta -= new Vector2(0f, BottomPadding);
            if (prevIndex < 0) return;

            _currentIndex = Mathf.Max(newIndex, 0);

            for (int i = prevIndex; i >= _currentIndex; i--)
            {
                var gridIndex = i * GridSize;

                for (int j = GridSize - 1; j >= 0; j--)
                {
                    var list = _poolingObjectList[_poolingObjectList.Count - 1];
                    //list.Reset();

                    SetListData(gridIndex + j, list);
                    list.transform.SetAsFirstSibling();

                    _poolingObjectList = ReorderList(_poolingObjectList, false);

                    var rectTr = (RectTransform)list.transform;
                    rectTr.anchoredPosition = new Vector2(GridWidth(j, rectTr.sizeDelta.x),
                        -(i * rectTr.sizeDelta.y) - (VerticalSpacing * i) - TopPadding);
                }
                content.sizeDelta -= new Vector2(0f, _elementHeight + VerticalSpacing);
            }
        }

        // 리스트의 첫 번째 요소를 마지막으로 보내거나, 마지막 요소를 첫 번째로 이동
        protected List<T> ReorderList<T>(List<T> list, bool isFirstToLast)
        {
            var reorderList = new List<T>(list);
            if (isFirstToLast)
            {
                var firstElement = reorderList[0];
                reorderList.RemoveAt(0);
                reorderList.Add(firstElement);
            }
            else
            {
                var lastElement = reorderList[reorderList.Count - 1];
                reorderList.RemoveAt(reorderList.Count - 1);
                reorderList.Insert(0, lastElement);
            }
            return reorderList;
        }

        // 리스트 아이템을 찾아 반환하는 함수
        public T GetList<T>(int index) where T : ListUI => _poolingObjectList.Find((list) => list.GetIndex() == index) as T;

        // 리스트 아이템을 추가하고 RectTransform 값 초기화
        public T AddListUI<T>(Transform parent, int index) where T : ListUI
        {
            GameObject obj = _baseListUI != null ? Instantiate(_baseListUI.gameObject, parent, false) : Manager.UI.GetListUI<T>(parent).gameObject;
            if (obj == null) return null;
            obj.name = $"Element_{index}";

            var rectTr = (RectTransform)obj.transform;
            RectValueInitialization(rectTr, index);

            T list = Util.GetOrAddComponent<T>(obj);
            list.SetIndex(index);
            list.SetParent(_parentUI);
            return list;
        }

        // RectTransform 값 초기화 함수
        private void RectValueInitialization(RectTransform rectTr, int index)
        {
            // 리스트 아이템의 너비 및 높이 설정
            if (_elementWidth < 0 || _elementHeight < 0)
            {
                _elementWidth = rectTr.rect.width;
                _elementHeight = rectTr.rect.height;
            }

            // 앵커 및 피벗 설정
            rectTr.anchorMin = new Vector2(0, 1);
            rectTr.anchorMax = new Vector2(0, 1);
            rectTr.pivot = new Vector2(0, 1);

            if (vertical)
            {
                var widthSize = content.rect.width / GridSize;
                if (_elementWidth > widthSize)
                {
                    var size = widthSize / _elementWidth;
                    rectTr.localScale = new Vector3(size, size, 1f);
                }

                var gridIndex = index / GridSize;
                var anchoredPos = new Vector2(GridWidth(index, rectTr.sizeDelta.x),
                    -(gridIndex * rectTr.sizeDelta.y) - (VerticalSpacing * gridIndex) - TopPadding);
                rectTr.anchoredPosition = anchoredPos;

                if (index % GridSize == 0)
                    content.sizeDelta += new Vector2(0f, _elementHeight + VerticalSpacing);
            }
        }

        // RectTransform 크기가 변경될 때 호출되는 함수
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (_completeCreatePool)
            {
                for (int i = 0; i < _poolingObjectList.Count; i++)
                {
                    var list = _poolingObjectList[i];
                    list.Reset();
                    var rectTr = (RectTransform)list.transform;
                    rectTr.anchoredPosition = new Vector2(GridWidth(i, rectTr.sizeDelta.x),
                        -((i / GridSize) * rectTr.sizeDelta.y) - (VerticalSpacing * i) - TopPadding);
                }

                content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0);
            }
        }
    }
}