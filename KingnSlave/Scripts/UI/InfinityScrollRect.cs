using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class InfinityScrollRect : ScrollRect
    {
        public int MaxPoolingSize = 10;
        public int GridSize = 1;
        public int HorizontalPading = 0;
        public int VerticalPading = 0;
        public UIBase ParentUI;

        public int MaxCount { get; set; }
        protected int LimitCount
        {
            get
            {
                var maxCount = MaxCount - poolingObjectList.Count;
                var limitCount = maxCount / GridSize;
                if (maxCount != 0 && MaxCount % GridSize != 0)
                    limitCount++;
                return limitCount;
            }
        }

        protected int currentIndex = 0;
        protected float elementWidth, elementHeight;
        protected List<UIList> poolingObjectList = new List<UIList>();
        protected bool completeCreatePool = false;

        protected override void Awake()
        {
            if (ParentUI == null)
                ParentUI = Util.FindComponentInParents<UIBase>(transform);
            onValueChanged.AddListener(CheckScroll);
        }

        public void CreatePoolingList<T>(string listName) where T : UIList
        {
            if (completeCreatePool) return;
            var maxSIze = MaxPoolingSize > MaxCount ? MaxCount : MaxPoolingSize;
            for (int i = 0; i < maxSIze; i++)
            {
                var uiList = AddList<T>(content, listName);
                uiList.Initialized();
                uiList.gameObject.name = $"Element_{i}";
                poolingObjectList.Add(uiList);
                var rectTr = (RectTransform)uiList.transform;
                if (i == 0)
                {
                    elementWidth = rectTr.rect.width;
                    elementHeight = rectTr.rect.height;
                }
                rectTr.anchorMin = new Vector2(0, 1);
                rectTr.anchorMax = new Vector2(0, 1);
                rectTr.pivot = new Vector2(0, 1);

                if (vertical)
                {
                    var widthSize = content.rect.width / GridSize;
                    if (elementWidth > widthSize)
                    {
                        var size = widthSize / elementWidth;
                        rectTr.localScale = new Vector3(size, size, 1f);
                    }
                    var gridIndex = i / GridSize;
                    var anchoredPos = new Vector2(GridWidth(i, rectTr.sizeDelta.x), -(gridIndex * rectTr.sizeDelta.y) - (VerticalPading * gridIndex));
                    rectTr.anchoredPosition = anchoredPos;

                    if ((i % GridSize) == 0)
                        content.sizeDelta += new Vector2(0f, elementHeight + VerticalPading);
                }
                //else if (horizontal)
                //{
                //    rectTr.anchoredPosition = new Vector2(-(i / GridSize * rectTr.sizeDelta.x), GridHeight(i));
                //    var heightSize = content.rect.height / GridSize;
                //    if (GridSize > 1 && rectTr.rect.height > heightSize)
                //        rectTr.sizeDelta = new Vector2(rectTr.sizeDelta.x, heightSize);

                //    if ((i % GridSize) == 0)
                //        content.sizeDelta += new Vector2(elementWidth, 0f);
                //}

                SetListData(i, uiList);
                completeCreatePool = true;
            }
        }

        protected float GridWidth(int index, float widthSize)
        {
            var gridIndex = (index % GridSize) + 1;
            var pading = HorizontalPading;
            if (pading != 0)
            {
                if (GridSize % 2 == 0)
                    pading *= gridIndex <= (GridSize / 2) ? -1 : 1;
                else
                {
                    var middle = (GridSize / 2) + 1;
                    if (gridIndex < middle) pading *= -1;
                    else if (gridIndex > middle) pading *= 1;
                    else pading = 0;
                }
            }
            var percent = 1f / (GridSize + 1);
            var contentWidth = content.rect.width * (percent * gridIndex) - (widthSize * 0.5f) + pading;
            return contentWidth;
        }

        protected float GridHeight(int index)
        {
            var gridIndex = index % GridSize;
            var result = 0f;
            if (GridSize > 1)
            {
                var contentHeight = content.rect.height / GridSize;
                result = contentHeight * gridIndex;
            }
            return result;
        }

        protected void SetListData(int index, UIList uiList)
        {
            var active = index >= 0 && index < MaxCount;
            if (active)
                uiList.gameObject.SetActive(true);
            else
            {
                uiList.gameObject.SetActive(false);
                return;
            }

            uiList.SetIndex(index);
            ParentUI.SetListData(uiList);
        }

        public void ResetListData()
        {
            for (int i = 0; i < poolingObjectList.Count; i++)
            {
                ParentUI.SetListData(poolingObjectList[i]);
            }
        }

        public void DestroyAllListData()
        {
            for (int i = 0; i < poolingObjectList.Count; i++)
            {
                Destroy(poolingObjectList[i].gameObject);
            }
            poolingObjectList.Clear();
        }

        private void CheckScroll(Vector2 vec)
        {
            if (vertical) CheckVertical();
            //else if (horizontal) CheckHorizontal();
        }

        private void CheckVertical()
        {
            if (elementHeight <= 0 || poolingObjectList.Count <= 0) return;

            var scrollY = content.localPosition.y;
            var index = (int)(scrollY / (elementHeight + VerticalPading));

            if (index > currentIndex)
            {
                var prevIndex = currentIndex + 1;
                if (prevIndex > LimitCount) return;
                currentIndex = index > LimitCount ? LimitCount : index;
                for (int i = prevIndex; i <= currentIndex; i++)
                {
                    var gridIndex = (i - 1) * GridSize;
                    for (int j = 0; j < GridSize; j++)
                    {
                        var maxIndex = (gridIndex + j) + poolingObjectList.Count;

                        var list = poolingObjectList[0];
                        SetListData(maxIndex, list);

                        list.transform.SetAsLastSibling();
                        poolingObjectList = ReorderList(poolingObjectList, true);

                        var rectTr = (RectTransform)list.transform;
                        var gridMaxIndex = maxIndex / GridSize;
                        rectTr.anchoredPosition = new Vector2(GridWidth(j, rectTr.sizeDelta.x), -(gridMaxIndex * rectTr.sizeDelta.y) - (VerticalPading * gridMaxIndex));
                    }
                    content.sizeDelta += new Vector2(0f, elementHeight + VerticalPading);
                }
            }
            else if (index < currentIndex)
            {
                var prevIndex = currentIndex - 1;
                if (prevIndex < 0) return;
                currentIndex = index < 0 ? 0 : index;
                for (int i = prevIndex; i >= currentIndex; i--)
                {
                    var gridIndex = i * GridSize;
                    for (int j = GridSize - 1; j >= 0; j--)
                    {
                        var list = poolingObjectList[poolingObjectList.Count - 1];
                        SetListData(gridIndex + j, list);

                        list.transform.SetAsFirstSibling();
                        poolingObjectList = ReorderList(poolingObjectList, false);

                        var rectTr = (RectTransform)list.transform;
                        rectTr.anchoredPosition = new Vector2(GridWidth(j, rectTr.sizeDelta.x), -(i * rectTr.sizeDelta.y) - (VerticalPading * i));
                    }
                    content.sizeDelta -= new Vector2(0f, elementHeight + VerticalPading);
                }
            }
        }

        private void CheckHorizontal()
        {
            if (elementWidth <= 0 || poolingObjectList.Count <= 0) return;

            var scrollX = content.localPosition.x;
            var index = (int)(scrollX / elementWidth);
            if (index < 0 || index > LimitCount) return;

            if (index > currentIndex)
            {
                var prevIndex = currentIndex + 1;
                currentIndex = index;
                for (int i = prevIndex; i <= currentIndex; i++)
                {
                    var gridIndex = (i - 1) * GridSize;
                    for (int j = 0; j < GridSize; j++)
                    {
                        var maxIndex = (gridIndex + j) + poolingObjectList.Count;

                        var list = poolingObjectList[0];
                        SetListData(maxIndex, list);

                        list.transform.SetAsLastSibling();
                        poolingObjectList = ReorderList(poolingObjectList, true);

                        var rectTr = (RectTransform)list.transform;
                        rectTr.anchoredPosition = new Vector2(-((maxIndex / GridSize) * rectTr.sizeDelta.x), GridHeight(j));
                    }
                    content.sizeDelta += new Vector2(elementWidth, 0f);
                }
            }
            else if (index < currentIndex)
            {
                var prevIndex = currentIndex - 1;
                currentIndex = index;
                for (int i = prevIndex; i >= currentIndex; i--)
                {
                    var gridIndex = i * GridSize;
                    for (int j = GridSize - 1; j >= 0; j--)
                    {
                        var list = poolingObjectList[poolingObjectList.Count - 1];
                        SetListData(gridIndex + j, list);

                        list.transform.SetAsFirstSibling();
                        poolingObjectList = ReorderList(poolingObjectList, false);

                        var rectTr = (RectTransform)list.transform;
                        rectTr.anchoredPosition = new Vector2(-(i * rectTr.sizeDelta.x), GridHeight(j));
                    }
                    content.sizeDelta -= new Vector2(elementWidth, 0f);
                }
            }
        }

        protected T AddList<T>(Transform parent, string listName) where T : UIList
        {
            return UIManager.Instance.AddListUI<T>(parent, listName);
        }

        /// <summary>
        /// 리스트의 처음 인덱스를 빼서 마지막으로 넣거나, 마지막 인덱스를 빼서 처음으로 넣는 정렬 함수
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="isFirstToLast"></param>
        /// <returns></returns>
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

        public T GetList<T>(int index) where T : UIList => poolingObjectList.Find((list) => list.GetIndex() == index) as T;

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (completeCreatePool)
            {
                for (int i = 0; i < poolingObjectList.Count; i++)
                {
                    var rectTr = (RectTransform)poolingObjectList[i].transform;
                    rectTr.anchoredPosition = new Vector2(GridWidth(i, rectTr.sizeDelta.x), -(i / GridSize * rectTr.sizeDelta.y));
                }
            }
        }
    }
}
