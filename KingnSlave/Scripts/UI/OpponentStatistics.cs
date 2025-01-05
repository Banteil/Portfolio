using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class OpponentStatistics : UISide
    {
        private const int ADDITION_X_POS = 1840;

        private GameObject openArrow;
        private Image windowOutside;

        enum StatisticsWindowImage
        {
            WindowOutside,
            StatisticsOpenArrow,
            CancelButton
        }

        enum StatisticsWindowText
        {
            King1 = 0,
            King2,
            King3,
            King4,
            King5,
            Slave1,
            Slave2,
            Slave3,
            Slave4,
            Slave5
        }

        protected override void Awake()
        {
            areaRectTr = (RectTransform)transform.Find("SideArea");
            direction = SideDirection.RIGHT;
            size = ADDITION_X_POS;
        }

        private void Start()
        {
            //Bind
            Bind<Image>(typeof(StatisticsWindowImage));
            Bind<TextMeshProUGUI>(typeof(StatisticsWindowText));

            //Bind Event
            openArrow = GetImage((int)StatisticsWindowImage.StatisticsOpenArrow).gameObject;
            openArrow.BindEvent(OpenDirectionEvent, Define.UIEvent.Click);
            windowOutside = GetImage((int)StatisticsWindowImage.WindowOutside);
            windowOutside.gameObject.BindEvent(CloseDirectionEvent, Define.UIEvent.Click);
			GetImage((int)StatisticsWindowImage.CancelButton).gameObject.BindEvent(CloseDirectionEvent, Define.UIEvent.Click);

			RectTransform backgroundRT = windowOutside.GetComponent<RectTransform>();
            backgroundRT.sizeDelta = windowOutside.transform.root.GetComponent<RectTransform>().sizeDelta;
            backgroundRT.localPosition = new Vector3(backgroundRT.rect.width / 2, backgroundRT.localPosition.y);
            SetStatisticsText();
        }

        async private void SetStatisticsText()
        {
            await CallAPI.APISelectUserSubmitStatistics(UserDataManager.Instance.MySid, UserDataManager.Instance.OpponentDataList[0].sid, (statistics) =>
            {
                string[] statisticsArray = new string[10] {statistics.king1, statistics.king2, statistics.king3, statistics.king4, statistics.king5,
                    statistics.slave1, statistics.slave2, statistics.slave3, statistics.slave4, statistics.slave5};
                for (int i = (int)StatisticsWindowText.King1; i <= (int)StatisticsWindowText.Slave5; i++)
                {
                    GetText(i).text = statisticsArray[i] + "%";
                }
            });
        }

        private void OpenDirectionEvent(PointerEventData eventData)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));

            openArrow.SetActive(false);
            windowOutside.raycastTarget = true;
            OpenDirection();
        }

        private void CloseDirectionEvent(PointerEventData eventData)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickCloseButton));

            windowOutside.raycastTarget = false;
            CloseDirection(() => { openArrow.SetActive(true); });
        }

        public override void InputEscape()
        {
            if (!isDirecting)
                CloseDirectionEvent(null);
        }
    }
}