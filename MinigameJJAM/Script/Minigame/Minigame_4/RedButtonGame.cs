using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class RedButtonGame : MinigameBase
    {
        #region Cache        
        private enum RedButtonGameImage
        {
            RedButton,
        }

        private float _scoreTimer = 0f;

        #endregion

        public override void Initialization()
        {
            Bind<Image>(typeof(RedButtonGameImage));
            var redButton = GetImage((int)RedButtonGameImage.RedButton);
            redButton.gameObject.BindEvent(PushRedButton);
        }

        public override void StartProcess()
        {
            base.StartProcess();
        }

        protected override void UpdateProcess()
        {
            _scoreTimer += Time.deltaTime;
            Score = (int)_scoreTimer;
        }

        public override async void EndProcess()
        {
            if (IsGameOver) return;
            IsGameOver = true;
            _onUpdateProcess -= UpdateProcess; 
            OnEndGame?.Invoke();
            CheckHighScore();
            Manager.Sound.StopBGM(0.2f);
            Manager.Sound.StopAllSFX(0.2f);

            PlaySFX("m4sfx_click");
            await UniTask.WaitForSeconds(0.5f);
            PlaySFX("m4sfx_doom");        
            await UniTask.WaitForSeconds(3f);            

            Manager.UI.ShowPopupUI<GameOverPopupUI>(null, "gameOver");
        }


        #region BindEvent
        private void PushRedButton(PointerEventData data)
        {  
            var redButton = GetImage((int)RedButtonGameImage.RedButton);
            redButton.sprite = GetSprite("red_button_push");
            EndProcess();
            redButton.gameObject.RemoveEvent();
        }
        #endregion
    }
}
