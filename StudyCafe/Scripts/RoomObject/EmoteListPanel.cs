using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmoteListPanel : MonoBehaviour
{
    public Sprite[] emoteSprites;
    public Transform emoteListContent; //이모트 아이템의 부모 트랜스폼    

    public void SettingEmoteList()
    {
        GameObject emoteItem = Resources.Load<GameObject>("Prefabs/UI/EmoteItem");
        //이모트 아이템 세팅
        for (int i = 0; i < emoteSprites.Length; i++)
        {
            GameObject emoteIcon = Instantiate(emoteItem, emoteListContent, false);
            EmoteItem emoteScript = emoteIcon.GetComponent<EmoteItem>();
            emoteScript.index = i;
            emoteScript.EmoteSprite = emoteSprites[i];
            emoteScript.displayEmote = RoomManager.Instance.myAvatar.GetComponent<AvatarAct>().StartDisplayEmote;
        }
    }
}
