using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class UIQuestPanel : MonoBehaviour
    {
        [SerializeField] private VoidEventSO _questUpdateEvent = default;
        [SerializeField] private RectTransform _questListObject;
        [SerializeField] private UIQuest _questResource;

        private List<UIQuest> _questList = new List<UIQuest>();

        //private void OnEnable()
        //{
        //    _questUpdateEvent.Regist(UpdateQuest);
        //}
        //private void OnDisable()
        //{
        //    _questUpdateEvent.Unregist(UpdateQuest);
        //}

        //private void UpdateQuest()
        //{
        //    for (int i = _questList.Count - 1; i >= 0; i--)
        //    {
        //        Destroy(_questList[i].gameObject);
        //        _questList.RemoveAt(i);
        //    }

        //    float height = 0f;
        //    var questList = QuestManager.Instance.Quests;
        //    for (int i = 0; i < questList.Count; i++)
        //    {
        //        var questInfo = Instantiate(_questResource, _questListObject);
        //        questInfo.gameObject.SetActive(true);
        //        questInfo.SetQuestInfo(questList[i]);
        //        height += questInfo.Height;
        //        _questList.Add(questInfo);
        //    }
        //    _questListObject.sizeDelta = new Vector2(170f, height);
        //}
    }
}