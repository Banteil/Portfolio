using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace starinc.io
{
    public class BreadGame : MinigameBase
    {
        #region Cache
        private const string BREAD_ITEM = "BreadItem";
        private const string HEART_FILLED = "heart_filled";
        private const string HEART_EMPTY = "heart_empty";

        private const int MAX_ACTIVE_ITEMS = 2;
        private const float MAP_SIZE = 22.5f;
        private const float SCORE_INCREASE_TIME = 0.1f;

        private enum BreadGameCanvasGroup
        {
            HeartUI,
        }

        private List<Image> _hearts = new List<Image>();

        #region Item
        private Transform _itemPoolRoot;
        public IObjectPool<BaseItem> ItemPool { get; private set; }
        private List<BaseItem> _activeItems = new List<BaseItem>();

        [SerializeField]
        private float _itemSpawnTimeLimit = 30f;
        private float _currentItemSpawnTime = 0;
        #endregion

        #region Boss
        private int _maxBossCount = 1;
        private int _currentBossIndex = 1;
        private float _bossSpwanTimeLimit = 60f;
        private float _currentBossSpwanTime = 0;
        #endregion

        private float _scoreTimer = 0f;

        public BreadController Player { get; private set; }        
        #endregion

        public override void Initialization()
        {
            Player = FindAnyObjectByType<BreadController>();
            Player.OnChangedHP += ChangedHP;
            Player.OnDisableController += EndProcess;

            Bind<CanvasGroup>(typeof(BreadGameCanvasGroup));
            var heartUI = Get<CanvasGroup>((int)BreadGameCanvasGroup.HeartUI).transform;
            for (int i = 0; i < heartUI.childCount; i++)
            {
                var heartImage = heartUI.GetChild(i).GetComponent<Image>();
                _hearts.Add(heartImage);
            }

            _itemPoolRoot ??= new GameObject("PoolRoot").transform;
            ItemPool = new ObjectPool<BaseItem>(CreateItem, OnGetItem, OnReleaseItem, OnDestroyItem);
        }

        public override void StartProcess()
        {
            base.StartProcess();
            var controller = Manager.UI.ShowOrGetVirtualController();
            controller.OnMovePlayer += Player.InputAction;
            OnEndGame += controller.DeactivateController;
            controller.ActivateController();

            var heartUI = Get<CanvasGroup>((int)BreadGameCanvasGroup.HeartUI);
            heartUI.alpha = 1;

            _currentBossSpwanTime = _bossSpwanTimeLimit;
            _onUpdateProcess += SpawnBoss;
        }

        protected override void UpdateProcess()
        {
            SpawnItem();
            IncreaseScore();
        }

        private void SpawnItem()
        {
            _currentItemSpawnTime += Time.deltaTime;

            if(_currentItemSpawnTime >= _itemSpawnTimeLimit)
            {
                var newItem = ItemPool.Get();
                if (_activeItems.Count >= MAX_ACTIVE_ITEMS)
                {
                    var oldestItem = _activeItems.Dequeue();
                    ItemPool.Release(oldestItem);
                }
                _activeItems.Add(newItem);
                _currentItemSpawnTime = 0f;
            }
        }

        private void IncreaseScore()
        {
            _scoreTimer += Time.deltaTime;
            if(_scoreTimer >= SCORE_INCREASE_TIME)
            {
                Score++;
                _scoreTimer = 0f;
            }
        }

        private void SetRandomPosition(BaseItem item)
        {
            float randomX = Random.Range(-MAP_SIZE, MAP_SIZE);
            float randomY = Random.Range(-MAP_SIZE, MAP_SIZE);
            item.transform.position = new Vector3(randomX, randomY, 0);
        }

        private void SpawnBoss()
        {
            _currentBossSpwanTime += Time.deltaTime;
            if(_currentBossSpwanTime >= _bossSpwanTimeLimit)
            {
                _gameObjectTable.GetPrefabObject($"Boss_{_currentBossIndex}");
                _currentBossSpwanTime = 0;

                _currentBossIndex++;
                if(_currentBossIndex > _maxBossCount)
                    _onUpdateProcess -= SpawnBoss;
            }            
        }

        #region ItemPoolEvent
        private BaseItem CreateItem()
        {
            var obj = _gameObjectTable.GetPrefabObject(BREAD_ITEM, _itemPoolRoot, false);
            var item = obj.GetComponent<BaseItem>();
            item.OnReturnToPool += ItemPool.Release;

            SetRandomPosition(item);
            return item;
        }

        private void OnGetItem(BaseItem item)
        {
            SetRandomPosition(item);
            item.gameObject.SetActive(true);
        }

        private void OnReleaseItem(BaseItem item)
        {
            _activeItems.Remove(item);
            item.gameObject.SetActive(false);
        }

        private void OnDestroyItem(BaseItem item)
        {
            if (_activeItems.Contains(item))
                _activeItems.Remove(item);
            item.OnReturnToPool -= OnReleaseItem;
            Destroy(item.gameObject);
        }
        #endregion

        #region Callback
        private void ChangedHP(int hp)
        {
            for (int i = 0; i < _hearts.Count; i++)
            {
                var sprite = _spriteTable.GetSprite(i < hp ? HEART_FILLED : HEART_EMPTY);
                _hearts[i].sprite = sprite;
            }
        }
        #endregion

        public override async void EndProcess()
        {
            IsGameOver = true;
            _onUpdateProcess -= UpdateProcess;
            OnEndGame?.Invoke();
            CheckHighScore();

            Manager.Sound.PlaySFX("m2sfx_gameover");
            await UniTask.WaitForSeconds(1f);

            Manager.Sound.StopBGM(0.5f);
            Manager.Sound.StopAllSFX(0.5f);
            Manager.UI.ShowPopupUI<GameOverPopupUI>(null, "gameOver");
        }
    }
}
