using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Zeus
{
    public class InteractionActor : InteractionBase
    {
        [SerializeField] private float _interval = 0.2f;
        [SerializeField] private TagMask _tag;

        private List<Transform> _contactTargets = new List<Transform>();
        private List<InteractionTrigger> _contactTriggers = new List<InteractionTrigger>();

        private void OnEnable()
        {
            StartCoroutine(TriggerRecognizeCO());
        }
        private void OnDisable()
        {
            StopAllCoroutines();
        }

        // TODO : trigger 접촉이 빈번하게 일어남. 그럴때마다 GetComponents를 수행? 
        private void OnTriggerEnter(Collider other)
        {
            var triggers = other.GetComponents<InteractionTrigger>();
            for (int i = 0; i < triggers.Length; i++)
            {
                var trigger = triggers[i];
                if (!_tag.Contains(trigger.tag)) continue;

                trigger.OnContact();

                if (_contactTriggers.Contains(trigger)) continue;

                _contactTriggers.Add(trigger);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            var triggers = other.GetComponents<InteractionTrigger>();
            for (int i = 0; i < triggers.Length; i++)
            {
                var trigger = triggers[i];
                if (!_tag.Contains(trigger.tag)) continue;

                trigger.OnLeave();

                if (!_contactTriggers.Contains(trigger)) continue;

                _contactTriggers.Remove(trigger);
            }
        }

        private IEnumerator TriggerRecognizeCO()
        {
            var actorTransform = transform;
            while (true)
            {
                yield return new WaitWhile(() => _contactTriggers.Count == 0);

                var actorPosition = actorTransform.position;

                for (int i = _contactTriggers.Count - 1; i > -1; i--)
                {
                    var trigger = _contactTriggers[i];
                    var isRecognized = trigger.IsRecognized(actorTransform, IsInSight(trigger.transform));

                    if (!isRecognized) continue;
                    
                    Color color = Color.red;
                    bool isHit = false;

                    var actorToTarget = trigger.transform.position - actorPosition;
                    var hits = Physics.RaycastAll(actorPosition, actorToTarget.normalized, actorToTarget.magnitude);
                    foreach (var hit in hits)
                    {
                        if (hit.transform == trigger.transform)
                        {
                            isHit = true;
                            break;
                        }
                    }

                    color = isHit ? Color.red : Color.blue;
                    Debug.DrawRay(actorPosition, actorToTarget.normalized * actorToTarget.magnitude, color, _interval + 0.01f);

                    if (isHit)
                    {
                        trigger.OnEnter(this);
                        _contactTriggers.Remove(trigger);
                    }

                    //Color color = Color.blue;
                    //if (Physics.Raycast(_target.position, actorToTarget, out var hit, actorToTarget.magnitude))
                    //{
                    //    // 충돌확인
                    //    if (hit.transform == trigger.transform)
                    //    {
                    //        var isRecognized = trigger.IsRecognized(_target, IsInSight(trigger.Target));
                    //        Debug.Log($"{trigger.Target.name} {isRecognized}");
                    //        color = Color.red;
                    //    }
                    //}

                    //var isRecognized = trigger.IsRecognized(_target, IsInSight(trigger.Target));
                    //Debug.Log($"{trigger.Target.name} {isRecognized}");
                }

                yield return new WaitForSeconds(_interval);
            }
        }
    }
}