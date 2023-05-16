using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeywordPanel : MonoBehaviour
{
    public Text questionText;
    public Image background;
    public InputField answerInput;
    public Button inputButton;
    public RectTransform keywordItemPanel;
    public ReviewQuiz reviewQuiz;
    public delegate void InteractActive();
    public InteractActive keywordDestroy;
    Coroutine directing;
    GameObject keywordItem;
    bool isPlaying;
    List<string> keywordList = new List<string>();

    private void Start() => keywordItem = Resources.Load<GameObject>("Prefabs/UI/KeywordItem");

    private void OnEnable()
    {
        isPlaying = true;
        inputButton.interactable = true;
        answerInput.interactable = true;
        answerInput.ActivateInputField();
        StartCoroutine(KeywordProcess());
    }

    /// <summary>
    /// 주관식, 키워드 문제 정답 입력 함수
    /// </summary>
    public void InputAnswer()
    {
        if (!isPlaying) return;
        if (directing != null) StopCoroutine(directing);
        directing = StartCoroutine(DirectingAnswer(reviewQuiz.ConfirmCorrectAnswer(answerInput.text)));
    }

    public void SetKeywordList(string keywords)
    {
        string[] keyword = keywords.Split(',');
        for (int i = 0; i < keyword.Length; i++)
        {
            keywordList.Add(keyword[i]);
        }
    }

    IEnumerator DirectingAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            reviewQuiz.source.PlayOneShot(reviewQuiz.correctSound);
            isPlaying = false;
            background.color = Color.green;
            inputButton.interactable = false;
            answerInput.interactable = false;
        }
        else
        {
            answerInput.ActivateInputField();
            background.color = Color.red;
            yield return new WaitForSeconds(1f);
            background.color = Color.white;
        }
    }

    IEnumerator KeywordProcess()
    {
        float coolTime = 0f;
        while (isPlaying)
        {
            coolTime += Time.deltaTime;
            if (coolTime >= 1.5f)
            {
                float itemWidth = keywordItem.GetComponent<RectTransform>().rect.width / 2;
                float startX = -(keywordItemPanel.rect.width / 2f) + itemWidth;
                float endX = (keywordItemPanel.rect.width / 2f) - itemWidth;
                float positionX = Random.Range(startX, endX);
                string keyword = keywordList[Random.Range(0, keywordList.Count)];

                GameObject item = Instantiate(keywordItem, keywordItemPanel);
                float height = keywordItemPanel.rect.height / 2f + item.GetComponent<RectTransform>().rect.height;
                item.GetComponent<RectTransform>().localPosition = new Vector3(positionX, height);

                KeywordItem keywordScript = item.GetComponent<KeywordItem>();
                keywordScript.keyword.text = keyword;
                keywordScript.max = -height;
                keywordScript.panel = this;
                keywordDestroy += keywordScript.DestroyItem;
                coolTime = 0f;
            }
            yield return null;
        }

        keywordDestroy?.Invoke();
    }

    private void OnDisable()
    {
        background.color = Color.white;
        questionText.text = "";
        answerInput.text = "";
        keywordList = new List<string>();
    }
}
