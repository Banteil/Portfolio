using System;
using UnityEngine;

namespace starinc.io
{
    public class VirtualController : BaseUI
    {
        #region Cache
        private enum VirtualMovePad
        {
            MovePad,
        }
        #endregion

        #region Callback
        public event Action<Vector2> OnMovePlayer;
        #endregion

        protected override void BindInitialization()
        {
            Bind<MovePad>(typeof(VirtualMovePad));
        }

        public void ActivateController()
        {
            var pad = Get<MovePad>((int)VirtualMovePad.MovePad);
            pad.IsActivated = true;
        }

        public void DeactivateController()
        {
            var pad = Get<MovePad>((int)VirtualMovePad.MovePad);
            pad.IsActivated = false;
            pad.Reset();
        }

        public void MoveHandle(Vector2 input)
        {
            OnMovePlayer?.Invoke(input);
        }
    }
}
