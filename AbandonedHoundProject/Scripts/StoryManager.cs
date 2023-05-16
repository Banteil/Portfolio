using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Xml;

public class StoryManager : MonoBehaviour
{
    bool check;
    int num = 0;
    int maxNum;

    List<string> type = new List<string>();
    List<string> talker = new List<string>();
    List<string> dialog = new List<string>();
    List<string> cutSceneImageSprite = new List<string>();

    public Image cutSceneImage;
    public GameObject playerTalker;
    public GameObject otherTalker;
    public SpriteRenderer playerStanding;
    public SpriteRenderer otherStanding;
    public Text dialogPanelText;

    void Awake()
    {
        TextAsset textAsset = Resources.Load("XML/StoryDB") as TextAsset;
        XmlDocument storyDB = new XmlDocument();
        storyDB.LoadXml(textAsset.text);
        XmlNodeList storyNodeList = storyDB.SelectNodes("rows/row");

        foreach (XmlNode node in storyNodeList)
        {
            if (node.SelectSingleNode("kind").InnerText.Equals(DataPassing.storyKind))
            {
                type.Add(node.SelectSingleNode("type").InnerText);
                talker.Add(node.SelectSingleNode("talker").InnerText);
                dialog.Add(node.SelectSingleNode("dialog").InnerText);
                cutSceneImageSprite.Add(node.SelectSingleNode("sprite").InnerText);              
            }
        }

        maxNum = dialog.Count;
    }

    void Start()
    {
        StartCoroutine(StartStory());
    }

    public void NextDialog()
    {
        check = true;
    }

    IEnumerator StartStory()
    {
        while (true)
        {
            switch(type[num])
            {
                case "cutScene":
                    playerTalker.SetActive(false);
                    otherTalker.SetActive(false);
                    if (!cutSceneImageSprite[num].Equals("none"))
                    {
                        cutSceneImage.sprite = Resources.Load<Sprite>("Sprite/" + cutSceneImageSprite[num]);
                        cutSceneImage.color = new Color32(255, 255, 255, 255);
                    }
                    break;
                case "dialog":
                    cutSceneImage.color = new Color32(255, 255, 255, 0);

                    if (talker[num].Equals("player"))
                    {
                        playerTalker.SetActive(true);
                        otherTalker.SetActive(false);
                        if (!cutSceneImageSprite[num].Equals("none"))
                        {
                            otherStanding.sprite = null;
                            playerStanding.sprite = Resources.Load<Sprite>("Sprite/" + cutSceneImageSprite[num]);
                        }
                    }
                    else
                    {
                        playerTalker.SetActive(false);
                        otherTalker.SetActive(true);
                        otherTalker.GetComponentInChildren<Text>().text = talker[num];
                        if (!cutSceneImageSprite[num].Equals("none"))
                        {
                            playerStanding.sprite = null;
                            otherStanding.sprite = Resources.Load<Sprite>("Sprite/" + cutSceneImageSprite[num]);
                        }
                    }                    
                    break;
            }

            dialogPanelText.text = dialog[num];

            while (!check) yield return null;
            check = false;
            num++;

            if (num >= maxNum)
            {
                DataPassing.isStory = false;
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(DataPassing.getSceneName));
                SceneManager.UnloadSceneAsync("StoryScene");
                break;
            }

            yield return null;
        }
    }
}
