using Newtonsoft.Json;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class DialogDataArray<T>
{
    public List<T> data = new List<T>();
}

[System.Serializable]
public class NPCTextData
{
    public int npcType;
    public string conversationType;
    public int index;
    public string contents;
    public string conversion;
    public string action;
}

[System.Serializable]
public class QuestionTextData
{
    public int npcType;
    public int phase;
    public string conversationType;
    public string contents;
    public string keyword;
}

public class NPCAct : MonoBehaviour
{
    bool fullScreenDialog;
    TextAsset textData, questionData;
    DialogDataArray<NPCTextData> dialogDataArray = new DialogDataArray<NPCTextData>();
    DialogDataArray<QuestionTextData> questionDataArray = new DialogDataArray<QuestionTextData>();

    [Header("NPCUI")]
    public GameObject chatBox;
    public GameObject fullChatBox;
    public GameObject switchBox;
    public Text chatText, fullChatText;
    public Text nameText;
    Color prevUIColor;

    public enum NPCType { MAIN, LOBBY, MASTER, GROUPER };
    NPCType npcType;
    public NPCType NPCTypeInfo
    {
        get { return npcType; }
        set
        {
            npcType = value;
            textData = Resources.Load<TextAsset>("Json/NPCDialog");
            DialogDataArray<NPCTextData> allDialog = JsonConvert.DeserializeObject<DialogDataArray<NPCTextData>>(textData.ToString());
            questionData = Resources.Load<TextAsset>("Json/QuestionsDialog");
            DialogDataArray<QuestionTextData> allQuestion = JsonConvert.DeserializeObject<DialogDataArray<QuestionTextData>>(questionData.ToString());
            conversationType = "start";
            nameText.text = "댕댕이";

            for (int i = 0; i < allDialog.data.Count; i++)
            {
                if (allDialog.data[i].npcType.Equals((int)value))
                    dialogDataArray.data.Add(allDialog.data[i]);
            }

            int lastTypeValue = Enum.GetValues(typeof(NPCType)).Cast<int>().Last();
            for (int i = 0; i < allQuestion.data.Count; i++)
            {
                if (allQuestion.data[i].npcType.Equals((int)value) || allQuestion.data[i].npcType > lastTypeValue)
                {
                    questionDataArray.data.Add(allQuestion.data[i]);
                }
            }
        }
    }

    [HideInInspector]
    public string conversationType;
    bool isChoice;
    List<string> choiceList = new List<string>();
    int currentPhase = 0;
    Coroutine chatBoxRoutine, conversationRoutine;
    public NPCObjectInteraction interObj;

    const float maxScale = 1f;

    public void Conversation()
    {
        Debug.Log("NPC 상호작용");
        if(conversationRoutine == null)
            conversationRoutine = StartCoroutine(ConversationProcess());        
    }

    public IEnumerator TalkingDialog(string content)
    {
        string boxContents = ConvertBoxChat(content);
        yield return chatBoxRoutine = StartCoroutine(ChatBoxProduction(boxContents));
    }

    public IEnumerator ConversationProcess()
    {
        while (true)
        {           
            while (chatBoxRoutine != null) { yield return null; };
            int count = 0;
            List<NPCTextData> dialog = new List<NPCTextData>();
            for (int i = 0; i < dialogDataArray.data.Count; i++)
            {
                if (conversationType.Equals(dialogDataArray.data[i].conversationType))
                    dialog.Add(dialogDataArray.data[i]);
            }

            while (count < dialog.Count)
            {
                //우선 실행 액션 여부 판단 및 실행                
                Action(dialog[count].action, true);

                if (!dialog[count].contents.Equals(""))
                {
                    //conversion 정보에 따라 변환문자 체크 후 내용 변경
                    string content = dialog[count].contents;
                    if (dialog[count].conversion != null)
                    {
                        string[] conversions = dialog[count].conversion.Split('_');
                        for (int i = 0; i < conversions.Length; i++)
                        {
                            content = Conversion(content, conversions[i]);
                        }
                    }
                    //채팅 패널에 내용 대입 및 말풍선용으로 string 변경
                    string boxContents = ConvertBoxChat(content);
                    //말풍선 대사 표시 코루틴 실행
                    yield return chatBoxRoutine = StartCoroutine(ChatBoxProduction(boxContents));
                }

                //액션 실행
                Action(dialog[count].action, false);
                count++;                
            }
            //;
            yield return null;
        }        
    }

