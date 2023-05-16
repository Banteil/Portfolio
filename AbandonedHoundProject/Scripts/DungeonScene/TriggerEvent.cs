using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerEvent : MonoBehaviour
{
    public int eventNum;

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            Debug.Log("트리거 이벤트");
            StartCoroutine(PlayerEncounter());
        }
    }

    IEnumerator PlayerEncounter()
    {
        DungeonManager.Instance.dungeonMainUICanvas.SetActive(false);
        GetComponent<BoxCollider>().enabled = false;
        DungeonManager.Instance.moveCount++;
        DungeonManager.Instance.currTileInfo[DungeonManager.Instance.y, DungeonManager.Instance.x].tile = TileType.NORMAL;

        DataPassing.isStory = true;
        string name = SceneManager.GetActiveScene().name;
        DataPassing.getSceneName = name;
        DataPassing.storyKind = "Stage_" + DataPassing.stageNum + "_" + eventNum;
        SceneManager.LoadScene("StoryScene", LoadSceneMode.Additive); //인카운트 스토리 씬 로딩
        while (DataPassing.isStory) yield return null;
        DungeonManager.Instance.dungeonMainUICanvas.SetActive(true);
    }

}
