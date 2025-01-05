using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public class GameResultWindow : MonoBehaviour
    {
        [SerializeField]
        private GameObject practiceModeContents;

        [SerializeField]
        private GameObject singleModeContents;
        [SerializeField]
        private GameObject singleModeNextStageButton;
        [SerializeField]
        private GameObject singleModeRestartButton;
        [SerializeField]
        private GameObject singleModeRewardUI;
        [SerializeField]
        private TMP_Text singleModeRewardText;

        [SerializeField]
        private GameObject normalModeContents;

        [SerializeField]
        private GameObject rankModeContents;

        private Animator gameResultAnimator;

        private void Awake()
        {
            gameResultAnimator = GetComponent<Animator>();
        }

        private void Start()
        {
            InGameManager.Instance.GameOver.AddPersistentListener(PopUp);
            transform.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            Close();
        }

        public void PopUp(Define.GameResult gameResult)
        {
            UIManager.Instance.CloseAllUI();
            AudioManager.Instance.StopMusic(0.1f);
            if (gameObject.activeInHierarchy) { return; }

            gameObject.SetActive(true);
            if (gameResult == Define.GameResult.Victory)
            {
                AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.Victory));
                gameResultAnimator.SetTrigger("Victory");
            }
            else
            {
                AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.Defeat));
                gameResultAnimator.SetTrigger("Defeat");
            }

            switch (GameManager.Instance.CurrentGameMode)
            {
                case Define.GamePlayMode.PVPNormal:
                    UserDataManager.Instance.UpdateMyNormalGameData();
                    normalModeContents.SetActive(true);
                    break;

                case Define.GamePlayMode.PVPRank:
                    UserDataManager.Instance.UpdateMyRankGameData();
                    rankModeContents.SetActive(true);
                    break;

                case Define.GamePlayMode.SingleStory:

                    if (gameResult == Define.GameResult.Victory)
                    {
                        if (GameManager.Instance.ChallengingStageInCycle == Define.MIDDLE_BOSS_STAGE)
                        {
                            // 중간 보스
                            ShowSingleModeRewardUI(Define.MIDDLE_BOSS_REWARD_GEM);
                        }
                        else if (GameManager.Instance.ChallengingStageInCycle == Define.FINAL_BOSS_STAGE)
                        {
                            // 최종 보스
                            ShowSingleModeRewardUI(Define.FINAL_BOSS_REWARD_GEM);
                        }

                        // 다음 스테이지로 이동 버튼 활성화
                        singleModeNextStageButton.SetActive(true);
                    }
                    else
                    {
                        // 스테이지 재도전 버튼 활성화
                        singleModeRestartButton.SetActive(true);
                    }

                    singleModeContents.SetActive(true);
                    break;

                default:
                    practiceModeContents.SetActive(true);
                    break;
            }

            GameManager.Instance.CurrentGameMode = Define.GamePlayMode.None;
        }

        public void Close()
        {
            gameObject.SetActive(false);
            practiceModeContents.SetActive(false);
            singleModeContents.SetActive(false);
            singleModeRewardUI.SetActive(false);
            singleModeNextStageButton.SetActive(false);
            singleModeRestartButton.SetActive(false);
            normalModeContents.SetActive(false);
            rankModeContents.SetActive(false);
        }

        private void ShowSingleModeRewardUI(int gem)
        {
            singleModeRewardText.text = $"+ {gem}  <sprite=\"Jewel\" index=0>";
            singleModeRewardText.transform.parent.gameObject.SetActive(true);
            singleModeRewardUI.SetActive(true);
        }
    }
}