    #region 채팅
    string Conversion(string chat, string conversionInfo)
    {
        switch (conversionInfo)
        {
            case "nickname":
                {
                    if (!DataManager.isFirstVisit)
                        return chat.Replace("&", PhotonNetwork.LocalPlayer.NickName);
                    else
                        return chat.Replace("&", "손");
                }
            default:
                return chat;
        }
    }

    string ConvertBoxChat(string chat)
    {
        int convertCount;
        if (fullScreenDialog)
            convertCount = 50;
        else
            convertCount = 20;

        string newLine = "";
        int max = chat.Length / convertCount;
        if (max.Equals(0)) newLine = chat;
        else
        {
            int count = 0;
            for (int i = 0; i < max; i++)
            {
                count++;
                string line = chat.Substring(convertCount * i, convertCount);
                if (!i.Equals(max - 1))
                    line += "\n";
                else if (chat.Substring(convertCount * count).Length > 0)
                {
                    line += "\n";
                    newLine += line + chat.Substring(convertCount * count);
                    count++;
                    break;
                }

                newLine += line;
            }
        }
        return newLine;
    }

    /// <summary>
    /// 말풍선 효과 + 표시 코루틴 함수
    /// </summary>
    IEnumerator ChatBoxProduction(string chat)
    {
        GameObject localChatBox;
        Text localChatText;
        if (fullScreenDialog)
        {
            localChatBox = fullChatBox;
            localChatText = fullChatText;
        }
        else
        {
            localChatBox = chatBox;
            localChatText = chatText;
        }

        //말풍선 크기 증가
        while (localChatBox.transform.localScale.y < maxScale)
        {
            localChatBox.transform.localScale += new Vector3(0f, maxScale / 10f, 0f);
            yield return null;
        }
        
        localChatBox.transform.localScale = new Vector3(maxScale, maxScale, 1f);

        //말풍선 텍스트 대입
        localChatText.text = chat;
        //이름 표시 텍스트 활성화
        if (!fullScreenDialog)
            nameText.gameObject.SetActive(true);

        //대기
        int waitTime = (chat.Trim().Length / 10) + 2;
        yield return new WaitForSeconds(waitTime);

        //이름 표시 텍스트 비활성화
        if (!fullScreenDialog)
            nameText.gameObject.SetActive(false);

        //말풍선 크기 감소
        while (localChatBox.transform.localScale.y > 0f)
        {
            localChatBox.transform.localScale -= new Vector3(0f, maxScale / 10f, 0f);
            yield return null;
        }
        localChatBox.transform.localScale = new Vector3(maxScale, 0f, 1f);

        //말풍선 텍스트 및 루틴 정보 초기화
        localChatText.text = "";
        chatBoxRoutine = null;
    }

    /// <summary>
    /// 선택지 말풍선 효과 + 표시 코루틴 함수
    /// </summary>
    IEnumerator SwitchProduction()
    {
        //말풍선 크기 증가
        while (switchBox.transform.localScale.y < maxScale)
        {
            switchBox.transform.localScale += new Vector3(0f, maxScale / 10f, 0f);
            yield return null;
        }
        switchBox.transform.localScale = new Vector3(maxScale, maxScale, 1f);

        //말풍선 텍스트 대입
        for (int i = 0; i < choiceList.Count; i++)
        {
            GameObject switchTextObj = Instantiate(Resources.Load<GameObject>("Prefabs/SwitchText"), switchBox.transform, false);
            NPCSwitchText nST = switchTextObj.GetComponent<NPCSwitchText>();
            string content = (i + 1).ToString() + ". " + choiceList[i];
            nST.switchText.text = content;
            nST.index = i + 1;
            nST.npc = this;
        }
        
        //이름 표시 텍스트 활성화
        if (!fullScreenDialog)
            nameText.gameObject.SetActive(true);

        //대기
        conversationType = "choice";
        while (isChoice) { yield return null; };

        //이름 표시 텍스트 비활성화
        if (!fullScreenDialog)
            nameText.gameObject.SetActive(false);
        
        //말풍선 크기 감소
        while (switchBox.transform.localScale.y > 0f)
        {
            switchBox.transform.localScale -= new Vector3(0f, maxScale / 10f, 0f);
            yield return null;
        }
        switchBox.transform.localScale = new Vector3(maxScale, 0f, 1f);

        //말풍선 텍스트 및 루틴 정보 초기화
        for (int i = 1; i < switchBox.transform.childCount; i++)
        {
            Destroy(switchBox.transform.GetChild(i).gameObject);
        }
        chatBoxRoutine = null;
    }
    #endregion

