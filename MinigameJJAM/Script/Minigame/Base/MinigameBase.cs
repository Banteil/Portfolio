using System;
using UnityEngine;

namespace starinc.io
{
    public class MinigameBase : BindBehavior
    {
        #region Cache
        protected int _score = 0;
        public int Score
        {
            get { return _score; }
            set
            {
                _score = value;
                OnChangedScore?.Invoke(_score);
            }
        }

        public bool IsGameOver { get; protected set; } = false;

        [SerializeField]
        protected PrefabObjectTable _gameObjectTable;

        [SerializeField]
        protected SpriteTable _spriteTable;

        protected event Action _onUpdateProcess;
        public Action OnEndGame;
        #endregion

        #region Callback
        public event Action<int> OnChangedScore;
        #endregion

        public virtual void Initialization() { }

        public virtual void StartProcess()
        {
            if (IsGameOver) return;
            var sceneUI = Manager.UI.SceneUI as MinigameSceneUI;
            if (sceneUI == null)
            {
                Debug.LogError("Not Minigame Scene");
                return;
            }
            var currentAddress = Manager.Game.CurrentGameAddress;
            var bgm = sceneUI.SoundTable.GetBGMClip($"minigame_{currentAddress}_bgm");
            Manager.Sound.PlayBGM(bgm);

            _onUpdateProcess += UpdateProcess;
        }

        protected virtual void UpdateProcess() { }

        public virtual void EndProcess()
        {
            if (IsGameOver) return;
            IsGameOver = true;
            _onUpdateProcess -= UpdateProcess;

            OnEndGame?.Invoke();            
            CheckHighScore();

            Manager.Sound.StopBGM(0.5f);
            Manager.Sound.StopAllSFX(0.5f);
            Manager.UI.ShowPopupUI<GameOverPopupUI>(null, "gameOver");
        }

        protected virtual void CheckHighScore()
        {
            var gameManager = Manager.Game;
            var currentAddress = gameManager.CurrentGameAddress;
            var highScore = gameManager.Scores.GetScoreByAddress(currentAddress);
            if (Score > highScore)
                gameManager.UpdateHighScore(currentAddress, Score);
        }

        public void PlayBGM(string clipName)
        {
            var soundTable = Manager.UI.SceneUI.SoundTable;
            if (soundTable != null)
            {
                var clip = soundTable.GetBGMClip(clipName);
                Manager.Sound.PlaySFX(clip);
            }
        }

        public void PlaySFX(string clipName)
        {
            var soundTable = Manager.UI.SceneUI.SoundTable;
            if (soundTable != null)
            {
                var clip = soundTable.GetSFXClip(clipName);
                Manager.Sound.PlaySFX(clip);
            }
        }

        public Sprite GetSprite(string spriteName) => _spriteTable.GetSprite(spriteName);

        public GameObject GetPrefabObject(string name, Transform parent = null, bool instantiateInWorldSpace = false) => _gameObjectTable.GetPrefabObject(name, parent, instantiateInWorldSpace);

        protected virtual void Update()
        {
            _onUpdateProcess?.Invoke();
        }
    }
}
