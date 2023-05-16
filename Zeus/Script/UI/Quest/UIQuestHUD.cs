using DG.Tweening;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Zeus
{
	public class UIQuestHUD : MonoBehaviour
	{
		[SerializeField] private CanvasGroup _canvasGroup;
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

        private void Start()
        {
            _canvasGroup.alpha = 0f;
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
            StopAllCoroutines();
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
			_canvasGroup.DOFade(1, 2f).SetEase(Ease.OutCubic).onComplete += () => 
			{
                isDone = true;
			};
			yield return new WaitUntil(() => isDone);

			yield return new WaitForSeconds(_duration);

            isDone = false;
            _canvasGroup.DOFade(0, 2f).SetEase(Ease.InOutCubic).onComplete += () =>
            {
                isDone = true;
            };
            yield return new WaitUntil(() => isDone);

            _questInfos.Remove(questInfo);
            _isPlaying = false;
        }

        public void AddQuest(QuestSO quest)
        {
            var questName = TableManager.GetString(quest.NameID);
            var descriptionName = TableManager.GetString(quest.DescriptionID);
            Appear(questName, descriptionName);
        }
		public void Appear(string questName, string stepDescription)
		{
            var questInfo = new QuestInfoText();
            questInfo.Name = questName;
            questInfo.Description= stepDescription;
            _questInfos.Add(questInfo);
        }

		public void SetText(string questName, string stepDescription)
        {
            _nameText.text = questName;
            _stepDescriptionText.text = stepDescription;
        }

		//private int count = 0;
  //      private void OnGUI()
  //      {
  //          if (GUILayout.Button("Play"))
		//	{
  //              count++;
  //              Appear($"테스트 {count}번째", $"{count}번째 테스트");
  //          }
  //      }
    }
}