    #region NPC 액션
    /// <summary>
    /// 액션 키워드를 체크하여 그에 맞는 동작을 취하는 함수
    /// </summary>
    void Action(string info, bool isFirst)
    {
        string[] actions = info.Split('|');
        for (int i = 0; i < actions.Length; i++)
        {
            string[] actionInfo = actions[i].Split('_');
            if (isFirst && !CheckFirstAction(actionInfo[0])) continue;

            switch (actionInfo[0])
            {
                case "switch":
                    {
                        isChoice = true;
                        string[] switchInfo = actionInfo[1].Split(',');
                        for (int j = 0; j < switchInfo.Length; j++)
                        {
                            choiceList.Add(ConversationTypeToText(switchInfo[j]));
                        }                        
                        chatBoxRoutine = StartCoroutine(SwitchProduction());
                    }
                    break;
                case "end":
                    {
                        conversationType = "start";
                        if (conversationRoutine != null)
                        {
                            StopCoroutine(conversationRoutine);
                            conversationRoutine = null;
                            StudyEnd();
                        }
                    }
                    break;
                case "uiFocus":
                    {
                        Image uiImage = GameObject.Find(actionInfo[1]).GetComponent<Image>();
                        if (!uiImage.color.Equals(Color.red))
                        {
                            prevUIColor = uiImage.color;
                            uiImage.color = Color.red;
                        }
                        else
                        {
                            uiImage.color = prevUIColor;
                        }
                    }
                    break;
                case "active":
                    {
                        interObj.SendMessage(actionInfo[1]);
                    }
                    break;
                case "fullScreen":
                    {
                        if (!fullScreenDialog) fullScreenDialog = true;
                        else fullScreenDialog = false;
                    }
                    break;
                case "move":
                    {
                        if (conversationRoutine != null)
                        {
                            StopCoroutine(conversationRoutine);
                            conversationRoutine = null;
                            conversationType = "start";
                        }
                        
                        LoadManager.LoadScene(actionInfo[1], LoadManager.WorkType.OFFLINEGUIDE);                        
                    }
                    break;
                default:
                    return;
            }
        }
    }

    /// <summary>
    /// 대화 유형 키워드를 체크하여 그에 맞는 질문 내용을 반환하는 함수
    /// </summary>
    string ConversationTypeToText(string type)
    {
        for (int i = 0; i < questionDataArray.data.Count; i++)
        {
            if (questionDataArray.data[i].conversationType.Equals(type))
            {
                if (!questionDataArray.data[i].phase.Equals(currentPhase))
                    currentPhase = questionDataArray.data[i].phase;

                return questionDataArray.data[i].contents;
            }
        }

        return "에러!";
    }

    /// <summary>
    /// 채팅 내용 중 질문 내용이 들어간 키워드를 참고, 그에 맞는 대화 유형을 반환하는 함수
    /// </summary>
    string TextToConversationType(string text)
    {
        for (int i = 0; i < questionDataArray.data.Count; i++)
        {
            if (!currentPhase.Equals(questionDataArray.data[i].phase))
                continue;

            string[] keywords = questionDataArray.data[i].keyword.Split(',');
            for (int j = 0; j < keywords.Length; j++)
            {
                if (text.Contains(keywords[j]))
                {
                    return questionDataArray.data[i].conversationType;
                }
            }            
        }
        return conversationType;
    }

    public void CheckChoice(string chat)
    {
        string prevType = conversationType;
        conversationType = TextToConversationType(chat);
        if (prevType.Equals(conversationType)) return;

        isChoice = false;
        choiceList = new List<string>();
    }

    public void CheckChoice(int index)
    {
        if (index <= 0 || index > choiceList.Count) return;
        string prevType = conversationType;
        conversationType = TextToConversationType(choiceList[index - 1]);
        if (prevType.Equals(conversationType)) return;

        isChoice = false;
        choiceList = new List<string>();
    }

    bool CheckFirstAction(string action)
    {
        if (action.Equals("uiFocus") || action.Equals("fullScreen"))
            return true;
        else
            return false;
    }

    public void StudyEnd()
    {        
        if(SceneManager.GetActiveScene().name.Equals("OpenStudyRoomScene"))
        {
            textData = null;
            questionData = null;
            dialogDataArray = new DialogDataArray<NPCTextData>();
            questionDataArray = new DialogDataArray<QuestionTextData>();
        }
    }
    #endregion
}
