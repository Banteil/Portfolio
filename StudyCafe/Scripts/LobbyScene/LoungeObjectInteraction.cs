using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LoungeObjectInteraction : MonoBehaviour
{
    /// <summary>
    /// 라운지 상호작용 오브젝트<br/>
    /// RECRUITMENT = 스터디 모집<br/>
    /// CREATE = 스터디 만들기<br/>
    /// ATTEND = 스터디 참가
    /// </summary>
    public enum LoungeObject { RECRUITMENT, CREATE, PARTICIPATE }
    public LoungeObject type;

    private void OnMouseUp()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        switch (type)
        {
            case LoungeObject.RECRUITMENT:
                {
                    LobbyManager.Instance.StudyRecruitment();
                }
                break;
            case LoungeObject.CREATE:
                {
                    LobbyManager.Instance.CreateStudy();
                }
                break;
            case LoungeObject.PARTICIPATE:
                {
                    LobbyManager.Instance.ParticipateStudy();
                }
                break;
        }
    }
}
