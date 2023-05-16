using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Zeus
{
    public class UIQuest : MonoBehaviour
    {
        [SerializeField] private RectTransform _transform;
        [SerializeField] private RectTransform _stepListObject;
        [SerializeField] private TMP_Text _questNameText;
        [SerializeField] private TMP_Text _textResource;

        private List<TMP_Text> _textList = new List<TMP_Text>();

        public float StepHeight => _textList.Count * 20f;
        public float Height => StepHeight + 20f;

        //public void SetQuestInfo(QuestProgress quest)
        //{
        //    for (int i = _textList.Count - 1; i >= 0; i--)
        //    {
        //        Destroy(_textList[i].gameObject);
        //        _textList.RemoveAt(i);
        //    }

        //    _questNameText.text = TableManager.GetString(quest.Quest.NameID);

        //    var stepList = quest.Steps;
        //    for (int i = 0; i < stepList.Count; i++)
        //    {
        //        var step = stepList[i];
        //        var stepInfo = Instantiate(_textResource, _stepListObject);
        //        var stepTransform = stepInfo.GetComponent<RectTransform>();
        //        var stepPos = stepTransform.anchoredPosition;
        //        stepPos.y = -20 * i;
        //        stepTransform.anchoredPosition = stepPos;
        //        stepInfo.gameObject.SetActive(true);
        //        stepInfo.color = step.IsDone ? Color.green : Color.white;
        //        var descriptionText = TableManager.GetString(stepList[i].Step.DescriptionID);
        //        var stepInfoText = string.Empty;
                
        //        if (stepList[i].Step is QuestStep_TalkSO talkStep)
        //        {
        //            stepInfoText = string.Format(descriptionText, talkStep.TargetActorId);
        //        }

        //        stepInfo.text = stepInfoText;
        //        _textList.Add(stepInfo);
        //    }
        //    _stepListObject.sizeDelta = new Vector2(0f, StepHeight);
        //    _transform.sizeDelta = new Vector2(170f, Height);
        //}
    }
}