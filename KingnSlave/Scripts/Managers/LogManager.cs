using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public class LogManager : Singleton<LogManager>
    {
        private List<ActionLogData> _logDatas; 

        public bool UserInformationSettingsCompleted { get; set; } = false;

        protected override void OnAwake()
        {
            base.OnAwake();
            var textAsset = Resources.Load<TextAsset>("ActionLogData");
            if (textAsset != null)
            {
                _logDatas = Util.JsonToObject<List<ActionLogData>>(textAsset.text);
            }
            UserDataManager.Instance.CompleteSetMyDataCallback += () => UserInformationSettingsCompleted = true;
        }

        public async void InsertActionLog(int seq, string parameter = null)
        {
            await UniTask.WaitUntil(() => UserInformationSettingsCompleted);
            if (seq < 0 || seq >= _logDatas.Count)
            {
                Debug.LogError("Action Log Parmeter Range Error");
                return;
            }

            var sid = UserDataManager.Instance.MySid;
            var actionData = _logDatas[seq];
            CallAPI.APIInsertKpiLog(sid, actionData.actionNo, actionData.actionCd, parameter);
        }
    }
}
