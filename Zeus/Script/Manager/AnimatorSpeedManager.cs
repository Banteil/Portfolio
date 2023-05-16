using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Zeus
{
    public class AnimatorSpeedManager : BaseObject<AnimatorSpeedManager>
    {
        struct AnimatorSpeedControllData
        {
            public Animator ControlAnimator;
            public float Duration;
            public float RemainTime;
            public float OldSpeed;
            public float NewSpeed;
            public int ID;
        }

        internal bool IsInitializedComplete { get; private set; }

        //한 에니메이터에 여러개의 속도가 걸릴수있다.
        //예 ) 슬로우 걸린 몬스터가 피격 당한경우. 피격속도가 끝나면 슬로우로 돌려줄지 슬로우가 끝나있는경우 슬로우 끝났을때의 속도로 돌려야한다.
        private List<AnimatorSpeedControllData> _animatorSpeedControllDatas;
        private Dictionary<Animator, List<AnimatorSpeedControllData>> _oldSpeeds;
        private List<Animator> _animators;
        private float _infinitySpeed;

        // Start is called before the first frame update
        void Start()
        {
            Application.targetFrameRate = 60;
            _animatorSpeedControllDatas ??= new List<AnimatorSpeedControllData>();
            _oldSpeeds ??= new Dictionary<Animator, List<AnimatorSpeedControllData>>();
            _animators ??= new List<Animator>();
            SceneManager.sceneLoaded += SceneLoadComplete;
            RefreshAnimator();
        }

        private void SceneLoadComplete(Scene loadScene, LoadSceneMode loadMode)
        {
            RefreshAnimator();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (_animatorSpeedControllDatas != null)
            {
                for (int i = _animatorSpeedControllDatas.Count - 1; i >= 0; --i)
                {
                    var data = _animatorSpeedControllDatas[i];
                    if (data.ControlAnimator == null)
                    {
                        _animatorSpeedControllDatas.RemoveAt(i);
                        continue;
                    }

                    if (data.Duration < 0)
                        continue;

                    data.RemainTime -= Time.deltaTime;
                    _animatorSpeedControllDatas[i] = data;
                    if (data.RemainTime <= 0)
                    {
                        ChangeAnimatorSpeed(data);
                        _animatorSpeedControllDatas.RemoveAt(i);
                    }

                }
            }
        }

        internal void RefreshAnimator()
        {
            IsInitializedComplete = false;
            var anims = FindObjectsOfType<Animator>();
            _animators.Clear();
            foreach (var anim in anims)
            {
                //Debug.Log(anim.name);
                _animators.Add(anim);
            }
            IsInitializedComplete = true;
        }

        internal void AddAnimator(Animator animator)
        {
            if (_animators.Contains(animator))
            {
                Debug.LogWarning("Already animator add!");
                return;
            }

            _animators.Add(animator);

            if (_infinitySpeed != 0f)
                SetAnimatorSpeed(animator, _infinitySpeed, -1f);
        }

        internal void ReleaseAnimator(Animator animator)
        {
            if (!_animators.Contains(animator))
            {
                Debug.LogError("Not Found animator");
                return;
            }

            _animators.Remove(animator);
        }

        internal void SetAllAnimatorSpeed(float destSpeed, float duration = -1f)
        {
            if (duration == 0f)
                return;

            if (duration < 0f)
                _infinitySpeed = destSpeed;

            foreach (var item in _animators)
            {
                SetAnimatorSpeed(item, destSpeed, duration);
            }
        }

        internal void RevertSpeed()
        {
            //Debug.Log("RevertSpeed");

            _infinitySpeed = 0f;

            for (int i = _animatorSpeedControllDatas.Count - 1; i >= 0; --i)
            {
                var data = _animatorSpeedControllDatas[i];
                if (data.ControlAnimator == null)
                {
                    _animatorSpeedControllDatas.RemoveAt(i);
                    continue;
                }

                if (data.Duration < 0)
                {
                    data.Duration = 0f;
                }

                _animatorSpeedControllDatas[i] = data;
            }
        }

        internal void SetAnimatorSpeed(Animator controlAnimator, float destSpeed, float duration)
        {
            if (controlAnimator == null)
                return;

            //Debug.Log($"{controlAnimator.gameObject.name} Speed Control!");
            var data = new AnimatorSpeedControllData
            {
                ControlAnimator = controlAnimator,
                OldSpeed = controlAnimator.speed,
                Duration = duration,
                RemainTime = duration,
                NewSpeed = destSpeed,
            };


            if (_oldSpeeds.ContainsKey(controlAnimator))
            {
                var list = _oldSpeeds[controlAnimator];
                //있던넘이면 전에 들어와있던녀석으로 올드 스피드를 맞춰주고. 현재속도는 제일 느린걸로 맞춘다.
                data.OldSpeed = list[^1].OldSpeed;
                data.ID = list[^1].ID + 1;
                list.Add(data);

                var speedArray = list.OrderBy(x => x.NewSpeed).ToArray();
                controlAnimator.speed = speedArray[0].NewSpeed;
                //Debug.Log($"2 name : {data.ControlAnimator.name} / data.ControlAnimator.speed :  {data.ControlAnimator.speed} / data.OldSpeed : {data.OldSpeed} / data ID {data.ID}");
            }
            else
            {
                //새로들어온 놈이면 그 속도로 맞춰준다.
                var list = new List<AnimatorSpeedControllData>();
                data.ID = 1;
                list.Add(data);
                _oldSpeeds.Add(controlAnimator, list);
                controlAnimator.speed *= destSpeed;

                //Debug.Log($"1 name : {data.ControlAnimator.name} / data.ControlAnimator.speed :  {data.ControlAnimator.speed} / data.OldSpeed : {data.OldSpeed} / data ID {data.ID}");
            }

            _animatorSpeedControllDatas ??= new List<AnimatorSpeedControllData>();
            _animatorSpeedControllDatas.Add(data);
        }

        private void ChangeAnimatorSpeed(AnimatorSpeedControllData data)
        {
            if (!_oldSpeeds.ContainsKey(data.ControlAnimator))
            {
                Debug.LogError($"Not Found animator Name : {data.ControlAnimator.name}");
                return;
            }

            var list = _oldSpeeds[data.ControlAnimator];
            for (int i = list.Count - 1; i >= 0; --i)
            {
                var item = list[i];
                if (item.ID == data.ID)
                {
                    //Debug.Log($"{data.ControlAnimator.name} Delete. data.Id : {data.ID} / list.count : {list.Count}");
                    list.RemoveAt(i);
                    break;
                }
            }

            if (list.Count == 0)
            {
                //걸린게 없으면 원래 속도로 돌려준다.
                _oldSpeeds.Remove(data.ControlAnimator);
                data.ControlAnimator.speed = data.OldSpeed;

                //Debug.Log($" 111 ChangeAnimatorSpeed Complete / Animator.speed Old: {data.ControlAnimator.speed}");
            }
            else
            {
                //가장 느린속도로 속도를 맞춰준다.
                var speedArray = list.OrderBy(x => x.NewSpeed).ToArray();
                data.ControlAnimator.speed = speedArray[0].NewSpeed;

                //Debug.Log($" 222 ChangeAnimatorSpeed Complete / Animator.speed : {data.ControlAnimator.speed}");
            }
        }

        //enemy = 9, player = 8,
        internal void SetAnimatorSpeedByTag(string tag, float speed, float duration)
        {
            for (int i = _animators.Count - 1; i >= 0; --i)
            {
                var item = _animators[i];
                if (item == null)
                {
                    _animators.RemoveAt(i);
                    continue;
                }

                if (!item.gameObject.CompareTag(tag))
                    continue;

                SetAnimatorSpeed(item, speed, duration);
            }
        }
    }
}