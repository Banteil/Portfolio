using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCBasic : CharacterBasic
{
    protected Action<CharacterBasic, List<Item>> itemGiveAction;
    protected int conversationOrder = 0;
    protected Coroutine talkRoutine;

    public override void HitHandling(float damage, int knockbackPower, Vector3 attackerPos) 
    {
        EndTalk();
        if (state != CharacterState.DIE)
        {
            if (target != null)
                state = CharacterState.TRACKING;

            Vector3 hitbackDir = (transform.position - attackerPos).normalized;
            rB.velocity = Vector2.zero;
            rB.AddForce(hitbackDir * knockbackPower);
            if (target.Equals(GameManager.Instance.Player.transform))
                DisplayEnemyHp();
            else
            {
                if (enemyHpUI != null)
                {
                    Destroy(enemyHpUI.gameObject);
                    enemyHpUI = null;
                }
            }
        }
    }

    protected void TalkNPC(string npcName, int switchNum)
    {        
        GameManager.Instance.IsDirecting = true;
        List<string> contentList = ResourceManager.Instance.GetConversation(npcName, switchNum);
        if (contentList.Count.Equals(0))
        {
            GameManager.Instance.IsDirecting = false;
            return;
        }
        talkRoutine = StartCoroutine(TalkingProcess(npcName, contentList));
    }


    IEnumerator DirectingKeyInputCheck()
    {
        yield return new WaitForSeconds(0.1f);
        while (GameManager.Instance.IsDirecting)
        {
            if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.F))
                conversationOrder++;
            yield return null;
        }
    }


    IEnumerator TalkingProcess(string npcName, List<string> content)
    {
        UIManager.Instance.GetUI("TextBox").gameObject.SetActive(true);
        conversationOrder = 0;
        UIManager.Instance.GetUI("TextBox").GetComponent<TextBox>().SetConversation(npcName, content[conversationOrder]);

        StartCoroutine(DirectingKeyInputCheck());
        while (conversationOrder < content.Count)
        {
            UIManager.Instance.GetUI("TextBox").GetComponent<TextBox>().SetConversation(npcName, content[conversationOrder]);
            yield return null;
        }
        EndTalk();
    }

    protected void EndTalk()
    {
        if(talkRoutine != null)
        {
            StopCoroutine(talkRoutine);
            talkRoutine = null;
        }
        UIManager.Instance.GetUI("TextBox").SetActive(false);
        GameManager.Instance.IsDirecting = false;
        QuestManager.Instance.QuestObserve(ConditionType.TALK, characterID, objectID);
    }


    public override void ObjectDetection(GameObject detectionObj) { }
    public override void ObjectDetecting(GameObject detectionObj) { }
    public override void OutOfDetection(GameObject detectionObj) { }

}
