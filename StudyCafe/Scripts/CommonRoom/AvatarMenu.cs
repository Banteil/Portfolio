using UnityEngine;

public class AvatarMenu : MonoBehaviour
{    
    public GameObject infoButton;
    public GameObject privateChatButton;

    public bool moveEnd; //iTween 연출이 종료되었는지 여부 체크용

    private void OnEnable()
    {
        iTween.MoveTo(infoButton, iTween.Hash("islocal", true, "x", 1f, "y", 0.6f, "time", 0.5f, "oncomplete", "DirectingEnd", "oncompletetarget", gameObject));
        iTween.MoveTo(privateChatButton, iTween.Hash("islocal", true, "x", 1f, "y", -0.6f, "time", 0.5f, "oncomplete", "DirectingEnd", "oncompletetarget", gameObject));
    }

    /// <summary>
    /// oncomplete 처리 시 moveEnd에 true값을 주는 함수
    /// </summary>
    void DirectingEnd() => moveEnd = true;

    /// <summary>
    /// 정보 초기화
    /// </summary>
    void Initialization()
    {
        infoButton.transform.localPosition = Vector3.zero;
        privateChatButton.transform.localPosition = Vector3.zero;
        moveEnd = false;
    }

    private void OnDisable()
    {
        Initialization();
    }
}
