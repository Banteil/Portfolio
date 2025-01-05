using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField]
        private Define.UserDataType scoreUIMaster;

        [SerializeField]
        private Image scoreBar;

        [SerializeField]
        private GameObject verticalLineContainter;

        [SerializeField]
        private GameObject verticalLinePrefab;

        [SerializeField]
        private TMP_Text scoreText;

        private UnityAction updateScore;

        private void Awake()
        {
            float scoreBarWidth = GetComponent<RectTransform>().rect.width;
            Debug.Log("width: " + scoreBarWidth);
            for (int i = 1; i < Define.DEFAULT_WIN_CONDITION_SCORE; i++)
            {
                RectTransform line = Instantiate(verticalLinePrefab, verticalLineContainter.transform).GetComponent<RectTransform>();
                float minX = 5f;
                float maxX = scoreBarWidth - minX;
                line.anchoredPosition = new Vector2((maxX - minX) / Define.DEFAULT_WIN_CONDITION_SCORE * i + minX, 0);
                line.name = $"{verticalLinePrefab.name}[{i}]";
            }
            scoreBar.fillAmount = 0f;
            updateScore = (scoreUIMaster == Define.UserDataType.You) ?
                (GameManager.Instance.CurrentGameMode == Define.GamePlayMode.SingleStory ? SingleGameUpdateYourScore : UpdateYourScore) :
                (GameManager.Instance.CurrentGameMode == Define.GamePlayMode.SingleStory ? SingleGameUpdateOpponentScore : UpdateOpponentScore);
        }

        private void Start()
        {
            if (GameManager.Instance.CurrentGameMode == Define.GamePlayMode.SingleStory)
                scoreText.text = $"0 / {InGameManager.Instance.WinConditionScore}";

            InGameManager.Instance.EndPhaseStart.AddPersistentListener(updateScore);
        }

        private void SingleGameUpdateYourScore()
        {
            scoreBar.fillAmount = (float)InGameManager.Instance.GetYourScore() / InGameManager.Instance.WinConditionScore;
            scoreText.text = Mathf.Clamp(InGameManager.Instance.GetYourScore(), 0, InGameManager.Instance.WinConditionScore).ToString() + $" / {InGameManager.Instance.WinConditionScore}";
        }

        private void SingleGameUpdateOpponentScore()
        {
            scoreBar.fillAmount = (float)InGameManager.Instance.GetOpponentScore() / InGameManager.Instance.WinConditionScore;
            scoreText.text = Mathf.Clamp(InGameManager.Instance.GetOpponentScore(), 0, InGameManager.Instance.WinConditionScore).ToString() + $" / {InGameManager.Instance.WinConditionScore}";
        }

        private void UpdateYourScore()
        {
            scoreBar.fillAmount = (float)InGameManager.Instance.GetYourScore() / InGameManager.Instance.WinConditionScore;
            scoreText.text = Mathf.Clamp(InGameManager.Instance.GetYourScore(), 0, InGameManager.Instance.WinConditionScore).ToString();
        }

        private void UpdateOpponentScore()
        {
            scoreBar.fillAmount = (float)InGameManager.Instance.GetOpponentScore() / InGameManager.Instance.WinConditionScore;
            scoreText.text = Mathf.Clamp(InGameManager.Instance.GetOpponentScore(), 0, InGameManager.Instance.WinConditionScore).ToString();
        }
    }
}