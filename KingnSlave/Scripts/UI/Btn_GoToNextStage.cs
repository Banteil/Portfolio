using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io.kingnslave
{
    public class Btn_GoToNextStage : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// 싱글 모드 다음 스테이지로 이동 버튼
        /// </summary>
        /// <param name="eventData"></param>
        public async void OnPointerClick(PointerEventData eventData)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));
            GameManager.Instance.CurrentGameMode = Define.GamePlayMode.SingleStory;
            var direction = ScreenTransitionManager.Instance.ShowDirection<SingleSceneLoadDirection_SingleStory>();
            await direction.StartDirection();

            //// 내 카드 스킨 설정
            //await NetworkManager.Instance.SetMyCardSkin();

            GameManager.Instance.LoadScene(Define.SingleGameSceneName);
        }
    }
}