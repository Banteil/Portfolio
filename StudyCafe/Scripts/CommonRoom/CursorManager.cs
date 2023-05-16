using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorManager : MonoBehaviour
{
    public Texture2D InteractionCursor;

    private void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        Vector2 cursorHotspot = new Vector2(InteractionCursor.width / 2, InteractionCursor.height / 2);
        Cursor.SetCursor(InteractionCursor, cursorHotspot, CursorMode.Auto);
    }

    private void OnMouseExit()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
