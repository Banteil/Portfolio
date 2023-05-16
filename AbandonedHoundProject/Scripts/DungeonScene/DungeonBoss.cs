using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonBoss : MonoBehaviour
{
    DungeonMonster dm;

    void Update()
    {
        transform.GetChild(0).LookAt(Camera.main.transform);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            Debug.Log("보스 조우");
            StartCoroutine(PlayerEncounter());
        }

        if (other.transform.CompareTag("MONSTER"))
        {
            dm = other.gameObject.GetComponent<DungeonMonster>();
            Invoke("DestroyEnemy", 1f);
        }
    }

    IEnumerator PlayerEncounter()
    {
        DungeonManager.Instance.dungeonMainUICanvas.SetActive(false);
        GetComponent<BoxCollider>().enabled = false;
        DungeonManager.Instance.moveCount++;        

        DataPassing.isStory = true;
        string name = SceneManager.GetActiveScene().name;
        DataPassing.getSceneName = name;
        DataPassing.storyKind = "Boss_" + DataPassing.stageNum + "_Encounter";
        SceneManager.LoadScene("StoryScene", LoadSceneMode.Additive); //인카운트 스토리 씬 로딩
        while (DataPassing.isStory) yield return null;

        transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
        DataPassing.isBoss = true;
        DungeonManager.Instance.BattleStart(false);
        while (DataPassing.isBattle) yield return null;
                
        if (!DataPassing.playerDie)
        {            
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
            DataPassing.isStory = true;
            name = SceneManager.GetActiveScene().name;
            DataPassing.getSceneName = name;
            DataPassing.storyKind = "Boss_" + DataPassing.stageNum + "_Defeat";
            SceneManager.LoadScene("StoryScene", LoadSceneMode.Additive); //적 퇴치 스토리 씬 로딩
            while (DataPassing.isStory) yield return null;

            if (DataPassing.stageNum.Equals(0) && DataPassing.tutorialEnd.Equals(true))
            {
                DataPassing.tutorialEnd = false;
                DataPassing.Reset();
                PlayerState.Instance.GetPotion = 1;
                PlayerState.Instance.GetFood = 1;
                PlayerState.Instance.GetBomb = 1;
                SceneManager.LoadScene("TownScene");                
                yield break;
            }

            DataPassing.isClear = true;
            DungeonManager.Instance.StartCoroutine(DungeonManager.Instance.AccountResult());
        }
    }

    void DestroyEnemy()
    {
        DungeonManager.Instance.DeleteEnemyToList(dm);
        Destroy(dm.gameObject);
        dm = null;
    }
}
