using System;

namespace starinc.io
{
    public class EnemyController : CreatureController
    {
        #region Cache
        public CreatureController Target { get; set; }
        public string State { get; set; } = "Idle";
        #endregion

        #region Callback
        public Action OnActionProcess;
        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
            Manager.Game.GetCurrentMinigameData().OnEndGame += EndGame;
        }

        private void Update()
        {
            OnActionProcess?.Invoke();
        }

        protected virtual void EndGame()
        {
            OnActionProcess = null;
        }
    }
}
