using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io.kingnslave
{
    public class UIUserProfile : UIPopup
    {
        protected UserData profileUserData;
        public UserData ProfileUserData { get { return profileUserData; } }
        public Texture ProfileUserTexture { get; set; }

        private enum ProfileTapMenu
        {
            TapMenu,
        }

        private enum ProfileFrame
        {
            CommonFrame,
            NormalGameFrame,
            RankGameFrame,
            RecordFrame,
        }

        async public void SetUserData(UserData userData)
        {
            await CallAPI.APISelectUser(UserDataManager.Instance.MySid, userData.sid, (data) =>
            {
                if(data == null)
                {
                    UIManager.Instance.ShowWarningUI("User Data is Null", false);
                    Destroy(gameObject);
                    return;
                }
                profileUserData = data;
            });            

            NetworkManager.Instance.GetTexture((texture) =>
            {
                ProfileUserTexture = texture;
            }, profileUserData.profile_image);
            SetFrameData();
        }

        private void Start() => Initialized();

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<TapMenu>(typeof(ProfileTapMenu));
            Bind<UIFrame>(typeof(ProfileFrame));
            var tapMenu = Get<TapMenu>((int)ProfileTapMenu.TapMenu);
            for (int i = 0; i < tapMenu.transform.childCount; i++)
            {
                tapMenu.transform.GetChild(i).gameObject.BindEvent(OnTapMenuClicked);
            }
            tapMenu.GetTapIndex(0).isOn = true;
        }

        private void SetFrameData()
        {
            var frames = GetAll<UIFrame>();
            foreach (var frame in frames)
            {
                if (frame == null) continue;
                frame.Initialized();
                frame.SetData(profileUserData);
            }
        }

        private void OnTapMenuClicked(PointerEventData eventData)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));
        }
    }
}
