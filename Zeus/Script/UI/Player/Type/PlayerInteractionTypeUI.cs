using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class PlayerInteractionTypeUI : PlayerUIType
    {
        public class InteractionTriggerInfo
        {
            public GameObject TargetTrigger;
            public Action CallEnter;
        }

        [SerializeField] private GameObject _promptObject;

        private List<InteractionTriggerInfo> _triggers = new List<InteractionTriggerInfo>();
        private bool _enabled;

        private void Awake()
        {
            UIType = TypePlayerUI.INTERACTION;
            _enabled = false;
        }
        protected override void Start()
        {
            base.Start();
            _canvas.alpha = 0f;
        }
        private void OnEnable()
        {
            InputReader.Instance.CallParry += Interaction;
        }
        private void Update()
        {
            if (_promptObject == null) return;

            if (_triggers.Count == 0)
            {
                if (_enabled)
                {
                    _enabled = false;
                    SetVisible(false, 0f);
                }
                return;
            }

            var target = _triggers[0].TargetTrigger;
            if (target == null || target.transform == null)
            {
                _triggers.RemoveAt(0);
                return;
            }

            _enabled = true;

            var targetPosition = target.transform.position;
            targetPosition.y += 1f;
            _promptObject.transform.SetPositionAndRotation(Camera.main.WorldToScreenPoint(targetPosition), _promptObject.transform.rotation);
        }

        public void OnEnter(GameObject target, Action interactionEnter)
        {
            if (_triggers.Count == 0) SetVisible(true, 0.5f);

            _triggers.Add(new InteractionTriggerInfo()
            {
                TargetTrigger = target,
                CallEnter = interactionEnter,
            });
        }
        public void OnLeave(GameObject target)
        {
            var triggerIndex = _triggers.FindIndex(x => x.TargetTrigger == target);
            if (triggerIndex == -1) return;
            _triggers.RemoveAt(triggerIndex);

            if (_triggers.Count == 0) SetVisible(false, 0f);
        }
        private void Interaction()
        {
            if (_triggers.Count == 0 || _promptObject == null) return;

            var trigger = _triggers[0];
            trigger.CallEnter?.Invoke();
            OnLeave(trigger.TargetTrigger);
        }
    }
}
