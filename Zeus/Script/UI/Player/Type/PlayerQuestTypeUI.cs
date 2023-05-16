using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Zeus
{
    public class PlayerQuestTypeUI : PlayerUIType
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _stepDescriptionText;
        [SerializeField] private float _duration;

        public struct QuestInfoText
        {
            public string Name;
            public string Description;
        }

        private bool _isPlaying;
        private List<QuestInfoText> _questInfos = new List<QuestInfoText>();

        private void Awake()
        {
            UIType = TypePlayerUI.QUEST;
        }
        protected override void Start()
        {
            base.Start();
            _canvas.alpha = 0f;
            _isPlaying = false;
        }
        private void OnEnable()
        {
            QuestManager.Instance.CallAddQuest += AddQuest;
        }
        private void OnDisable()
        {
            if (QuestManager.HasInstance)
                QuestManager.Instance.CallAddQuest -= AddQuest;
        }
        private void Update()
        {
            if (_questInfos.Count == 0) return;
            if (_isPlaying) return;

            StartCoroutine(AppearCO(_questInfos[0]));
        }
        private IEnumerator AppearCO(QuestInfoText questInfo)
        {
            _isPlaying = true;

            SetText(questInfo.Name, questInfo.Description);

            bool isDone = false;
            _canvas.DOFade(1, 2f).SetEase(Ease.OutCubic).onComplete += () =>
            {
                isDone = true;
            };
            yield return new WaitUntil(() => isDone);

            yield return new WaitForSeconds(_duration);

            isDone = false;
            _canvas.DOFade(0, 2f).SetEase(Ease.InOutCubic).onComplete += () =>
            {
                isDone = true;
            };
            yield return new WaitUntil(() => isDone);

            _questInfos.Remove(questInfo);
            _isPlaying = false;
        }

        public void SetVisible(bool visiabled, Ease ease, TweenCallback onComplete = null)
        {
            _canvas.DOFade(visiabled ? 1 : 0, _duration).SetEase(ease).onComplete = onComplete;
        }

        public void AddQuest(QuestSO quest)
        {
            var questName = TableManager.GetString(quest.NameID);
            var description = TableManager.GetString(quest.DescriptionID);

            var questInfo = new QuestInfoText();
            questInfo.Name = questName;
            questInfo.Description = description;
            _questInfos.Add(questInfo);
        }

        public void SetText(string questName, string stepDescription)
        {
            _nameText.text = questName;
            _stepDescriptionText.text = stepDescription;
        }
    }
}