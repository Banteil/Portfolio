using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UILobbyHelpPopup : UIPopup
    {
        private List<GameObject> frameList = new List<GameObject>();
        private int currentIndex = 0;

        enum LobbyHelpGameObject
        {
            Rule,
            Tip,
        }

        enum LobbyHelpButton
        {
            LeftButton = 1,
            RightButton,
        }

        private void Start() => Initialized();

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<GameObject>(typeof(LobbyHelpGameObject));
            var frames = GetAll<GameObject>().ToList();
            foreach (var frame in frames)
            {
                if (frame == null) break;
                frameList.Add(frame);
            }
            Bind<Button>(typeof(LobbyHelpButton));
            GetButton((int)LobbyHelpButton.LeftButton).gameObject.BindEvent(ClickLeftButton);
            GetButton((int)LobbyHelpButton.RightButton).gameObject.BindEvent(ClickRightButton);

            frameList[currentIndex].gameObject.SetActive(true);
            CheckActiveButton();
        }

        private void ClickLeftButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));

            frameList[currentIndex].gameObject.SetActive(false);
            currentIndex--;
            frameList[currentIndex].gameObject.SetActive(true);
            CheckActiveButton();
        }

        private void ClickRightButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));

            frameList[currentIndex].gameObject.SetActive(false);
            currentIndex++;
            frameList[currentIndex].gameObject.SetActive(true);
            CheckActiveButton();
        }

        private void CheckActiveButton()
        {
            var leftButton = GetButton((int)LobbyHelpButton.LeftButton);
            var rightButton = GetButton((int)LobbyHelpButton.RightButton);

            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);
            if (currentIndex <= 0)
                leftButton.gameObject.SetActive(false);
            if (currentIndex >= frameList.Count - 1)
                rightButton.gameObject.SetActive(false);
        }
    }
}
