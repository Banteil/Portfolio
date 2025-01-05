using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace starinc.io.kingnslave
{
    public class Btn_RestartStage : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// 싱글 모드 스테이지 재시작 버튼
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));
            GameManager.Instance.CurrentGameMode = Define.GamePlayMode.SingleStory; 
            SceneManager.LoadScene(Define.SingleGameSceneName);
        }
    }
}