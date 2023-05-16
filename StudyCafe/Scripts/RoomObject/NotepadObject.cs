using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotepadObject : MonoBehaviour
{
    private void OnMouseUp()
    {
        AvatarAct interactAvatar = RoomManager.Instance.myAvatar.GetComponent<AvatarAct>();
        if (interactAvatar.state.Equals(AvatarAct.AvatarState.SIT))
        {
            if (RoomManager.Instance.chatRoom != null)
                RoomManager.Instance.chatRoom.OnMemoNotepad();
        }
    }
}
