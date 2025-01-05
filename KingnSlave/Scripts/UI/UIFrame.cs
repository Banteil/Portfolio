using System.Threading;
using Cysharp.Threading.Tasks;

namespace starinc.io.kingnslave
{
    public class UIFrame : UIBase
    {
        public bool IsSetData { get; set; }

        protected bool completeSettingData;
        private CancellationTokenSource processCancel = new CancellationTokenSource();

        protected override void InitializedProcess() { }

        public void SetData<T>(T data) where T : class
        {
            completeSettingData = false;
            SetDataProcess(data);
            completeSettingData = true;
        }

        protected virtual void SetDataProcess<T>(T data) where T : class { }

        public virtual void ActiveFrame(bool isActive)
        {
            if (!isActive) processCancel.Cancel();
            ActiveFrameProcess(isActive, cancellationToken: processCancel.Token);
        }

        async protected virtual void ActiveFrameProcess(bool isActive, CancellationToken cancellationToken)
        {
            if (isActive)
            {
                await UniTask.WaitUntil(() => completeSettingData);
                if (cancellationToken.IsCancellationRequested)
                {
                    processCancel = new CancellationTokenSource();
                    return;
                }
                gameObject.SetActive(true);
            }
            else
            {
                processCancel = new CancellationTokenSource();
                gameObject.SetActive(false);                
            }
        }
    }
}
