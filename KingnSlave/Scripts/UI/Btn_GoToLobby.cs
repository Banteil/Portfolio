using starinc.io;
using starinc.io.kingnslave;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Btn_GoToLobby : MonoBehaviour, IPointerClickHandler
{
    /// <summary>
    /// 버튼 클릭 시 로비 씬을 로딩하는 메서드
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));
        //GameManager.Instance.LoadScene(Define.LobbySceneName);
        GameManager.Instance.ClearGame();
        SceneManager.LoadScene(Define.LobbySceneName);
    }
}