using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomType { SOLITARY1, SOLITARY2, SOLITARY3, OPENSTUDY }

public class RoomObject : MonoBehaviour
{
    public RoomType roomType;
    public Transform topPos, bottomPos, leftPos, rightPos, spawnPos;
    public GameObject slideScreen;
    public Canvas slideCanvas;
    public GameObject readyText;
    public BoxCollider2D backgroundCollider;

    [Header("TableObjects")]
    public TableObject teacherTable;
    public TableObject[] studentTable;
    public TableObject myTable;

    public IEnumerator ShowReadyText()
    {
        readyText.SetActive(true);
        float timer = 0f;
        while(timer < 2f)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        readyText.SetActive(false);
    }

    public Vector3 GetRoomBoundsMin() => backgroundCollider.bounds.min;
    public Vector3 GetRoomBoundsMax() => backgroundCollider.bounds.max;
}
