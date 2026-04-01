using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapObject : MonoBehaviour
{
    SpriteRenderer sR;
    SpriteOutline outline;
    SpriteRenderer xMark;

    [SerializeField]
    int worldIndex;
    [SerializeField]
    string worldName;
    [SerializeField]
    int bossID;
    [SerializeField]
    bool isOpen;

    void Awake()
    {
        sR = GetComponent<SpriteRenderer>();
        outline = GetComponent<SpriteOutline>();
        xMark = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
        if (hit.collider != null)
        {
            Debug.Log(hit.collider.gameObject.name);
        }
    }

    private void OnMouseDown()
    {
        if (LobbyStage.Instance.SelectPopup.gameObject.activeSelf) return;

        if (isOpen)
            outline.enabled = true;
        else
        {
            sR.color = Color.gray;
            xMark.enabled = true;
        }
    }

    private void OnMouseUp()
    {
        if (isOpen)
            outline.enabled = false;
        else
        {
            sR.color = Color.white;
            xMark.enabled = false;
        }
    }

    private void OnMouseUpAsButton()
    {
        if (LobbyStage.Instance.SelectPopup.gameObject.activeSelf) return;

        if (isOpen && LobbyStage.Instance.enabled)
        {
            LobbyStage.Instance.OpenStageList(worldIndex, worldName, bossID);
            outline.enabled = false;
        }
    }
}
