using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrinkObject : MonoBehaviour
{
    public AudioClip clip;

    private void OnMouseUp()
    {
        AvatarAct interactAvatar = RoomManager.Instance.myAvatar.GetComponent<AvatarAct>();
        if (interactAvatar.state.Equals(AvatarAct.AvatarState.SIT))
        {
            interactAvatar.avatarSound.clip = clip;
            interactAvatar.avatarSound.Play();
        }
    }
}
