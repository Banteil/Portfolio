using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class GameTimeManager : UnitySingleton<GameTimeManager>
    {
        public float WorldTimeScale
        {
            get { return _timeScale; }
            private set
            {
                _timeScale = value;

                if (AnimatorSpeedManager.Get() != null)
                {
                    if (Time.timeScale == value)
                        AnimatorSpeedManager.Get().RevertSpeed();
                    else
                        AnimatorSpeedManager.Get().SetAllAnimatorSpeed(value);
                }
                if (EffectsManager.Get() != null)
                {
                    if (Time.timeScale == value)
                        EffectsManager.Get().RevertParticleSpeed();
                    else
                        EffectsManager.Get().SetParticlesSpeed(value);
                }
            }
        }
        internal bool IsPause => _pauseStartTime != 0f;
        internal float DeltaTime => Time.deltaTime * WorldTimeScale;
        internal float FixedDeltaTime => Time.fixedDeltaTime * WorldTimeScale;

        private List<ZeusTimer> timerUpdate = new List<ZeusTimer>();

        private static float _timeScale = 1f;
        private static float _pauseStartTime;
        private static float _startTime;
        private static float _duration;
        private static float _oldScale;

        private void Start()
        {
            DOTween.Init();
            Physics.autoSimulation = false;
        }

        internal void Pause(bool pause)
        {
            if (pause)
            {
                _oldScale = WorldTimeScale;
                _pauseStartTime = Time.realtimeSinceStartup;
                SetWorldTimeScale(0);
            }
            else
            {
                var elpasedTime = Time.realtimeSinceStartup - _pauseStartTime;
                var lifeTime = _duration - elpasedTime;
                if (lifeTime < 0)
                {
                    _oldScale = 1f;
                    lifeTime = 0;
                }
                _pauseStartTime = 0f;
                SetWorldTimeScale(_oldScale, lifeTime);
            }
        }

        internal void SetWorldTimeScale(float timeScale, float duration = 0)
        {
            WorldTimeScale = 1f;
            WorldTimeScale = timeScale;
            DOTween.timeScale = timeScale == 0 ? 0.00001f : timeScale;
            _startTime = Time.realtimeSinceStartup;
            _duration = duration;
        }

        internal void RevertTime()
        {
            WorldTimeScale = 1f;
            DOTween.timeScale = 1f;
            _startTime = 0f;
            _duration = 0f;
        }

        float prevStep;
        private void FixedUpdate()
        {
            if (timerUpdate.Count > 0)
            {
                for (int i = timerUpdate.Count - 1; i >= 0; --i)
                {
                    var item = timerUpdate[i];
                    if (item == null || item.RemainTime <= 0)
                    {
                        timerUpdate.RemoveAt(i);
                        continue;
                    }

                    item.Update(FixedDeltaTime);
                }
            }

            if (!Physics.autoSimulation)
            {
                Physics.Simulate(FixedDeltaTime);
            }

            if (_duration <= 0)
                return;

            var remainTime = _duration - (Time.realtimeSinceStartup - _startTime);
            if (remainTime <= 0)
            {
                RevertTime();
            }
        }

        #region CoolTime
        internal void AddTimer(ZeusTimer zeusTimer)
        {
            timerUpdate.Add(zeusTimer);
        }

        internal void ClearCoolTimeUI()
        {
            foreach (var item in timerUpdate)
            {
                var cooltimer = item as CoolTimer;
                if (cooltimer == null)
                    continue;

                if (cooltimer.UpdateUI == null || cooltimer.UpdateUI.skillType == TypeSkillUI.RUNE)
                    continue;

                cooltimer.UpdateUI = null;
            }
        }

        internal void ClearCoolTime()
        {
            timerUpdate.Clear();
        }

        internal ZeusTimer GetCoolTime(int id)
        {
            var data = timerUpdate.Find(_ => _.ID == id);
            if (data == null)
                return null;

            return data;
        }
        #endregion
    }
}
