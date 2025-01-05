using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public enum ChatBoxType
    {
        MINE,
        OPPONENT,
    }

    public class UIChatList : UIList
    {
        private Color32 _mineColor = new Color32(253, 240, 203, 255);
        private Color32 _opponentColor = new Color32(240, 240, 240, 255);

        enum ChatListGameObject
        {
            LinkButtons,
        }

        enum ChatListImage
        {
            TextBox,
        }

        enum ChatListText
        {
            ChatText,
        }

        enum ChatListOldText
        {
            LoadingText,
        }

        private ChatBoxType _type;
        private Sequence _sequence;
        [SerializeField]
        private List<HorizontalLayoutGroup> _groups;

        protected override void OnAwake()
        {
            Bind<GameObject>(typeof(ChatListGameObject));
            GetObject((int)ChatListGameObject.LinkButtons).SetActive(false);
            Bind<Image>(typeof(ChatListImage));
            Bind<TextMeshProUGUI>(typeof(ChatListText));
            Bind<Text>(typeof(ChatListOldText));
        }

        public void SetType(ChatBoxType type = ChatBoxType.MINE)
        {
            _type = type;
            var isMine = _type == ChatBoxType.MINE;
            var loadingObj = Get<Text>((int)ChatListOldText.LoadingText).gameObject;
            if (isMine)
                loadingObj.SetActive(false);
            else
            {
                loadingObj.SetActive(true);
                LoadingDirectionPlayback();
            }
            GetImage((int)ChatListImage.TextBox).color = isMine ? _mineColor : _opponentColor;
            for (int i = 0; i < _groups.Count; i++)
            {
                _groups[i].childAlignment = isMine ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
                GetText((int)ChatListText.ChatText).alignment = isMine ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;
            }
        }

        public void SetText(string text)
        {
            GetText((int)ChatListText.ChatText).text = text;
            if (_type == ChatBoxType.OPPONENT)
                StopLoadingDirection();

        }

        private void LoadingDirectionPlayback()
        {
            var dotText = Get<Text>((int)ChatListOldText.LoadingText);
            _sequence = DOTween.Sequence() // 시퀀스를 변수에 저장
                .Append(dotText.DOText("·", 0.5f))
                .Append(dotText.DOText("··", 0.5f))
                .Append(dotText.DOText("···", 0.5f))
                .SetLoops(-1, LoopType.Restart);
            _sequence.Play(); // 연출 시작
        }

        private void StopLoadingDirection()
        {
            if (_sequence != null && _sequence.IsActive())
            {
                _sequence.Kill(); // 연출을 멈춥니다
            }
            var dotText = Get<Text>((int)ChatListOldText.LoadingText);
            dotText.gameObject.SetActive(false);
        }

        public void ActiveLinkButton(string linkURL)
        {
            if (string.IsNullOrEmpty(linkURL)) return;
            var group = GetImage((int)ChatListImage.TextBox).GetComponent<VerticalLayoutGroup>();
            group.spacing = 20;
            var buttons = GetObject((int)ChatListGameObject.LinkButtons);
            buttons.SetActive(true);

            var splitLinks = linkURL.Split('&');
            for (int i = 0; i < splitLinks.Length; i++)
            {
                var buttonObj = ResourceManager.Instance.Instantiate("UI/Button/LinkButton", buttons.transform, false);
                var linkButton = buttonObj.GetComponent<LinkButton>();
                linkButton.URL = splitLinks[i];
            }
        }

        public void SetPreferredWidth(int width)
        {
            var insideFrameLayoutElement = _groups[1].GetComponent<LayoutElement>();
            insideFrameLayoutElement.preferredWidth = width;
        }

        public void SetFontSize(int size)
        {
            GetText((int)ChatListText.ChatText).fontSize = size;
        }
    }
}
