using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIModifyCountry : UIPopup
    {
        private UIUserProfile profile;
        public int SelectedIndex { get; set; } = -1;

        enum ModifyCountryScrollView
        {
            CountryScrollView,
        }

        enum ModifyCountryButtons
        {
            ConfirmButton = 1,
        }

        private void Start() => Initialized();

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<ScrollRect>(typeof(ModifyCountryScrollView));
            Bind<Button>(typeof(ModifyCountryButtons));
            GetButton((int)ModifyCountryButtons.ConfirmButton).gameObject.BindEvent(ConfirmButton);

            var scrollRect = GetScrollRect((int)ModifyCountryScrollView.CountryScrollView) as InfinityScrollRect;
            scrollRect.MaxCount = ResourceManager.Instance.FlagSpriteList.Count;
            scrollRect.CreatePoolingList<UICountryList>("CountryListUI");

            var findIndex = UserDataManager.Instance.MyData.country_seq;
            SelectedIndex = findIndex;
            if (SelectedIndex != -1)
            {
                var list = scrollRect.GetList<UICountryList>(SelectedIndex);
                if (list != null)
                    list.GetToggle().isOn = true;
            }
        }

        public override void SetListData(UIList list)
        {
            var countryList = list as UICountryList;
            var index = countryList.GetIndex();
            countryList.SetListData(ResourceManager.Instance.FlagSpriteList[index]);
            countryList.GetToggle().isOn = index == SelectedIndex;
        }

        async private void ConfirmButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));
            var countryList = ResourceManager.Instance.FlagSpriteList;
            if (SelectedIndex < 0 || SelectedIndex >= countryList.Count)
            {
                UIManager.Instance.CloseUI();
                return;
            }

            if (profile != null)
            {
                var sid = profile.ProfileUserData.sid;

                var processSuccess = false;
                await CallAPI.APIUpdateUserCountrySeq(sid, SelectedIndex, (returnCd) =>
                {
                    processSuccess = returnCd == (int)Define.APIReturnCd.OK;
                });

                if(processSuccess)
                {
                    await CallAPI.APISelectUser(sid, sid, (data) =>
                    {
                        UserDataManager.Instance.MyData = data;
                        profile.SetUserData(data);
                    });
                }

                UIManager.Instance.CloseUI();
            }
            else
                UIManager.Instance.CloseUI();
        }

        public void SetProfile(UIUserProfile profile) => this.profile = profile;

        public void ResetToggle(UICountryList interactList)
        {
            var scrollRect = GetScrollRect((int)ModifyCountryScrollView.CountryScrollView) as InfinityScrollRect;
            if (scrollRect != null)
            {
                var list = scrollRect.GetList<UICountryList>(SelectedIndex);
                if (list != null)
                    list.GetToggle().isOn = true;
                else
                    interactList.GetToggle().isOn = false;
            }
        }
    }
}
