using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static starinc.io.Define;

namespace starinc.io.kingnslave
{
    public class Btn_PlayAgain : MonoBehaviour, IPointerClickHandler
    {
        public SingleFirstTurnMode singleFirstTurnMode;

        /// <summary>
        /// 연습 모드 다시하기 버튼
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));
            GameManager.Instance.SingleMode = singleFirstTurnMode == SingleFirstTurnMode.King ? SingleFirstTurnMode.King : SingleFirstTurnMode.Slave;
            GameManager.Instance.CurrentGameMode = GamePlayMode.Practice;
            SceneManager.LoadScene(SingleGameSceneName);
            //GameManager.Instance.LoadScene(SingleGameSceneName);
        }
    }
}