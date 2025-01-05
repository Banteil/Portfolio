using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io.kingnslave
{
    public class UIRoundList : UIList
    {
        private const string ROUND_TEXT_KEY = "roundText";

        enum RoundListText
        {
            RoundTitleText,
        }

        enum RoundListTransform
        {
            ActionContent,
        }

        private void Awake() => Initialized();

        protected override void InitializedProcess()
        {
            SetParent<UIRoundInfo>();
            Bind<TextMeshProUGUI>(typeof(RoundListText));
            Bind<Transform>(typeof(RoundListTransform));
            gameObject.BindEvent(DisplayActionList);
        }

        public void SetListData(RoundData data)
        {
            GetText((int)RoundListText.RoundTitleText).text = $"{Util.GetLocalizationTableString(Define.CommonLocalizationTable, ROUND_TEXT_KEY)} {index + 1}";
            var summitCount = data.submit_red.Length;
            var actionContent = Get<Transform>((int)RoundListTransform.ActionContent);
            for (int i = 0; i < summitCount; i++)
            {
                var actionList = AddActionList(actionContent, i);
                actionList.Initialized();

                var redInfo = data.submit_red[i];
                var blueInfo = data.submit_blue[i];
                actionList.SetListData(redInfo, blueInfo, data.card_array);
            }
        }

        private UIActionList AddActionList(Transform parent, int index) => UIManager.Instance.AddListUI<UIActionList>(parent, "ActionListUI", index);

        private void DisplayActionList(PointerEventData data)
        {
            if (isDrag) return;
            var actionContentObj = Get<Transform>((int)RoundListTransform.ActionContent).gameObject;
            actionContentObj.SetActive(!actionContentObj.activeSelf);
        }
    }
}
