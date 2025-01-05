using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class Btn_GetAdditionalReward : MonoBehaviour, IPointerClickHandler
    {
        private const int MAX_TRY_COUNT = 5;
        private int rewardGem;
        private int gemLogType;

        /// <summary>
        /// 광고를 통해 추가 보상 받기 버튼
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"GetAdditionalReward button clicked [Stage {GameManager.Instance.ChallengingStageIndex}={UserDataManager.Instance.MyData.single_stage}]");

            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));
            if (GameManager.Instance.ChallengingStageInCycle == Define.FINAL_BOSS_STAGE)
            {
                rewardGem = Define.FINAL_BOSS_REWARD_GEM;
                gemLogType = (int)Define.GemLogType.BossAddition;
            }
            else if (GameManager.Instance.ChallengingStageInCycle == Define.MIDDLE_BOSS_STAGE)
            {
                rewardGem = Define.MIDDLE_BOSS_REWARD_GEM;
                gemLogType = (int)Define.GemLogType.MiddleBossAddition;
            }
            else
            {
                return;
            }

            //if (AdMediationManager.Instance.IntervalTimer > 0)
            //{
            //    UIManager.Instance.ShowWarningUI("adIntervalText");
            //    return;
            //}

            AdMediationManager.Instance.ShowRewardedAd(() =>
            {
                AdRewardProcess();
            },
            () =>
            {
                AdRewardProcess();
            });
        }

        async private void AdRewardProcess()
        {
            int count = 0;
            do
            {
                Debug.Log($"Single AD try count: {count}");
                bool success = false;
                await UniTask.Yield();
                await CallAPI.APIUpdateUserGemAmount(UserDataManager.Instance.MySid, rewardGem, gemLogType, (data) =>
                {
                    Debug.Log($"Single AD API Called. data: {data}, gem: {data.gem_amount}");
                    if (data != null)
                    {
                        UserDataManager.Instance.MyGem = data.gem_amount;
                        success = true;
                    }
                });

                if (success)
                {
                    //AdMediationManager.Instance.CountingInterval();
                    var image = GetComponent<Image>();
                    image.raycastTarget = false;
                    image.color = Color.gray;
                    UIManager.Instance.ShowWarningUI(Define.AD_REWARD_RECIEVED_KEY, true);
                    return;
                }
                count++;
            }
            while (count < MAX_TRY_COUNT);
            UIManager.Instance.ShowWarningUI("Failure to obtain Ad rewards", false);
        }
    }
}