using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StageCharacterBox : MonoBehaviour, IDropHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform saveTr;
    [SerializeField]
    Image backgroundImage;
    [SerializeField]
    Image characterStandingImage;

    int characterID;
    public int CharacterID 
    { 
        get { return characterID; } 
        set 
        { 
            characterID = value;
            backgroundImage.sprite = ResourceManager.Instance.CharacterBoxBackgroundSprite[characterID];
            characterStandingImage.sprite = ResourceManager.Instance.CharacterBoxStandingSprite[characterID];
        } 
    }

    bool isDrag = false;

    void Awake()
    {
        saveTr = transform.parent;
        backgroundImage = GetComponent<Image>();
        characterStandingImage = transform.GetChild(0).GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDrag) return;

        Debug.Log(gameObject.name + "의 캐릭터 선택창 UI 표시");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDrag = true;
        backgroundImage.raycastTarget = false;
        transform.SetParent(LobbyStage.Instance.StageInfoPopup.PopupTr);
        transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        LobbyStage.Instance.StageInfoPopup.SaveBox = this;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(saveTr);
        transform.localPosition = Vector3.zero;
        LobbyStage.Instance.StageInfoPopup.SaveBox = null;
        backgroundImage.raycastTarget = true;
        isDrag = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        int tempID = characterID;
        CharacterID = LobbyStage.Instance.StageInfoPopup.SaveBox.characterID;
        LobbyStage.Instance.StageInfoPopup.SaveBox.CharacterID = tempID;

        for (int i = 0; i < GameManager.Instance.SelectCharacter.Length; i++)
        {
            GameManager.Instance.SelectCharacter[i] = LobbyStage.Instance.StageInfoPopup.CharacterBoxes[i].CharacterID;
        }
    }
}
