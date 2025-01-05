using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static starinc.io.Define;

namespace starinc.io.kingnslave
{
    public class UIActionList : UIList
    {
        enum ActionListText
        {
            ActionIndexText,
            RedCardNameText,
            BlueCardNameText,
        }

        enum ActionListImage
        {
            ActionImage,
            RedBackground,
            RedCharacter,
            BlueBackground,
            BlueCharacter,
        }

        private void Awake() => Initialized();

        protected override void InitializedProcess()
        {
            Bind<TextMeshProUGUI>(typeof(ActionListText));
            Bind<Image>(typeof(ActionListImage));
        }

        public void SetListData(char redInfo, char blueInfo, string cardArray)
        {
            GetText((int)ActionListText.ActionIndexText).text = (index + 1).ToString();

            var indexRed = int.Parse(redInfo.ToString());
            var resultRed = cardArray[indexRed];
            GetImage((int)ActionListImage.RedCharacter).sprite = ResourceManager.Instance.GetCardCharacterSprite(GetActionCardType(resultRed));
            GetImage((int)ActionListImage.RedBackground).sprite = ResourceManager.Instance.GetCardBackgroundSprite(GetActionCardType(resultRed));
            GetText((int)ActionListText.RedCardNameText).text = GetActionCardType(resultRed).ToString().ToUpper();

            var indexBlue = int.Parse(blueInfo.ToString());
            var resultBlue = cardArray[indexBlue];
            GetImage((int)ActionListImage.BlueCharacter).sprite = ResourceManager.Instance.GetCardCharacterSprite(GetActionCardType(resultBlue));
            GetImage((int)ActionListImage.BlueBackground).sprite = ResourceManager.Instance.GetCardBackgroundSprite(GetActionCardType(resultBlue));
            GetText((int)ActionListText.BlueCardNameText).text = GetActionCardType(resultBlue).ToString().ToUpper();

            var roundReslutSpirte = GetActionResultSprite(resultRed, resultBlue);
            GetImage((int)ActionListImage.ActionImage).sprite = roundReslutSpirte;
        }

        private CardType GetActionCardType(char result)
        {
            switch (result)
            {
                case 'C':
                    return CardType.Citizen;
                case 'K':
                    return CardType.King;
                case 'S':
                    return CardType.Slave;
                default:
                    return CardType.None;
            }
        }

        private Sprite GetActionResultSprite(char red, char blue)
        {
            var redWin = (red == 'K' && blue == 'C') || (red == 'S' && blue == 'K') || (red == 'C' && blue == 'S');
            var blueWin = (blue == 'K' && red == 'C') || (blue == 'S' && red == 'K') || (blue == 'C' && red == 'S');

            if (redWin)
                return ResourceManager.Instance.GetSprite("Round_RedWin");
            else if (blueWin)
                return ResourceManager.Instance.GetSprite("Round_BlueWin");
            else
                return ResourceManager.Instance.GetSprite("Round_Draw");
        }
    }
}
